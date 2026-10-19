import test from 'node:test'
import assert from 'node:assert/strict'
import { validateWasteRequest } from '../src/validation/wasteRequest.js'
import { mockSettings, mockCreateWasteRequest, mockGetWasteRequests } from '../src/api/mockWasteRequests.js'

const valid = { title: 'Вывоз кислот', laboratoryId: 3, wasteTypeId: 7, description: 'Накопилось 20 литров отработанных кислот' }

test('границы полей: пробелы, длина, целые числовые ссылки', () => {
  assert.deepEqual(validateWasteRequest(valid), {})
  for (const title of ['', '    ', 'abcd', 'x'.repeat(81)]) {
    assert.ok(validateWasteRequest({ ...valid, title }).title)
  }
  for (const title of ['x'.repeat(5), 'x'.repeat(80)]) {
    assert.equal(validateWasteRequest({ ...valid, title }).title, undefined)
  }
  for (const description of ['x'.repeat(9), 'x'.repeat(501)]) {
    assert.ok(validateWasteRequest({ ...valid, description }).description)
  }
  for (const field of ['laboratoryId', 'wasteTypeId']) {
    for (const value of [0, -1, 1.5, '3', '', null]) {
      assert.ok(validateWasteRequest({ ...valid, [field]: value })[field])
    }
  }
})

test('mock отклоняет невалидное создание с 400 и не меняет список', async () => {
  const before = await mockGetWasteRequests()
  const result = await mockCreateWasteRequest({ ...valid, title: 'abc' })
  assert.equal(result.status, 400)
  assert.equal(result.ok, false)
  assert.ok(result.data.errors.title)
  assert.equal((await mockGetWasteRequests()).data.length, before.data.length)
})

test('mock назначает id, номер и New; клиент не подменяет служебные поля', async () => {
  const result = await mockCreateWasteRequest({ ...valid, title: '  Вывоз кислот  ', id: 999, number: 'FAKE', status: 'Closed', assigneeUserId: 99 })
  assert.equal(result.status, 201)
  assert.equal(result.data.title, valid.title)
  assert.notEqual(result.data.id, 999)
  assert.match(result.data.number, /^WR-2026-\d{4}$/)
  assert.equal(result.data.status, 'New')
  assert.equal(result.data.assigneeUserId, null)
  const another = await mockCreateWasteRequest(valid)
  assert.notEqual(another.data.id, result.data.id)
  assert.notEqual(another.data.number, result.data.number)
})

test('учебный 500 не создаёт запись, пустой список — успешный 200', async () => {
  const before = (await mockGetWasteRequests()).data.length
  mockSettings.force500 = true
  try {
    assert.equal((await mockCreateWasteRequest(valid)).status, 500)
    assert.equal((await mockGetWasteRequests()).status, 500)
  } finally {
    mockSettings.force500 = false
  }
  assert.equal((await mockGetWasteRequests()).data.length, before)
  mockSettings.empty = true
  try {
    const result = await mockGetWasteRequests()
    assert.equal(result.status, 200)
    assert.deepEqual(result.data, [])
  } finally {
    mockSettings.empty = false
  }
})
