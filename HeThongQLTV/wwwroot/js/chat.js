(() => {
 const panel=document.querySelector('#library-chat'); if(!panel)return;
 const launch=document.querySelector('#chat-launch'), input=document.querySelector('#chat-input'), form=document.querySelector('#chat-form'), messages=document.querySelector('#chat-messages'), scroll=document.querySelector('#chat-scroll'), welcome=document.querySelector('#chat-welcome'), send=document.querySelector('#chat-send');
 let history=[], pending=false, controller=null, generation=0;
 const dragHandle=panel.querySelector('.chat-tools');
 let position=null, drag=null;
 function place(x,y){
  const rect=panel.getBoundingClientRect(), margin=10;
  const viewport=window.visualViewport;
  const left=viewport?.offsetLeft||0, top=viewport?.offsetTop||0;
  const width=viewport?.width||window.innerWidth, height=viewport?.height||window.innerHeight;
  position={x:Math.max(left+margin,Math.min(x,left+width-rect.width-margin)),y:Math.max(top+margin,Math.min(y,top+height-rect.height-margin))};
  panel.style.left=position.x+'px';panel.style.top=position.y+'px';panel.style.right='auto';panel.style.bottom='auto';
 }
 let launchPosition=null, launchDrag=null;
 function placeLaunch(x,y){
  const rect=launch.getBoundingClientRect(), viewport=window.visualViewport;
  const left=viewport?.offsetLeft||0, top=viewport?.offsetTop||0;
  launchPosition={x:Math.max(left+10,Math.min(x,left+(viewport?.width||innerWidth)-rect.width-10)),y:Math.max(top+10,Math.min(y,top+(viewport?.height||innerHeight)-rect.height-10))};
  launch.style.left=launchPosition.x+'px';launch.style.top=launchPosition.y+'px';launch.style.right='auto';launch.style.bottom='auto';
 }
 launch.addEventListener('pointerdown',e=>{
  if(!e.isPrimary||e.button!==0)return;
  const rect=launch.getBoundingClientRect();
  launchDrag={id:e.pointerId,x:e.clientX,y:e.clientY,left:rect.left,top:rect.top,moved:false};
  launch.setPointerCapture(e.pointerId);
 });
 launch.addEventListener('pointermove',e=>{
  if(!launchDrag||launchDrag.id!==e.pointerId)return;
  const dx=e.clientX-launchDrag.x,dy=e.clientY-launchDrag.y;
  if(!launchDrag.moved&&Math.hypot(dx,dy)<6)return;
  launchDrag.moved=true;launch.classList.add('dragging');
  placeLaunch(launchDrag.left+dx,launchDrag.top+dy);
 });
 function finishLaunchDrag(e){
  if(!launchDrag||launchDrag.id!==e.pointerId)return;
  const activate=e.type==='pointerup'&&!launchDrag.moved;
  launchDrag=null;launch.classList.remove('dragging');
  if(launch.hasPointerCapture(e.pointerId))launch.releasePointerCapture(e.pointerId);
  if(activate)open(true);
 }
 for(const event of ['pointerup','pointercancel','lostpointercapture'])launch.addEventListener(event,finishLaunchDrag);
 function keepVisible(){if(!panel.hidden&&position)place(position.x,position.y);if(!launch.hidden&&launchPosition)placeLaunch(launchPosition.x,launchPosition.y);}
 dragHandle.addEventListener('pointerdown',e=>{
  if(!e.isPrimary||e.button!==0||e.target.closest('button'))return;
  const rect=panel.getBoundingClientRect();
  drag={id:e.pointerId,x:e.clientX,y:e.clientY,left:rect.left,top:rect.top};
  dragHandle.setPointerCapture(e.pointerId);panel.classList.add('dragging');
  e.preventDefault();
 });
 dragHandle.addEventListener('pointermove',e=>{
  if(drag&&e.pointerId===drag.id)place(drag.left+e.clientX-drag.x,drag.top+e.clientY-drag.y);
 });
 function finishDrag(e){
  if(!drag||e.pointerId!==drag.id)return;
  drag=null;panel.classList.remove('dragging');
  if(dragHandle.hasPointerCapture(e.pointerId))dragHandle.releasePointerCapture(e.pointerId);
 }
 for(const event of ['pointerup','pointercancel','lostpointercapture'])dragHandle.addEventListener(event,finishDrag);
 dragHandle.querySelector('.chat-drag-label').addEventListener('keydown',e=>{
  const directions={ArrowLeft:[-1,0],ArrowRight:[1,0],ArrowUp:[0,-1],ArrowDown:[0,1]};
  const direction=directions[e.key];if(!direction)return;
  e.preventDefault();const rect=panel.getBoundingClientRect(),step=e.shiftKey?40:10;
  place(rect.left+direction[0]*step,rect.top+direction[1]*step);
 });
 window.addEventListener('resize',keepVisible);
 window.visualViewport?.addEventListener('resize',keepVisible);
 window.visualViewport?.addEventListener('scroll',keepVisible);
 function open(value){panel.hidden=!value;launch.hidden=value;launch.setAttribute('aria-expanded',String(value));keepVisible();(value?input:launch).focus();}
 // Pointer activation is handled on release; clicks with detail 0 come from keyboard/assistive technology.
 launch.onclick=e=>{if(e.detail===0)open(true);};document.querySelector('#chat-close').onclick=()=>open(false);
 panel.addEventListener('keydown',e=>{if(e.key==='Escape')open(false);});
 document.querySelector('#chat-expand').onclick=e=>{e.currentTarget.setAttribute('aria-pressed',String(panel.classList.toggle('expanded')));keepVisible();};
 document.querySelector('#chat-reset').onclick=()=>{generation++;controller?.abort();history=[];messages.replaceChildren();welcome.hidden=false;pending=false;send.disabled=false;input.value='';input.focus();};
 function add(text,role){const el=document.createElement('div');el.className='chat-message '+role;el.textContent=text;messages.append(el);scroll.scrollTop=scroll.scrollHeight;return el;}
 form.onsubmit=async e=>{
  e.preventDefault();const text=input.value.trim();if(!text||pending)return;
  pending=true;send.disabled=true;welcome.hidden=true;input.value='';add(text,'user');const status=add('Lá đang tìm trong những trang sách…','assistant');const current=++generation;controller=new AbortController();const timeout=setTimeout(()=>controller?.abort(),55000);
  try{
   const response=await fetch('/Chat/Ask',{method:'POST',headers:{'Content-Type':'application/json','RequestVerificationToken':form.querySelector('[name=__RequestVerificationToken]').value},body:JSON.stringify({message:text,history}),signal:controller.signal});
   if(current!==generation)return;
   if(response.redirected)throw new Error('Phiên đăng nhập đã hết hạn. Vui lòng tải lại trang và đăng nhập.');
   if(response.status===429)throw new Error('Bạn gửi quá nhanh. Hãy chờ một phút rồi thử lại.');
   const data=await response.json();if(!response.ok)throw new Error(data.error||'Không thể gửi tin nhắn. Vui lòng thử lại.');
   status.textContent=data.answer;history.push({role:'user',text},{role:'assistant',text:data.answer.slice(0,4000)});history=history.slice(-8);
   // Only turn numeric catalog references into local links; never render model HTML.
   const parts=data.answer.split(/(#\d+)/g);status.replaceChildren();for(const part of parts){if(/^#\d+$/.test(part)){const a=document.createElement('a');a.href='/Books/Details/'+part.slice(1);a.textContent=part;status.append(a);}else status.append(document.createTextNode(part));}
  }catch(err){if(current!==generation)return;status.className='chat-message error';status.textContent=err.name==='AbortError'?'AI phản hồi quá lâu. Hãy thử lại.':err.message;input.value=text;}
  finally{clearTimeout(timeout);if(current===generation){pending=false;send.disabled=false;scroll.scrollTop=scroll.scrollHeight;input.focus();}}
 };
 input.addEventListener('keydown',e=>{if(e.key==='Enter'&&!e.shiftKey&&!e.isComposing){e.preventDefault();form.requestSubmit();}});
 document.querySelectorAll('[data-prompt]').forEach(b=>b.onclick=()=>{input.value=b.dataset.prompt;form.requestSubmit();});
})();
