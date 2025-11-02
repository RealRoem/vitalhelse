/* Product form dropdown checkbox functionality */

// Private Scope
// Strict
(function() {
    'use strict';

    // Loads DOM
    document.addEventListener('DOMContentLoaded', () => {
        
        // Find all dropdowns components
        document.querySelectorAll('.dropdown-checkbox').forEach(dropdown => {
            const selected = dropdown.querySelector('.dropdown-selected');
            const options = dropdown.querySelector('.dropdown-options');

            // Safety check
            // If not on right index
            if (!selected || !options) return;
            
            
            selected.addEventListener('click', () => {
                // When clicked, toggles the display of the options
                // If block is displayed, it will be hidden, if hidden it will be displayed
                options.style.display = options.style.display === 'block' ? 'none' : 'block';
            });

            // Close dropdown when clicking outside
            document.addEventListener('click', (e) => {
                if (!dropdown.contains(e.target)) {
                    options.style.display = 'none';
                }
            });
        });
    });
})();