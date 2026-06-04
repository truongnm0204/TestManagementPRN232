function openDeleteModal(actionUrl, itemName) {
    const form = document.getElementById('deleteConfirmationForm');
    const nameElement = document.getElementById('deleteItemName');
    if (!form || !nameElement) {
        return;
    }

    form.setAttribute('action', actionUrl);
    nameElement.textContent = itemName || 'mục này';
    bootstrap.Modal.getOrCreateInstance(document.getElementById('deleteConfirmationModal')).show();
}

function bindDeleteButtons() {
    document.addEventListener('click', (event) => {
        const button = event.target.closest('.js-delete');
        if (!button) {
            return;
        }

        openDeleteModal(button.dataset.deleteUrl, button.dataset.deleteName);
    });
}

function bindSubmitLoading() {
    document.querySelectorAll('form').forEach((form) => {
        form.addEventListener('submit', () => {
            const submitButton = form.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.dataset.originalText = submitButton.innerHTML;
                submitButton.innerHTML = 'Đang xử lý...';
            }
        });
    });
}

document.addEventListener('DOMContentLoaded', () => {
    bindDeleteButtons();
    bindSubmitLoading();
});
