import { clearSession } from './api.js'
import { requireSession } from './guard.js'

const session = requireSession()

if (session) {
  document.getElementById('welcome').textContent = `Hola, ${session.fullName} (${session.email})`
  document.getElementById('role').textContent = session.roles.join(', ')

  document.getElementById('logout-button').addEventListener('click', () => {
    clearSession()
    window.location.replace('/index.html')
  })
}
