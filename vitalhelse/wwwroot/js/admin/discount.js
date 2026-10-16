// Discount code box in the checkout summary. The server decides what applies; this only talks to it.
(function () {
    const input = document.getElementById("discount-code");
    const applyBtn = document.getElementById("apply-discount");
    const removeBtn = document.getElementById("remove-discount");
    const msg = document.getElementById("discount-message");
    if (!input || !applyBtn) return;

    const token = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? "";

    async function post(url, body) {
        const res = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded", "RequestVerificationToken": token() },
            body
        });
        if (!res.ok) throw new Error("Request failed");
        return res.json();
    }

    function show(text, ok) {
        msg.textContent = text;
        msg.style.color = ok ? "green" : "red";
    }

    async function apply() {
        const code = input.value.trim();
        if (!code) return show("Skriv inn en kode.", false);

        try {
            const data = await post("/rabattkode/valider", `code=${encodeURIComponent(code)}`);
            show(data.message, data.success);
            if (data.success) input.value = "";
        } catch {
            show("Kunne ikke sjekke rabattkoden. Prøv igjen.", false);
        }
        await Summary();
    }

    applyBtn.addEventListener("click", apply);
    input.addEventListener("keydown", e => { if (e.key === "Enter") { e.preventDefault(); apply(); } });

    removeBtn?.addEventListener("click", async () => {
        try { await post("/rabattkode/fjern", ""); } catch { /* the summary below shows the real state */ }
        show("", true);
        await Summary();
    });
})();
