// GJØR DEN GLOBAL:
window.discountPercent = 0;

// 1. Håndter rabattkode bekreftelse
document.getElementById("apply-discount").addEventListener("click", async () => {
    const code = document.getElementById("discount-code").value.trim();
    const msg = document.getElementById("discount-message");

    if (!code) {
        msg.textContent = "Skriv inn en kode.";
        msg.style.color = "red";
        return;
    }

    msg.style.color = "black";

    const res = await fetch("/DiscountCode/ValidateDiscountCode", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: `code=${encodeURIComponent(code)}`
    });

    const data = await res.json();

    if (!data.success) {
        msg.textContent = data.message;
        msg.style.color = "red";
        window.discountPercent = 0;
        Summary();
        return;
    }

    window.discountPercent = data.percent;
    msg.textContent = data.message;
    msg.style.color = "green";

    Summary();
});
