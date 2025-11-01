/* Product form tag input functionality */

(function() {
    'use strict';

    let tags = [];
    let allTags = [];
    let suggestionBox = null;

    document.addEventListener('DOMContentLoaded', () => {
        const tagInput = document.getElementById('tag-input');
        const tagContainer = document.getElementById('tag-input-container');
        const hiddenTagsContainer = document.getElementById('hidden-tags-container');

        // Exit early if elements don't exist (not on product add/edit page)
        if (!tagInput || !tagContainer || !hiddenTagsContainer) {
            return;
        }

        // Initialize tags from window global variables (set in AddProduct.cshtml)
        if (window.productTags) {
            tags = window.productTags;
        }
        if (window.allProductTags) {
            allTags = window.allProductTags;
        }

        // Render existing tags on load
        renderTags();

        // Handle Enter key to add tags
        tagInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                const value = tagInput.value.trim();
                if (value !== '') {
                    addTag(value);
                    tagInput.value = '';
                    hideSuggestions();
                }
            }
        });

        // Handle input for suggestions
        tagInput.addEventListener('input', function () {
            const inputValue = tagInput.value.toLowerCase();
            const filtered = allTags.filter(tag => 
                tag.toLowerCase().includes(inputValue) && !tags.includes(tag)
            );
            if (filtered.length > 0 && inputValue.length > 0) {
                showSuggestions(filtered);
            } else {
                hideSuggestions();
            }
        });

        // Close suggestions when clicking outside
        document.addEventListener('click', (e) => {
            if (!tagContainer.contains(e.target)) {
                hideSuggestions();
            }
        });

        function addTag(tagText) {
            if (!tags.includes(tagText)) {
                tags.push(tagText);
                renderTags();
            }
        }

        function showSuggestions(filtered) {
            hideSuggestions();
            suggestionBox = document.createElement('div');
            suggestionBox.classList.add('suggestion-box');
            
            filtered.forEach(tag => {
                const option = document.createElement('div');
                option.textContent = tag;
                option.classList.add('suggestion-item');
                option.addEventListener('click', () => {
                    addTag(tag);
                    tagInput.value = '';
                    hideSuggestions();
                });
                suggestionBox.appendChild(option);
            });
            
            tagContainer.appendChild(suggestionBox);
        }

        function hideSuggestions() {
            if (suggestionBox) {
                suggestionBox.remove();
            }
            suggestionBox = null;
        }

        function renderTags() {
            // Remove existing tag elements
            tagContainer.querySelectorAll('.tag').forEach(t => t.remove());
            hiddenTagsContainer.innerHTML = '';

            tags.forEach(tag => {
                // Create visible tag element
                const tagEl = document.createElement('div');
                tagEl.classList.add('tag');
                tagEl.innerHTML = `${tag} <span class="remove-tag">&times;</span>`;
                
                tagEl.querySelector('.remove-tag').addEventListener('click', () => {
                    tags = tags.filter(t => t !== tag);
                    renderTags();
                });
                
                tagContainer.insertBefore(tagEl, tagInput);

                // Create hidden input for form submission
                const hiddenInput = document.createElement('input');
                hiddenInput.type = 'hidden';
                hiddenInput.name = 'Tags';
                hiddenInput.value = tag;
                hiddenTagsContainer.appendChild(hiddenInput);
            });
        }
    });
})();