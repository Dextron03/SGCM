import { getSession } from './api.js'

function authHeaders() {
  const session = getSession()
  const token = session?.jwToken || session?.jwtToken || session?.token
  return token ? { Authorization: `Bearer ${token}` } : {}
}

const days = ['', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado', 'Domingo']
const form = document.getElementById('availability-form')
const doctorIdInput = document.getElementById('doctor-id')
const list = document.getElementById('availability-list')
const errorEl = document.getElementById('error')
const messageEl = document.getElementById('message')

let feedbackTimeoutId = null

function clearFeedbackTimer() {
  if (feedbackTimeoutId) {
    clearTimeout(feedbackTimeoutId)
    feedbackTimeoutId = null
  }
}

function show(element, message, html = false, autoHide = true) {
  clearFeedbackTimer()
  if (html) {
    element.innerHTML = message
  } else {
    element.textContent = message
  }
  element.hidden = false

  if (autoHide) {
    feedbackTimeoutId = window.setTimeout(() => {
      element.hidden = true
      element.textContent = ''
      element.innerHTML = ''
      feedbackTimeoutId = null
    }, 4000)
  }
}

function hideMessages() {
  clearFeedbackTimer()
  errorEl.hidden = true
  messageEl.hidden = true
  messageEl.textContent = ''
  messageEl.innerHTML = ''
}

function formatTime(value) {
  return value.slice(0, 5)
}

async function request(path, options = {}) {
  const controller = new AbortController()
  const timeout = setTimeout(() => controller.abort(), 10000)

  try {
    const response = await fetch(`/api/availability${path}`, {
      ...options,
      headers: { 'Content-Type': 'application/json', ...authHeaders(), ...(options.headers || {}) },
      signal: controller.signal,
    })

    if (response.status === 401) {
      throw new Error('Tu sesión no es válida o ha expirado.')
    }

    const body = await response.text()
    let result

    try {
      result = body ? JSON.parse(body) : null
    } catch {
      throw new Error('El servidor devolvió una respuesta no válida.')
    }

    if (!response.ok || !result?.success) {
      throw new Error(result?.message ?? 'No se pudo completar la operación.')
    }
    return result.data
  } catch (error) {
    if (error.name === 'AbortError') throw new Error('La solicitud tardó demasiado. Intenta nuevamente.')
    if (error instanceof TypeError) throw new Error('No se pudo conectar con el servidor.')
    throw error
  } finally {
    clearTimeout(timeout)
  }
}

async function loadAvailability() {
  const doctorId = doctorIdInput.value.trim()
  if (!doctorId) return

  try {
    const availabilities = await request(`/doctor/${encodeURIComponent(doctorId)}`)
    list.replaceChildren()
    if (!availabilities.length) {
      const empty = document.createElement('p')
      empty.className = 'empty-state'
      empty.textContent = 'Aún no hay horarios registrados para este doctor.'
      list.append(empty)
      return
    }

    availabilities
      .sort((a, b) => a.day - b.day || a.startTime.localeCompare(b.startTime))
      .forEach((availability) => {
        const row = document.createElement('div')
        row.className = 'availability-item'
        const detail = document.createElement('p')
        detail.textContent = `${days[availability.day]} · ${formatTime(availability.startTime)}–${formatTime(availability.endTime)}`
        row.append(detail)
        const remove = document.createElement('button')
        remove.type = 'button'
        remove.textContent = 'Eliminar'
        remove.className = 'cancel-button'
        remove.addEventListener('click', async () => {
          if (!confirm('¿Eliminar este bloque de horario?')) return
          try {
            await request(`/${availability.id}`, { method: 'DELETE' })
            show(messageEl, 'Horario eliminado.')
            await loadAvailability()
          } catch (error) {
            show(errorEl, error.message)
          }
        })
        row.append(remove)
        list.append(row)
      })
  } catch (error) {
    show(errorEl, error.message)
  }
}

doctorIdInput.addEventListener('change', loadAvailability)

form.addEventListener('submit', async (event) => {
  event.preventDefault()
  hideMessages()
  const dto = {
    doctorId: doctorIdInput.value.trim(),
    day: Number(form.day.value),
    startTime: form.startTime.value,
    endTime: form.endTime.value,
  }

  try {
    await request('', { method: 'POST', body: JSON.stringify(dto) })
    form.reset()
    show(messageEl, 'La disponibilidad se registró correctamente y ya está lista para su uso en la agenda.', false)
    await loadAvailability()
  } catch (error) {
    show(errorEl, error.message)
  }
})
