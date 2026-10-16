// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
//Waits for the page to load before executing the code
//Uses JQuery which is allready included in the project
//@ select the whole html document
//.ready waits for the document to be fully loaded
//function is a anonymous function
$(document).ready(function ()
{
    //Declares variable to store timer (for debounce)
    //Will hols a timer that will be used to debounce the search input
    let searchTimeout;
    //Get the search input and suggestions container
    //# means find by ID
    //finds the html element with the id searchInput//searchSuggestions
    //returns a jQuery object and stores it in the variable searchInput
    const searchInput = $('#searchInput');
    //Get the suggestions container and specifies the class
    const suggestionsContainer = $('#searchSuggestions');

    //on input event, fires every time the user types in the search input
    //.on is jQuery method that listens for an event and input is the event we are listening for
    //funcion is the code we run when the event is triggered
    searchInput.on('input', function ()
    {
        //Get the current value of the search input and removes spaces
        //$this refers to the search input (the element triggering the event)
        //.val() returns the value of the input
        //.trim() removes spaces from the beginning and end of the string " hello " --> "hello"
        //stores the cleaned value in the variable term
        const term = $(this).val().trim();

        //debouncing, cancels the previous timer
        clearTimeout(searchTimeout);

        //If the term is less than 2 characters, hide the suggestions and return
        //also empties out previous suggestions if user deletes input
        //reduce unnecessary requests to server
        if (term.length < 1)
        {
            suggestionsContainer.removeClass('show').empty();
            return;
        }

        //settimeout is a function that runs after a certain amount of time
        //function is the code we run after the timeout
        //this returns a timer id and stores it in the variable searchTimeout
        //we store the timer id in a variable so we can cancel it later when the user keeps typing
        searchTimeout = setTimeout(function ()
        {
            //ajax request to server to get suggestions
            //AJAX = Asynchronous JavaScript and XML
            //allows JS to send and receive data from a server without having to refresh the page
            $.ajax(
                {
                    //url is the address of the server we want to send the request to
                    //specifically the SearchSuggestions action in the SearchController
                    url: '/Search/SearchSuggestions',
                    //the type of request we are making
                    //HTTP GET request
                    type: 'GET',
                    //parameters to send to the server
                    //e.g = /Search/SearchSuggestions?term=hello
                    //revamp this to use SEO friendly url
                    //{ term: term } is a JS object that contains the term parameter
                    //e.g { term: 'hello' } becomes /Search/SearchSuggestions?term=hello
                    data: { term: term },
                    // success is the function that runs when the request is successful
                    //anonymous function that takes the data from the server as an argument
                    success: function (data)
                    {
                        //class our custom fucntion with the data from the server
                        //data is the json array returned from our controller
                        //see searchcontroller.cs for more info
                        displaySuggestions(data);
                    },
                    //error is the function that runs when the request fails
                    error: function ()
                    {
                        //log the error to the console
                        console.error('Error fetching suggestions');
                    }
                });
            // closes the suggestions after 300ms, makes it so the user can see the results
            // and not update the suggestions while they are typing
        }, 300);
    });

    //function to display suggestions
    //takes in the data from the server and displays it in the suggestions container
    //see function above for more info in regards to the data
    function displaySuggestions(suggestions)
    {
        //empty the suggestions container and hide it
        //clears old sugestions before displaying new ones
        suggestionsContainer.empty();

        //if there are no suggestions, display a message and return
        //checks if he array is empty
        if (suggestions.length === 0)
        {
            suggestionsContainer.html('<div class="no-suggestions">No products found</div>');
            //displays the message// make the dropdown visible
            suggestionsContainer.addClass('show');
            return;
        }

        //loop through the suggestions and create a div for each one
        //suggestions is the array of products returned from the server
        //function is anonymous function that takes in the item as an argument
        //item is the current item in the array
        suggestions.forEach(function (item)
        {
            //create a div for each suggestion
            //$ is a Jquery function that creates a new element
            const suggestionItem = $('<div class="search-suggestion-item"></div>');

            //set the inner html of the suggestion item to the name, description, and price of the product
            suggestionItem.html
            (`
                <div class="suggestion-name">${item.name}</div>
                <!-- if item description is empty set it to null -->
                <div class="suggestion-description">${item.description || ''}</div>
                <div class="suggestion-price">Kr ${item.price}</div>
            `);

            //on click event, set the value of the search input to the name of the product
            //Change to ID if product name is not unique
            suggestionItem.on('click', function () {
                searchInput.val(item.name);
                //remove the suggestions container and hide it
                suggestionsContainer.removeClass('show').empty();
                // You can redirect to product detail page or submit search
                //this will redirect to the search page with the query parameter set to the product name
                //encoding the product name will do the following example:
                //url = "/Search/Search?query=Vitamin C 1000mg" -> url = "/Search/Search?query=Vitamin%20C%201000mg"
                window.location.href = '/Search/Search?query=' + encodeURIComponent(item.name);
            });

            //this adds the suggestion item to the suggestions container as a child element
            //
            suggestionsContainer.append(suggestionItem);

            //close the loop
        });

        // makes the drop down visible
        suggestionsContainer.addClass('show');
    }

    // Hide suggestions when clicking outside
    //$document is a Jquery object that represents the entire document
    //.on is a Jquery method that listens for an event
    //click is the event we are listening for
    //funcion is the code we run when the event is triggered
    //.closest is a Jquery method that returns the closest parent that matches the selector
    //#searchInput is the selector for the search input
    // basically it checks if the click event happened inside the search input
    //if if it doesnt close the suggestions

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#searchInput, #searchSuggestions').length) {
            suggestionsContainer.removeClass('show');
        }
        //shows the suggestions agian if the user clicks inside the search input
        if ($(e.target).closest('#searchInput, #searchSuggestions').length) {
            suggestionsContainer.addClass('show');
        }
    });

    // Handle keyboard navigation
    // Add keyboard navigation to suggestions

    searchInput.on('keydown', function (e)
    {
        //select all items
        const items = $('.search-suggestion-item');
        const active = $('.search-suggestion-item.active');
        
        //e-key is the key that was pressed
        if (e.key === 'ArrowDown')
        {
            
            //if the active item is the last item, select the first item
            e.preventDefault();
            if (active.length === 0)
            {
                items.first().addClass('active');
            }
            if (items.last().hasClass('active') && e.key == 'ArrowDown')
            {
                active.removeClass('active') && items.first().addClass('active');
            }
            else
            {
                //remove the active class from the current active item
                //add the active class to the next item
                active.removeClass('active').next().addClass('active');
            }
        }
        else if (e.key === 'ArrowUp')
        {
            //if the active item is the first item, select the last item
            e.preventDefault();
            if (active.length > 0)
            {
                active.removeClass('active').prev().addClass('active');
            }
            else
            {
                items.last().addClass('active');
            }
        }
        else if (e.key === 'Enter' && active.length > 0)
        {
            //prevents the default action of the event
            //so we can add custom logic to the event
            //the default action we stop is the cursor movement
            e.preventDefault();
            //enter clicks the active item and navigates to the search result page
            active.click();
        }
    });
});