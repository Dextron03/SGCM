import { defineConfig } from 'vite'
import { fileURLToPath, URL } from 'node:url'

const resolveFromRoot = (path) =>
  fileURLToPath(new URL(path, import.meta.url))

export default defineConfig({
  server: {
    proxy: {
      '/api': 'http://localhost:5236',
    },
  },

  build: {
    outDir: '../SGCM/wwwroot',
    emptyOutDir: true,

    rollupOptions: {
      input: {
        main: resolveFromRoot('./index.html'),
        register: resolveFromRoot('./register.html'),
        dashboard: resolveFromRoot('./dashboard.html'),
        confirmEmail: resolveFromRoot('./confirm-email.html'),
        availability: resolveFromRoot('./disponibilidad.html'),
        scheduleAppointment: resolveFromRoot('./agendar-cita.html'),
        appointments: resolveFromRoot('./mis-citas.html'),
        doctorAppointments: resolveFromRoot('./citas-doctor.html'),
        doctorProfile: resolveFromRoot('./perfil-doctor.html'),
        specialtiesAdmin: resolveFromRoot('./admin-especialidades.html'),
        patientProfile: resolveFromRoot('./perfil-paciente.html'),
        medicalRecords: resolveFromRoot('./expediente-medico.html'),
      },
    },
  },
})