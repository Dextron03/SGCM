import { getSession } from "./api.js";

const RECORDS_URL = "/api/medical-records";
const DOCTORS_URL = "/api/doctors";
const PATIENTS_URL = "/api/patients";

function getToken() {
    const session = getSession();

    if (!session) {
        return null;
    }

    return (
        session.jwToken ||
        session.jwtToken ||
        session.token ||
        null
    );
}

async function apiFetch(url, options = {}) {
    const token = getToken();

    if (!token) {
        throw new Error(
            "Debes iniciar sesión para consultar los expedientes."
        );
    }

    const headers = {
        Authorization: `Bearer ${token}`,
        ...(options.headers || {})
    };

    if (options.body) {
        headers["Content-Type"] = "application/json";
    }

    const response = await fetch(url, {
        ...options,
        headers
    });

    const payload = await response
        .json()
        .catch(() => null);

    if (response.status === 401) {
        throw new Error(
            "Tu sesión no es válida o ha expirado."
        );
    }

    if (!response.ok || payload?.success === false) {
        throw new Error(
            payload?.message ||
            `No se pudo completar la solicitud (${response.status}).`
        );
    }

    return payload;
}

function unwrap(payload) {
    if (
        payload &&
        Object.prototype.hasOwnProperty.call(
            payload,
            "data"
        )
    ) {
        return payload.data;
    }

    return payload;
}

const recordApi = {
    getByPatient: patientId =>
        apiFetch(
            `${RECORDS_URL}/patient/${encodeURIComponent(patientId)}`
        ).then(unwrap),

    create: dto =>
        apiFetch(RECORDS_URL, {
            method: "POST",
            body: JSON.stringify(dto)
        }).then(unwrap)
};

function getCurrentDoctor() {
    return apiFetch(`${DOCTORS_URL}/me`).then(unwrap);
}

function getCurrentPatient() {
    return apiFetch(`${PATIENTS_URL}/me`).then(unwrap);
}

function showMessage(message, type = "error") {
    const element = document.getElementById(
        "record-message"
    );

    if (!element) {
        return;
    }

    element.textContent = message;
    element.className = `message visible ${type}`;
}

function formatDateTime(value) {
    if (!value) {
        return "No disponible";
    }

    return new Date(value).toLocaleString("es-DO", {
        dateStyle: "medium",
        timeStyle: "short"
    });
}

function renderRecords(records) {
    const list = document.getElementById("records-list");

    list.replaceChildren();

    if (!records.length) {
        const empty = document.createElement("p");
        empty.className = "empty-state";
        empty.textContent =
            "Todavía no tienes expedientes registrados.";
        list.append(empty);
        return;
    }

    records
        .slice()
        .sort(
            (first, second) =>
                new Date(second.creationDate) -
                new Date(first.creationDate)
        )
        .forEach(record => {
            const card = document.createElement("article");
            card.className = "record-card";

            const date = document.createElement("p");
            date.className = "record-date";
            date.textContent = formatDateTime(
                record.creationDate
            );
            card.append(date);

            const dl = document.createElement("dl");

            const fields = [
                ["Diagnóstico", record.diagnosis],
                ["Tratamiento", record.treatment],
                ["Notas", record.notes || "Sin notas"],
                ["Cita relacionada", record.appointmentId]
            ];

            fields.forEach(([label, value]) => {
                const dt = document.createElement("dt");
                dt.textContent = label;

                const dd = document.createElement("dd");
                dd.textContent = value || "No disponible";

                dl.append(dt, dd);
            });

            card.append(dl);
            list.append(card);
        });
}

async function loadPatientRecords() {
    const panel = document.getElementById("patient-panel");
    panel.hidden = false;

    try {
        const patient = await getCurrentPatient();
        const records = await recordApi.getByPatient(
            patient.id
        );

        renderRecords(records || []);
    } catch (error) {
        renderRecords([]);

        showMessage(
            error.message ||
            "No se pudieron cargar tus expedientes."
        );
    }
}

async function setupDoctorPanel() {
    const panel = document.getElementById("doctor-panel");
    panel.hidden = false;

    let doctor;

    try {
        doctor = await getCurrentDoctor();
    } catch (error) {
        showMessage(
            error.message ||
            "No se pudo identificar tu perfil médico."
        );
        return;
    }

    document
        .getElementById("record-form")
        .addEventListener("submit", async event => {
            event.preventDefault();

            const dto = {
                appointmentId: document
                    .getElementById("appointment-id")
                    .value.trim(),

                patientId: document
                    .getElementById("patient-id")
                    .value.trim(),

                doctorId: doctor.id,

                diagnosis: document
                    .getElementById("diagnosis")
                    .value.trim(),

                treatment: document
                    .getElementById("treatment")
                    .value.trim(),

                notes: document
                    .getElementById("notes")
                    .value.trim()
            };

            const saveButton = document.getElementById(
                "save-record"
            );

            saveButton.disabled = true;

            try {
                await recordApi.create(dto);

                document
                    .getElementById("record-form")
                    .reset();

                showMessage(
                    "Expediente guardado correctamente.",
                    "success"
                );
            } catch (error) {
                showMessage(
                    error.message ||
                    "No se pudo guardar el expediente."
                );
            } finally {
                saveButton.disabled = false;
            }
        });
}

async function init() {
    const session = getSession();

    if (!session) {
        showMessage(
            "Debes iniciar sesión para ver esta página."
        );
        return;
    }

    const roles = session.roles || [];
    const tasks = [];

    if (roles.includes("Doctor")) {
        tasks.push(setupDoctorPanel());
    }

    if (roles.includes("Patient")) {
        tasks.push(loadPatientRecords());
    }

    if (!tasks.length) {
        showMessage(
            "Tu perfil no tiene acceso a los expedientes médicos."
        );
        return;
    }

    await Promise.all(tasks);
}

window.sgcmMedicalRecords = recordApi;

document.addEventListener("DOMContentLoaded", () => {
    if (document.body.dataset.page !== "medical-records") {
        return;
    }

    init();
});
