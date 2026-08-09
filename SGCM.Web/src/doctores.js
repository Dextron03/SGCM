import { getSession } from "./api.js";

const DOCTORS_URL = "/api/doctors";
const SPECIALTIES_URL = "/api/specialties";

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

const doctorApi = {
    getAll: () =>
        apiFetch(DOCTORS_URL).then(unwrap),

    getById: id =>
        apiFetch(
            `${DOCTORS_URL}/${encodeURIComponent(id)}`
        ).then(unwrap),

    getBySpecialty: specialtyId =>
        apiFetch(
            `${DOCTORS_URL}/by-specialty/${encodeURIComponent(specialtyId)}`
        ).then(unwrap),

    getCurrent: () =>
        apiFetch(`${DOCTORS_URL}/me`).then(unwrap),

    create: dto =>
        apiFetch(DOCTORS_URL, {
            method: "POST",
            body: JSON.stringify(dto)
        }).then(unwrap),

    update: (id, dto) =>
        apiFetch(
            `${DOCTORS_URL}/${encodeURIComponent(id)}`,
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
            `${DOCTORS_URL}/${encodeURIComponent(id)}`,
            {
                method: "DELETE"
            }
        )
};

async function getSpecialty(id) {
    const payload = await apiFetch(
        `${SPECIALTIES_URL}/${encodeURIComponent(id)}`
    );

    return unwrap(payload);
}

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

async function loadDoctorProfile() {
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

        const doctor = await doctorApi.getCurrent();

        let specialtyName = "No asignada";

        if (doctor.specialtyId) {
            const specialty = await getSpecialty(
                doctor.specialtyId
            );

            specialtyName =
                specialty.name || specialtyName;
        }

        const fullName =
            session.fullName || "Doctor";

        const email =
            session.email || "No disponible";

        setText("doctor-name", fullName);
        setText("doctor-email", email);
        setText(
            "doctor-license",
            doctor.medicalLicense
        );
        setText(
            "doctor-specialty",
            specialtyName
        );
        setText("doctor-id", doctor.id);
        setText(
            "doctor-initials",
            calculateInitials(fullName) || "DR"
        );

        showMessage(
            "Perfil cargado correctamente.",
            "success"
        );
    } catch (error) {
        showMessage(
            error.message ||
            "No se pudo cargar el perfil del doctor."
        );

        if (retryButton) {
            retryButton.hidden = false;
        }
    }
}

window.sgcmDoctors = doctorApi;

document.addEventListener(
    "DOMContentLoaded",
    () => {
        if (
            document.body.dataset.page !==
            "doctor-profile"
        ) {
            return;
        }

        document
            .getElementById("retry-profile")
            ?.addEventListener(
                "click",
                loadDoctorProfile
            );

        loadDoctorProfile();
    }
);