/* Product form tag input functionality */

// `(function() {`
// The outer parentheses () turn it into an expression
// The final () at the end immideatly executes in
// Purpose: Creates a private scope to avoid polluting the global namespace
// Bsically, the declaration of variables is private and only this file can acces them
(function() {
    // Enables JS strict mode
    'use strict';

    // Declares variables
    // [] empty array literal
    
    // Store currently selected tags for this product
    let tags = [];
    
    // Stores all available tags in the system (for suggestions)
    let allTags = [];
    
    // Start suggestions as null (dont show before user start typing)
    let suggestionBox = null;

    // Finds HTML elements by ID
    document.addEventListener('DOMContentLoaded', () => {
        const tagInput = document.getElementById('tag-input');
        const tagContainer = document.getElementById('tag-input-container');
        const hiddenTagsContainer = document.getElementById('hidden-tags-container');

        // Exit early if elements don't exist (not on product add/edit page)
        if (!tagInput || !tagContainer || !hiddenTagsContainer) {
            return;
        }

        // `window` = Global object in browser
        // Any object declared globally lives on window
        // See relevant cshtml file to see how values are passed
        
        // If we edit product loads products existing tags
        if (window.productTags) {
            tags = window.productTags;
        }
        if (window.allProductTags) {
            allTags = window.allProductTags;
        }

        // Render existing tags on load if editing product and tags exist
        renderTags();

        // Handle Enter key to add tags
        // `keydown` = Event listener for if ANY keu is pressed
        tagInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                // `preventDefault` = Prevents default action of the event
                // so pressing enter doesn't submit the form
                e.preventDefault();
                const value = tagInput.value.trim();
                if (value !== '') {
                    // Adds tag to the list of tags if not empty
                    addTag(value);
                    // Then clears the input field and hides suggestions
                    tagInput.value = '';
                    hideSuggestions();
                }
            }
        });

        // Handle input for suggestions
        // `input` = Event listener for when the user is typing
        tagInput.addEventListener('input', function () {
            // Convert input to lowercase to make case insensitive
            const inputValue = tagInput.value.toLowerCase();
            // `.filter` = Array method that creates a new array with items passing a test
            // Original array is not modified
            // `tag` = Current tag being tested
            // `tag.toLowerCase().includes(inputValue)` = Checks if tag includes input value
            // `!tags.includes(tag)` = Checks if tag is already in the list of tags
            const filtered = allTags.filter(tag => 
                tag.toLowerCase().includes(inputValue) && !tags.includes(tag)
            );
            // If there are filtered tags and input value is not empty, show suggestions
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
            // Check if tag is already in the list of tags
            if (!tags.includes(tagText)) {
                tags.push(tagText);
                // Update display of tags
                renderTags();
            }
        }

        function showSuggestions(filtered) {
            // Alwasy hide previous suggestions
            hideSuggestions();
            // Create new suggestion box
            suggestionBox = document.createElement('div');
            suggestionBox.classList.add('suggestion-box');
            
            // Loop through filtered tags and create suggestion items
            filtered.forEach(tag => {
                const option = document.createElement('div');
                // `textContent` = Sets the text content of an element
                // In this case it uses the tahds we loop throigh
                option.textContent = tag
                // `classList` = Adds or removes classes from an element
                option.classList.add('suggestion-item');
                // `addTag` adds the tag to the list of tags
                option.addEventListener('click', () => {
                    addTag(tag);
                    // Clear input field and hide suggestions
                    tagInput.value = '';
                    hideSuggestions();
                });
                
                // Add suggestion item to suggestion box
                suggestionBox.appendChild(option);
            });
            
            // Append suggestion box to tag container
            // So adds all the suggestions added to the suggestionst box to the tag container
            tagContainer.appendChild(suggestionBox);
        }

        // Function to hide suggestions
        // `suggestionBox` = Variable that holds the suggestion box element
        function hideSuggestions() {
            // If suggestion box exists, remove it from the DOM
            if (suggestionBox) {
                suggestionBox.remove();
            }
            
            // Set it to null for memory management, state tracking, etc.
            suggestionBox = null;
        }

       // Render tags to the page
        function renderTags() {
            
            // Remove existing tag elements
            tagContainer.querySelectorAll('.tag').forEach(t => t.remove());
            hiddenTagsContainer.innerHTML = '';
            
            // Loop through tags array and create tag elements
            tags.forEach(tag => {
                // Create visible tag element
                const tagEl = document.createElement('div');
                tagEl.classList.add('tag');
                
                // `${tag}` = Expression interpolation (inserts variable value)
                // `&times;` = HTML entity for multiplication symbol, isnt used for any maths, its a visual symbol for removing a tag
                tagEl.innerHTML = `${tag} <span class="remove-tag">&times;</span>`;
                
                // Add click event listener to remove tag
                tagEl.querySelector('.remove-tag').addEventListener('click', () => {
                    // Remove tag from the array
                    // Loops throught array and removes the tag if it matches
                    // `.filter` creates a new array with items that pass the test
                    // Replaces original array with the new one
                    tags = tags.filter(t => t !== tag);
                    // Update display of tags
                    renderTags();
                });
                
                // Add tag element to tag container, and insert it before the input field
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