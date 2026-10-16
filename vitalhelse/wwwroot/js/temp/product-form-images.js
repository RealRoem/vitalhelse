/* Product form image upload and management */

// Private scope
(function() {
    'use strict';

    // Get anti-forgery token from form
    function getAntiForgeryToken() {
        return document.querySelector('input[name="__RequestVerificationToken"]').value;
    }

    // Image preview function
    // `path` = Path to the image
    // `container` = DOM element to append the preview to
    // `tempID` = Unique identifier for the upload session
    function addImagePreview(path, container, tempID) {
        // Create wrapper div for image and delete button
        const wrapper = document.createElement('div');
        wrapper.classList.add('image-preview-wrapper');

        // Create image element and set source
        const img = document.createElement('img');
        img.src = path;

        // Add error handler for images that fail to load
        // `this` in regular function = the element that triggered the event
        img.onerror = function() {
            console.error('Failed to load image:', path);
            console.error('Image path:', this.src);
            // Styling for error message
            this.style.border = '2px solid red';
            this.alt = 'Bilde kunne ikke lastes';
        };

        // Create delete button
        const deleteBtn = document.createElement('button');
        // `type` = Type of the button
        deleteBtn.type = 'button';
        // `innerHTML` = HTML content of the button this case an X
        deleteBtn.innerHTML = '&times;';
        // `classList` = Adds or removes classes from an element
        deleteBtn.classList.add('delete-image-btn');
        deleteBtn.title = 'Slett bilde';

        
        deleteBtn.addEventListener('click', (e) => {
            // `preventDefault` = Prevents default action of the event
            e.preventDefault();
            // `stopPropagation` = Prevents event from bubbling up the DOM tree
            // This is important to prevent the click event from firing on the wrapper div (moving up the DOM tree)
            e.stopPropagation();

            // JavaScript object that simulates a form submission
            const formData = new FormData();
            // `fromData.append (`key`, `value`)` = Adds a key-value pair to the form data
            formData.append('imagePath', path);
            formData.append('tempID', tempID);
            formData.append('__RequestVerificationToken', getAntiForgeryToken());

            // Send form data to server to delete the image
            // Calls ProductController's DeleteTempPicture method
            fetch('/AdminProduct/DeleteTempPicture', {
                method: 'POST',
                body: formData
            })
                // `then` = Chaining method to execute a function after another promise is resolved
                // So waits for the response from the server before continuing
                // Its a Json response so we use `res.json()` to parse it
                .then(res => res.json())
                .then(data => {
                    // `data.success` = Boolean indicating if the deletion was successful
                    if (data.success) {
                        // Remove image preview from DOM
                        wrapper.remove();
                        // Error handeling if unsuccesfull
                    } else {
                        // `console.error` = Logs an error message to the console for debugging
                        console.error('Failed to delete image');
                        // For the end user
                        alert('Kunne ikke slette bildet');
                    }
                })
                // General error handling
                .catch(error => {
                    console.error('Error deleting image:', error);
                    alert('En feil oppstod ved sletting av bilde');
                });
        });

        // Append image and delete button to wrapper div
        wrapper.appendChild(img);
        wrapper.appendChild(deleteBtn);
        container.appendChild(wrapper);
    }
    
    // Page load event listener
    document.addEventListener('DOMContentLoaded', () => {
        const tempIDElement = document.getElementById('TempID');
        const uploadButton = document.getElementById('uploadButton');
        const previewContainer = document.getElementById('previewImages');

        // Exit if not on product add/edit page
        if (!tempIDElement || !uploadButton || !previewContainer) {
            return;
        }
        
        // Get the value of the hidden input field with the TempID
        const TempID = tempIDElement.value;

        // Upload button click handler
        uploadButton.addEventListener('click', () => {
            // Create file input element
            const input = document.createElement('input');
            // What input type to use
            input.type = 'file';
            // What file types to accept (added several here for testing)
            input.accept = 'image/jpeg,image/png,image/gif,image/webp,image/avif,.jpg,.jpeg,.png,.gif,.webp,.avif';
            // Allow multiple files to be selected
            // So user dont need to add one picture at a time
            input.multiple = true;
            // This opens the file dialog allowing the user to select files
            input.click();

            // This runs when the user selects files
            input.onchange = () => {
                // This enforces the rules of image files
                const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'image/avif'];
                // This enforces the maximum file size to 10MB
                // Change to allow larger files if needed
                // Google calculations for file sizes if needed
                const maxSize = 10 * 1024 * 1024;

                // Loop through selected files
                // RUns checks if the file/files are supported
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

                    // Creates a new form 
                    const formData = new FormData();
                    // Adding the file (image)
                    formData.append('file', file);
                    // Adding the TempID
                    formData.append('tempID', TempID);

                    // Calls the UploadPicture method
                    fetch('/AdminProduct/UploadPicture', {
                        method: 'POST',
                        body: formData
                    })
                        // Parses Json response
                        .then(res => res.json())
                        .then(data => {
                            // If success calls the addImagePreview function with relevant params
                            if (data.success) {
                                addImagePreview(data.path, previewContainer, TempID);
                            }
                            // Error Handeling
                            else {
                                alert(`Kunne ikke laste opp ${file.name}: ${data.error || 'Ukjent feil'}`);
                            }
                        })
                        // Error handeling
                        .catch(error => {
                            console.error('Error uploading image:', error);
                            alert(`En feil oppstod ved opplasting av ${file.name}`);
                        });
                }
            };
        });

        // Preload existing images from window global variable
        // See cshtml ProductForm for reference
        if (window.existingProductImages && window.existingProductImages.length > 0) {
            window.existingProductImages.forEach(path => {
                addImagePreview(path, previewContainer, TempID);
            });
        }
    });
})();