// Product Filter JavaScript
// Handles auto-submit on checkbox change and enhanced user experience

$(document).ready(function () {
    const filterForm = $('#filterForm');
    const categoryCheckboxes = $('input[name="categories"]');
    
    // Clear the timeout if user keeps clicking
    categoryCheckboxes.on('mousedown', function () {
        clearTimeout(window.filterTimeout);
    });

    // Highlight active filters
    function highlightActiveFilters() {
        const hasActiveCategories = categoryCheckboxes.filter(':checked').length > 0;
        const hasActivePrice = $('input[name="minPrice"]').val() || $('input[name="maxPrice"]').val();
        const hasActiveVolume = $('input[name="minVolume"]').val() || $('input[name="maxVolume"]').val();

        // Add visual indicator to sections with active filters
        if (hasActiveCategories) {
            $('.filter-section').first().addClass('filter-active');
        }
        if (hasActivePrice) {
            $('.filter-section').eq(1).addClass('filter-active');
        }
        if (hasActiveVolume) {
            $('.filter-section').eq(2).addClass('filter-active');
        }
    }

    // Run on page load
    highlightActiveFilters();

    // Handle number input validation
    $('input[type="number"]').on('input', function () {
        const val = parseFloat($(this).val());
        const min = parseFloat($(this).attr('min'));

        if (val < min) {
            $(this).val(min);
        }
    });

    // Prevent form submission on Enter key in number inputs
    // This allows users to enter values without accidentally submitting
    $('input[type="number"]').on('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            filterForm.submit();
        }
    });

    // Add loading state when form is submitted
    filterForm.on('submit', function () {
        $(this).addClass('filter-loading');
        $('.filter-actions .btn-primary').prop('disabled', true).text('Filtrerer...');
    });

    // Show active filter count in title (optional enhancement)
    function updateFilterCount() {
        const activeCount = categoryCheckboxes.filter(':checked').length;

        if (activeCount > 0) {
            if ($('.filter-badge').length === 0) {
                $('.filter-title').append('<span class="filter-badge">' + activeCount + '</span>');
            } else {
                $('.filter-badge').text(activeCount);
            }
        } else {
            $('.filter-badge').remove();
        }
    }

    categoryCheckboxes.on('change', updateFilterCount);
    updateFilterCount();

    // Collapse/expand filter sections on mobile (optional)
    if (window.innerWidth < 768) {
        $('.filter-section-title').on('click', function () {
            $(this).next('.filter-options, .row').slideToggle(200);
            $(this).toggleClass('collapsed');
        });
    }

    // Add smooth scroll to results after filter
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.has('categories') || urlParams.has('minPrice') || urlParams.has('maxPrice')) {
        // User has applied filters, scroll to results on mobile
        if (window.innerWidth < 768) {
            setTimeout(function () {
                $('html, body').animate({
                    scrollTop: $('.col-md-9').offset().top - 20
                }, 300);
            }, 100);
        }
    }
});