// ASGifiks Web Shop — localStorage cart + login store for Blazor JSInterop
(function () {
    'use strict';

    const CART_KEY = 'asg_cart_v1';
    const LOGIN_KEY = 'asg_login_v1';

    const cartStore = {
        _read() {
            try { return JSON.parse(localStorage.getItem(CART_KEY) || '[]'); }
            catch (e) { return []; }
        },
        _write(items) {
            localStorage.setItem(CART_KEY, JSON.stringify(items));
            _updateBadge();
        },
        get items() { return this._read(); },
        add(line) {
            const items = this._read();
            const idx = items.findIndex(i => i.productSlug === line.productSlug && i.skuKey === line.skuKey);
            if (idx >= 0) items[idx].qty += line.qty;
            else items.push(line);
            this._write(items);
        },
        update(skuKey, productSlug, qty) {
            const items = this._read();
            const idx = items.findIndex(i => i.productSlug === productSlug && i.skuKey === skuKey);
            if (idx >= 0) {
                if (qty <= 0) items.splice(idx, 1);
                else items[idx].qty = qty;
                this._write(items);
            }
        },
        remove(skuKey, productSlug) { this.update(skuKey, productSlug, 0); },
        clear() { this._write([]); },
        count() { return this._read().reduce((s, i) => s + i.qty, 0); },
    };

    const loginStore = {
        get user() {
            try { return JSON.parse(localStorage.getItem(LOGIN_KEY) || 'null'); }
            catch (e) { return null; }
        },
        signIn() {
            localStorage.setItem(LOGIN_KEY, JSON.stringify({
                name: 'Marin Galić',
                company: 'Galić Gradnja d.o.o.',
                email: 'marin@galicgradnja.ba',
                partnerType: 'B2B Partner',
                tier: 'Silver — 8% rabat',
                since: '2019',
            }));
            _updateChip();
        },
        signOut() { localStorage.removeItem(LOGIN_KEY); _updateChip(); },
    };

    function _updateBadge() {
        const el = document.getElementById('asg-cart-badge-count');
        if (!el) return;
        const n = cartStore.count();
        el.textContent = n;
        el.style.display = n > 0 ? '' : 'none';
    }

    function _updateChip() {
        const chip = document.getElementById('asg-login-chip');
        const label = document.getElementById('asg-login-chip-label');
        if (!chip || !label) return;
        const u = loginStore.user;
        label.textContent = u ? u.name.split(' ')[0] : 'Prijava';
        chip.classList.toggle('is-in', !!u);
        chip.onclick = u
            ? () => { window.location.href = '/account'; }
            : () => { openLoginModal(); };
    }

    function openLoginModal() {
        if (document.getElementById('asg-login-modal-bg')) return;
        const bg = document.createElement('div');
        bg.id = 'asg-login-modal-bg';
        bg.className = 'asg-modal-bg';
        bg.innerHTML = `
          <div class="asg-modal" onclick="event.stopPropagation()">
            <button class="asg-modal-close" onclick="window.asgShop.closeLogin()">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" style="width:18px;height:18px"><path d="M6 6L18 18M18 6L6 18"/></svg>
            </button>
            <h2>Prijava partnera</h2>
            <p>Pristupite B2B cijenama, partnerskim rabatima i istoriji narudžbi.</p>
            <div style="display:grid;gap:14px;margin-bottom:20px">
              <div class="asg-field">
                <label>E-mail</label>
                <input type="email" value="marin@galicgradnja.ba" />
              </div>
              <div class="asg-field">
                <label>Lozinka</label>
                <input type="password" value="demo1234" />
              </div>
            </div>
            <button class="asg-btn asg-btn-pri" style="width:100%;justify-content:center" onclick="window.asgShop.doSignIn()">
              Prijavi se
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" style="width:14px;height:14px"><path d="M5 12H19M13 6l6 6-6 6"/></svg>
            </button>
            <div style="font-family:var(--asg-mono);font-size:10px;letter-spacing:.08em;text-transform:uppercase;color:var(--asg-muted);margin-top:18px;text-align:center">Demo — kliknite Prijavi se za simulirani B2B nalog</div>
          </div>`;
        bg.onclick = () => window.asgShop.closeLogin();
        document.body.appendChild(bg);
    }

    window.asgShop = {
        getCartJson() { return JSON.stringify(cartStore.items); },
        getUserJson() { return JSON.stringify(loginStore.user); },
        addToCart(line) { cartStore.add(line); },
        removeFromCart(skuKey, productSlug) { cartStore.remove(skuKey, productSlug); },
        updateCartItem(skuKey, productSlug, qty) { cartStore.update(skuKey, productSlug, qty); },
        clearCart() { cartStore.clear(); },
        signOut() { loginStore.signOut(); },
        openLogin() { openLoginModal(); },
        closeLogin() { const el = document.getElementById('asg-login-modal-bg'); if (el) el.remove(); },
        doSignIn() { loginStore.signIn(); window.asgShop.closeLogin(); window.location.href = '/account'; },
        saveLastOrder(json) { sessionStorage.setItem('asg_last_order', json); },
        getLastOrderJson() {
            try { return sessionStorage.getItem('asg_last_order') || 'null'; }
            catch (e) { return 'null'; }
        },
        initNav() { _updateBadge(); _updateChip(); },
    };

    function init() { _updateBadge(); _updateChip(); }
    if (document.readyState === 'loading') { document.addEventListener('DOMContentLoaded', init); }
    else { init(); }
    document.addEventListener('enhancedload', () => { window.scrollTo(0, 0); init(); });
})();
