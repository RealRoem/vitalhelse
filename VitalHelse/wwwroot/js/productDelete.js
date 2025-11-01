(function() {
    'use strict';

    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]').value;
    }

    document.addEventListener('DOMContentLoaded', () => {
        const deleteProductBtn = document.getElementById('deleteProductBtn');
        const deleteModalElement = document.getElementById('deleteConfirmModal');
        const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

        // Exit if not in edit mode (these elements only exist in edit mode)
        if (!deleteProductBtn || !deleteModalElement || !confirmDeleteBtn) {
            return;
        }

        const deleteModal = new bootstrap.Modal(deleteModalElement);
        const productIdInput = document.querySelector('input[name="Product.ProductId"]');

        deleteProductBtn.addEventListener('click', (e) => {
            e.preventDefault();
            deleteModal.show();
        });

        confirmDeleteBtn.addEventListener('click', () => {
            const productId = productIdInput ? productIdInput.value : null;

            if (!productId) {
                alert('Kunne ikke finne produkt ID');
                return;
            }

            const formData = new FormData();
            formData.append('id', productId);
            formData.append('__RequestVerificationToken', getAntiForgeryToken());

            fetch('/Product/DeleteProduct', {
                method: 'POST',
                body: formData
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        deleteModal.hide();
                        alert('Produkt slettet');
                        window.location.href = '/';
                    } else {
                        alert('Kunne ikke slette produkt: ' + (data.error || 'Ukjent feil'));
                    }
                })
                .catch(error => {
                    console.error('Error deleting product:', error);
                    alert('En feil oppstod ved sletting av produkt');
                });
        });
    });
})();