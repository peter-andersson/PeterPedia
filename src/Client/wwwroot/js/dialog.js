export function ShowDialog(elementId) {
    let modal = bootstrap.Modal.getOrCreateInstance(document.getElementById(elementId))

    modal.show();
}

export function HideDialog(elementId) {
    let modal = bootstrap.Modal.getOrCreateInstance(document.getElementById(elementId))

    modal.hide();
}