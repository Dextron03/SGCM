import { resetPassword } from './api.js'

const form = document.getElementById('reset-password-form')
const feedbackEl = document.getElementById('feedback')

function showFeedback(message, isError) {
  feedbackEl.textContent = message
  feedbackEl.className = isError ? 'error' : 'success'
  feedbackEl.hidden = false
}

const params = new URLSearchParams(window.location.search)
const userId = params.get('userId')
const token = params.get('token')

if (!userId || !token) {
  showFeedback('Enlace de restablecimiento inválido.', true)
  form.hidden = true
} else {
  form.addEventListener('submit', async (event) => {
    event.preventDefault()
    feedbackEl.hidden = true

    if (form.newPassword.value !== form.confirmPassword.value) {
      showFeedback('Las contraseñas no coinciden.', true)
      return
    }

    const result = await resetPassword(userId, token, form.newPassword.value, form.confirmPassword.value)

    if (!result.success) {
      showFeedback(result.message ?? 'No se pudo restablecer la contraseña.', true)
      return
    }

    form.reset()
    form.hidden = true
    showFeedback(result.message ?? 'Tu contraseña fue restablecida. Ya puedes iniciar sesión.', false)
  })
}
