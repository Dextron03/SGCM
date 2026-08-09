const BASE_URL = '/api/account'
const SESSION_KEY = 'sgcm.session'

async function postJson(path, body) {
  const response = await fetch(`${BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  return response.json()
}

export function register(dto) {
  return postJson('/register', dto)
}

export function login(dto) {
  return postJson('/login', dto)
}

export async function confirmEmail(userId, token) {
  const params = new URLSearchParams({ userId, token })
  const response = await fetch(`${BASE_URL}/confirm-email?${params.toString()}`)
  return response.json()
}

export function saveSession(authResponse) {
  localStorage.setItem(SESSION_KEY, JSON.stringify(authResponse))
}

export function getSession() {
  const raw = localStorage.getItem(SESSION_KEY)
  if (!raw) return null

  const session = JSON.parse(raw)
  if (new Date(session.expiration) <= new Date()) {
    clearSession()
    return null
  }
  return session
}

export function clearSession() {
  localStorage.removeItem(SESSION_KEY)
}
