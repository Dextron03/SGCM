import { register, getSession } from './api.js'

if (getSession()) {
  window.location.replace('/dashboard.html')
}

const form = document.getElementById('register-form')
const feedbackEl = document.getElementById('feedback')

function showFeedback(message, isError) {
  feedbackEl.textContent = message
  feedbackEl.className = isError ? 'error' : 'success'
  feedbackEl.hidden = false
}

form.addEventListener('submit', async (event) => {
  event.preventDefault()
  feedbackEl.hidden = true

  const dto = {
    fullName: form.fullName.value,
    email: form.email.value,
    phoneNumber: form.phoneNumber.value,
    password: form.password.value,
    confirmPassword: form.confirmPassword.value,
    role: form.role.value,
  }

  if (dto.password !== dto.confirmPassword) {
    showFeedback('Las contraseñas no coinciden.', true)
    return
  }

  const result = await register(dto)

  if (!result.success) {
    showFeedback(result.message ?? 'No se pudo crear la cuenta.', true)
    return
  }

  form.reset()
  showFeedback(result.message ?? 'Revisa tu correo electrónico para confirmar tu cuenta.', false)
})
