import { validateWasteRequest } from '../validation/wasteRequest.js'

const delay = () => new Promise((resolve) => setTimeout(resolve, 500))

// Учебные переключатели для воспроизведения отказа и пустого списка.
export const mockSettings = { force500: false, empty: false }

let nextId = 3
const records = [
  { id: 1, number: 'WR-2026-0001', title: 'Вывоз отработанных кислот', description: 'Вывезти накопившиеся отработанные кислоты', laboratoryId: 3, wasteTypeId: 7, status: 'New' },
  { id: 2, number: 'WR-2026-0002', title: 'Вывоз люминесцентных ламп', description: 'Вывезти отработанные люминесцентные лампы', laboratoryId: 1, wasteTypeId: 4, status: 'InProgress' },
]

function reply(method, status, data) {
  const response = { ok: status >= 200 && status < 300, status, data }
  console.info(`[mock без HTTP] ${method} /api/waste-requests`, response)
  return response
}

export async function mockGetWasteRequests() {
  await delay()
  if (mockSettings.force500) return reply('GET', 500, { error: 'Учебный отказ сервера' })
  return reply('GET', 200, mockSettings.empty ? [] : records.map((item) => ({ ...item })))
}

export async function mockCreateWasteRequest(body) {
  await delay()
  if (mockSettings.force500) return reply('POST', 500, { error: 'Учебный отказ сервера' })
  const errors = validateWasteRequest(body)
  if (Object.keys(errors).length) return reply('POST', 400, { errors })
  const id = nextId++
  const now = new Date().toISOString()
  const item = {
    title: body.title.trim(),
    laboratoryId: body.laboratoryId,
    wasteTypeId: body.wasteTypeId,
    description: body.description.trim(),
    id,
    number: `WR-2026-${String(id).padStart(4, '0')}`,
    status: 'New',
    createdByUserId: 5,
    assigneeUserId: null,
    createdAt: now,
    updatedAt: now,
  }
  if (mockSettings.empty) records.length = 0
  mockSettings.empty = false
  records.push(item)
  return reply('POST', 201, { ...item })
}
