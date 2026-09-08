(() => {
 const panel=document.querySelector('#library-chat'); if(!panel)return;
 const launch=document.querySelector('#chat-launch'), input=document.querySelector('#chat-input'), form=document.querySelector('#chat-form'), messages=document.querySelector('#chat-messages'), scroll=document.querySelector('#chat-scroll'), welcome=document.querySelector('#chat-welcome'), send=document.querySelector('#chat-send');
 let history=[], pending=false, controller=null, generation=0;
 function open(value){panel.hidden=!value;launch.hidden=value;launch.setAttribute('aria-expanded',String(value));(value?input:launch).focus();}
 launch.onclick=()=>open(true);document.querySelector('#chat-close').onclick=()=>open(false);
 panel.addEventListener('keydown',e=>{if(e.key==='Escape')open(false);});
 document.querySelector('#chat-expand').onclick=e=>{e.currentTarget.setAttribute('aria-pressed',String(panel.classList.toggle('expanded')));};
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
