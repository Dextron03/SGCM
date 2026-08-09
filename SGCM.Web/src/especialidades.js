import { getSession } from "./api.js";

const API_URL = "/api/specialties";

let specialties = [];

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

async function apiFetch(path = "", options = {}) {
    const token = getToken();

    if (!token) {
        throw new Error(
            "Debes iniciar sesión para administrar las especialidades."
        );
    }

    const headers = {
        Authorization: `Bearer ${token}`,
        ...(options.headers || {})
    };

    if (options.body) {
        headers["Content-Type"] = "application/json";
    }

    const response = await fetch(
        `${API_URL}${path}`,
        {
            ...options,
            headers
        }
    );

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

const specialtyApi = {
    getAll: () =>
        apiFetch().then(unwrap),

    getById: id =>
        apiFetch(
            `/${encodeURIComponent(id)}`
        ).then(unwrap),

    create: dto =>
        apiFetch("", {
            method: "POST",
            body: JSON.stringify(dto)
        }).then(unwrap),

    update: (id, dto) =>
        apiFetch(
            `/${encodeURIComponent(id)}`,
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
            `/${encodeURIComponent(id)}`,
            {
                method: "DELETE"
            }
        )
};

function showMessage(message, type = "error") {
    const element = document.getElementById(
        "specialty-message"
    );

    if (!element) {
        return;
    }

    element.textContent = message;
    element.className = `message visible ${type}`;
}

function clearMessage() {
    const element = document.getElementById(
        "specialty-message"
    );

    if (!element) {
        return;
    }

    element.textContent = "";
    element.className = "message";
}

function resetForm() {
    const form = document.getElementById(
        "specialty-form"
    );

    form.reset();

    document.getElementById(
        "specialty-id"
    ).value = "";

    document.getElementById(
        "form-title"
    ).textContent = "Nueva especialidad";

    document.getElementById(
        "save-specialty"
    ).textContent = "Guardar";

    document.getElementById(
        "cancel-edit"
    ).hidden = true;
}

function startEditing(id) {
    const specialty = specialties.find(
        item => item.id === id
    );

    if (!specialty) {
        return;
    }

    document.getElementById(
        "specialty-id"
    ).value = specialty.id;

    document.getElementById(
        "specialty-name"
    ).value = specialty.name || "";

    document.getElementById(
        "specialty-description"
    ).value = specialty.description || "";

    document.getElementById(
        "form-title"
    ).textContent = "Editar especialidad";

    document.getElementById(
        "save-specialty"
    ).textContent = "Actualizar";

    document.getElementById(
        "cancel-edit"
    ).hidden = false;

    document.getElementById(
        "specialty-name"
    ).focus();
}

function createActionButton(
    label,
    className,
    onClick
) {
    const button = document.createElement("button");

    button.type = "button";
    button.className = className;
    button.textContent = label;
    button.addEventListener("click", onClick);

    return button;
}

function renderSpecialties() {
    const tableBody = document.getElementById(
        "specialties-body"
    );

    tableBody.replaceChildren();

    if (specialties.length === 0) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");

        cell.colSpan = 3;
        cell.className = "empty-state";
        cell.textContent =
            "Todavía no hay especialidades registradas.";

        row.appendChild(cell);
        tableBody.appendChild(row);

        return;
    }

    specialties.forEach(specialty => {
        const row = document.createElement("tr");

        const nameCell =
            document.createElement("td");

        const descriptionCell =
            document.createElement("td");

        const actionsCell =
            document.createElement("td");

        const actions =
            document.createElement("div");

        nameCell.textContent =
            specialty.name;

        descriptionCell.textContent =
            specialty.description ||
            "Sin descripción";

        actions.className = "row-actions";

        actions.append(
            createActionButton(
                "Editar",
                "edit-button",
                () => startEditing(specialty.id)
            ),
            createActionButton(
                "Eliminar",
                "delete-button",
                () => deleteSpecialty(
                    specialty.id,
                    specialty.name
                )
            )
        );

        actionsCell.appendChild(actions);

        row.append(
            nameCell,
            descriptionCell,
            actionsCell
        );

        tableBody.appendChild(row);
    });
}

async function loadSpecialties(options = {}) {
    if (!options.preserveMessage) {
        clearMessage();
    }

    try {
        specialties =
            await specialtyApi.getAll() || [];

        specialties.sort((first, second) =>
            (first.name || "").localeCompare(
                second.name || "",
                "es"
            )
        );

        renderSpecialties();
    } catch (error) {
        specialties = [];
        renderSpecialties();

        showMessage(
            error.message ||
            "No se pudieron cargar las especialidades."
        );
    }
}

async function saveSpecialty(event) {
    event.preventDefault();
    clearMessage();

    const id = document.getElementById(
        "specialty-id"
    ).value;

    const dto = {
        name: document.getElementById(
            "specialty-name"
        ).value.trim(),

        description: document.getElementById(
            "specialty-description"
        ).value.trim()
    };

    if (!dto.name) {
        showMessage(
            "Escribe el nombre de la especialidad."
        );

        return;
    }

    const saveButton = document.getElementById(
        "save-specialty"
    );

    saveButton.disabled = true;

    try {
        if (id) {
            await specialtyApi.update(id, dto);

            showMessage(
                "Especialidad actualizada correctamente.",
                "success"
            );
        } else {
            await specialtyApi.create(dto);

            showMessage(
                "Especialidad creada correctamente.",
                "success"
            );
        }

        resetForm();

        await loadSpecialties({
            preserveMessage: true
        });
    } catch (error) {
        showMessage(
            error.message ||
            "No se pudo guardar la especialidad."
        );
    } finally {
        saveButton.disabled = false;
    }
}

async function deleteSpecialty(id, name) {
    const confirmed = window.confirm(
        `¿Seguro que deseas eliminar la especialidad "${name}"?`
    );

    if (!confirmed) {
        return;
    }

    clearMessage();

    try {
        await specialtyApi.remove(id);

        if (
            document.getElementById(
                "specialty-id"
            ).value === id
        ) {
            resetForm();
        }

        showMessage(
            "Especialidad eliminada correctamente.",
            "success"
        );

        await loadSpecialties({
            preserveMessage: true
        });
    } catch (error) {
        showMessage(
            error.message ||
            "No se pudo eliminar la especialidad."
        );
    }
}

window.sgcmSpecialties = specialtyApi;

document.addEventListener(
    "DOMContentLoaded",
    () => {
        if (
            document.body.dataset.page !==
            "specialties-admin"
        ) {
            return;
        }

        document
            .getElementById("specialty-form")
            .addEventListener(
                "submit",
                saveSpecialty
            );

        document
            .getElementById("cancel-edit")
            .addEventListener(
                "click",
                resetForm
            );

        loadSpecialties();
    }
);