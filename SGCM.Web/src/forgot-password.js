import { forgotPassword, getSession } from './api.js'

if (getSession()) {
  window.location.replace('/dashboard.html')
}

const form = document.getElementById('forgot-password-form')
const feedbackEl = document.getElementById('feedback')

function showFeedback(message, isError) {
  feedbackEl.textContent = message
  feedbackEl.className = isError ? 'error' : 'success'
  feedbackEl.hidden = false
}

form.addEventListener('submit', async (event) => {
  event.preventDefault()
  feedbackEl.hidden = true

  const result = await forgotPassword(form.email.value)

  form.reset()
  showFeedback(
    result.message ?? 'Si el correo está registrado, recibirás un enlace para restablecer tu contraseña.',
    !result.success,
  )
})
