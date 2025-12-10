const ShippingModule = {

    base: 0,
    radios: null,

    init() {
        const baseElement = document.getElementById("shipping-data");
        this.base = parseFloat(baseElement.dataset.basePrice);

        this.radios = document.querySelectorAll(".shipping-radio");

        this.radios.forEach(r => {
            r.addEventListener("change", async () => {

                const rate = parseFloat(r.dataset.rate);
                //const finalShipping = this.base * rate;
                const methodId = r.value;

                // Set shipping method
                await fetch(`/Checkout/SetShippingMethod?methodId=${methodId}`, {
                    method: "POST"
                });
                
                // Update summary
                await Summary();
            });
        });
    }
};


// When page is loaded
document.addEventListener("DOMContentLoaded", () => {
    ShippingModule.init();
});
