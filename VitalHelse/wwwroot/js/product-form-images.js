/* Product form image upload and management */
(function() {
    'use strict';

    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]').value;
    }

    function addImagePreview(path, container, tempID) {
        const wrapper = document.createElement('div');
        wrapper.classList.add('image-preview-wrapper');

        const img = document.createElement('img');
        img.src = path;

        // Add error handler for images that fail to load
        img.onerror = function() {
            console.error('Failed to load image:', path);
            console.error('Image path:', this.src);
            this.style.border = '2px solid red';
            this.alt = 'Bilde kunne ikke lastes';
        };

        const deleteBtn = document.createElement('button');
        deleteBtn.type = 'button';
        deleteBtn.innerHTML = '&times;';
        deleteBtn.classList.add('delete-image-btn');
        deleteBtn.title = 'Slett bilde';

        deleteBtn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();

            const formData = new FormData();
            formData.append('imagePath', path);
            formData.append('tempID', tempID);
            formData.append('__RequestVerificationToken', getAntiForgeryToken());

            fetch('/Product/DeleteTempPicture', {
                method: 'POST',
                body: formData
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        wrapper.remove();
                    } else {
                        console.error('Failed to delete image');
                        alert('Kunne ikke slette bildet');
                    }
                })
                .catch(error => {
                    console.error('Error deleting image:', error);
                    alert('En feil oppstod ved sletting av bilde');
                });
        });

        wrapper.appendChild(img);
        wrapper.appendChild(deleteBtn);
        container.appendChild(wrapper);
    }

    document.addEventListener('DOMContentLoaded', () => {
        const tempIDElement = document.getElementById('TempID');
        const uploadButton = document.getElementById('uploadButton');
        const previewContainer = document.getElementById('previewImages');

        // Exit if not on product add/edit page
        if (!tempIDElement || !uploadButton || !previewContainer) {
            return;
        }

        const TempID = tempIDElement.value;

        // Upload button click handler
        uploadButton.addEventListener('click', () => {
            const input = document.createElement('input');
            input.type = 'file';
            input.accept = 'image/jpeg,image/png,image/gif,image/webp,image/avif,.jpg,.jpeg,.png,.gif,.webp,.avif';
            input.multiple = true;
            input.click();

            input.onchange = () => {
                const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'image/avif'];
                const maxSize = 10 * 1024 * 1024; // 10MB

                for (let file of input.files) {
                    // Validate file type
                    if (!allowedTypes.includes(file.type)) {
                        alert(`${file.name} er ikke et støttet bildeformat`);
                        continue;
                    }

                    // Validate file size
                    if (file.size > maxSize) {
                        alert(`${file.name} er for stor (maks 10MB)`);
                        continue;
                    }

                    const formData = new FormData();
                    formData.append('file', file);
                    formData.append('tempID', TempID);

                    fetch('/Product/UploadPicture', {
                        method: 'POST',
                        body: formData
                    })
                        .then(res => res.json())
                        .then(data => {
                            if (data.success) {
                                addImagePreview(data.path, previewContainer, TempID);
                            } else {
                                alert(`Kunne ikke laste opp ${file.name}: ${data.error || 'Ukjent feil'}`);
                            }
                        })
                        .catch(error => {
                            console.error('Error uploading image:', error);
                            alert(`En feil oppstod ved opplasting av ${file.name}`);
                        });
                }
            };
        });

        // Preload existing images from window global variable
        if (window.existingProductImages && window.existingProductImages.length > 0) {
            window.existingProductImages.forEach(path => {
                addImagePreview(path, previewContainer, TempID);
            });
        }
    });
})();