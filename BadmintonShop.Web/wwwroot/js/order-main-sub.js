(() => {
    const panel = document.getElementById('order-detail');
    if (!panel) return;
    let pending;
    async function select(row) {
        pending?.abort();
        const request = new AbortController();
        pending = request;
        document.querySelectorAll('[data-order-detail]').forEach(r => {
            r.classList.toggle('bg-gray-100', r === row);
            r.setAttribute('aria-current', r === row ? 'true' : 'false');
        });
        panel.textContent = 'Đang tải chi tiết…';
        panel.setAttribute('aria-busy', 'true');
        try {
            const response = await fetch(row.dataset.orderDetail, { signal: request.signal, cache: 'no-store' });
            if (!response.ok || response.redirected) throw new Error('unavailable');
            const html = await response.text();
            if (pending === request) panel.innerHTML = html;
        } catch (error) {
            if (error.name !== 'AbortError' && pending === request)
                panel.textContent = 'Không tải được chi tiết. Hãy đăng nhập lại hoặc chọn lại đơn hàng.';
        } finally {
            if (pending === request) panel.setAttribute('aria-busy', 'false');
        }
    }
    document.querySelectorAll('[data-order-detail]').forEach(row => {
        row.addEventListener('click', event => {
            if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
            event.preventDefault();
            select(row);
        });
        row.addEventListener('keydown', event => {
            if (event.target === row && (event.key === 'Enter' || event.key === ' ')) {
                event.preventDefault();
                select(row);
            }
        });
    });
})();
