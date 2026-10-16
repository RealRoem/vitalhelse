
document.addEventListener("click", async (e) => {
    const btn = e.target.closest('#AddAllToCart');
    if (!btn) return;

    try {
        const res = await fetch("/Favorites/QuickAddFavorites/", {
            method: "POST"
        });

        if (!res.ok) {
            console.error("Feil ved QuickAddFavorites");
            return;
        }

        window.location.reload();

    } catch (err) {
        console.error("Nettverksfeil:", err);
    }
});



document.addEventListener("DOMContentLoaded", () => {
    // Sjekk om brukeren er logget inn – sett via Razor i HTML
    const isAuthenticated = window.isAuthenticated || false;


    // Finn produktsiden 
    const productpage = document.getElementById("product-page");
    if (!productpage) return;

    // Event delegation – én lytter for alle hjerter
    productpage.addEventListener('click', async (e) => {
        const btn = e.target.closest('.heart-btn');
        if (!btn) return;

        if (!isAuthenticated) {
            alert("Du må være logget inn for å legge til favoritter ❤️");
            return;
        }

        const id = btn.dataset.id;
        try {
            const res = await fetch('/Favorites/ToggleFavorite', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `productId=${encodeURIComponent(id)}`,
            });

            if (!res.ok) return;

            // Oppdater ikon basert på respons
            let isFav = null;
            try {
                const data = await res.json();
                isFav = !!data.isFavorite;
            } catch {
                // fallback hvis ingen JSON returneres
                isFav = !btn.classList.contains('active');
            }

            btn.classList.toggle('active', isFav);
            const icon = btn.querySelector('i');
            icon.classList.toggle('fa-regular', !isFav);
            icon.classList.toggle('fa-solid', isFav);

            // Hvis man er på /favoritter og fjerner favoritt, fjern kortet
            if (window.location.pathname.startsWith('/favoritter') && !isFav) {
                const card = btn.closest('.product-card');
                card.style.transition = 'opacity .3s ease, transform .25s ease';
                card.style.opacity = '0';
                card.style.transform = 'scale(0.96)';
                setTimeout(() => card.remove(), 300);
            }
            

            // Sett flag hvis vi endrer favoritt for å oppdatere grid når man går tilbake
            sessionStorage.setItem('favoritesChanged', 'true');
            

        } catch (err) {
            console.error("Feil ved toggling av favoritt:", err);
        }
    });
});
