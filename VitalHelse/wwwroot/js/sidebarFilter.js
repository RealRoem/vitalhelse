/* Side-Bar Filter for filtering products on search index */

// `document` = The entire HTML page
// `.addEventListener()` = Method that "listens" for events
// ``DOMContentLoaded`` = Event that fires when HTML is fully loaded, but but before images/stylesheets finishes
// `function() {}` = Anonymous function that runs when the event fires
// purpose: Wait for the page to fully load before running the code
document.addEventListener('DOMContentLoaded', function () {
    // `const` = Declares a constant variable (cant be reassigned)
    // `document.querySelectorAll()` = Selects all elements that match the given selector
    // `.parent.header` = Selects all elements with the class "parent-header"
    // Returns a NodeList of all matching elements
    const parentHeaders = document.querySelectorAll('.parent-header');

    // `forEach()` = Loops through each element in the NodeList
    // `header` = The current element in the loop
    // `=>` Arrow function syntax (modern JS shorthand for `function(header) {}`
    // Purpose: For each `.parent-header` element found, run this code
    parentHeaders.forEach(header => {
        // `header` = The current element in the loop
        // `.addEventListener(`click`)` = Listen for click events on this specific header
        // `function(e) {}` = Anonymous function that runs when the event fires (clicks)
        // `e` = Event object containing information about the click event (where clicked, what was clicked, etc)
        header.addEventListener('click', function (e) {
            // `e.target` = The exact element that was clicked (could be the header, checkbox, text, etc)
            // `.tagName.toLowerCase()` = Get the tag name of the element and converts it to lowercase
            // === `input` = Strict equality check (checks if the tag name is "input")
            // `return` = Exit the function early (no further code is executed)
            // Purpose = If you click directly on a checkbox, dont toggle the expand/collapse. Only toggle when clicking the text or other parts of the header
            if (e.target.tagName.toLowerCase() === 'input') return; 
            // `header` = The `.parent-header` element that was clicked
            // `.parentElement` = Get the direct parent element of the header (the `.category-parent` element)
            // `parentDiv` = Variable that stores the parent element of the header
            const parentDiv = header.parentElement;
            // `classList` = Object containing all CSS classes on the element
            // `.toggle('active')` = Toggle the "active" class on the element/Remove the class if it exists
            // `active` = CSS class that is added to the element when it is expanded/collapsed`
            parentDiv.classList.toggle('active');
        });
    });

    // Sync parent & children checkboxes
    // Returns a NodeList of all matching elements `parent-category``
    const parentCheckboxes = document.querySelectorAll('.parent-category');
    // Loops through each element in the NodeList
    parentCheckboxes.forEach(parent => {
        // Listens for changes on the parent checkbox
        // `change` event fires when the checkbox is toggled (checked/unchecked)
        parent.addEventListener('change', function () {
            // `parentDiv` = Variable that stores the parent element of the header
            // `.closest('.category-parent')` = Travels up the DOM tree to find the closest `.category-parent` element matching the selector
            const parentDiv = parent.closest('.category-parent');
            // If no `.category-parent` element is found, exit the function early
            if (!parentDiv) return;
            // `parentDiv.querySelectorAll('.child-category')` = Selects all elements with the class "child-category" inside the `.category-parent` element
            const children = parentDiv.querySelectorAll('.child-category');
            // Loops through each element in the NodeList children
            // `childe =>` arrow function syntax (modern JS shorthand for `function(child) {}`)
            // `child.checked = parent.checked` = Sets the `checked` property of each child checkbox to the same value as the parent checkbox
            // So this checks/unchecks all children categories if their matching parent is checked/unchecked
            children.forEach(child => child.checked = parent.checked);
        });
    });

    // Gets all child checkboxes in the entire document
    const childCheckboxes = document.querySelectorAll('.child-category');
    // Loops through each element in the NodeList
    childCheckboxes.forEach(child => {
        child.addEventListener('change', function () {
            // Travel up to find the `.category-parent` element
            const parentDiv = child.closest('.category-parent');
            // If no `.category-parent` element is found, exit the function early
            if (!parentDiv) return;
            //`querySelector` finds the FIRST element matching the selector and returns it
            // Purpose = Find the parent checkbox within this category group
            const parent = parentDiv.querySelector('.parent-category');
            // Stores all child checkboxes inside the `.category-parent` element in a NodeList
            const children = parentDiv.querySelectorAll('.child-category');
            // `Array.from(children) converts the NodeList to a real array`
            // This makes it so we can use methods like `.some`
            // `.some` is an array method that returns true if any element passes the test
            // `(c => c.checked)` = returns true if at least ine child checkbox is checked and returns false if all child checkboxes are unchecked
            //Basically checks the parent checkbox if any of its children are checked
            parent.checked = Array.from(children).some(c => c.checked);
        });
    });
});
