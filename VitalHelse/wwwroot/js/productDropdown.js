
(function() {
    'use strict';

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelectorAll('.dropdown-checkbox').forEach(dropdown => {
            const selected = dropdown.querySelector('.dropdown-selected');
            const options = dropdown.querySelector('.dropdown-options');

            if (!selected || !options) return; // Safety check

            selected.addEventListener('click', () => {
                options.style.display = options.style.display === 'block' ? 'none' : 'block';
            });

            document.addEventListener('click', (e) => {
                if (!dropdown.contains(e.target)) {
                    options.style.display = 'none';
                }
            });
        });
    });
})();