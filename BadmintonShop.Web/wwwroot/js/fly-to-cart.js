// TV1: hieu ung anh san pham bay vao gio hang (Web Animations API, khong dung thu vien)
window.bayVaoGio = (() => {
    const giamChuyenDong = window.matchMedia?.('(prefers-reduced-motion: reduce)');
    const coAnimate = typeof Element !== 'undefined' && typeof Element.prototype.animate === 'function';
    const soBayToiDa = 4;
    const kichThuocDich = 28;
    const soKhung = 36;
    let dangBay = 0;
    let nayIcon;
    let nayBadge;

    // TV1: tao ham easing cubic-bezier giong CSS
    function cubicBezier(x1, y1, x2, y2) {
        const cx = 3 * x1, bx = 3 * (x2 - x1) - cx, ax = 1 - cx - bx;
        const cy = 3 * y1, by = 3 * (y2 - y1) - cy, ay = 1 - cy - by;
        // TV1: toa do x theo tham so t
        const sx = t => ((ax * t + bx) * t + cx) * t;
        // TV1: toa do y theo tham so t
        const sy = t => ((ay * t + by) * t + cy) * t;
        // TV1: dao ham cua x theo t
        const dx = t => (3 * ax * t + 2 * bx) * t + cx;
        // TV1: giai t theo x bang Newton roi tra ve y
        return x => {
            let t = x;
            for (let i = 0; i < 8; i++) {
                const sai = sx(t) - x;
                const d = dx(t);
                if (Math.abs(sai) < 1e-5 || Math.abs(d) < 1e-6) break;
                t -= sai / d;
            }
            return sy(Math.min(1, Math.max(0, t)));
        };
    }

    const easing = cubicBezier(0.45, 0.05, 0.2, 1);

    // TV1: kiem tra nguoi dung co muon giam chuyen dong
    function nenGiam() {
        return Boolean(giamChuyenDong?.matches) || !coAnimate;
    }

    // TV1: kiem tra hinh chu nhat co hien trong khung nhin
    function trongKhung(r) {
        return r.width > 0 && r.height > 0 && r.bottom > 0 && r.right > 0 &&
            r.top < window.innerHeight && r.left < window.innerWidth;
    }

    // TV1: tao ban sao anh (hoac o chu cai khi anh khong tai duoc) de bay
    function taoBanSao(khung, r) {
        const ghost = document.createElement('div');
        ghost.className = 'fly-ghost';
        ghost.setAttribute('aria-hidden', 'true');
        ghost.style.left = `${r.left}px`;
        ghost.style.top = `${r.top}px`;
        ghost.style.width = `${r.width}px`;
        ghost.style.height = `${r.height}px`;

        const goc = khung.querySelector('img');
        if (goc && goc.complete && goc.naturalWidth > 0) {
            const anh = new Image();
            anh.alt = '';
            anh.decoding = 'sync';
            anh.src = goc.currentSrc || goc.src;
            const bienDoi = getComputedStyle(goc).transform;
            if (bienDoi && bienDoi !== 'none') anh.style.transform = bienDoi;
            ghost.append(anh);
        } else {
            ghost.classList.add('fly-ghost-chu');
            ghost.style.fontSize = `${Math.round(Math.min(r.width, r.height) * 0.45)}px`;
            ghost.textContent = (khung.textContent || '').trim().charAt(0) || '•';
        }
        return ghost;
    }

    // TV1: lam bieu tuong gio va so luong nay nhe khi nhan hang
    function nay(gio) {
        if (!gio || nenGiam()) return;
        const icon = gio.querySelector('svg') || gio;
        nayIcon?.cancel();
        nayIcon = icon.animate([
            { transform: 'scale(1)' },
            { transform: 'scale(1.28) rotate(-8deg)', offset: 0.28 },
            { transform: 'scale(0.9) rotate(4deg)', offset: 0.55 },
            { transform: 'scale(1.06)', offset: 0.78 },
            { transform: 'scale(1)' }
        ], { duration: 520, easing: 'ease-out' });

        const badge = gio.querySelector('.cart-count');
        if (badge) {
            nayBadge?.cancel();
            nayBadge = badge.animate([
                { transform: 'scale(1)' },
                { transform: 'scale(1.45)', offset: 0.35 },
                { transform: 'scale(1)' }
            ], { duration: 420, delay: 60, easing: 'cubic-bezier(.3,1.6,.5,1)' });
        }
    }

    // TV1: cho anh bay tu khung san pham ve gio, tra ve Promise khi anh cham gio
    function bay(khung, gio) {
        if (nenGiam() || !khung || !gio || !khung.isConnected) return Promise.resolve();

        const dichEl = gio.querySelector('svg') || gio;
        const nguon = khung.getBoundingClientRect();
        const dich = dichEl.getBoundingClientRect();
        if (!trongKhung(nguon) || !trongKhung(dich)) {
            nay(gio);
            return Promise.resolve();
        }
        if (dangBay >= soBayToiDa) {
            nay(gio);
            return Promise.resolve();
        }

        // Diem dau (tam anh), diem cuoi (tam icon gio) - lay tu vi tri thuc te moi lan bay.
        const sx = nguon.left + nguon.width / 2;
        const sy = nguon.top + nguon.height / 2;
        const ex = dich.left + dich.width / 2;
        const ey = dich.top + dich.height / 2;
        const vx = ex - sx;
        const vy = ey - sy;
        const doDai = Math.hypot(vx, vy);
        if (doDai < 4) {
            nay(gio);
            return Promise.resolve();
        }

        // Diem dieu khien Bezier: lech vuong goc voi duong thang, uu tien vong len tren.
        let nx = -vy / doDai;
        let ny = vx / doDai;
        if (ny > 0.01 || (Math.abs(ny) <= 0.01 && nx > 0)) { nx = -nx; ny = -ny; }
        const do_cong = Math.min(240, Math.max(60, doDai * 0.35));
        const cx = sx + vx / 2 + nx * do_cong;
        const cy = Math.max(0, sy + vy / 2 + ny * do_cong);

        const tiLeCuoi = Math.min(1, kichThuocDich / Math.max(nguon.width, nguon.height));
        const huong = vx >= 0 ? 1 : -1;
        const thoiGian = Math.round(Math.min(900, Math.max(700, 640 + doDai * 0.18)));

        const khungHinh = [];
        for (let i = 0; i <= soKhung; i++) {
            const u = i / soKhung;
            const p = easing(u);
            const q = 1 - p;
            const x = q * q * sx + 2 * q * p * cx + p * p * ex;
            const y = q * q * sy + 2 * q * p * cy + p * p * ey;
            // Thu nho lien tuc tu dau den cuoi, nhanh hon o nua dau de anh khong che giao dien.
            const tiLe = 1 - (1 - tiLeCuoi) * (1 - Math.pow(1 - p, 1.7));
            const xoay = huong * (10 * Math.sin(Math.PI * p) + 6 * p);
            khungHinh.push({
                offset: u,
                transform: `translate(${(x - sx).toFixed(2)}px, ${(y - sy).toFixed(2)}px) rotate(${xoay.toFixed(2)}deg) scale(${tiLe.toFixed(4)})`,
                borderRadius: `${Math.min(50, p * 80).toFixed(1)}%`,
                opacity: p < 0.75 ? 1 : 1 - (p - 0.75) / 0.25 * 0.75
            });
        }

        const ghost = taoBanSao(khung, nguon);
        document.body.append(ghost);
        dangBay++;

        return new Promise(
            // TV1: chay animation va resolve khi anh cham gio
            resolve => {
                let xong = false;
                let henGio;
                // TV1: don dep ban sao va bao hieu anh da cham gio
                const ketThuc = (coNay) => {
                    if (xong) return;
                    xong = true;
                    dangBay--;
                    clearTimeout(henGio);
                    ghost.remove();
                    resolve();
                    if (coNay) nay(gio);
                };
                let hieuUng;
                try {
                    hieuUng = ghost.animate(khungHinh, { duration: thoiGian, easing: 'linear', fill: 'forwards' });
                } catch {
                    ketThuc(true);
                    return;
                }
                // TV1: ket thuc khi animation chay xong
                hieuUng.onfinish = () => ketThuc(true);
                // TV1: don dep khi animation bi huy
                hieuUng.oncancel = () => ketThuc(false);
                // Phong khi tab bi an lam animation khong ket thuc: van don dep.
                henGio = setTimeout(
                    // TV1: don dep du phong neu animation khong bao ket thuc
                    () => ketThuc(true), thoiGian + 500);
            });
    }

    return { bay, nay };
})();
