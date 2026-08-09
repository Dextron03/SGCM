import { login, saveSession, getSession } from './api.js'

if (getSession()) {
  window.location.replace('/dashboard.html')
}

const form = document.getElementById('login-form')
const errorEl = document.getElementById('error')

function showError(message) {
  errorEl.textContent = message
  errorEl.hidden = false
}

form.addEventListener('submit', async (event) => {
  event.preventDefault()
  errorEl.hidden = true

  const dto = {
    email: form.email.value,
    password: form.password.value,
  }

  const result = await login(dto)

  if (!result.success) {
    showError(result.message ?? 'No se pudo iniciar sesión.')
    return
  }

  saveSession(result.data)
  window.location.replace('/dashboard.html')
})
