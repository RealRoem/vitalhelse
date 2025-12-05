async function removItemShoppingCart(productId) {

    // Saves antiforgerytoken to protect against CSRF-attacks
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    // Fetch the delete function from the controller
    await fetch(`/ShoppingCart/Delete?id=${productId}`, {
        method: 'DELETE',
        headers: {
            'RequestVerificationToken': token
        }
    });

    // Removes the product from the page instead of reloading the entire page
    document.getElementById(`row-${productId}`).remove();

    const tbody = document.querySelector('#shopping-cart tbody');
    if (!tbody || tbody.rows.length === 0) {
        // If there is no items left, reload the entire page
        window.location.reload();
    }
    Summary();
}

async function AddToCart(productId){
    const quantityElement = document.getElementById(`quantity-${productId}`);
    const quantity = quantityElement ? parseInt(quantityElement.value, 10) || 1:1;
    
    // Saves antiforgerytoken to protect against CSRF-attacks
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    const formData = new FormData();
    formData.append("id", productId);
    formData.append("quantity", quantity);
    
    // Fetch the delete function from the controller
   //const response = await fetch(`/ShoppingCart/AddToCart/?id=${productId}&quantity=${quantity}`, {
    const response = await fetch("/ShoppingCart/AddToCart", {
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        },
        body: formData
    });

    // Prints the error message if bad request is returned, and keeps it there for 3 seconds
    if (!response.ok) {
        const msg = await response.text();
        document.getElementById("error-box").innerText = msg;
        setTimeout(() => document.getElementById("error-box").innerText = "", 3000);
    } else {
        if (!isAuthenticated) {
            const currentUrl = window.location.pathname + window.location.search;
            window.location.href =`/identity/account/login?returnUrl=${encodeURIComponent(currentUrl)}`;
            return;
        }
        /*
        const popup = document.getElementById("cart-popup");
        popup.style.display = "block";
        document.getElementById("go-to-cart").onclick = () => {
            window.location.href = "/checkout";
        };
        document.getElementById("continue-shopping").onclick = () => {
            popup.style.display = "none";
        }; */
        window.location.reload();
    }
}

// Helper that formats numers as norwegian currency (1000 as 1000,00 kr)
const nok = new Intl.NumberFormat('no-NO', { style: 'currency', currency: 'NOK' });

async function AddQuantity(productId, unitPrice) {
    const quantityElement = document.getElementById(`quantity-${productId}`);
    if(!quantityElement) return; // if element does not exist
    
    const totalElement = document.getElementById(`total-${productId}`);

    // Save old values in case of rollback, parse to int because we will treat it as a number
    const oldQuantity = parseInt(quantityElement.value, 10) || 0; // || 0 means if the field is not a number(empty or error) then use 0
    //const oldTotal = totalElement.innerText;
    const oldTotal = totalElement ? totalElement.innerText : '';

    // Add quantity
    const newQuantity = oldQuantity + 1;
    
    // Display the new value
    quantityElement.value = String(newQuantity);
    // If in shopping cart view, update price
    if(totalElement) {
        totalElement.innerText = nok.format(newQuantity * unitPrice);
    }
    
    // If on product page, stop here
    const productPage = document.getElementById(`product-page`);
    if(productPage) return;
    
    // If in shopping cart, send PATCH-request to server
    // Saves the Anti forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    // Fetch the response in case of error
    const response = await fetch(`/ShoppingCart/AddQuantity/?id=${productId}`, {
        method: 'PATCH',
        headers: { 'RequestVerificationToken': token },
    });

    // If the response is an error
    if (!response.ok) {
        // Rollback to the cart values before the error occured
        quantityElement.value = String(oldQuantity);
        if(totalElement) totalElement.innerText = oldTotal;

        // Prints error message
        const msg = await response.text();
        document.getElementById("error-box").innerText = msg;
        setTimeout(() => document.getElementById("error-box").innerText = "", 3000);
    }
    else {
        Summary();
    }
}


async function DecreaseQuantity(productId, unitPrice) {

    const quantityElement = document.getElementById(`quantity-${productId}`);
    if(!quantityElement) return;
    
    const totalElement = document.getElementById(`total-${productId}`);

    // Save old values in case of rollback, parse to int because we will treat it as a number
    const oldQuantity = parseInt(quantityElement.value, 10) || 0;
    const oldTotal = totalElement ? totalElement.innerText : '';
    
    // Decrease quantity
    let newQuantity = oldQuantity - 1;
    
    // If quatity is 0 or less set to 1
    if (newQuantity <= 0) {
        newQuantity = 1;
    }
   
    // Display new value
    quantityElement.value = String(newQuantity);
    
    // If in shopping cart view, update price
    if (totalElement) {
        totalElement.innerText = nok.format(newQuantity * unitPrice);
    }
    

    // If on product page, stop here
    const productPage = document.getElementById(`product-page`);
    if(productPage) return;

    // If in shopping cart, send PATCH-request to server
    // Saves antiforgerytoken to protect against CSRF-attacks
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    // Saves the response from the decreaseQuantity function, so that we may print out error message if they try to decrease more than allowed
    const response = await fetch(`/ShoppingCart/DecreaseQuantity/?id=${productId}`, {
        method: 'PATCH',
        headers: { 'RequestVerificationToken': token },
    });

    // Prints the error message if bad request is returned, and keeps it there for 3 seconds
    if (!response.ok) {
        if (newQuantity <= 0) {
            window.location.reload();
        }
        else {
            // Rollback to the cart values before the error occured
            quantityElement.value = String(oldQuantity);
            if(totalElement) totalElement.innerText = oldTotal;
        }
        
        // Prints error message
        const msg = await response.text();
        document.getElementById("error-box").innerText = msg;
        setTimeout(() => document.getElementById("error-box").innerText = "", 3000);
    }
    else {
        Summary();
    }
}


async function UpdateQuantity(productId, unitPrice) {
    
    // Get the inputfield for quantity and the total price element
    const quantityElement = document.getElementById(`quantity-${productId}`);
    const totalElement = document.getElementById(`total-${productId}`);
    
    // Parse inputvalue to int
    let newQuantity = parseInt(quantityElement.value, 10);

    // If the user inputs something other that a number
    if (isNaN(newQuantity) || newQuantity <= 0) {
        // Set the value to 1 and update summary
        newQuantity = 1;
        quantityElement.value = 1;
        Summary();
    }

    // Update the total price
    totalElement.innerText = nok.format(newQuantity * unitPrice);
    
    // Saves antiforgerytoken to protect against CSRF-attacks
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    
    // Send the new quantity in to the ShoppingCart controller (SetQuantity action)
    const response = await fetch(`/ShoppingCart/SetQuantity/?id=${productId}&quantity=${newQuantity}`, {
        method: 'PATCH',
        headers: { 'RequestVerificationToken': token },
    });

    // If an error occours
    if (!response.ok) {
        const msg = await response.text();
        document.getElementById("error-box").innerText = msg;
        setTimeout(() => document.getElementById("error-box").innerText = "", 3000);
    } 
    else {
        // If the server adjusted the quantity, update it in the input box
        const data = await response.json();
        quantityElement.value = data.correctedQuantity;
        totalElement.innerText = nok.format(data.correctedQuantity * unitPrice);
        Summary();
    }
}
function UpdateQuantityLocal(productId) {
    const quantityElement = document.getElementById(`quantity-${productId}`);

    let newQuantity = parseInt(quantityElement.value, 10);

    if (isNaN(newQuantity) || newQuantity < 1) {
        newQuantity = 1;
        quantityElement.value = 1;
    }
} 

async function Summary() {
    const sumProducts = document.getElementById('sum-products');
    if (!sumProducts) return; // return if user is on productpage 
    
    try {
        // Fetch the data by sending a request to the Summary action in the controller that returns JSON-data
        const response = await fetch('/Checkout/Summary', {
            credentials: 'same-origin',
            cache: 'no-store'
        });

        // If response was not 200 then throw error and go to catch
        if (!response.ok) throw new Error();

        // Save the JSON-data
        const s = await response.json();

        // Update the HTML
        document.getElementById('sum-products').textContent = nok.format(s.sumProducts);
        document.getElementById('sum-before').textContent = nok.format(s.sumBefore);
        // Show a "-" when it is a discount
        document.getElementById('sum-discount').textContent = (s.discount > 0 ? '-' : '') + nok.format(Math.abs(s.discount));
        document.getElementById('sum-shipping').textContent = nok.format(s.shipping);
        document.getElementById('sum-total').textContent = nok.format(s.total);
        // Vis rabattkode-prosent i sammendraget
        if (document.getElementById("sum-code-discount")) {
            document.getElementById("sum-code-discount").textContent =
                s.codeDiscountPercent > 0 ? `-${s.codeDiscountPercent}%` : "0%";
        }

    } catch {// If we get an error show 0,00 kr
        ['sum-products','sum-before','sum-discount','sum-shipping','sum-total']
            .forEach(id => document.getElementById(id).textContent = '0,00 kr');
    }
}
