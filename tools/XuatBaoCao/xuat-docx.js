// TV1: chuyen 1 file Markdown cua bao cao (docs/bao-cao/*.md) sang .docx de ghep vao bao cao Word.
// Ho tro: #/##/### tieu de, doan van, danh sach "- ", bang "|", khoi ma ```, hinh "![chu thich](duong-dan.png)",
// in dam **x**, in nghieng _x_, `ma`, gach chan <ins>x</ins> (khoa chinh trong luoc do quan he).
// Dong bat dau bang ">" la ghi chu cho repo, khong dua vao Word.
// Cach chay (tu thu muc goc repo):  node tools/XuatBaoCao/xuat-docx.js docs/bao-cao/chuong-kien-truc.md
const fs = require("fs");
const path = require("path");
const {
  Document, Packer, Paragraph, TextRun, HeadingLevel, Table, TableRow, TableCell, ImageRun,
  WidthType, ShadingType, AlignmentType, LevelFormat, BorderStyle, Footer, PageNumber,
} = require("docx");

const FONT = "Times New Roman";
const CO_CHU = 26;            // 13pt (don vi nua point)
const RONG_TRANG = 9070;      // A4 rong 11906 - le trai 1701 (3cm) - le phai 1134 (2cm), don vi DXA
const RONG_HINH_PX = 600;     // chieu rong toi da cua hinh (px @96dpi ~ 15.9cm)
const CAO_HINH_PX = 820;      // chieu cao toi da (de hinh dai van nam gon 1 trang)

// TV1: tach chuoi Markdown 1 dong thanh cac TextRun (bold / italic / underline / code)
function tachInline(s, kieu = {}) {
  const runs = [];
  const re = /(\*\*(.+?)\*\*|<ins>(.+?)<\/ins>|(?<![\w])_(.+?)_(?![\w])|`(.+?)`)/g;
  let viTri = 0, m;
  while ((m = re.exec(s)) !== null) {
    if (m.index > viTri) runs.push(new TextRun({ text: s.slice(viTri, m.index), ...kieu }));
    if (m[2] !== undefined) runs.push(...tachInline(m[2], { ...kieu, bold: true }));
    else if (m[3] !== undefined) runs.push(...tachInline(m[3], { ...kieu, underline: {} }));
    else if (m[4] !== undefined) runs.push(...tachInline(m[4], { ...kieu, italics: true }));
    else if (m[5] !== undefined) runs.push(new TextRun({ text: m[5], ...kieu, font: "Consolas", size: CO_CHU - 4 }));
    viTri = re.lastIndex;
  }
  if (viTri < s.length) runs.push(new TextRun({ text: s.slice(viTri), ...kieu }));
  return runs;
}

// TV1: doc kich thuoc PNG (rong, cao) tu header file
function kichThuocPng(buf) {
  return { w: buf.readUInt32BE(16), h: buf.readUInt32BE(20) };
}

// TV1: tao hinh + dong chu thich "Hinh n. ..." can giua, thu nho de vua trang
function taoHinh(chuThich, duongDan, soHinh) {
  const buf = fs.readFileSync(duongDan);
  const { w, h } = kichThuocPng(buf);
  const tiLe = Math.min(RONG_HINH_PX / w, CAO_HINH_PX / h);
  return [
    new Paragraph({
      alignment: AlignmentType.CENTER, keepNext: true, spacing: { before: 120 },
      children: [new ImageRun({ type: "png", data: buf, transformation: { width: Math.round(w * tiLe), height: Math.round(h * tiLe) },
                                altText: { title: chuThich, description: chuThich, name: path.basename(duongDan) } })],
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER, spacing: { after: 200 },
      children: [new TextRun({ text: `Hình ${soHinh}. `, bold: true, italics: true }), ...tachInline(chuThich, { italics: true })],
    }),
  ];
}

// TV1: tao bang Word tu cac dong Markdown "| a | b |" (dong 1 = tieu de, dong 2 = can le)
function taoBang(dong) {
  const tach = (l) => l.trim().replace(/^\||\|$/g, "").split("|").map((c) => c.trim());
  const tieuDe = tach(dong[0]);
  const canLe = tach(dong[1]).map((c) => (c.startsWith(":") && c.endsWith(":") ? AlignmentType.CENTER : AlignmentType.LEFT));
  const duLieu = dong.slice(2).map(tach);
  // chia do rong cot theo do dai trung binh cua noi dung (toi thieu = tu dai nhat trong cot, de khong bi ngat giua tu)
  const doDai = tieuDe.map((_, i) => {
    const o = [tieuDe, ...duLieu].map((r) => (r[i] || "").replace(/\*\*|<\/?ins>|`/g, ""));
    const trungBinh = o.reduce((a, s) => a + s.length, 0) / o.length;
    const tuDaiNhat = Math.max(...o.flatMap((s) => s.split(/\s+/).map((t) => t.length)));
    return Math.max(6, trungBinh, tuDaiNhat * 0.9);
  });
  const tong = doDai.reduce((a, b) => a + b, 0);
  const rong = doDai.map((d) => Math.floor((d / tong) * RONG_TRANG));
  rong[rong.length - 1] += RONG_TRANG - rong.reduce((a, b) => a + b, 0);
  const vien = { style: BorderStyle.SINGLE, size: 4, color: "808080" };
  const o = (text, i, laTieuDe) => new TableCell({
    width: { size: rong[i], type: WidthType.DXA },
    borders: { top: vien, bottom: vien, left: vien, right: vien },
    shading: laTieuDe ? { type: ShadingType.CLEAR, color: "auto", fill: "D9E2F3" } : undefined,
    margins: { top: 40, bottom: 40, left: 80, right: 80 },
    children: [new Paragraph({
      alignment: laTieuDe ? AlignmentType.CENTER : canLe[i], spacing: { after: 0, line: 276 },
      children: tachInline(text, laTieuDe ? { bold: true } : {}),
    })],
  });
  return new Table({
    width: { size: RONG_TRANG, type: WidthType.DXA }, columnWidths: rong,
    rows: [
      new TableRow({ tableHeader: true, children: tieuDe.map((t, i) => o(t, i, true)) }),
      ...duLieu.map((r) => new TableRow({ cantSplit: true, children: tieuDe.map((_, i) => o(r[i] || "", i, false)) })),
    ],
  });
}

// TV1: doc file Markdown va dung danh sach phan tu Word theo tung dong
function chuyenDoi(fileMd) {
  const thuMuc = path.dirname(fileMd);
  const dong = fs.readFileSync(fileMd, "utf8").replace(/\r/g, "").split("\n");
  const kq = [];
  let soHinh = 0;
  for (let i = 0; i < dong.length; i++) {
    const l = dong[i];
    if (!l.trim() || l.startsWith(">")) continue;
    let m;
    if (l.startsWith("```")) {
      // khoi ma: font Consolas, nen xam nhat, giu nguyen khoang trang dau dong
      const ma = [];
      while (++i < dong.length && !dong[i].startsWith("```")) ma.push(dong[i]);
      ma.forEach((dm, k) => kq.push(new Paragraph({
        shading: { type: ShadingType.CLEAR, color: "auto", fill: "F2F2F2" },
        spacing: { before: k === 0 ? 60 : 0, after: k === ma.length - 1 ? 160 : 0, line: 240 },
        indent: { left: 284 }, keepLines: true, keepNext: k < ma.length - 1,
        children: [new TextRun({ text: dm.length ? dm : " ", font: "Consolas", size: 19 })],
      })));
    } else if ((m = l.match(/^(#{1,3}) (.*)$/))) {
      const muc = [HeadingLevel.HEADING_1, HeadingLevel.HEADING_2, HeadingLevel.HEADING_3][m[1].length - 1];
      kq.push(new Paragraph({ heading: muc, children: tachInline(m[2]) }));
    } else if ((m = l.match(/^!\[(.*)\]\((.*)\)$/))) {
      if (m[1].startsWith("TODO:")) {
        kq.push(new Paragraph({ children: [new TextRun({ text: m[1], italics: true })] }));
      } else {
        kq.push(...taoHinh(m[1], path.resolve(thuMuc, m[2]), ++soHinh));
      }
    } else if (l.startsWith("|")) {
      const bang = [];
      while (i < dong.length && dong[i].startsWith("|")) bang.push(dong[i++]);
      i--;
      kq.push(taoBang(bang), new Paragraph({ spacing: { after: 120 }, children: [] }));
    } else if ((m = l.match(/^- (.*)$/))) {
      kq.push(new Paragraph({ numbering: { reference: "cham", level: 0 }, children: tachInline(m[1]) }));
    } else if ((m = l.match(/^(\d+)\. (.*)$/))) {
      kq.push(new Paragraph({ indent: { left: 567, hanging: 397 }, spacing: { after: 60 },
                              children: [new TextRun(`${m[1]}. `), ...tachInline(m[2])] }));
    } else {
      kq.push(new Paragraph({ alignment: AlignmentType.JUSTIFIED, indent: { firstLine: 567 }, children: tachInline(l) }));
    }
  }
  return kq;
}

// TV1: dung tai lieu Word (A4, Times New Roman 13, le chuan bao cao) va ghi ra file .docx canh file .md
async function main() {
  const fileMd = process.argv[2];
  if (!fileMd) { console.error("Cach dung: node tools/XuatBaoCao/xuat-docx.js <file.md>"); process.exit(1); }
  const doc = new Document({
    styles: {
      default: { document: { run: { font: FONT, size: CO_CHU }, paragraph: { spacing: { after: 120, line: 312 } } } },
      paragraphStyles: [
        { id: "Heading1", name: "Heading 1", basedOn: "Normal", next: "Normal", quickFormat: true,
          run: { font: FONT, size: 32, bold: true, color: "000000" },
          paragraph: { alignment: AlignmentType.CENTER, spacing: { before: 240, after: 360 }, outlineLevel: 0 } },
        { id: "Heading2", name: "Heading 2", basedOn: "Normal", next: "Normal", quickFormat: true,
          run: { font: FONT, size: 28, bold: true, color: "000000" },
          paragraph: { spacing: { before: 280, after: 120 }, outlineLevel: 1, keepNext: true } },
        { id: "Heading3", name: "Heading 3", basedOn: "Normal", next: "Normal", quickFormat: true,
          run: { font: FONT, size: 26, bold: true, italics: true, color: "000000" },
          paragraph: { spacing: { before: 200, after: 100 }, outlineLevel: 2, keepNext: true } },
      ],
    },
    numbering: { config: [{ reference: "cham", levels: [{ level: 0, format: LevelFormat.BULLET, text: "–", alignment: AlignmentType.LEFT,
                                                           style: { paragraph: { indent: { left: 567, hanging: 283 } } } }] }] },
    sections: [{
      properties: { page: { size: { width: 11906, height: 16838 },
                            margin: { top: 1134, bottom: 1134, left: 1701, right: 1134 } } },
      footers: { default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER,
                                                                  children: [new TextRun({ children: [PageNumber.CURRENT], size: 22 })] })] }) },
      children: chuyenDoi(fileMd),
    }],
  });
  const fileDocx = fileMd.replace(/\.md$/, ".docx");
  fs.writeFileSync(fileDocx, await Packer.toBuffer(doc));
  console.log("Da xuat:", fileDocx);
}

main();
