
    document.addEventListener('DOMContentLoaded', function () {
    //this just makes the allready made filterbar from the search result page work on the stock page aswell
    const productRows = document.querySelectorAll('.product-row');
    const statusFilters = document.querySelectorAll('.status-filter');
    const categoryFilters = document.querySelectorAll('.parent-category, .child-category');
    const showAllBtn = document.getElementById('showAllBtn');

    //add event listeners to status filters
    statusFilters.forEach(filter => {
    filter.addEventListener('change', function() {
    setTimeout(applyFilters, 0);
});
});

    //add event listeners to category filters
    categoryFilters.forEach(filter => {
    filter.addEventListener('change', function() {
    setTimeout(applyFilters, 0);
});
});

    //show All button
    showAllBtn.addEventListener('click', function() {
    statusFilters.forEach(filter => filter.checked = true);
    categoryFilters.forEach(filter => filter.checked = true);
    setTimeout(applyFilters, 0);
});

    //Main filter function
    function applyFilters() {
    const checkedStatuses = Array.from(statusFilters)
    .filter(checkbox => checkbox.checked)
    .map(checkbox => checkbox.value);

    const checkedCategories = Array.from(categoryFilters)
    .filter(checkbox => checkbox.checked)
    .map(checkbox => checkbox.value);

    productRows.forEach(row => {
    const rowStatus = row.getAttribute('data-status');
    const rowCategoriesAttr = row.getAttribute('data-categories');

    const rowCategoryArray = rowCategoriesAttr && rowCategoriesAttr.trim() !== ''
    ? rowCategoriesAttr.split(',').map(c => c.trim())
    : [];

    //Status matching
    const statusMatch = checkedStatuses.length === 0 ? false : checkedStatuses.includes(rowStatus);

    //Category matching
    let categoryMatch;

    if (checkedCategories.length === 0) {
    categoryMatch = false;
} else if (rowCategoryArray.length === 0) {
    //Products without categories show when filtering is active
    categoryMatch = true;
} else {
    categoryMatch = rowCategoryArray.some(category =>
    checkedCategories.includes(category)
    );
}

    //Show row only if both match
    if (statusMatch && categoryMatch) {
    row.classList.remove('hidden');
} else {
    row.classList.add('hidden');
}

});
}
});
