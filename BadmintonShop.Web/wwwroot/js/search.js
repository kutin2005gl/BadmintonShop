// O tim kiem mo rong tren header: mo/dong, bang kham pha va goi y san pham
(function () {
    var khung = document.getElementById('tim');
    if (!khung) return;

    var nutMo = document.getElementById('btn-search');
    var form = document.getElementById('tim-form');
    var oNhap = document.getElementById('tim-nhap');
    var bang = document.getElementById('tim-bang');
    var noiDung = document.getElementById('tim-noi-dung');
    var thongBao = document.getElementById('tim-thong-bao');

    var khamPha = null;     // HTML bang kham pha da tai
    var henGio = null;
    var yeuCau = null;      // request dang cho, huy khi go tiep
    var lan = 0;            // so thu tu de bo qua ket qua cu

    // TV1: kiem tra o tim kiem dang mo
    function dangMo() { return khung.getAttribute('data-mo') === 'co'; }

    // TV1: tai mot doan HTML tu may chu
    function tai(url, tinHieu) {
        return fetch(url, { signal: tinHieu, headers: { 'X-Requested-With': 'XMLHttpRequest' } })
            .then(function (r) { if (!r.ok) throw new Error(r.status); return r.text(); });
    }

    // TV1: thay noi dung bang va doc thong bao cho trinh doc man hinh
    function ve(html) {
        noiDung.innerHTML = html;
        bang.scrollTop = 0;
        var o = noiDung.querySelector('[data-thong-bao]');
        thongBao.textContent = o ? o.getAttribute('data-thong-bao') : '';
    }

    // TV1: hien thong bao dang tai / loi trong bang
    function veChu(chu) {
        var p = document.createElement('p');
        p.className = 'tim-trong';
        p.textContent = chu;
        noiDung.replaceChildren(p);
        thongBao.textContent = chu;
    }

    // TV1: cap nhat bang theo tu khoa hien tai (rong -> kham pha, co chu -> goi y)
    function capNhat() {
        var tuKhoa = oNhap.value.trim();
        var so = ++lan;
        if (yeuCau) yeuCau.abort();
        yeuCau = new AbortController();

        if (!tuKhoa && khamPha) { ve(khamPha); return; }
        if (!noiDung.childElementCount) veChu('Đang tải…');

        var url = tuKhoa
            ? bang.getAttribute('data-goi-y') + '?tuKhoa=' + encodeURIComponent(tuKhoa)
            : bang.getAttribute('data-kham-pha');
        tai(url, yeuCau.signal).then(function (html) {
            if (so !== lan) return;
            if (!tuKhoa) khamPha = html;
            ve(html);
        }).catch(function (e) {
            if (so !== lan || e.name === 'AbortError') return;
            veChu('Không tải được gợi ý. Nhấn Enter để tìm trên trang sản phẩm.');
        });
    }

    // TV1: mo o tim kiem va dua con tro vao o nhap
    function mo() {
        var menu = document.getElementById('menu-mobile');
        if (menu) menu.classList.add('hidden');
        khung.setAttribute('data-mo', 'co');
        nutMo.setAttribute('aria-expanded', 'true');
        form.inert = false;
        bang.inert = false;
        oNhap.focus({ preventScroll: true });
        capNhat();
    }

    // TV1: thu gon o tim kiem; traFocus = true thi dua con tro ve nut kinh lup
    function dong(traFocus) {
        if (!dangMo()) return;
        khung.setAttribute('data-mo', 'khong');
        nutMo.setAttribute('aria-expanded', 'false');
        clearTimeout(henGio);
        if (yeuCau) yeuCau.abort();
        lan++;
        form.inert = true;
        bang.inert = true;
        if (traFocus) nutMo.focus({ preventScroll: true });
    }

    nutMo.addEventListener('click', mo);
    document.getElementById('tim-dong').addEventListener('click', function () { dong(true); });

    oNhap.addEventListener('input', function () {
        clearTimeout(henGio);
        henGio = setTimeout(capNhat, 200);
    });

    // TV1: Esc de dong; mui ten len/xuong de di chuyen giua o nhap va cac goi y
    function banPhim(e) {
        if (!dangMo()) return;
        if (e.key === 'Escape') { e.preventDefault(); dong(true); return; }
        if (e.key !== 'ArrowDown' && e.key !== 'ArrowUp') return;
        var muc = Array.prototype.slice.call(noiDung.querySelectorAll('a'));
        var viTri = muc.indexOf(document.activeElement);
        if (viTri < 0 && document.activeElement !== oNhap) return;
        e.preventDefault();
        var moi = e.key === 'ArrowDown' ? Math.min(viTri + 1, muc.length - 1) : viTri - 1;
        (moi < 0 ? oNhap : muc[moi] || oNhap).focus();
    }
    document.addEventListener('keydown', banPhim);

    // TV1: bam ra ngoai hoac Tab ra ngoai thi thu gon
    document.addEventListener('pointerdown', function (e) {
        if (!form.contains(e.target) && !bang.contains(e.target) && !nutMo.contains(e.target)) dong(false);
    });
    document.addEventListener('focusin', function (e) {
        if (!khung.contains(e.target)) dong(false);
    });

    // TV1: quay lai trang bang nut Back thi o tim kiem o trang thai dong
    window.addEventListener('pageshow', function (e) { if (e.persisted) dong(false); });
})();
