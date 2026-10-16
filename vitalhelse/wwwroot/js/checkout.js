initialize();

// Create a Checkout Session
async function initialize() {
    const config = await (await fetch("/create-checkout-session/config")).json();
    const stripe = Stripe(config.publishableKey);

    const fetchClientSecret = async () => {
        const response = await fetch("/create-checkout-session", {
            method: "POST",
            credentials: "include"
        });
        const { clientSecret } = await response.json();
        return clientSecret;
    };

    const checkout = await stripe.initEmbeddedCheckout({
        fetchClientSecret,
    });

    // Mount Checkout
    checkout.mount('#checkout');
}
