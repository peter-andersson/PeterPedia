export function ToastSuccess(text) {
    const toast = CreateToastElement(text);

    toast.classList.add("bg-success");

    ShowToast(toast);
}

export function ToastError(text) {
    const toast = CreateToastElement(text);

    toast.classList.add("bg-danger");

    ShowToast(toast);
}

function ShowToast(element) {
    const container = document.getElementById("toast-container");

    container.appendChild(element);

    element.addEventListener("hidden.bs.toast", function () {
        container.removeChild(element);
    });

    const toast = bootstrap.Toast.getOrCreateInstance(element);

    toast.show();
}

function CreateToastElement(text) {
    const div = document.createElement("div");

    div.classList.add("toast", "align-items-center", "text-white");

    div.setAttribute("role", "alert");
    div.setAttribute("aria-live", "assertive");
    div.setAttribute("aria-atomic", "true");
    div.setAttribute("data-bs-delay", "2000");

    div.innerHTML = `
<div class="d-flex">
    <div class="toast-body">${text}</div>
    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
</div>
`;

    return div;
}