const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '../..');
const base = 'docs/bao-cao/';
const chapters = [
  { title: 'Giới thiệu', text: 'BadmintonShop gồm Web ASP.NET Core MVC, Desktop WinForms, thư viện Core và SQL Server, sử dụng .NET 8.\n\nTODO(SV1): Điền thông tin nhóm, giảng viên, phạm vi và mục tiêu nghiệm thu.' },
  { title: 'SRS và use case tổng quát — SV2', text: 'TODO(SV2): Cung cấp SRS và use case khách hàng đã duyệt.' },
  { title: 'Kiến trúc hệ thống — SV1', files: ['chuong-kien-truc.md'] },
  { title: 'Cơ sở dữ liệu — SV1', files: ['mo-ta-csdl.md'], text: '### Kiểm chứng ngày 12/11/2026\n\n16 bảng, 3 trigger, 1 proc, 40 SP, 23 KH, 5 NV, 11 phiếu nhập, 28 hóa đơn Online, 4 CTKM, 34 đánh giá, tồn âm bằng 0. Hóa đơn trải 5 tháng 06–10/2026.\n\nTODO(SV3): Phối hợp SV4 bổ sung hóa đơn trải 6–12 tháng.\n\nTODO(SV4): Bổ sung hóa đơn tại quầy vào dữ liệu nộp.' },
  { title: 'Thiết kế', parts: [
    { title: 'Sequence và activity đặt hàng — SV3', text: 'TODO(SV3): Cung cấp sequence đặt hàng/thanh toán và activity xử lý đơn.' },
    { title: 'Class diagram — SV4', files: ['../diagrams/06-lop-core-sv4.md'] },
    { title: 'Sequence lập phiếu nhập — SV1', files: ['../diagrams/04-ton-kho.md'] },
    { title: 'Activity đăng nhập Desktop — SV1', files: ['../diagrams/05-dang-nhap.md'] }
  ] },
  { title: 'Cài đặt', files: ['huong-dan-cai-dat.md'] },
  { title: 'Kiểm thử', parts: [
    { title: 'Kế hoạch kiểm thử SV4', files: ['kiem-thu-sv4.md'], omitTests: true },
    { title: 'Bảng test case chung SV1 và SV4', tests: true },
    { title: 'Kết quả kiểm thử SV4 theo từng đợt', files: ['ket-qua-kiem-thu-sv4.md'] },
    { title: 'Tích hợp SV3–SV4', files: ['tich-hop-sv3-sv4.md'] },
    { title: 'Quy ước tích hợp SV4', files: ['tich-hop-sv4.md'] },
    { title: 'Checklist kiểm thử chéo SV2', files: ['checklist-sv2-desktop.md'] }
  ] },
  { title: 'Hướng dẫn sử dụng', parts: [
    { title: 'Phần SV1', files: ['huong-dan-su-dung-sv1.md'] },
    { title: 'Phần SV4', files: ['huong-dan-su-dung-sv4.md'] },
    { title: 'Phần SV2 và SV3', text: 'TODO(SV2): Bổ sung hướng dẫn khách hàng và đánh giá.\n\nTODO(SV3): Bổ sung hướng dẫn giỏ hàng, đặt hàng và khuyến mãi.' }
  ] },
  { title: 'Tổng kết — SV4', files: ['tong-ket-sv4.md'] },
  { title: 'Bảng tự đánh giá', text: '| Thành viên | Họ tên/MSSV | Công việc | Tự đánh giá | Xác nhận |\n|---|---|---|---|---|\n' + [1,2,3,4].map(n => `| SV${n} | TODO(SV${n}) | TODO(SV${n}) | TODO(SV${n}) | TODO(SV${n}) |`).join('\n') },
  { title: 'Phụ lục đặc tả', parts: [
    { title: 'Đặc tả SV1', files: ['dac-ta-sv1.md'], omitTests: true },
    { title: 'Đặc tả SV4', files: ['dac-ta-sv4.md'] }
  ] }
];

// TV1: đọc nguồn UTF-8 theo đường dẫn tương đối của chương báo cáo.
function readSource(file) {
  return fs.readFileSync(path.join(root, base, file), 'utf8').replace(/^\uFEFF/, '').replace(/\r/g, '');
}

// TV1: ghép bảng 36 TC SV1 và 61 TC SV4, giữ nguyên dữ liệu và kết quả nguồn.
function commonTests() {
  const rows = [];
  for (const [file, owner] of [['dac-ta-sv1.md', 'SV1'], ['kiem-thu-sv4.md', 'SV4']]) {
    for (const line of readSource(file).split('\n').filter(line => /^\| TC\d+\s*\|/.test(line))) {
      const cells = line.split('|').slice(1, -1).map(cell => cell.trim());
      const original = cells.shift();
      const values = owner === 'SV1' ? cells : ['', '', ...cells];
      rows.push(`| TC${String(rows.length + 1).padStart(3, '0')} | ${owner} | ${original} | ${values.join(' | ')} |`);
    }
  }
  if (rows.length !== 97) throw new Error(`Cần 97 test case, nhận ${rows.length}`);
  return 'Kết quả giữ theo mốc của từng nguồn, gồm hồ sơ SV1 ngày 15/10 và kế hoạch lịch sử SV4 ngày 18/10. Các đợt kiểm mới được trình bày riêng ở mục kết quả.\n\n| ID chung | Phụ trách | ID nguồn | Use case | Tình huống | Bước/dữ liệu thử | Kết quả mong đợi | Kết quả / bằng chứng |\n|---|---|---|---|---|---|---|---|\n' + rows.join('\n');
}

// TV1: hạ cấp tiêu đề nguồn, giữ nội dung và chuyển liên kết về thư mục báo cáo tổng.
function includeSource(file, level, omitTests) {
  let text = readSource(file);
  if (omitTests) text = text.replace(/(?:^\|.*\n)+/gm, block => /\| TC\d+\s*\|/.test(block) ? '' : block);
  let firstTitle = true;
  let fenced = false;
  text = text.split('\n').filter(line => {
    return !line.startsWith('>');
  }).map(line => {
    if (line.startsWith('```')) fenced = !fenced;
    if (fenced) return line;
    const heading = line.match(/^(#{1,6})\s+(.+)$/);
    if (!heading) return line;
    if (firstTitle && heading[1].length === 1) { firstTitle = false; return ''; }
    firstTitle = false;
    const depth = Math.min(4, level + Math.max(0, heading[1].length - 2));
    return '#'.repeat(depth) + ' ' + heading[2];
  }).join('\n');
  // Liên kết ở nguồn diagrams cần quy về thư mục bao-cao.
  text = text.replace(/(!?\[[^\]]*\]\()([^):]+)(\))/g, (all, before, target, after) => {
    if (target.startsWith('#') || path.isAbsolute(target)) return all;
    const absolute = path.resolve(root, base, path.dirname(file), target);
    return before + path.relative(path.join(root, base), absolute).replace(/\\/g, '/') + after;
  });
  return text.trim();
}

// TV1: tạo nội dung chương từ cấu hình nguồn hoặc TODO.
function content(part, level) {
  const sections = (part.files || []).map(file => includeSource(file, level, part.omitTests));
  if (part.text) sections.push(part.text);
  if (part.tests) sections.push(commonTests());
  for (const child of part.parts || []) sections.push(`### ${child.title}\n\n${content(child, 4)}`);
  return sections.join('\n\n');
}

// TV1: đánh số liên tục theo chương, mục, mục con và chú thích hình bảng.
function numberChapters(text) {
  let chapter = 0, section = 0, subsection = 0, figure = 0, table = 0;
  let fenced = false, inTable = false;
  return text.split('\n').map(line => {
    if (line.startsWith('```')) { fenced = !fenced; return line; }
    if (fenced) return line;
    const heading = line.match(/^(#{2,4}) (.+)$/);
    if (heading) {
      const title = heading[2].replace(/^(?:\d+\.)+\d*\.?\s+/, '');
      if (heading[1].length === 2) { chapter++; section = 0; subsection = 0; figure = 0; table = 0; }
      else if (heading[1].length === 3) { section++; subsection = 0; }
      else { if (!section) section = 1; subsection++; }
      const number = heading[1].length === 2 ? `${chapter}` : heading[1].length === 3 ? `${chapter}.${section}` : `${chapter}.${section}.${subsection}`;
      return `${heading[1]} ${number}. ${title}`;
    }
    if (line.startsWith('|')) {
      if (!inTable) { inTable = true; return `Bảng ${chapter}.${++table}\n\n${line}`; }
    } else inTable = false;
    if (/^!\[/.test(line) && !line.startsWith('![TODO:')) return line.replace(/^!\[/, `![Hình ${chapter}.${++figure}. `);
    return line;
  }).join('\n');
}

// TV1: sinh báo cáo tổng có mục lục và lưu UTF-8 để tái lập từ các nguồn.
function main() {
  const body = numberChapters(chapters.map(part => `## ${part.title}\n\n${content(part, 3)}`).join('\n\n'));
  const toc = body.split('\n').filter(line => /^## /.test(line)).map(line => line.slice(3)).join('\n\n');
  const result = '# BÁO CÁO TỔNG HỢP ĐỒ ÁN BADMINTONSHOP\n\n<!-- Sinh tự động bởi tools/XuatBaoCao/ghep-bao-cao.js – sửa file nguồn rồi chạy lại -->\n\n**Mục lục chương**\n\n' + toc + '\n\n' + body + '\n';
  fs.writeFileSync(path.join(root, base, 'bao-cao-tong.md'), result);
  console.log('Đã ghép 11 chương và 97 test case vào bao-cao-tong.md');
}

main();
