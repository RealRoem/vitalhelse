// Shows the number of items in the cart next to the cart icon
function refreshCartCount() {
    const badge = document.getElementById("cart-count");
    if (!badge || window.isAuthenticated === false) return;

    fetch('/Checkout/CartCount', { credentials: 'same-origin' })
        .then(response => (response.ok ? response.json() : 0))
        .then(count => {
            badge.innerText = count;
            badge.style.display = count > 0 ? "block" : "none";
        })
        .catch(() => { badge.style.display = "none"; });
}

document.addEventListener("DOMContentLoaded", refreshCartCount);
