'use strict';
const http=require('node:http');
const fs=require('node:fs');
const path=require('node:path');
const crypto=require('node:crypto');
const {URL,URLSearchParams}=require('node:url');
const KEY=process.env.ADDON_KEY||'';
const PORT=Number(process.env.PORT)||7000;
if(process.env.NODE_ENV==='production'&&!KEY) throw new Error('ADDON_KEY required');
if(KEY&&!/^[\w-]{20,128}$/.test(KEY)) throw new Error('Invalid ADDON_KEY');
const BASEPATH=KEY?'/addon/'+KEY:'';
const KINDS=['movie','series','tv'];
const CATS=[
 {type:'movie',id:'my-movies',name:'🎬 أفلام | Movies',category:'movies'},
 {type:'series',id:'my-series',name:'📺 مسلسلات | Series',category:'series'},
 {type:'series',id:'my-anime',name:'🌸 أنمي | Anime',category:'anime'},
 {type:'tv',id:'my-tv',name:'📡 قنوات | Live TV',category:'tv'}
];
function readItems(){
 let all=[];
 for(const name of ['content.json','channels.json']){
  try{const a=JSON.parse(fs.readFileSync(path.join(__dirname,name),'utf8'));if(Array.isArray(a))all.push(...a);}catch(e){console.error('Media file',name,e.message);}
 }
 return all.filter(x=>x&&/^mine_[\w:-]+$/.test(x.id)&&KINDS.includes(x.type)&&typeof x.name==='string');
}
function manifest(){return {
 id:'org.personalmediahub.arabic',version:'2.1.0',name:'Personal Media Hub | مكتبتي',
 description:'Personal catalog of openly licensed movies, sample series, anime and live TV with Arabic subtitle support.',
 types:KINDS,idPrefixes:['mine_'],
 resources:[{name:'catalog',types:KINDS},{name:'meta',types:KINDS,idPrefixes:['mine_']},{name:'stream',types:KINDS,idPrefixes:['mine_']},{name:'subtitles',types:KINDS,idPrefixes:['mine_']}],
 catalogs:CATS.map(x=>({type:x.type,id:x.id,name:x.name,extra:[{name:'search',isRequired:false},{name:'skip',isRequired:false}]}))
};}
function send(res,status,obj){const data=JSON.stringify(obj);res.writeHead(status,{'Content-Type':'application/json; charset=utf-8','Access-Control-Allow-Origin':'*','Cache-Control':'no-store','Content-Length':Buffer.byteLength(data)});res.end(data);}
function asMeta(x,full=false){
 const o={id:x.id,type:x.type,name:x.name,description:x.description||''};
 if(x.poster&&/^https:\/\//.test(x.poster))o.poster=x.poster;
 if(full&&x.type==='tv')o.behaviorHints={isLive:true};
 if(full&&x.type==='series')o.videos=(x.episodes||[]).map(e=>({id:e.id,title:e.title||'Episode',season:e.season||1,episode:e.episode||1}));
 return o;
}
function locate(items,type,id){
 const direct=items.find(x=>x.type===type&&x.id===id);
 if(direct)return direct;
 if(type!=='series')return null;
 for(const x of items.filter(x=>x.type==='series')){
  const e=(x.episodes||[]).find(v=>v.id===id);
  if(e)return e;
 }
 return null;
}
function esc(s){return String(s).replace(/[&<>"']/g,ch=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[ch]));}
function htmlPage(base,items){
 const h='<h1>مكتبتي | Personal Media Hub</h1><p>أفلام ومسلسلات وأنمي وقنوات من مصادر مسموحة. روابط تشغيل تجريبية فقط، وليس مكتبة تجارية شاملة.</p>';
 const cards=CATS.map(c=>'<li>'+esc(c.name)+' — '+items.filter(x=>x.category===c.category).length+'</li>').join('');
 const adult=process.env.ENABLE_ADULT_LINK==='1'?'<p><a href="https://hanime.tv/browse" target="_blank" rel="noopener noreferrer nofollow">اختصار خارجي لـ Hanime (+18) — ليس بثًا داخل الإضافة</a></p>':'';
 return '<!doctype html><html dir="rtl" lang="ar"><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Personal Media Hub</title><style>body{background:#101b30;color:#f1f5fb;font:17px system-ui,Tahoma;max-width:820px;margin:48px auto;padding:16px;line-height:1.8}h1{font-size:2em}a{color:#79d3ff}code{background:#1e3150;padding:12px;display:block;direction:ltr;overflow-wrap:anywhere;border-radius:8px}section{background:#162844;border:1px solid #345171;padding:22px;border-radius:14px;margin:20px 0}</style><section>'+h+'</section><section><h2>الأقسام</h2><ul>'+cards+'</ul></section><section><h2>رابط Manifest</h2><code>'+esc(base)+'/manifest.json</code><p><a href="'+esc(base)+'/manifest.json">فتح Manifest</a></p></section><section><p>لتغيير المصادر عدّل content.json أو channels.json في فرع المشروع. الرابط الخاص لا يشكل نظام تسجيل دخول كاملًا.</p>'+adult+'</section></html>';
}
const server=http.createServer((req,res)=>{
 try{
  if(req.method==='OPTIONS')return send(res,200,{});
  if(req.method!=='GET')return send(res,405,{error:'Method not allowed'});
  const url=new URL(req.url,'http://localhost');
  if(url.pathname==='/health')return send(res,200,{ok:true});
  if(KEY&&!url.pathname.startsWith(BASEPATH+'/'))return send(res,404,{error:'Not found'});
  const rp=KEY?url.pathname.slice(BASEPATH.length):url.pathname;
  const origin=process.env.PUBLIC_URL||((req.headers['x-forwarded-proto']==='https'?'https':'http')+'://'+req.headers.host);
  const base=origin.replace(/\/+$/,'')+BASEPATH;
  if(rp==='/manifest.json')return send(res,200,manifest());
  const items=readItems();
  if(rp==='/'||rp==='/configure'){
   res.writeHead(200,{'Content-Type':'text/html; charset=utf-8','Cache-Control':'no-store','Referrer-Policy':'no-referrer'});
   return res.end(htmlPage(base,items));
  }
  if(rp==='/subtitles/sample-ar.vtt'){
   res.writeHead(200,{'Content-Type':'text/vtt; charset=utf-8','Access-Control-Allow-Origin':'*'});
   return res.end('WEBVTT\n\n00:00:01.000 --> 00:00:05.000\nهذه عينة تقنية وليست ترجمة الفيلم.\n');
  }
  const m=/^\/(catalog|meta|stream|subtitles)\/(movie|series|tv)\/([^/]+?)(?:\/(.*))?\.json$/.exec(rp);
  if(!m)return send(res,404,{error:'Not found'});
  const [,kind,type,id,rawExtra]=m;
  if(kind==='catalog'){
   const cat=CATS.find(c=>c.type===type&&c.id===id);
   if(!cat)return send(res,200,{metas:[]});
   const params=new URLSearchParams(rawExtra||url.search.slice(1));
   const search=(params.get('search')||'').toLowerCase().slice(0,100);
   const skip=Math.max(0,Number(params.get('skip'))||0);
   const metas=items.filter(x=>x.type===type&&x.category===cat.category).filter(x=>(x.name+' '+(x.description||'')).toLowerCase().includes(search)).slice(skip,skip+100).map(x=>asMeta(x));
   return send(res,200,{metas});
  }
  if(kind==='meta'){
   const item=items.find(x=>x.type===type&&x.id===id);
   return send(res,200,{meta:item?asMeta(item,true):null});
  }
  const item=locate(items,type,id);
  if(kind==='stream'){
   const streams=(item?.streams||[]).filter(s=>typeof s.url==='string'&&/^https?:\/\//.test(s.url)).map(s=>({name:'Personal Media Hub',title:s.title||'Stream',url:s.url,behaviorHints:{notWebReady:/\.m3u8(\?|$)/.test(s.url)}}));
   return send(res,200,{streams});
  }
  if(kind==='subtitles'){
   const subtitles=(item?.subtitles||[]).filter(s=>s.lang==='ara'&&s.file==='sample-ar.vtt').map((s,i)=>({id:'ar-'+i,lang:'ara',url:base+'/subtitles/sample-ar.vtt'}));
   return send(res,200,{subtitles});
  }
 }catch(e){console.error(e);return send(res,500,{error:'Server error'});}
});
server.listen(PORT,'0.0.0.0',()=>console.log('Personal Media Hub listening on port '+PORT));
