import { getSession } from './api.js'

export function requireSession() {
  const session = getSession()
  if (!session) {
    window.location.replace('/index.html')
    return null
  }
  return session
}
