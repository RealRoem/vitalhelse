// Run the function when HTML is loaded
document.addEventListener("DOMContentLoaded", function () {

    // Get and save all the necessary elemnts 
    const hasAddresses = document.getElementById("address-page").dataset.hasAddresses === "true";
    const link = document.getElementById("show-form-link");
    
    const formWrap = document.getElementById("add-address-form-wrapper");
    const listWrap = document.getElementById("address-list-wrapper");
    const noAddressBox = document.getElementById("no-address-box");
    const editBtn = document.getElementById("edit-btn");
    const deleteBtn = document.getElementById("delete-address-btn");
    const cancelBtn = document.getElementById("cancel-add-address");
    const form = document.getElementById("add-address-form");
    const addAction = form ? form.getAttribute("action") : null;
    
    // Function to only show the add address form (hides everything else)
    function showFormOnly() {
        if (listWrap) listWrap.style.display = "none";
        if (noAddressBox) noAddressBox.style.display = "none";
        if (formWrap) formWrap.style.display = "block";
    }

    // Function that changes view with the list if there are addresses or only to text for adding address
    function showListOrEmpty() {
        // Hide add address form
        formWrap.style.display = "none";
        if (hasAddresses) {
            if (listWrap) listWrap.style.display = "block";
        } 
        else {
            if (noAddressBox) noAddressBox.style.display = "block";
        }
    }
    
    // If the achor to add address is found
    if (link) {
        // When clicked
        link.addEventListener("click", function (e) {
            // Prevent the link from navigating
            e.preventDefault();

            const form = document.getElementById("add-address-form");
            if (form) {
                form.reset();
                if (addAction) form.setAttribute("action", addAction);
            }
            
            if (deleteBtn) {
                deleteBtn.style.display = "none";
                deleteBtn.onclick = null;
            }
            
            showFormOnly();
        });
    }
    
    // If the cancel button on add form is found
    if (cancelBtn) {
        // When clicked
        cancelBtn.addEventListener("click", function () {
            // Save the form element
            const form = document.getElementById("add-address-form");
            // If the element is found
            if (form) {
                // Reset the form
                form.reset();
                if (addAction) form.setAttribute("action", addAction);
            }

            if (deleteBtn) {
                deleteBtn.style.display = "none";
                deleteBtn.onclick = null;
            }
            
            showListOrEmpty();
        });
    }
    
    // Make the edit function global so that the user can call the function
    window.editAddress = async function (addressId) {
        // Fetch the response
        const response = await fetch(`/Checkout/GetAddress?id=${addressId}`);

        if (!response.ok) {
            // Legge til skikkelig error senere
            console.error("Kunne ikke hente adresse");
            return;
        }

        const addr = await response.json();

        if (!form) return;
        
        // Reset the form
        form.reset();

        // Fill out the input fields
        form.querySelector("[name='NewAddress.FirstName']").value = addr.firstName ?? "";
        form.querySelector("[name='NewAddress.LastName']").value = addr.lastName ?? "";
        form.querySelector("[name='NewAddress.PhoneNumber']").value = addr.phoneNumber ?? "";
        form.querySelector("[name='NewAddress.Street']").value = addr.street ?? "";
        form.querySelector("[name='NewAddress.PostalCode']").value = addr.postalCode ?? "";
        form.querySelector("[name='NewAddress.City']").value = addr.city ?? "";

        // Change the action in the form
        form.setAttribute("action", `/Checkout/EditAddress/${addr.id}`);

        // Show delete button
        if (deleteBtn) {
            deleteBtn.style.display = "block";

            deleteBtn.onclick = async function () {
                const ok = confirm("Er du sikker på at du vil slette denne adressen?");
                if (!ok) return;

                // Saves antiforgerytoken to protect against CSRF-attacks
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                
                const deleteResponse = await fetch(`/Checkout/DeleteAddress?id=${addr.id}`, {
                    method: "DELETE",
                    headers: {
                        'RequestVerificationToken': token
                    }
                });

                if (!deleteResponse.ok) {
                    console.error("Kunne ikke slette adressen");
                    return;
                }
                // Reload after deletion
                window.location.reload();
            };
        }
        showFormOnly();
    };
});