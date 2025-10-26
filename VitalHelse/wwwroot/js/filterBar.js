document.addEventListener('DOMContentLoaded', function () {
    // Parent toggle click (expand/collapse)
    //finds all elements with class parent-header and adds an event listener to them
    const parentHeaders = document.querySelectorAll('.parent-header');

    parentHeaders.forEach(header => {
        header.addEventListener('click', function (e) {
            if (e.target.tagName.toLowerCase() === 'input') return; // Don't toggle if clicking checkbox
            const parentDiv = header.parentElement;
            parentDiv.classList.toggle('active');
        });
    });

    // Sync parent & children checkboxes
    const parentCheckboxes = document.querySelectorAll('.parent-category');
    parentCheckboxes.forEach(parent => {
        parent.addEventListener('change', function () {
            const parentDiv = parent.closest('.category-parent');
            if (!parentDiv) return;
            const children = parentDiv.querySelectorAll('.child-category');
            children.forEach(child => child.checked = parent.checked);
        });
    });

    const childCheckboxes = document.querySelectorAll('.child-category');
    childCheckboxes.forEach(child => {
        child.addEventListener('change', function () {
            const parentDiv = child.closest('.category-parent');
            if (!parentDiv) return;
            const parent = parentDiv.querySelector('.parent-category');
            const children = parentDiv.querySelectorAll('.child-category');
            parent.checked = Array.from(children).some(c => c.checked);
        });
    });
});
