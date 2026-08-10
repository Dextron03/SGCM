import { getSession } from "./api.js";

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
            "Debes iniciar sesión para consultar el perfil."
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

const patientApi = {
    getAll: () =>
        apiFetch(PATIENTS_URL).then(unwrap),

    getById: id =>
        apiFetch(
            `${PATIENTS_URL}/${encodeURIComponent(id)}`
        ).then(unwrap),

    getCurrent: () =>
        apiFetch(`${PATIENTS_URL}/me`).then(unwrap),

    create: dto =>
        apiFetch(PATIENTS_URL, {
            method: "POST",
            body: JSON.stringify(dto)
        }).then(unwrap),

    update: (id, dto) =>
        apiFetch(
            `${PATIENTS_URL}/${encodeURIComponent(id)}`,
            {
                method: "PUT",
                body: JSON.stringify({
                    ...dto,
                    id
                })
            }
        ).then(unwrap),

    remove: id =>
        apiFetch(
            `${PATIENTS_URL}/${encodeURIComponent(id)}`,
            {
                method: "DELETE"
            }
        )
};

function setText(elementId, value) {
    const element = document.getElementById(elementId);

    if (!element) {
        return;
    }

    element.textContent = value || "No disponible";
    element.classList.remove("loading");
}

function showMessage(message, type = "error") {
    const element = document.getElementById(
        "profile-message"
    );

    if (!element) {
        return;
    }

    element.textContent = message;
    element.className = `message visible ${type}`;
}

function calculateInitials(name) {
    return name
        .split(/\s+/)
        .filter(Boolean)
        .slice(0, 2)
        .map(part => part[0].toUpperCase())
        .join("");
}

function formatDate(value) {
    if (!value) {
        return "No disponible";
    }

    return new Date(value).toLocaleDateString("es-DO", {
        year: "numeric",
        month: "long",
        day: "numeric"
    });
}

async function loadPatientProfile() {
    const retryButton = document.getElementById(
        "retry-profile"
    );

    if (retryButton) {
        retryButton.hidden = true;
    }

    try {
        const session = getSession();

        if (!session) {
            throw new Error(
                "Debes iniciar sesión para consultar el perfil."
            );
        }

        const patient = await patientApi.getCurrent();

        const fullName =
            session.fullName || "Paciente";

        const email =
            session.email || "No disponible";

        setText("patient-name", fullName);
        setText("patient-email", email);
        setText(
            "patient-ssn",
            patient.socialSecurityNumber
        );
        setText(
            "patient-dob",
            formatDate(patient.dateOfBirth)
        );
        setText("patient-address", patient.address);
        setText("patient-id", patient.id);
        setText(
            "patient-initials",
            calculateInitials(fullName) || "PA"
        );

        showMessage(
            "Perfil cargado correctamente.",
            "success"
        );
    } catch (error) {
        showMessage(
            error.message ||
            "No se pudo cargar el perfil del paciente."
        );

        if (retryButton) {
            retryButton.hidden = false;
        }
    }
}

window.sgcmPatients = patientApi;

document.addEventListener(
    "DOMContentLoaded",
    () => {
        if (
            document.body.dataset.page !==
            "patient-profile"
        ) {
            return;
        }

        document
            .getElementById("retry-profile")
            ?.addEventListener(
                "click",
                loadPatientProfile
            );

        loadPatientProfile();
    }
);
