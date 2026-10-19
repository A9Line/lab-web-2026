import { mockGetWasteRequests, mockCreateWasteRequest } from './mockWasteRequests.js'

const baseUrl = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000').replace(/\/$/, '')
const USE_MOCK = import.meta.env.VITE_USE_MOCK !== 'false'

export class ApiError extends Error {
  constructor(message, status = null, errors = {}) {
    super(message)
    this.status = status
    this.errors = errors
  }
}

async function request(method, body) {
  let response
  let data
  try {
    if (USE_MOCK) {
      response = method === 'GET' ? await mockGetWasteRequests() : await mockCreateWasteRequest(body)
      data = response.data
    } else {
      response = await fetch(`${baseUrl}/api/waste-requests`, {
        method,
        ...(body ? { headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) } : {}),
      })
    }
  } catch {
    throw new ApiError('Нет связи с сервером. Проверьте адрес и запуск API.')
  }

  // HTTP-отказ не является сетевой ошибкой fetch.
  if (!response.ok) {
    if (!USE_MOCK) data = await response.json().catch(() => ({}))
    if (response.status === 400) throw new ApiError('Проверьте поля формы.', 400, data.errors)
    if (response.status >= 500) throw new ApiError('Сервер не смог обработать запрос. Попробуйте позже.', response.status)
    throw new ApiError(`Запрос отклонён (HTTP ${response.status}).`, response.status)
  }
  if (!USE_MOCK) {
    try {
      data = await response.json()
    } catch {
      throw new ApiError('Сервер вернул некорректный ответ.', response.status)
    }
  }
  return data
}

export function getWasteRequests() {
  return request('GET')
}

export function createWasteRequest(body) {
  return request('POST', body)
}
