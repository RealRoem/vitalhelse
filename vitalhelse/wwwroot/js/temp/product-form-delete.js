/* Product form delete functionality */

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

        // Modal is a popup window that appears when the user clicks the delete button
        const deleteModal = new bootstrap.Modal(deleteModalElement);
        // Gets product ID from hidden input field
        const productIdInput = document.querySelector('input[name="Product.ProductId"]');

        // When user clicks the delete button, show the modal
        deleteProductBtn.addEventListener('click', (e) => {
            e.preventDefault();
            deleteModal.show();
        });
    
        // When user clicks the confirm delete button, send a request to delete the product
        confirmDeleteBtn.addEventListener('click', () => {
            // Gets product ID from hidden input field
            // `null` = If the input field doesn't exist, it returns null
            // `const productId = condition ? valueIfTrue : valueIfFalse;`
            const productId = productIdInput ? productIdInput.value : null;

            // Error handling if product ID is not found
            if (!productId) {
                alert('Kunne ikke finne produkt ID');
                return;
            }

            // Create form data to send to server
            const formData = new FormData();
            formData.append('id', productId);
            formData.append('__RequestVerificationToken', getAntiForgeryToken());

            // Send form data to server to delete the product
            fetch('/AdminProduct/DeleteProduct', {
                method: 'POST',
                body: formData
            })
                // Parses Json response from server
                .then(res => res.json())
                .then(data => {
                    // If success, hide Modal, alert and redirect to home page
                    if (data.success) {
                        deleteModal.hide();
                        alert('Produkt slettet');
                        window.location.href = '/';
                    } 
                    // Error handling
                    else {
                        alert('Kunne ikke slette produkt: ' + (data.error || 'Ukjent feil'));
                    }
                })
                // General error handling
                .catch(error => {
                    console.error('Error deleting product:', error);
                    alert('En feil oppstod ved sletting av produkt');
                });
        });
    });
})();