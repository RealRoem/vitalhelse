
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

    function updatePriceView() {
        const incVat = document.querySelectorAll(".price-incvat");
        const exVat = document.querySelectorAll(".price-exvat");

        if (toggle.checked) {
            // Vis pris uten MVA
            exVat.forEach(x => x.style.display = "");
            incVat.forEach(x => x.style.display = "none");
        } else {
            // Vis pris med MVA
            exVat.forEach(x => x.style.display = "none");
            incVat.forEach(x => x.style.display = "");
        }
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


