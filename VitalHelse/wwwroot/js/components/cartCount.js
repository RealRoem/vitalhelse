
    document.addEventListener("DOMContentLoaded", function () {
    fetch('/Checkout/CartCount')
        .then(response => response.json())
        .then(count => {
            const badge = document.getElementById("cart-count");

            if (count > 0) {
                badge.innerText = count;
                badge.style.display = "block";
            } else {
                badge.style.display = "none";
            }
        });
});
