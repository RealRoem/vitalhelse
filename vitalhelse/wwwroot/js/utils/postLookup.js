let postalCodeData = [];

// Last JSON-fil
fetch("/js/data/postdata.json")
    .then(res => res.json())
    .then(data => {
        postalCodeData = data;
    })
    .catch(err => console.log(err));

function updateArea() {
    const postalCodeInput = document.getElementById('postalcode');
    const areaInput = document.getElementById('city');
    const postalCode = postalCodeInput.value.trim();

    // Finn objektet med riktig postnummer
    const match = postalCodeData.find(item => item.postnummer === postalCode);

    if (match) {
        areaInput.value = match.poststed;
    } else {
        areaInput.value = "";
    }
}
    
document.addEventListener('DOMContentLoaded', () => {
    const postalCodeInput = document.getElementById('postalcode');
    postalCodeInput.addEventListener('input', updateArea);
}); 
