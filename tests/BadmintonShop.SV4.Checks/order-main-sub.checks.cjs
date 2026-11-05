// Local DOM contract tests of the shipped script. Not browser/UI acceptance.
const { readFileSync } = require('node:fs');
const { resolve } = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const source = readFileSync(resolve(__dirname, '../../BadmintonShop.Web/wwwroot/js/order-main-sub.js'), 'utf8');
function fixture() {
  const panel = { textContent: '', innerHTML: '', attrs: {}, setAttribute(k,v) { this.attrs[k]=v; } };
  const rows = [11,12].map(id => ({dataset:{orderDetail:`/DonHang/ChiTietDong/${id}`},attrs:{},events:{},
    classList:{toggle(){}},setAttribute(k,v){this.attrs[k]=v;},addEventListener(k,f){this.events[k]=f;}}));
  const requests=[];
  vm.runInNewContext(source, { AbortController, document:{ getElementById:()=>panel,querySelectorAll:()=>rows },
    fetch:(url, options)=>new Promise((resolve,reject)=>requests.push({url,options,resolve,reject})) });
  function fire(i,kind='click',extra={}) {
    let prevented=false;
    const event={target:rows[i],preventDefault(){prevented=true;},...extra};
    rows[i].events[kind](event); return prevented;
  }
  const ok=(i,html)=>requests[i].resolve({ok:true,redirected:false,text:async()=>html});
  return {panel,rows,requests,fire,ok};
}
const tick=()=>new Promise(resolve=>setImmediate(resolve));
(async()=>{
  let passed=0;
  async function test(name,fn){await fn(); console.log('PASS '+name);passed++;}
  await test('MS01 stale success cannot overwrite latest selection even when transport ignores abort',async()=>{
    const f=fixture();f.fire(0);f.fire(1);assert.equal(f.requests[0].options.signal.aborted,true);
    f.ok(1,'ORDER12');await tick();f.ok(0,'ORDER11');await tick();
    assert.equal(f.panel.innerHTML,'ORDER12');assert.equal(f.rows[1].attrs['aria-current'],'true');
    assert.equal(f.panel.attrs['aria-busy'],'false');assert.equal(f.requests[1].options.cache,'no-store');
  });
  await test('MS02 delayed response body cannot overwrite latest selection',async()=>{
    const f=fixture();let body;f.fire(0);f.requests[0].resolve({ok:true,text:()=>new Promise(r=>body=r)});
    await tick();f.fire(1);f.ok(1,'ORDER12');await tick();body('ORDER11');await tick();assert.equal(f.panel.innerHTML,'ORDER12');
  });
  await test('MS03 stale error/finally does not clear busy or replace pending latest content',async()=>{
    const f=fixture();f.fire(0);f.fire(1);f.requests[0].reject(new Error('offline'));await tick();
    assert.equal(f.panel.attrs['aria-busy'],'true');assert.match(f.panel.textContent,/Đang tải/);
    f.ok(1,'ORDER12');await tick();assert.equal(f.panel.innerHTML,'ORDER12');
  });
  await test('MS04 network failure shows error; selecting again recovers',async()=>{
    const f=fixture();f.fire(0);f.requests[0].reject(new Error('offline'));await tick();
    assert.match(f.panel.textContent,/Không tải được/);assert.equal(f.panel.attrs['aria-busy'],'false');
    f.fire(0);f.ok(1,'RECOVERED');await tick();assert.equal(f.panel.innerHTML,'RECOVERED');
  });
  await test('MS05 HTTP errors and login redirects do not inject their bodies',async()=>{
    for(const response of [{ok:false},{ok:true,redirected:true}]){
      const f=fixture();f.fire(0);f.requests[0].resolve({...response,text:async()=>{throw new Error('body must not be read');}});
      await tick();assert.match(f.panel.textContent,/Không tải được/);
    }
  });
  await test('MS06 Space and Enter on row select; nested link key keeps native action',async()=>{
    for(const key of [' ','Enter']){const f=fixture();assert.equal(f.fire(1,'keydown',{key}),true);assert.equal(f.requests.length,1);}
    const f=fixture();assert.equal(f.fire(0,'keydown',{key:'Enter',target:{tagName:'A'}}),false);assert.equal(f.requests.length,0);
  });
  await test('MS07 modifier clicks preserve native navigation',async()=>{
    for(const modifier of ['ctrlKey','metaKey','shiftKey','altKey']){
      const f=fixture();assert.equal(f.fire(0,'click',{[modifier]:true}),false);assert.equal(f.requests.length,0);
    }
  });
  await test('MS08 page without detail panel loads safely',async()=>{
    vm.runInNewContext(source,{document:{getElementById:()=>null}});
  });
  console.log(`RESULT: ${passed}/8 local script contracts passed; no browser/network/DPI acceptance implied.`);
})().catch(error=>{console.error(error);process.exitCode=1;});
