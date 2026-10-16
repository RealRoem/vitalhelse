
window.addEventListener("scroll", function() {
    // Hente element etter id
    const navbar = document.getElementById("navbar");
    // Sjekke om brukeren har skrollet mer en 30 piksler 
    if (window.scrollY > 30) {
        //Hvis ja legg til en klasse
        navbar.classList.add("shrink");
    } 
    else {
        // Hvis ikke fjern klassen
        navbar.classList.remove("shrink");
    }
});

document.addEventListener("DOMContentLoaded", () => {
    const toggle = document.getElementById("myToggle");
    if (!toggle) return; // only business customers and staff have the MVA toggle

    // The price spans for both views are in the page; CSS on <html data-vat> shows the right ones,
    // which also covers content added later (recommendations, cart rows).
    function updatePriceView() {
        document.documentElement.dataset.vat = toggle.checked ? "ex" : "inc";
    }

    // 🔹 Last toggle-verdi fra localStorage
    toggle.checked = localStorage.getItem("showExVat") === "1";

    // 🔹 Kjør en gang når siden lastes
    updatePriceView();

    // 🔹 Når brukeren endrer toggle
    toggle.addEventListener("change", () => {
        localStorage.setItem("showExVat", toggle.checked ? "1" : "0");
        updatePriceView();
    });
});

document.addEventListener("DOMContentLoaded", () => {
    const toggle = document.getElementById("myToggle");
    if (!toggle) return;
    const inc = document.querySelector(".vat-label-inc");
    const ex = document.querySelector(".vat-label-ex");

    function updateLabels() {
        if (toggle.checked) {
            ex.classList.add("active");
            inc.classList.remove("active");
        } else {
            inc.classList.add("active");
            ex.classList.remove("active");
        }
    }

    toggle.addEventListener("change", updateLabels);
    updateLabels();
});


