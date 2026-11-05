// Loopback-only fault injection. Never deploy or add this to the production Web pipeline.
// node main-sub-test-proxy.cjs <upstream-port> <proxy-port> <control.json> <events.jsonl>
const http = require('node:http');
const fs = require('node:fs');
const [upstream, port, control, log] = process.argv.slice(2);
if (![upstream, port].every(p => /^\d+$/.test(p))) throw Error('Explicit ports required');
let sequence = 0;
const record = event => fs.appendFileSync(log, JSON.stringify({ time: new Date().toISOString(), ...event }) + '\n');
http.createServer((req, res) => {
    let settings;
    try { settings = JSON.parse(fs.readFileSync(control, 'utf8').replace(/^\uFEFF/, '')); }
    catch { res.writeHead(503); res.end('Invalid test control'); return; }
    const path = new URL(req.url, 'http://localhost').pathname;
    const id = ++sequence;
    const rule = path.startsWith('/DonHang/ChiTietDong/') ? settings.details?.[path.split('/').pop()] ?? {} : {};
    record({ id, phase: 'request', method: req.method, path, rule, scriptsBlocked: !!settings.noScripts });
    if (rule.disconnect) { req.resume(); res.destroy(); record({ id, phase: 'disconnected' }); return; }
    if (rule.status) {
        req.resume(); res.writeHead(rule.status, { 'Cache-Control': 'no-store' });
        res.end('Injected test-only HTTP error'); record({ id, phase: 'injected', status: rule.status }); return;
    }
    const outgoing = http.request({ hostname: '127.0.0.1', port: Number(upstream), path: req.url,
        method: req.method, headers: { ...req.headers, host: req.headers.host } }, incoming => {
        const chunks = [];
        incoming.on('data', chunk => chunks.push(chunk));
        incoming.on('end', () => {
            const body = Buffer.concat(chunks);
            record({ id, phase: 'upstream-ready', status: incoming.statusCode });
            setTimeout(() => {
                const headers = { ...incoming.headers };
                if (settings.noScripts) headers['content-security-policy'] = "script-src 'none'";
                res.writeHead(incoming.statusCode, headers); res.end(body);
                record({ id, phase: 'released', browserClosed: res.destroyed, delayMs: rule.delayMs ?? 0 });
            }, rule.delayMs ?? 0);
        });
    });
    outgoing.on('error', error => { record({ id, phase: 'upstream-error', code: error.code }); res.writeHead(502); res.end('Test upstream unavailable'); });
    req.pipe(outgoing);
}).listen(Number(port), '127.0.0.1', () => record({ phase: 'listening', port: Number(port), upstream: Number(upstream) }));
