// Trang dat hang: chon tinh/phuong, giu thong tin da go khi sua gio, chan bam "Dat hang" hai lan
(function () {
    var form = document.getElementById('form-dat-hang');
    if (!form) return;

    var KHOA = 'DatHang.NhapTam';
    var TEN = ['hoTen', 'sdt', 'diaChi', 'ghiChu', 'tinhThanh', 'phuongXa'];
    var khoiDiaChi = form.querySelector('[data-dia-chi]');
    var oTinh = form.elements.tinhThanh;
    var oPhuong = form.elements.phuongXa;
    var oChiTiet = form.elements.diaChi;
    var goiY = document.getElementById('dh-goi-y-dia-chi');
    var nutDat = document.getElementById('dh-dat');
    var danhSach = [];

    var luu = null;
    try { luu = JSON.parse(sessionStorage.getItem(KHOA) || 'null'); sessionStorage.removeItem(KHOA); } catch (e) { }

    // TV3: giu lai thong tin da go khi doi so luong, xoa san pham, ap dung / bo ma giam gia
    document.querySelectorAll('form.giu-thong-tin').forEach(function (f) {
        f.addEventListener('submit', function () {
            var d = { cuon: window.scrollY };
            TEN.forEach(function (n) {
                var el = form.elements[n];
                if (el) d[n] = el.disabled ? el.getAttribute('data-chon') : el.value;
            });
            var tt = form.querySelector('input[name="hinhThucTT"]:checked');
            if (tt) d.hinhThucTT = tt.value;
            try { sessionStorage.setItem(KHOA, JSON.stringify(d)); } catch (e) { }
        });
    });

    if (luu) {
        TEN.forEach(function (n) {
            var el = form.elements[n];
            if (!el || luu[n] == null) return;
            if (el.tagName === 'SELECT') el.setAttribute('data-chon', luu[n]);
            else if (!el.readOnly) el.value = luu[n];
        });
        var tt = form.querySelector('input[name="hinhThucTT"][value="' + luu.hinhThucTT + '"]');
        if (tt) tt.checked = true;
        window.addEventListener('load', function () { window.scrollTo(0, luu.cuon || 0); });
    }

    // TV1: do cac lua chon vao mot o chon
    function doLuaChon(o, nhan, ten, chon) {
        o.replaceChildren(new Option(nhan, ''));
        ten.forEach(function (t) { o.add(new Option(t, t, false, t === chon)); });
    }

    // TV1: nap phuong/xa theo tinh/thanh dang chon
    function napPhuong(chon) {
        var tinh = danhSach.find(function (t) { return t.FullName === oTinh.value; });
        doLuaChon(oPhuong, tinh ? 'Chọn phường/xã' : 'Chọn tỉnh/thành trước',
            tinh ? tinh.Wards.map(function (p) { return p.FullName; }).sort(function (a, b) { return a.localeCompare(b, 'vi'); }) : [], chon);
        oPhuong.disabled = !tinh;
    }

    // TV1: hien dong goi y duoi o dia chi chi tiet
    function nhac(chu) {
        goiY.textContent = chu;
        goiY.classList.toggle('hidden', !chu);
    }

    // TV1: tach dia chi trong ho so dang "chi tiet, phuong, tinh" de chon san hai o
    function tachDiaChiHoSo() {
        if (oTinh.getAttribute('data-chon') || !oChiTiet.value.trim()) return;
        var phan = oChiTiet.value.split(',').map(function (s) { return s.trim(); });
        var tinh = phan.length >= 3 && danhSach.find(function (t) { return t.FullName === phan[phan.length - 1]; });
        var phuong = tinh && tinh.Wards.find(function (p) { return p.FullName === phan[phan.length - 2]; });
        if (!phuong) {
            nhac('Hãy chọn tỉnh/thành, phường/xã và chỉ giữ số nhà, tên đường trong địa chỉ chi tiết.');
            return;
        }
        oTinh.setAttribute('data-chon', tinh.FullName);
        oPhuong.setAttribute('data-chon', phuong.FullName);
        oChiTiet.value = phan.slice(0, -2).join(', ');
    }

    fetch(khoiDiaChi.getAttribute('data-dia-chi'))
        .then(function (r) { if (!r.ok) throw new Error(r.status); return r.json(); })
        .then(function (duLieu) {
            danhSach = duLieu;
            tachDiaChiHoSo();
            doLuaChon(oTinh, 'Chọn tỉnh/thành', danhSach.map(function (t) { return t.FullName; }), oTinh.getAttribute('data-chon'));
            oTinh.disabled = false;
            napPhuong(oPhuong.getAttribute('data-chon'));
        })
        .catch(function () {
            // hai o chon van khoa nen khong bat buoc va khong gui len
            doLuaChon(oTinh, 'Không tải được danh sách', []);
            doLuaChon(oPhuong, 'Không tải được danh sách', []);
            nhac('Không tải được danh sách tỉnh/phường. Hãy ghi đầy đủ số nhà, đường, phường/xã, tỉnh/thành vào địa chỉ chi tiết.');
        });

    oTinh.addEventListener('change', function () { nhac(''); napPhuong(''); });

    // TV1: khoa nut sau khi gui de khong tao hai don; mo lai khi quay lai bang nut Back
    form.addEventListener('submit', function () { nutDat.disabled = true; });
    window.addEventListener('pageshow', function (e) { if (e.persisted) nutDat.disabled = false; });
})();
