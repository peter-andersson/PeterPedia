export function ShowAddModal() {
    ShowModal("add-movie-dialog");
}

export function HideAddModal() {
    HideModal("add-movie-dialog");
}

export function ShowEditModal() {
    ShowModal("edit-movie-dialog");
}

export function HideEditModal() {
    HideModal("edit-movie-dialog");
}

export function ShowDeleteModal() {
    ShowModal("delete-movie-dialog");
}

export function HideDeleteModal() {
    HideModal("delete-movie-dialog");
}

function ShowModal(element) {
    let modal = bootstrap.Modal.getOrCreateInstance(document.getElementById(element))

    modal.show();
}

function HideModal(element) {
    let modal = bootstrap.Modal.getOrCreateInstance(document.getElementById(element))

    modal.hide();
}