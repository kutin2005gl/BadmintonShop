// Ghép báo cáo tổng từ các file nguồn: node tools/XuatBaoCao/ghep-bao-cao.js
// Mỗi nguồn là tên file (lấy cả file) hoặc { file, only, omit, rename, dropIntro }:
//   only/omit: danh sách tiền tố tiêu đề mục cấp ## cần lấy/bỏ (so sánh sau khi bỏ số thứ tự)
//   rename   : { 'tiền tố tiêu đề cũ': 'tiêu đề mới' }
//   dropIntro: bỏ đoạn văn trước mục ## đầu tiên (thường là ghi chú nhánh/ngày làm việc)
// Liên kết tới file khác bị đổi thành chữ thường (báo cáo in không bấm được link);
// khối ```mermaid được thay bằng ảnh PNG trong docs/diagrams/png nếu có, chưa có thì để TODO.
const fs = require('fs');
const path = require('path');
const root = path.resolve(__dirname, '../..');
const base = 'docs/bao-cao/';
const pngDir = path.join(root, 'docs/diagrams/png');
const chapters = [
  { title: 'Giới thiệu', text: 'BadmintonShop gồm Web ASP.NET Core MVC, Desktop WinForms, thư viện Core và SQL Server, sử dụng .NET 8.\n\nTODO(SV1): Điền thông tin nhóm, giảng viên, phạm vi và mục tiêu nghiệm thu.' },
  { title: 'SRS và use case tổng quát — SV2', text: 'TODO(SV2): Cung cấp SRS và use case khách hàng đã duyệt.' },
  { title: 'Kiến trúc hệ thống — SV1', files: ['chuong-kien-truc.md'] },
  { title: 'Cơ sở dữ liệu — SV1', files: ['mo-ta-csdl.md'], text: '### Kiểm chứng dữ liệu mẫu\n\nChạy `database/BadmintonShop.sql` trên CSDL trống ngày 12/11/2026: 16 bảng, 3 trigger, 1 thủ tục, 40 sản phẩm, 23 khách hàng, 5 nhân viên, 11 phiếu nhập, 28 hóa đơn Online, 4 CTKM, 34 đánh giá, không sản phẩm nào tồn kho âm. Hóa đơn trải 5 tháng 06–10/2026.\n\nTODO(SV3): Phối hợp SV4 bổ sung hóa đơn trải 6–12 tháng.\n\nTODO(SV4): Bổ sung hóa đơn tại quầy vào dữ liệu nộp.' },
  { title: 'Thiết kế', parts: [
    { title: 'Sequence và activity đặt hàng — SV3', text: 'TODO(SV3): Cung cấp sequence đặt hàng/thanh toán và activity xử lý đơn.' },
    { title: 'Class diagram — SV4', files: [{ file: '../diagrams/06-lop-core-sv4.md', dropIntro: true }] },
    { title: 'Sequence lập phiếu nhập — SV1', files: ['../diagrams/04-ton-kho.md'] },
    { title: 'Activity đăng nhập Desktop — SV1', files: ['../diagrams/05-dang-nhap.md'] }
  ] },
  { title: 'Cài đặt', files: ['huong-dan-cai-dat.md'] },
  { title: 'Kiểm thử', parts: [
    { title: 'Kế hoạch kiểm thử SV4', files: [{ file: 'kiem-thu-sv4.md', dropIntro: true, only: ['Bộ kiểm tra hiện tại', 'Phạm vi và môi trường', 'Tiêu chí bàn giao'] }], omitTests: true },
    { title: 'Bảng test case chung SV1 và SV4', tests: true },
    { title: 'Kết quả kiểm thử SV4', files: [{ file: 'ket-qua-kiem-thu-sv4.md', only: ['Nghiệp vụ đã kiểm chứng', 'Chưa kiểm thử và phối hợp còn lại'] }] },
    { title: 'Tích hợp SV3–SV4', files: [{ file: 'tich-hop-sv3-sv4.md', dropIntro: true, omit: ['Nền code và phạm vi', 'Chạy kiểm tra và demo an toàn', 'File triển khai và phối hợp'] }] }
  ] },
  { title: 'Hướng dẫn sử dụng', parts: [
    { title: 'Phần SV1', files: ['huong-dan-su-dung-sv1.md'] },
    { title: 'Phần SV4', files: [{ file: 'huong-dan-su-dung-sv4.md', only: ['Thao tác chính'] }] },
    { title: 'Phần SV2 và SV3', text: 'TODO(SV2): Bổ sung hướng dẫn khách hàng và đánh giá.\n\nTODO(SV3): Bổ sung hướng dẫn giỏ hàng, đặt hàng và khuyến mãi.' }
  ] },
  { title: 'Tổng kết', files: [{ file: 'tong-ket-sv4.md', dropIntro: true, only: ['Kết quả triển khai', 'Phối hợp và hướng phát triển'], rename: { 'Kết quả triển khai': 'Kết quả triển khai phần SV4' } }], text: 'TODO(SV4): Bổ sung kết quả của SV1, SV2, SV3 để chương tổng kết bao quát cả nhóm.' },
  { title: 'Bảng tự đánh giá', text: '| Thành viên | Họ tên/MSSV | Công việc | Tự đánh giá | Xác nhận |\n|---|---|---|---|---|\n' + [1,2,3,4].map(n => `| SV${n} | TODO(SV${n}) | TODO(SV${n}) | TODO(SV${n}) | TODO(SV${n}) |`).join('\n') },
  { title: 'Phụ lục đặc tả', parts: [
    { title: 'Đặc tả SV1', files: ['dac-ta-sv1.md'], omitTests: true },
    { title: 'Đặc tả SV4', files: [{ file: 'dac-ta-sv4.md', dropIntro: true, omit: ['Rà cuối trước PR', 'Đối chiếu yêu cầu giảng viên', 'Cập nhật nghiệm thu đợt 3'], rename: { 'Bổ sung đợt 2': 'Chức năng bổ sung: bộ nhớ đệm, lọc nâng cao, nhập Excel' } }] }
  ] }
];

// TV1: đọc nguồn UTF-8 theo đường dẫn tương đối của chương báo cáo.
function readSource(file) {
  return fs.readFileSync(path.join(root, base, file), 'utf8').replace(/^﻿/, '').replace(/\r/g, '');
}

// TV1: bỏ số thứ tự đầu tiêu đề ("3.1. ") để so khớp và đánh số lại.
function stripNumber(title) {
  return title.replace(/^(?:\d+\.)+\d*\.?\s+/, '');
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
  return 'Kết quả giữ theo mốc của từng nguồn, gồm hồ sơ SV1 ngày 15/10 và kế hoạch SV4 ngày 18/10. Các lần kiểm sau được trình bày ở mục kết quả.\n\n| ID chung | Phụ trách | ID nguồn | Use case | Tình huống | Bước/dữ liệu thử | Kết quả mong đợi | Kết quả / bằng chứng |\n|---|---|---|---|---|---|---|---|\n' + rows.join('\n');
}

// TV1: tách nguồn thành phần mở đầu + các mục cấp ##, bỏ tiêu đề cấp # và dòng trích dẫn ghi chú.
function splitSections(text) {
  const sections = [{ title: null, lines: [] }];
  let fenced = false;
  for (const line of text.split('\n')) {
    if (line.startsWith('```')) fenced = !fenced;
    if (!fenced && (/^# /.test(line) || line.startsWith('>'))) continue;
    const h2 = !fenced && line.match(/^## (.+)$/);
    if (h2) sections.push({ title: stripNumber(h2[1].trim()), lines: [line] });
    else sections[sections.length - 1].lines.push(line);
  }
  return sections;
}

// TV1: lọc/đổi tên mục theo cấu hình only, omit, rename, dropIntro của một nguồn.
function selectSections(sections, spec) {
  const match = (title, list) => (list || []).some(prefix => title.startsWith(prefix));
  return sections.filter(s => {
    if (s.title === null) return !spec.dropIntro && !spec.only;
    if (spec.only && !match(s.title, spec.only)) return false;
    return !match(s.title, spec.omit);
  }).map(s => {
    const from = Object.keys(spec.rename || {}).find(prefix => s.title && s.title.startsWith(prefix));
    if (from) s.lines[0] = `## ${spec.rename[from]}`;
    return s.lines.join('\n');
  }).join('\n');
}

// TV1: thay khối mermaid bằng ảnh PNG đã xuất (đặt tên <file>.png hoặc <file>-<n>.png).
function mermaidToImages(text, file, title) {
  const name = path.basename(file, '.md');
  const total = (text.match(/^```mermaid/gm) || []).length;
  let index = 0, inMermaid = false, caption = title || name;
  const out = [];
  for (const line of text.split('\n')) {
    if (inMermaid) { if (line.startsWith('```')) inMermaid = false; continue; }
    const heading = line.match(/^#{1,6} (.+)$/);
    if (heading) caption = stripNumber(heading[1]);
    if (!line.startsWith('```mermaid')) { out.push(line); continue; }
    inMermaid = true;
    const png = total === 1 ? `${name}.png` : `${name}-${++index}.png`;
    out.push(fs.existsSync(path.join(pngDir, png))
      ? `![${caption}](../diagrams/png/${png})`
      : `![TODO: xuất sơ đồ "${caption}" (Mermaid trong ${path.basename(file)}) ra docs/diagrams/png/${png}](../diagrams/png/${png})`);
  }
  return out.join('\n');
}

// TV1: hạ cấp tiêu đề nguồn, giữ ảnh, đổi liên kết thành chữ và thay sơ đồ mermaid.
function includeSource(source, level, omitTests) {
  const spec = typeof source === 'string' ? { file: source } : source;
  const raw = readSource(spec.file);
  const title = (raw.match(/^# (.+)$/m) || [])[1];
  let text = selectSections(splitSections(raw), spec);
  if (omitTests) text = text.replace(/(?:^\|.*\n)+/gm, block => /\| TC\d+\s*\|/.test(block) ? '' : block);
  text = mermaidToImages(text, spec.file, title);
  let fenced = false;
  text = text.split('\n').map(line => {
    if (line.startsWith('```')) fenced = !fenced;
    if (fenced) return line;
    const heading = line.match(/^(#{2,6})\s+(.+)$/);
    if (heading) return '#'.repeat(Math.min(4, level + heading[1].length - 2)) + ' ' + heading[2];
    // Ảnh: quy đường dẫn về thư mục bao-cao. Liên kết thường: chỉ giữ chữ.
    return line.replace(/(!?)\[([^\]]*)\]\(([^)]+)\)/g, (all, bang, label, target) => {
      if (!bang) return /^https?:/.test(target) ? all : label;
      if (/^https?:/.test(target) || path.isAbsolute(target)) return all;
      const absolute = path.resolve(root, base, path.dirname(spec.file), target);
      return `![${label}](${path.relative(path.join(root, base), absolute).replace(/\\/g, '/')})`;
    });
  }).join('\n');
  return text.replace(/\n{3,}/g, '\n\n').trim();
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
      const title = stripNumber(heading[2]);
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
  console.log(`Đã ghép ${chapters.length} chương và 97 test case vào bao-cao-tong.md`);
}

main();
