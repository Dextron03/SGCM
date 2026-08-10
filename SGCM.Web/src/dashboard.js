import { clearSession } from './api.js'
import { requireSession } from './guard.js'

const LINKS_BY_ROLE = {
  Doctor: [
    { href: '/perfil-doctor.html', title: 'Mi perfil médico', description: 'Consulta tus datos profesionales y tu especialidad.' },
    { href: '/citas-doctor.html', title: 'Mi agenda', description: 'Confirma, rechaza, reprograma o cancela citas de tus pacientes.' },
    { href: '/disponibilidad.html', title: 'Mi disponibilidad', description: 'Define tu horario semanal de atención.' },
    { href: '/expediente-medico.html', title: 'Expedientes médicos', description: 'Registra el expediente de tus pacientes tras la consulta.' },
  ],
  Patient: [
    { href: '/perfil-paciente.html', title: 'Mi perfil', description: 'Consulta tu información personal registrada.' },
    { href: '/agendar-cita.html', title: 'Agendar cita', description: 'Busca disponibilidad y reserva un horario.' },
    { href: '/mis-citas.html', title: 'Mis citas', description: 'Revisa tus próximas citas y cancela si lo necesitas.' },
    { href: '/expediente-medico.html', title: 'Mis expedientes', description: 'Consulta el historial de tus consultas médicas.' },
  ],
  Admin: [
    { href: '/admin-especialidades.html', title: 'Especialidades', description: 'Administra el catálogo de especialidades médicas.' },
  ],
}

function renderQuickLinks(roles) {
  const nav = document.getElementById('quick-links')
  const seen = new Set()

  roles
    .flatMap((role) => LINKS_BY_ROLE[role] ?? [])
    .filter((link) => (seen.has(link.href) ? false : seen.add(link.href)))
    .forEach((link) => {
      const anchor = document.createElement('a')
      anchor.className = 'quick-link-card'
      anchor.href = link.href

      const title = document.createElement('strong')
      title.textContent = link.title

      const description = document.createElement('span')
      description.textContent = link.description

      anchor.append(title, description)
      nav.append(anchor)
    })
}

const session = requireSession()

if (session) {
  document.getElementById('welcome').textContent = `Hola, ${session.fullName} (${session.email})`
  document.getElementById('role').textContent = session.roles.join(', ')

  renderQuickLinks(session.roles)

  document.getElementById('logout-button').addEventListener('click', () => {
    clearSession()
    window.location.replace('/index.html')
  })
}
