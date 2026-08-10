import { serializeLocalDateTime } from './appointment-utils.js'
import { getSession } from './api.js'

const errorEl = document.getElementById('error')
const messageEl = document.getElementById('message')

let feedbackTimeoutId = null

function clearFeedbackTimer() {
  if (feedbackTimeoutId) {
    clearTimeout(feedbackTimeoutId)
    feedbackTimeoutId = null
  }
}

function show(element, message) {
  clearFeedbackTimer()
  element.textContent = message
  element.hidden = false
  feedbackTimeoutId = window.setTimeout(() => {
    element.hidden = true
    element.textContent = ''
    feedbackTimeoutId = null
  }, 4000)
}

function authHeaders() {
  const session = getSession()
  const token = session?.jwToken || session?.jwtToken || session?.token
  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function requestJson(url, options = {}) {
  if (!getSession()) throw new Error('Debes iniciar sesión para consultar tu agenda.')

  const controller = new AbortController()
  const timeout = setTimeout(() => controller.abort(), 10000)

  try {
    const response = await fetch(url, {
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

function formatDateTime(value) {
  return new Date(value).toLocaleString('es-DO', { dateStyle: 'medium', timeStyle: 'short' })
}

function statusLabel(status) {
  return ({ 1: 'Pendiente', 2: 'Confirmada', 3: 'Completada', 4: 'Cancelada' })[status] ?? 'Sin estado'
}

async function changeStatus(id, status) {
  return requestJson(`/api/appointments/${id}/status`, {
    method: 'PATCH',
    body: JSON.stringify({ status }),
  })
}

function buildRescheduleForm(appointment, onDone) {
  const form = document.createElement('form')
  form.className = 'reschedule-form'
  form.hidden = true

  const input = document.createElement('input')
  input.type = 'datetime-local'
  input.required = true

  const save = document.createElement('button')
  save.type = 'submit'
  save.textContent = 'Guardar nueva fecha'

  form.append(input, save)

  form.addEventListener('submit', async (event) => {
    event.preventDefault()
    if (!input.value) return
    try {
      await requestJson(`/api/appointments/${appointment.id}`, {
        method: 'PUT',
        body: JSON.stringify({
          dateTime: serializeLocalDateTime(new Date(input.value)),
          reason: appointment.reason,
        }),
      })
      show(messageEl, 'La cita fue reprogramada y quedó pendiente de confirmación.')
      onDone()
    } catch (error) {
      show(errorEl, error.message)
    }
  })

  return form
}

async function loadAppointments(doctorId) {
  const list = document.getElementById('appointments-list')

  try {
    const appointments = await requestJson(`/api/appointments/doctor/${encodeURIComponent(doctorId)}`)
    renderAppointments(list, appointments || [], doctorId)
  } catch (error) {
    list.replaceChildren()
    show(errorEl, error.message)
  }
}

function renderAppointments(list, appointments, doctorId) {
  list.replaceChildren()
  if (!appointments.length) {
    list.textContent = 'No tienes citas registradas.'
    return
  }

  appointments.sort((a, b) => new Date(a.dateTime) - new Date(b.dateTime)).forEach((appointment) => {
    const row = document.createElement('div')
    row.className = 'appointment-item'

    const detail = document.createElement('p')
    detail.className = 'appointment-meta'
    detail.textContent = `${formatDateTime(appointment.dateTime)} — ${appointment.reason} · ${statusLabel(appointment.status)}`
    row.append(detail)

    const reload = () => loadAppointments(doctorId)

    if (appointment.status === 1 || appointment.status === 2) {
      const actions = document.createElement('div')
      actions.className = 'appointment-actions'
      const rescheduleForm = buildRescheduleForm(appointment, reload)

      if (appointment.status === 1) {
        const confirmButton = document.createElement('button')
        confirmButton.type = 'button'
        confirmButton.textContent = 'Confirmar'
        confirmButton.addEventListener('click', async () => {
          try {
            await changeStatus(appointment.id, 2)
            show(messageEl, 'La cita fue confirmada.')
            reload()
          } catch (error) {
            show(errorEl, error.message)
          }
        })
        actions.append(confirmButton)

        const rejectButton = document.createElement('button')
        rejectButton.type = 'button'
        rejectButton.textContent = 'Rechazar'
        rejectButton.className = 'cancel-button'
        rejectButton.addEventListener('click', async () => {
          if (!confirm('¿Deseas rechazar esta solicitud de cita?')) return
          try {
            await changeStatus(appointment.id, 4)
            show(messageEl, 'La cita fue rechazada.')
            reload()
          } catch (error) {
            show(errorEl, error.message)
          }
        })
        actions.append(rejectButton)
      }

      if (appointment.status === 2) {
        const completeButton = document.createElement('button')
        completeButton.type = 'button'
        completeButton.textContent = 'Marcar como completada'
        completeButton.addEventListener('click', async () => {
          try {
            await changeStatus(appointment.id, 3)
            show(messageEl, 'La cita fue marcada como completada.')
            reload()
          } catch (error) {
            show(errorEl, error.message)
          }
        })
        actions.append(completeButton)

        const cancelButton = document.createElement('button')
        cancelButton.type = 'button'
        cancelButton.textContent = 'Cancelar cita'
        cancelButton.className = 'cancel-button'
        cancelButton.addEventListener('click', async () => {
          if (!confirm('¿Deseas cancelar esta cita?')) return
          try {
            await changeStatus(appointment.id, 4)
            show(messageEl, 'La cita fue cancelada.')
            reload()
          } catch (error) {
            show(errorEl, error.message)
          }
        })
        actions.append(cancelButton)
      }

      const rescheduleButton = document.createElement('button')
      rescheduleButton.type = 'button'
      rescheduleButton.textContent = 'Reprogramar'
      rescheduleButton.className = 'button-secondary'
      rescheduleButton.addEventListener('click', () => {
        rescheduleForm.hidden = !rescheduleForm.hidden
      })
      actions.append(rescheduleButton)

      row.append(actions, rescheduleForm)
    }

    list.append(row)
  })
}

async function init() {
  const list = document.getElementById('appointments-list')

  try {
    const doctor = await requestJson('/api/doctors/me')
    await loadAppointments(doctor.id)
  } catch (error) {
    list.replaceChildren()
    show(errorEl, error.message || 'No se pudo identificar tu perfil médico.')
  }
}

init()
