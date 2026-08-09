import { serializeLocalDateTime } from './appointment-utils.js'

const appointmentForm = document.getElementById('appointment-form')
const searchForm = document.getElementById('search-form')
const appointmentsForm = document.getElementById('appointments-form')
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

async function api(path, options = {}) {
  return requestJson(`/api/appointments${path}`, options)
}

async function availability(path) {
  return requestJson(`/api/availability${path}`)
}

async function requestJson(url, options = {}) {
  const controller = new AbortController()
  const timeout = setTimeout(() => controller.abort(), 10000)

  try {
    const response = await fetch(url, {
      headers: { 'Content-Type': 'application/json' },
      ...options,
      signal: controller.signal,
    })
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

function toDateTime(date, time) {
  return new Date(`${date}T${time}`)
}

function getDay(date) {
  return ((new Date(`${date}T00:00:00`).getDay() + 6) % 7) + 1
}

function formatDateTime(value) {
  return new Date(value).toLocaleString('es-DO', { dateStyle: 'medium', timeStyle: 'short' })
}

function statusLabel(status) {
  return ({ 1: 'Pendiente', 2: 'Confirmada', 3: 'Completada', 4: 'Cancelada' })[status] ?? 'Sin estado'
}

function renderAppointments(appointments) {
  const list = document.getElementById('appointments-list')
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
    if (appointment.status === 1 || appointment.status === 2) {
      const cancel = document.createElement('button')
      cancel.type = 'button'
      cancel.textContent = 'Cancelar cita'
      cancel.className = 'cancel-button'
      cancel.addEventListener('click', async () => {
        if (!confirm('¿Deseas cancelar esta cita?')) return
        try {
          await api(`/${appointment.id}/cancel`, { method: 'PATCH' })
          show(messageEl, 'La cita fue cancelada.')
          appointmentsForm.requestSubmit()
        } catch (error) {
          show(errorEl, error.message)
        }
      })
      row.append(cancel)
    }
    list.append(row)
  })
}

if (searchForm) {
  let selectedDateTime = null
  searchForm.addEventListener('submit', async (event) => {
    event.preventDefault()
    hideMessages()
    const doctorId = document.getElementById('doctor-id').value.trim()
    const date = document.getElementById('date').value
    const slots = document.getElementById('slots')
    appointmentForm.hidden = true
    slots.replaceChildren()
    slots.hidden = false

    try {
      const [weeklyAvailability, appointments] = await Promise.all([
        availability(`/doctor/${encodeURIComponent(doctorId)}`),
        api(`/doctor/${encodeURIComponent(doctorId)}`),
      ])
      const daySlots = weeklyAvailability.filter((item) => item.day === getDay(date))
      const occupied = new Set(appointments
        .filter((item) => item.status !== 4 && new Date(item.dateTime).toDateString() === toDateTime(date, '00:00').toDateString())
        .map((item) => new Date(item.dateTime).getTime()))
      const availableSlots = []
      daySlots.forEach((block) => {
        const cursor = toDateTime(date, block.startTime)
        const end = toDateTime(date, block.endTime)
        while (cursor < end) {
          if (!occupied.has(cursor.getTime()) && cursor > new Date()) availableSlots.push(new Date(cursor))
          cursor.setMinutes(cursor.getMinutes() + 30)
        }
      })
      if (!availableSlots.length) {
        const empty = document.createElement('p')
        empty.className = 'empty-state'
        empty.textContent = 'No hay horarios libres para esta fecha. Prueba con otro día.'
        slots.append(empty)
        return
      }
      const title = document.createElement('p')
      title.className = 'section-description'
      title.textContent = 'Paso 2: selecciona un horario disponible.'
      slots.append(title)
      const grid = document.createElement('div')
      grid.className = 'slot-grid'
      slots.append(grid)
      availableSlots.forEach((slot) => {
        const button = document.createElement('button')
        button.type = 'button'
        button.textContent = slot.toLocaleTimeString('es-DO', { hour: '2-digit', minute: '2-digit' })
        button.className = 'slot-button'
        button.addEventListener('click', () => {
          selectedDateTime = slot
          grid.querySelectorAll('.slot-button').forEach((item) => item.classList.remove('is-selected'))
          button.classList.add('is-selected')
          document.getElementById('selected-slot').textContent = `Horario seleccionado: ${formatDateTime(slot.toISOString())}`
          appointmentForm.hidden = false
          appointmentForm.scrollIntoView({ behavior: 'smooth', block: 'start' })
        })
        grid.append(button)
      })
    } catch (error) {
      show(errorEl, error.message)
    }
  })

  appointmentForm.addEventListener('submit', async (event) => {
    event.preventDefault()
    hideMessages()
    try {
      await api('', {
        method: 'POST',
        body: JSON.stringify({
          patientId: document.getElementById('patient-id').value.trim(),
          doctorId: document.getElementById('doctor-id').value.trim(),
          dateTime: serializeLocalDateTime(selectedDateTime),
          reason: document.getElementById('reason').value.trim(),
        }),
      })
      appointmentForm.reset()
      searchForm.reset()
      appointmentForm.hidden = true
      document.getElementById('selected-slot').textContent = ''
      const slots = document.getElementById('slots')
      slots.replaceChildren()
      slots.hidden = true
      selectedDateTime = null
      show(
        messageEl,
        'La cita se registró correctamente. Puedes consultar y verificar sus detalles desde <a class="success-link" href="/mis-citas.html">Seguimiento de Citas</a>.',
        true,
        false
      )
    } catch (error) {
      show(errorEl, error.message)
    }
  })
}

if (appointmentsForm) {
  appointmentsForm.addEventListener('submit', async (event) => {
    event.preventDefault()
    hideMessages()
    try {
      const patientId = document.getElementById('patient-id').value.trim()
      renderAppointments(await api(`/patient/${encodeURIComponent(patientId)}`))
    } catch (error) {
      show(errorEl, error.message)
    }
  })
}
