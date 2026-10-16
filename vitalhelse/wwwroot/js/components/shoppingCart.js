// Shopping cart behaviour shared by the product grid, product page and the checkout steps.

const MAX_QUANTITY = 99;

// Norwegian money format that matches the server (1 234,50 kr)
function money(value) {
    return Number(value).toLocaleString('nb-NO', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' kr';
}

function antiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

function showCartMessage(text, isError) {
    let box = document.getElementById('cart-toast');
    if (!box) {
        box = document.createElement('div');
        box.id = 'cart-toast';
        box.setAttribute('role', 'status');
        box.style.cssText = 'position:fixed;right:20px;bottom:20px;z-index:2000;padding:12px 18px;border-radius:8px;' +
            'color:#fff;font-size:15px;box-shadow:0 4px 14px rgba(0,0,0,.25);transition:opacity .3s;max-width:320px;';
        document.body.appendChild(box);
    }
    box.textContent = text;
    box.style.background = isError ? '#b3261e' : '#3b6b4a';
    box.style.opacity = '1';
    clearTimeout(box._timer);
    box._timer = setTimeout(() => { box.style.opacity = '0'; }, 3000);

    // Legacy inline error box on the product page
    const legacy = document.getElementById('error-box');
    if (legacy) legacy.innerText = isError ? text : '';
}

function clampQuantity(value) {
    const n = parseInt(value, 10);
    if (isNaN(n) || n < 1) return 1;
    return Math.min(n, MAX_QUANTITY);
}

async function removItemShoppingCart(productId) {
    const response = await fetch(`/ShoppingCart/Delete?id=${productId}`, {
        method: 'DELETE',
        headers: { 'RequestVerificationToken': antiForgeryToken() }
    });
    if (!response.ok) return showCartMessage('Kunne ikke fjerne varen. Prøv igjen.', true);

    document.getElementById(`row-${productId}`)?.remove();

    if (document.querySelectorAll('[data-line]').length === 0) {
        window.location.reload(); // shows the empty-cart message
        return;
    }
    await Summary();
    if (typeof refreshCartCount === 'function') refreshCartCount();
}

async function AddToCart(productId) {
    // Guests are sent to log in first
    if (window.isAuthenticated === false) {
        const here = window.location.pathname + window.location.search;
        window.location.href = `/identity/account/login?returnUrl=${encodeURIComponent(here)}`;
        return;
    }

    const quantityElement = document.getElementById(`quantity-${productId}`);
    const quantity = quantityElement ? clampQuantity(quantityElement.value) : 1;

    const formData = new FormData();
    formData.append('id', productId);
    formData.append('quantity', quantity);

    const response = await fetch('/ShoppingCart/AddToCart', {
        method: 'POST',
        headers: { 'RequestVerificationToken': antiForgeryToken() },
        body: formData
    });

    if (!response.ok) {
        const text = await response.text();
        showCartMessage(text && text.length < 200 ? text : 'Kunne ikke legge varen i handlekurven.', true);
        return;
    }

    if (quantityElement) quantityElement.value = 1;
    showCartMessage('Lagt i handlekurven', false);
    if (typeof refreshCartCount === 'function') refreshCartCount();
}

// Updates the price figures on one cart row from the unit prices stored on the row
function updateLine(productId, quantity) {
    const row = document.querySelector(`[data-line="${productId}"]`);
    if (!row) return;

    const set = (selector, unit) => {
        const el = row.querySelector(selector);
        if (el && unit) el.textContent = money(parseFloat(unit) * quantity);
    };
    set('.line-total-inc', row.dataset.unit);
    set('.line-old-inc', row.dataset.old);
    set('.line-total-ex', row.dataset.unitEx);
    set('.line-old-ex', row.dataset.oldEx);
}

async function sendQuantity(productId, url, method) {
    const response = await fetch(url, { method, headers: { 'RequestVerificationToken': antiForgeryToken() } });
    if (!response.ok) {
        const text = await response.text();
        showCartMessage(text && text.length < 200 ? text : 'Kunne ikke endre antall.', true);
        return null;
    }
    return response;
}

async function AddQuantity(productId) {
    const input = document.getElementById(`quantity-${productId}`);
    if (!input) return;

    const oldQuantity = clampQuantity(input.value);
    const newQuantity = Math.min(oldQuantity + 1, MAX_QUANTITY);
    input.value = newQuantity;

    if (document.getElementById('product-page')) return; // product page: only changes the number to add

    updateLine(productId, newQuantity);
    const ok = await sendQuantity(productId, `/ShoppingCart/AddQuantity/?id=${productId}`, 'PATCH');
    if (!ok) { input.value = oldQuantity; updateLine(productId, oldQuantity); }
    await Summary();
}

async function DecreaseQuantity(productId) {
    const input = document.getElementById(`quantity-${productId}`);
    if (!input) return;

    const oldQuantity = clampQuantity(input.value);
    const newQuantity = Math.max(oldQuantity - 1, 1);
    input.value = newQuantity;

    if (document.getElementById('product-page')) return;
    if (newQuantity === oldQuantity) return;

    updateLine(productId, newQuantity);
    const ok = await sendQuantity(productId, `/ShoppingCart/DecreaseQuantity/?id=${productId}`, 'PATCH');
    if (!ok) { input.value = oldQuantity; updateLine(productId, oldQuantity); }
    await Summary();
}

async function UpdateQuantity(productId) {
    const input = document.getElementById(`quantity-${productId}`);
    if (!input) return;

    const newQuantity = clampQuantity(input.value);
    input.value = newQuantity;
    updateLine(productId, newQuantity);

    const response = await sendQuantity(productId, `/ShoppingCart/SetQuantity/?id=${productId}&quantity=${newQuantity}`, 'PATCH');
    if (response) {
        const data = await response.json();
        input.value = data.correctedQuantity;
        updateLine(productId, data.correctedQuantity);
    }
    await Summary();
}

function UpdateQuantityLocal(productId) {
    const input = document.getElementById(`quantity-${productId}`);
    if (input) input.value = clampQuantity(input.value);
}

// Product grid: the +/- next to the Buy button
function ChangeQty(productId, delta) {
    const input = document.getElementById(`quantity-${productId}`);
    if (!input) return;
    input.value = clampQuantity((parseInt(input.value, 10) || 1) + delta);
}

function setText(id, text) {
    const el = document.getElementById(id);
    if (el) el.textContent = text;
}

// Fills the summary panel in the checkout from the server's numbers
async function Summary() {
    if (!document.getElementById('sum-total')) return; // not a checkout page

    try {
        const response = await fetch('/Checkout/Summary', { credentials: 'same-origin', cache: 'no-store' });
        if (!response.ok) throw new Error();
        const d = await response.json();

        setText('sum-before', money(d.sumBefore));
        setText('sum-discount', d.campaignDiscount > 0 ? '−' + money(d.campaignDiscount) : money(0));
        setText('sum-shipping', d.shippingChosen ? money(d.shipping) : 'Velges i neste steg');
        setText('sum-total', money(d.total));
        setText('sum-vat', money(d.vat));
        setText('sum-exvat', money(d.totalExVat));

        const badge = document.getElementById('cart-count');
        if (badge) {
            badge.innerText = d.itemCount;
            badge.style.display = d.itemCount > 0 ? 'block' : 'none';
        }

        const hasCode = d.codePercent > 0;
        document.getElementById('row-code')?.classList.toggle('d-none', !hasCode);
        setText('sum-code-label', hasCode ? `Rabattkode ${d.codeName} (${d.codePercent} %)` : 'Rabattkode');
        setText('sum-code-discount', hasCode ? '−' + money(d.codeDiscount) : '');

        document.getElementById('discount-form')?.classList.toggle('d-none', hasCode);
        document.getElementById('discount-applied')?.classList.toggle('d-none', !hasCode);
        setText('discount-applied-text', hasCode ? `${d.codeName}: −${d.codePercent} %` : '');
    } catch {
        showCartMessage('Kunne ikke hente sammendraget. Last siden på nytt.', true);
    }
}

document.addEventListener('DOMContentLoaded', () => { Summary(); });
