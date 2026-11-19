// TV1: khoi tao tuong tac gio hang
(() => {
    const badge = document.getElementById('gio-so-luong');
    const cartButton = document.getElementById('btn-gio');
    const toast = document.getElementById('gio-toast');
    const drawer = document.getElementById('gio-drawer');
    const overlay = document.getElementById('gio-overlay');
    const drawerContent = document.getElementById('gio-noi-dung');
    const returnKey = 'cart.return';
    const restoreKey = 'cart.restore';
    let toastTimer;
    let drawerRequest;
    let pendingAdd = Promise.resolve();

    // TV1: doc diem quay lai tu phien trinh duyet
    function readPoint(key) {
        try {
            const point = JSON.parse(sessionStorage.getItem(key));
            const url = new URL(point.url, location.origin);
            return url.origin === location.origin && Number.isFinite(point.scroll)
                ? { url: url.href, scroll: point.scroll }
                : null;
        } catch {
            return null;
        }
    }

    // TV1: luu hoac xoa diem quay lai
    function savePoint(key, point) {
        try {
            if (point) sessionStorage.setItem(key, JSON.stringify(point));
            else sessionStorage.removeItem(key);
        } catch {
            // Links keep their normal destination when session storage is unavailable.
        }
    }

    // TV1: hien thi thong bao gio hang
    function showToast(message, success) {
        clearTimeout(toastTimer);
        toast.textContent = message;
        toast.classList.toggle('cart-toast-error', !success);
        toastTimer = setTimeout(
            // TV1: an thong bao sau thoi gian hien thi
            () => { toast.textContent = ''; }, 4500);
    }

    // TV1: cap nhat tong so luong tren bieu tuong gio
    function setCount(count) {
        badge.textContent = count;
        badge.hidden = count === 0;
        cartButton.setAttribute('aria-label', `Giỏ hàng (${count})`);
    }

    // TV1: tai lai noi dung ngan gio
    async function refreshDrawer() {
        drawerRequest?.abort();
        const request = new AbortController();
        drawerRequest = request;
        drawerContent.textContent = 'Đang tải giỏ hàng…';
        try {
            const response = await fetch(drawerContent.dataset.url, { signal: request.signal });
            if (!response.ok) throw new Error();
            const html = await response.text();
            if (!request.signal.aborted) drawerContent.innerHTML = html;
        } catch {
            if (!request.signal.aborted) drawerContent.textContent = 'Chưa tải được giỏ hàng. Hãy mở lại để thử lại.';
        }
    }

    // TV1: dong ngan gio va tra tieu diem ve nut gio
    function closeDrawer() {
        drawerRequest?.abort();
        drawer.inert = true;
        drawer.classList.add('translate-x-full');
        overlay.classList.add('hidden');
        cartButton.focus({ preventScroll: true });
    }

    if (drawer) {
        document.getElementById('gio-dong').addEventListener('click', closeDrawer);
        document.getElementById('gio-tiep-tuc').addEventListener('click', closeDrawer);
        overlay.addEventListener('click', closeDrawer);
        document.addEventListener('keydown',
        // TV1: dong ngan gio khi bam Escape
        event => {
            if (event.key === 'Escape' && !drawer.inert) closeDrawer();
        });
    }

    document.addEventListener('click',
    // TV1: xu ly lien ket mo gio va tiep tuc chon hang
    event => {
        const link = event.target.closest('[data-cart-link], [data-cart-return]');
        if (!link || event.defaultPrevented || event.button !== 0 ||
            event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;

        if (link.hasAttribute('data-cart-return')) {
            const point = readPoint(returnKey);
            if (!point) return;
            event.preventDefault();
            savePoint(restoreKey, point);
            savePoint(returnKey, null);
            location.assign(point.url);
            return;
        }

        // TV1: chi luu diem quay lai khi dang o trang mua hang (khong phai trang gio/dat hang)
        if (document.body.dataset.cartPage !== 'true' &&!link.closest('#gio-drawer')) {
            savePoint(returnKey, { url: location.href, scroll: window.scrollY });
        }
        if (link === cartButton && drawer) {
            event.preventDefault();
            drawer.inert = false;
            drawer.classList.remove('translate-x-full');
            overlay.classList.remove('hidden');
            document.getElementById('gio-dong').focus({ preventScroll: true });
            refreshDrawer();
        }
    });

    document.addEventListener('submit',
    // TV1: gui form them hang bang AJAX
    event => {
        const form = event.target;
        if (!form.matches('[data-cart-add]') || !window.fetch || event.defaultPrevented) return;
        event.preventDefault();
        const button = form.querySelector('button[type="submit"]');
        if (button.disabled) return;
        const body = new URLSearchParams(new FormData(form));
        button.disabled = true;
        // Queue requests on this page so each submission sees the saved session.
        pendingAdd = pendingAdd.then(
        // TV1: gui lan luot cac yeu cau ghi gio
        async () => {
            try {
                const response = await fetch(form.action, {
                    method: 'POST',
                    headers: { 'X-Requested-With': 'XMLHttpRequest' },
                    body
                });
                const result = await response.json();
                if (typeof result.thanhCong !== 'boolean' || !Number.isInteger(result.tongSoLuong)) throw new Error();
                setCount(result.tongSoLuong);
                showToast(result.thongBao, result.thanhCong);
                if (result.thanhCong && drawer && !drawer.inert) refreshDrawer();
            } catch {
                showToast('Chưa xác nhận được kết quả. Hãy kiểm tra giỏ hàng rồi thử lại.', false);
            } finally {
                button.disabled = false;
            }
        });
    });

    const restore = readPoint(restoreKey);
    if (restore?.url === location.href) {
        savePoint(restoreKey, null);
        window.scrollTo(0, restore.scroll);
        if (window.scrollY < restore.scroll && document.readyState !== 'complete') {
            window.addEventListener('load',
                // TV1: khoi phuc vi tri cuon sau khi tai trang
                () => window.scrollTo(0, restore.scroll), { once: true });
        }
    }
})();
