import { confirmEmail } from './api.js'

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
  showFeedback('Enlace de confirmación inválido.', true)
} else {
  const result = await confirmEmail(userId, token)
  showFeedback(
    result.message ?? (result.success ? 'Correo confirmado.' : 'No se pudo confirmar el correo.'),
    !result.success,
  )
}
