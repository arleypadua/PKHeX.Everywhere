(function(){"use strict";var L=typeof globalThis<"u"?globalThis:typeof window<"u"?window:typeof global<"u"?global:typeof self<"u"?self:{};function Xn(r){return r&&r.__esModule&&Object.prototype.hasOwnProperty.call(r,"default")?r.default:r}function Yn(r){if(r.__esModule)return r;var e=r.default;if(typeof e=="function"){var t=function n(){return this instanceof n?Reflect.construct(e,arguments,this.constructor):e.apply(this,arguments)};t.prototype=e.prototype}else t={};return Object.defineProperty(t,"__esModule",{value:!0}),Object.keys(r).forEach(function(n){var i=Object.getOwnPropertyDescriptor(r,n);Object.defineProperty(t,n,i.get?i:{enumerable:!0,get:function(){return r[n]}})}),t}var Rr={exports:{}};function Zn(r){throw new Error('Could not dynamically require "'+r+'". Please configure the dynamicRequireTargets or/and ignoreDynamicRequires option of @rollup/plugin-commonjs appropriately for this require call to work.')}var bt={exports:{}};const Qn=Yn(Object.freeze(Object.defineProperty({__proto__:null,default:{}},Symbol.toStringTag,{value:"Module"})));var Pr;function U(){return Pr||(Pr=1,function(r,e){(function(t,n){r.exports=n()})(L,function(){var t=t||function(n,i){var s;if(typeof window<"u"&&window.crypto&&(s=window.crypto),typeof self<"u"&&self.crypto&&(s=self.crypto),typeof globalThis<"u"&&globalThis.crypto&&(s=globalThis.crypto),!s&&typeof window<"u"&&window.msCrypto&&(s=window.msCrypto),!s&&typeof L<"u"&&L.crypto&&(s=L.crypto),!s&&typeof Zn=="function")try{s=Qn}catch{}var o=function(){if(s){if(typeof s.getRandomValues=="function")try{return s.getRandomValues(new Uint32Array(1))[0]}catch{}if(typeof s.randomBytes=="function")try{return s.randomBytes(4).readInt32LE()}catch{}}throw new Error("Native crypto module could not be used to get secure random number.")},u=Object.create||function(){function x(){}return function(h){var g;return x.prototype=h,g=new x,x.prototype=null,g}}(),c={},a=c.lib={},l=a.Base=function(){return{extend:function(x){var h=u(this);return x&&h.mixIn(x),(!h.hasOwnProperty("init")||this.init===h.init)&&(h.init=function(){h.$super.init.apply(this,arguments)}),h.init.prototype=h,h.$super=this,h},create:function(){var x=this.extend();return x.init.apply(x,arguments),x},init:function(){},mixIn:function(x){for(var h in x)x.hasOwnProperty(h)&&(this[h]=x[h]);x.hasOwnProperty("toString")&&(this.toString=x.toString)},clone:function(){return this.init.prototype.extend(this)}}}(),E=a.WordArray=l.extend({init:function(x,h){x=this.words=x||[],h!=i?this.sigBytes=h:this.sigBytes=x.length*4},toString:function(x){return(x||p).stringify(this)},concat:function(x){var h=this.words,g=x.words,b=this.sigBytes,A=x.sigBytes;if(this.clamp(),b%4)for(var B=0;B<A;B++){var D=g[B>>>2]>>>24-B%4*8&255;h[b+B>>>2]|=D<<24-(b+B)%4*8}else for(var R=0;R<A;R+=4)h[b+R>>>2]=g[R>>>2];return this.sigBytes+=A,this},clamp:function(){var x=this.words,h=this.sigBytes;x[h>>>2]&=4294967295<<32-h%4*8,x.length=n.ceil(h/4)},clone:function(){var x=l.clone.call(this);return x.words=this.words.slice(0),x},random:function(x){for(var h=[],g=0;g<x;g+=4)h.push(o());return new E.init(h,x)}}),d=c.enc={},p=d.Hex={stringify:function(x){for(var h=x.words,g=x.sigBytes,b=[],A=0;A<g;A++){var B=h[A>>>2]>>>24-A%4*8&255;b.push((B>>>4).toString(16)),b.push((B&15).toString(16))}return b.join("")},parse:function(x){for(var h=x.length,g=[],b=0;b<h;b+=2)g[b>>>3]|=parseInt(x.substr(b,2),16)<<24-b%8*4;return new E.init(g,h/2)}},f=d.Latin1={stringify:function(x){for(var h=x.words,g=x.sigBytes,b=[],A=0;A<g;A++){var B=h[A>>>2]>>>24-A%4*8&255;b.push(String.fromCharCode(B))}return b.join("")},parse:function(x){for(var h=x.length,g=[],b=0;b<h;b++)g[b>>>2]|=(x.charCodeAt(b)&255)<<24-b%4*8;return new E.init(g,h)}},_=d.Utf8={stringify:function(x){try{return decodeURIComponent(escape(f.stringify(x)))}catch{throw new Error("Malformed UTF-8 data")}},parse:function(x){return f.parse(unescape(encodeURIComponent(x)))}},v=a.BufferedBlockAlgorithm=l.extend({reset:function(){this._data=new E.init,this._nDataBytes=0},_append:function(x){typeof x=="string"&&(x=_.parse(x)),this._data.concat(x),this._nDataBytes+=x.sigBytes},_process:function(x){var h,g=this._data,b=g.words,A=g.sigBytes,B=this.blockSize,D=B*4,R=A/D;x?R=n.ceil(R):R=n.max((R|0)-this._minBufferSize,0);var m=R*B,y=n.min(m*4,A);if(m){for(var F=0;F<m;F+=B)this._doProcessBlock(b,F);h=b.splice(0,m),g.sigBytes-=y}return new E.init(h,y)},clone:function(){var x=l.clone.call(this);return x._data=this._data.clone(),x},_minBufferSize:0});a.Hasher=v.extend({cfg:l.extend(),init:function(x){this.cfg=this.cfg.extend(x),this.reset()},reset:function(){v.reset.call(this),this._doReset()},update:function(x){return this._append(x),this._process(),this},finalize:function(x){x&&this._append(x);var h=this._doFinalize();return h},blockSize:16,_createHelper:function(x){return function(h,g){return new x.init(g).finalize(h)}},_createHmacHelper:function(x){return function(h,g){return new C.HMAC.init(x,g).finalize(h)}}});var C=c.algo={};return c}(Math);return t})}(bt)),bt.exports}var Bt={exports:{}},Or;function nt(){return Or||(Or=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){return function(n){var i=t,s=i.lib,o=s.Base,u=s.WordArray,c=i.x64={};c.Word=o.extend({init:function(a,l){this.high=a,this.low=l}}),c.WordArray=o.extend({init:function(a,l){a=this.words=a||[],l!=n?this.sigBytes=l:this.sigBytes=a.length*8},toX32:function(){for(var a=this.words,l=a.length,E=[],d=0;d<l;d++){var p=a[d];E.push(p.high),E.push(p.low)}return u.create(E,this.sigBytes)},clone:function(){for(var a=o.clone.call(this),l=a.words=this.words.slice(0),E=l.length,d=0;d<E;d++)l[d]=l[d].clone();return a}})}(),t})}(Bt)),Bt.exports}var yt={exports:{}},Nr;function Jn(){return Nr||(Nr=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){return function(){if(typeof ArrayBuffer=="function"){var n=t,i=n.lib,s=i.WordArray,o=s.init,u=s.init=function(c){if(c instanceof ArrayBuffer&&(c=new Uint8Array(c)),(c instanceof Int8Array||typeof Uint8ClampedArray<"u"&&c instanceof Uint8ClampedArray||c instanceof Int16Array||c instanceof Uint16Array||c instanceof Int32Array||c instanceof Uint32Array||c instanceof Float32Array||c instanceof Float64Array)&&(c=new Uint8Array(c.buffer,c.byteOffset,c.byteLength)),c instanceof Uint8Array){for(var a=c.byteLength,l=[],E=0;E<a;E++)l[E>>>2]|=c[E]<<24-E%4*8;o.call(this,l,a)}else o.apply(this,arguments)};u.prototype=s}}(),t.lib.WordArray})}(yt)),yt.exports}var Dt={exports:{}},Lr;function ei(){return Lr||(Lr=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){return function(){var n=t,i=n.lib,s=i.WordArray,o=n.enc;o.Utf16=o.Utf16BE={stringify:function(c){for(var a=c.words,l=c.sigBytes,E=[],d=0;d<l;d+=2){var p=a[d>>>2]>>>16-d%4*8&65535;E.push(String.fromCharCode(p))}return E.join("")},parse:function(c){for(var a=c.length,l=[],E=0;E<a;E++)l[E>>>1]|=c.charCodeAt(E)<<16-E%2*16;return s.create(l,a*2)}},o.Utf16LE={stringify:function(c){for(var a=c.words,l=c.sigBytes,E=[],d=0;d<l;d+=2){var p=u(a[d>>>2]>>>16-d%4*8&65535);E.push(String.fromCharCode(p))}return E.join("")},parse:function(c){for(var a=c.length,l=[],E=0;E<a;E++)l[E>>>1]|=u(c.charCodeAt(E)<<16-E%2*16);return s.create(l,a*2)}};function u(c){return c<<8&4278255360|c>>>8&16711935}}(),t.enc.Utf16})}(Dt)),Dt.exports}var wt={exports:{}},Hr;function ye(){return Hr||(Hr=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){return function(){var n=t,i=n.lib,s=i.WordArray,o=n.enc;o.Base64={stringify:function(c){var a=c.words,l=c.sigBytes,E=this._map;c.clamp();for(var d=[],p=0;p<l;p+=3)for(var f=a[p>>>2]>>>24-p%4*8&255,_=a[p+1>>>2]>>>24-(p+1)%4*8&255,v=a[p+2>>>2]>>>24-(p+2)%4*8&255,C=f<<16|_<<8|v,x=0;x<4&&p+x*.75<l;x++)d.push(E.charAt(C>>>6*(3-x)&63));var h=E.charAt(64);if(h)for(;d.length%4;)d.push(h);return d.join("")},parse:function(c){var a=c.length,l=this._map,E=this._reverseMap;if(!E){E=this._reverseMap=[];for(var d=0;d<l.length;d++)E[l.charCodeAt(d)]=d}var p=l.charAt(64);if(p){var f=c.indexOf(p);f!==-1&&(a=f)}return u(c,a,E)},_map:"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="};function u(c,a,l){for(var E=[],d=0,p=0;p<a;p++)if(p%4){var f=l[c.charCodeAt(p-1)]<<p%4*2,_=l[c.charCodeAt(p)]>>>6-p%4*2,v=f|_;E[d>>>2]|=v<<24-d%4*8,d++}return s.create(E,d)}}(),t.enc.Base64})}(wt)),wt.exports}var Ft={exports:{}},Mr;function ti(){return Mr||(Mr=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){return function(){var n=t,i=n.lib,s=i.WordArray,o=n.enc;o.Base64url={stringify:function(c,a){a===void 0&&(a=!0);var l=c.words,E=c.sigBytes,d=a?this._safe_map:this._map;c.clamp();for(var p=[],f=0;f<E;f+=3)for(var _=l[f>>>2]>>>24-f%4*8&255,v=l[f+1>>>2]>>>24-(f+1)%4*8&255,C=l[f+2>>>2]>>>24-(f+2)%4*8&255,x=_<<16|v<<8|C,h=0;h<4&&f+h*.75<E;h++)p.push(d.charAt(x>>>6*(3-h)&63));var g=d.charAt(64);if(g)for(;p.length%4;)p.push(g);return p.join("")},parse:function(c,a){a===void 0&&(a=!0);var l=c.length,E=a?this._safe_map:this._map,d=this._reverseMap;if(!d){d=this._reverseMap=[];for(var p=0;p<E.length;p++)d[E.charCodeAt(p)]=p}var f=E.charAt(64);if(f){var _=c.indexOf(f);_!==-1&&(l=_)}return u(c,l,d)},_map:"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=",_safe_map:"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"};function u(c,a,l){for(var E=[],d=0,p=0;p<a;p++)if(p%4){var f=l[c.charCodeAt(p-1)]<<p%4*2,_=l[c.charCodeAt(p)]>>>6-p%4*2,v=f|_;E[d>>>2]|=v<<24-d%4*8,d++}return s.create(E,d)}}(),t.enc.Base64url})}(Ft)),Ft.exports}var It={exports:{}},Ur;function De(){return Ur||(Ur=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){return function(n){var i=t,s=i.lib,o=s.WordArray,u=s.Hasher,c=i.algo,a=[];(function(){for(var _=0;_<64;_++)a[_]=n.abs(n.sin(_+1))*4294967296|0})();var l=c.MD5=u.extend({_doReset:function(){this._hash=new o.init([1732584193,4023233417,2562383102,271733878])},_doProcessBlock:function(_,v){for(var C=0;C<16;C++){var x=v+C,h=_[x];_[x]=(h<<8|h>>>24)&16711935|(h<<24|h>>>8)&4278255360}var g=this._hash.words,b=_[v+0],A=_[v+1],B=_[v+2],D=_[v+3],R=_[v+4],m=_[v+5],y=_[v+6],F=_[v+7],I=_[v+8],P=_[v+9],O=_[v+10],H=_[v+11],j=_[v+12],z=_[v+13],q=_[v+14],W=_[v+15],w=g[0],S=g[1],T=g[2],k=g[3];w=E(w,S,T,k,b,7,a[0]),k=E(k,w,S,T,A,12,a[1]),T=E(T,k,w,S,B,17,a[2]),S=E(S,T,k,w,D,22,a[3]),w=E(w,S,T,k,R,7,a[4]),k=E(k,w,S,T,m,12,a[5]),T=E(T,k,w,S,y,17,a[6]),S=E(S,T,k,w,F,22,a[7]),w=E(w,S,T,k,I,7,a[8]),k=E(k,w,S,T,P,12,a[9]),T=E(T,k,w,S,O,17,a[10]),S=E(S,T,k,w,H,22,a[11]),w=E(w,S,T,k,j,7,a[12]),k=E(k,w,S,T,z,12,a[13]),T=E(T,k,w,S,q,17,a[14]),S=E(S,T,k,w,W,22,a[15]),w=d(w,S,T,k,A,5,a[16]),k=d(k,w,S,T,y,9,a[17]),T=d(T,k,w,S,H,14,a[18]),S=d(S,T,k,w,b,20,a[19]),w=d(w,S,T,k,m,5,a[20]),k=d(k,w,S,T,O,9,a[21]),T=d(T,k,w,S,W,14,a[22]),S=d(S,T,k,w,R,20,a[23]),w=d(w,S,T,k,P,5,a[24]),k=d(k,w,S,T,q,9,a[25]),T=d(T,k,w,S,D,14,a[26]),S=d(S,T,k,w,I,20,a[27]),w=d(w,S,T,k,z,5,a[28]),k=d(k,w,S,T,B,9,a[29]),T=d(T,k,w,S,F,14,a[30]),S=d(S,T,k,w,j,20,a[31]),w=p(w,S,T,k,m,4,a[32]),k=p(k,w,S,T,I,11,a[33]),T=p(T,k,w,S,H,16,a[34]),S=p(S,T,k,w,q,23,a[35]),w=p(w,S,T,k,A,4,a[36]),k=p(k,w,S,T,R,11,a[37]),T=p(T,k,w,S,F,16,a[38]),S=p(S,T,k,w,O,23,a[39]),w=p(w,S,T,k,z,4,a[40]),k=p(k,w,S,T,b,11,a[41]),T=p(T,k,w,S,D,16,a[42]),S=p(S,T,k,w,y,23,a[43]),w=p(w,S,T,k,P,4,a[44]),k=p(k,w,S,T,j,11,a[45]),T=p(T,k,w,S,W,16,a[46]),S=p(S,T,k,w,B,23,a[47]),w=f(w,S,T,k,b,6,a[48]),k=f(k,w,S,T,F,10,a[49]),T=f(T,k,w,S,q,15,a[50]),S=f(S,T,k,w,m,21,a[51]),w=f(w,S,T,k,j,6,a[52]),k=f(k,w,S,T,D,10,a[53]),T=f(T,k,w,S,O,15,a[54]),S=f(S,T,k,w,A,21,a[55]),w=f(w,S,T,k,I,6,a[56]),k=f(k,w,S,T,W,10,a[57]),T=f(T,k,w,S,y,15,a[58]),S=f(S,T,k,w,z,21,a[59]),w=f(w,S,T,k,R,6,a[60]),k=f(k,w,S,T,H,10,a[61]),T=f(T,k,w,S,B,15,a[62]),S=f(S,T,k,w,P,21,a[63]),g[0]=g[0]+w|0,g[1]=g[1]+S|0,g[2]=g[2]+T|0,g[3]=g[3]+k|0},_doFinalize:function(){var _=this._data,v=_.words,C=this._nDataBytes*8,x=_.sigBytes*8;v[x>>>5]|=128<<24-x%32;var h=n.floor(C/4294967296),g=C;v[(x+64>>>9<<4)+15]=(h<<8|h>>>24)&16711935|(h<<24|h>>>8)&4278255360,v[(x+64>>>9<<4)+14]=(g<<8|g>>>24)&16711935|(g<<24|g>>>8)&4278255360,_.sigBytes=(v.length+1)*4,this._process();for(var b=this._hash,A=b.words,B=0;B<4;B++){var D=A[B];A[B]=(D<<8|D>>>24)&16711935|(D<<24|D>>>8)&4278255360}return b},clone:function(){var _=u.clone.call(this);return _._hash=this._hash.clone(),_}});function E(_,v,C,x,h,g,b){var A=_+(v&C|~v&x)+h+b;return(A<<g|A>>>32-g)+v}function d(_,v,C,x,h,g,b){var A=_+(v&x|C&~x)+h+b;return(A<<g|A>>>32-g)+v}function p(_,v,C,x,h,g,b){var A=_+(v^C^x)+h+b;return(A<<g|A>>>32-g)+v}function f(_,v,C,x,h,g,b){var A=_+(C^(v|~x))+h+b;return(A<<g|A>>>32-g)+v}i.MD5=u._createHelper(l),i.HmacMD5=u._createHmacHelper(l)}(Math),t.MD5})}(It)),It.exports}var kt={exports:{}},zr;function Wr(){return zr||(zr=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){return function(){var n=t,i=n.lib,s=i.WordArray,o=i.Hasher,u=n.algo,c=[],a=u.SHA1=o.extend({_doReset:function(){this._hash=new s.init([1732584193,4023233417,2562383102,271733878,3285377520])},_doProcessBlock:function(l,E){for(var d=this._hash.words,p=d[0],f=d[1],_=d[2],v=d[3],C=d[4],x=0;x<80;x++){if(x<16)c[x]=l[E+x]|0;else{var h=c[x-3]^c[x-8]^c[x-14]^c[x-16];c[x]=h<<1|h>>>31}var g=(p<<5|p>>>27)+C+c[x];x<20?g+=(f&_|~f&v)+1518500249:x<40?g+=(f^_^v)+1859775393:x<60?g+=(f&_|f&v|_&v)-1894007588:g+=(f^_^v)-899497514,C=v,v=_,_=f<<30|f>>>2,f=p,p=g}d[0]=d[0]+p|0,d[1]=d[1]+f|0,d[2]=d[2]+_|0,d[3]=d[3]+v|0,d[4]=d[4]+C|0},_doFinalize:function(){var l=this._data,E=l.words,d=this._nDataBytes*8,p=l.sigBytes*8;return E[p>>>5]|=128<<24-p%32,E[(p+64>>>9<<4)+14]=Math.floor(d/4294967296),E[(p+64>>>9<<4)+15]=d,l.sigBytes=E.length*4,this._process(),this._hash},clone:function(){var l=o.clone.call(this);return l._hash=this._hash.clone(),l}});n.SHA1=o._createHelper(a),n.HmacSHA1=o._createHmacHelper(a)}(),t.SHA1})}(kt)),kt.exports}var St={exports:{}},$r;function Tt(){return $r||($r=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){return function(n){var i=t,s=i.lib,o=s.WordArray,u=s.Hasher,c=i.algo,a=[],l=[];(function(){function p(C){for(var x=n.sqrt(C),h=2;h<=x;h++)if(!(C%h))return!1;return!0}function f(C){return(C-(C|0))*4294967296|0}for(var _=2,v=0;v<64;)p(_)&&(v<8&&(a[v]=f(n.pow(_,1/2))),l[v]=f(n.pow(_,1/3)),v++),_++})();var E=[],d=c.SHA256=u.extend({_doReset:function(){this._hash=new o.init(a.slice(0))},_doProcessBlock:function(p,f){for(var _=this._hash.words,v=_[0],C=_[1],x=_[2],h=_[3],g=_[4],b=_[5],A=_[6],B=_[7],D=0;D<64;D++){if(D<16)E[D]=p[f+D]|0;else{var R=E[D-15],m=(R<<25|R>>>7)^(R<<14|R>>>18)^R>>>3,y=E[D-2],F=(y<<15|y>>>17)^(y<<13|y>>>19)^y>>>10;E[D]=m+E[D-7]+F+E[D-16]}var I=g&b^~g&A,P=v&C^v&x^C&x,O=(v<<30|v>>>2)^(v<<19|v>>>13)^(v<<10|v>>>22),H=(g<<26|g>>>6)^(g<<21|g>>>11)^(g<<7|g>>>25),j=B+H+I+l[D]+E[D],z=O+P;B=A,A=b,b=g,g=h+j|0,h=x,x=C,C=v,v=j+z|0}_[0]=_[0]+v|0,_[1]=_[1]+C|0,_[2]=_[2]+x|0,_[3]=_[3]+h|0,_[4]=_[4]+g|0,_[5]=_[5]+b|0,_[6]=_[6]+A|0,_[7]=_[7]+B|0},_doFinalize:function(){var p=this._data,f=p.words,_=this._nDataBytes*8,v=p.sigBytes*8;return f[v>>>5]|=128<<24-v%32,f[(v+64>>>9<<4)+14]=n.floor(_/4294967296),f[(v+64>>>9<<4)+15]=_,p.sigBytes=f.length*4,this._process(),this._hash},clone:function(){var p=u.clone.call(this);return p._hash=this._hash.clone(),p}});i.SHA256=u._createHelper(d),i.HmacSHA256=u._createHmacHelper(d)}(Math),t.SHA256})}(St)),St.exports}var Rt={exports:{}},qr;function ri(){return qr||(qr=1,function(r,e){(function(t,n,i){r.exports=n(U(),Tt())})(L,function(t){return function(){var n=t,i=n.lib,s=i.WordArray,o=n.algo,u=o.SHA256,c=o.SHA224=u.extend({_doReset:function(){this._hash=new s.init([3238371032,914150663,812702999,4144912697,4290775857,1750603025,1694076839,3204075428])},_doFinalize:function(){var a=u._doFinalize.call(this);return a.sigBytes-=4,a}});n.SHA224=u._createHelper(c),n.HmacSHA224=u._createHmacHelper(c)}(),t.SHA224})}(Rt)),Rt.exports}var Pt={exports:{}},Vr;function jr(){return Vr||(Vr=1,function(r,e){(function(t,n,i){r.exports=n(U(),nt())})(L,function(t){return function(){var n=t,i=n.lib,s=i.Hasher,o=n.x64,u=o.Word,c=o.WordArray,a=n.algo;function l(){return u.create.apply(u,arguments)}var E=[l(1116352408,3609767458),l(1899447441,602891725),l(3049323471,3964484399),l(3921009573,2173295548),l(961987163,4081628472),l(1508970993,3053834265),l(2453635748,2937671579),l(2870763221,3664609560),l(3624381080,2734883394),l(310598401,1164996542),l(607225278,1323610764),l(1426881987,3590304994),l(1925078388,4068182383),l(2162078206,991336113),l(2614888103,633803317),l(3248222580,3479774868),l(3835390401,2666613458),l(4022224774,944711139),l(264347078,2341262773),l(604807628,2007800933),l(770255983,1495990901),l(1249150122,1856431235),l(1555081692,3175218132),l(1996064986,2198950837),l(2554220882,3999719339),l(2821834349,766784016),l(2952996808,2566594879),l(3210313671,3203337956),l(3336571891,1034457026),l(3584528711,2466948901),l(113926993,3758326383),l(338241895,168717936),l(666307205,1188179964),l(773529912,1546045734),l(1294757372,1522805485),l(1396182291,2643833823),l(1695183700,2343527390),l(1986661051,1014477480),l(2177026350,1206759142),l(2456956037,344077627),l(2730485921,1290863460),l(2820302411,3158454273),l(3259730800,3505952657),l(3345764771,106217008),l(3516065817,3606008344),l(3600352804,1432725776),l(4094571909,1467031594),l(275423344,851169720),l(430227734,3100823752),l(506948616,1363258195),l(659060556,3750685593),l(883997877,3785050280),l(958139571,3318307427),l(1322822218,3812723403),l(1537002063,2003034995),l(1747873779,3602036899),l(1955562222,1575990012),l(2024104815,1125592928),l(2227730452,2716904306),l(2361852424,442776044),l(2428436474,593698344),l(2756734187,3733110249),l(3204031479,2999351573),l(3329325298,3815920427),l(3391569614,3928383900),l(3515267271,566280711),l(3940187606,3454069534),l(4118630271,4000239992),l(116418474,1914138554),l(174292421,2731055270),l(289380356,3203993006),l(460393269,320620315),l(685471733,587496836),l(852142971,1086792851),l(1017036298,365543100),l(1126000580,2618297676),l(1288033470,3409855158),l(1501505948,4234509866),l(1607167915,987167468),l(1816402316,1246189591)],d=[];(function(){for(var f=0;f<80;f++)d[f]=l()})();var p=a.SHA512=s.extend({_doReset:function(){this._hash=new c.init([new u.init(1779033703,4089235720),new u.init(3144134277,2227873595),new u.init(1013904242,4271175723),new u.init(2773480762,1595750129),new u.init(1359893119,2917565137),new u.init(2600822924,725511199),new u.init(528734635,4215389547),new u.init(1541459225,327033209)])},_doProcessBlock:function(f,_){for(var v=this._hash.words,C=v[0],x=v[1],h=v[2],g=v[3],b=v[4],A=v[5],B=v[6],D=v[7],R=C.high,m=C.low,y=x.high,F=x.low,I=h.high,P=h.low,O=g.high,H=g.low,j=b.high,z=b.low,q=A.high,W=A.low,w=B.high,S=B.low,T=D.high,k=D.low,G=R,V=m,Z=y,M=F,Ye=I,Le=P,Sr=O,Ze=H,te=j,Q=z,mt=q,Qe=W,Ct=w,Je=S,Tr=T,et=k,re=0;re<80;re++){var ee,be,At=d[re];if(re<16)be=At.high=f[_+re*2]|0,ee=At.low=f[_+re*2+1]|0;else{var Ln=d[re-15],He=Ln.high,tt=Ln.low,dc=(He>>>1|tt<<31)^(He>>>8|tt<<24)^He>>>7,Hn=(tt>>>1|He<<31)^(tt>>>8|He<<24)^(tt>>>7|He<<25),Mn=d[re-2],Me=Mn.high,rt=Mn.low,xc=(Me>>>19|rt<<13)^(Me<<3|rt>>>29)^Me>>>6,Un=(rt>>>19|Me<<13)^(rt<<3|Me>>>29)^(rt>>>6|Me<<26),zn=d[re-7],hc=zn.high,fc=zn.low,Wn=d[re-16],pc=Wn.high,$n=Wn.low;ee=Hn+fc,be=dc+hc+(ee>>>0<Hn>>>0?1:0),ee=ee+Un,be=be+xc+(ee>>>0<Un>>>0?1:0),ee=ee+$n,be=be+pc+(ee>>>0<$n>>>0?1:0),At.high=be,At.low=ee}var vc=te&mt^~te&Ct,qn=Q&Qe^~Q&Je,gc=G&Z^G&Ye^Z&Ye,_c=V&M^V&Le^M&Le,Ec=(G>>>28|V<<4)^(G<<30|V>>>2)^(G<<25|V>>>7),Vn=(V>>>28|G<<4)^(V<<30|G>>>2)^(V<<25|G>>>7),mc=(te>>>14|Q<<18)^(te>>>18|Q<<14)^(te<<23|Q>>>9),Cc=(Q>>>14|te<<18)^(Q>>>18|te<<14)^(Q<<23|te>>>9),jn=E[re],Ac=jn.high,Gn=jn.low,J=et+Cc,Be=Tr+mc+(J>>>0<et>>>0?1:0),J=J+qn,Be=Be+vc+(J>>>0<qn>>>0?1:0),J=J+Gn,Be=Be+Ac+(J>>>0<Gn>>>0?1:0),J=J+ee,Be=Be+be+(J>>>0<ee>>>0?1:0),Kn=Vn+_c,bc=Ec+gc+(Kn>>>0<Vn>>>0?1:0);Tr=Ct,et=Je,Ct=mt,Je=Qe,mt=te,Qe=Q,Q=Ze+J|0,te=Sr+Be+(Q>>>0<Ze>>>0?1:0)|0,Sr=Ye,Ze=Le,Ye=Z,Le=M,Z=G,M=V,V=J+Kn|0,G=Be+bc+(V>>>0<J>>>0?1:0)|0}m=C.low=m+V,C.high=R+G+(m>>>0<V>>>0?1:0),F=x.low=F+M,x.high=y+Z+(F>>>0<M>>>0?1:0),P=h.low=P+Le,h.high=I+Ye+(P>>>0<Le>>>0?1:0),H=g.low=H+Ze,g.high=O+Sr+(H>>>0<Ze>>>0?1:0),z=b.low=z+Q,b.high=j+te+(z>>>0<Q>>>0?1:0),W=A.low=W+Qe,A.high=q+mt+(W>>>0<Qe>>>0?1:0),S=B.low=S+Je,B.high=w+Ct+(S>>>0<Je>>>0?1:0),k=D.low=k+et,D.high=T+Tr+(k>>>0<et>>>0?1:0)},_doFinalize:function(){var f=this._data,_=f.words,v=this._nDataBytes*8,C=f.sigBytes*8;_[C>>>5]|=128<<24-C%32,_[(C+128>>>10<<5)+30]=Math.floor(v/4294967296),_[(C+128>>>10<<5)+31]=v,f.sigBytes=_.length*4,this._process();var x=this._hash.toX32();return x},clone:function(){var f=s.clone.call(this);return f._hash=this._hash.clone(),f},blockSize:1024/32});n.SHA512=s._createHelper(p),n.HmacSHA512=s._createHmacHelper(p)}(),t.SHA512})}(Pt)),Pt.exports}var Ot={exports:{}},Gr;function ni(){return Gr||(Gr=1,function(r,e){(function(t,n,i){r.exports=n(U(),nt(),jr())})(L,function(t){return function(){var n=t,i=n.x64,s=i.Word,o=i.WordArray,u=n.algo,c=u.SHA512,a=u.SHA384=c.extend({_doReset:function(){this._hash=new o.init([new s.init(3418070365,3238371032),new s.init(1654270250,914150663),new s.init(2438529370,812702999),new s.init(355462360,4144912697),new s.init(1731405415,4290775857),new s.init(2394180231,1750603025),new s.init(3675008525,1694076839),new s.init(1203062813,3204075428)])},_doFinalize:function(){var l=c._doFinalize.call(this);return l.sigBytes-=16,l}});n.SHA384=c._createHelper(a),n.HmacSHA384=c._createHmacHelper(a)}(),t.SHA384})}(Ot)),Ot.exports}var Nt={exports:{}},Kr;function ii(){return Kr||(Kr=1,function(r,e){(function(t,n,i){r.exports=n(U(),nt())})(L,function(t){return function(n){var i=t,s=i.lib,o=s.WordArray,u=s.Hasher,c=i.x64,a=c.Word,l=i.algo,E=[],d=[],p=[];(function(){for(var v=1,C=0,x=0;x<24;x++){E[v+5*C]=(x+1)*(x+2)/2%64;var h=C%5,g=(2*v+3*C)%5;v=h,C=g}for(var v=0;v<5;v++)for(var C=0;C<5;C++)d[v+5*C]=C+(2*v+3*C)%5*5;for(var b=1,A=0;A<24;A++){for(var B=0,D=0,R=0;R<7;R++){if(b&1){var m=(1<<R)-1;m<32?D^=1<<m:B^=1<<m-32}b&128?b=b<<1^113:b<<=1}p[A]=a.create(B,D)}})();var f=[];(function(){for(var v=0;v<25;v++)f[v]=a.create()})();var _=l.SHA3=u.extend({cfg:u.cfg.extend({outputLength:512}),_doReset:function(){for(var v=this._state=[],C=0;C<25;C++)v[C]=new a.init;this.blockSize=(1600-2*this.cfg.outputLength)/32},_doProcessBlock:function(v,C){for(var x=this._state,h=this.blockSize/2,g=0;g<h;g++){var b=v[C+2*g],A=v[C+2*g+1];b=(b<<8|b>>>24)&16711935|(b<<24|b>>>8)&4278255360,A=(A<<8|A>>>24)&16711935|(A<<24|A>>>8)&4278255360;var B=x[g];B.high^=A,B.low^=b}for(var D=0;D<24;D++){for(var R=0;R<5;R++){for(var m=0,y=0,F=0;F<5;F++){var B=x[R+5*F];m^=B.high,y^=B.low}var I=f[R];I.high=m,I.low=y}for(var R=0;R<5;R++)for(var P=f[(R+4)%5],O=f[(R+1)%5],H=O.high,j=O.low,m=P.high^(H<<1|j>>>31),y=P.low^(j<<1|H>>>31),F=0;F<5;F++){var B=x[R+5*F];B.high^=m,B.low^=y}for(var z=1;z<25;z++){var m,y,B=x[z],q=B.high,W=B.low,w=E[z];w<32?(m=q<<w|W>>>32-w,y=W<<w|q>>>32-w):(m=W<<w-32|q>>>64-w,y=q<<w-32|W>>>64-w);var S=f[d[z]];S.high=m,S.low=y}var T=f[0],k=x[0];T.high=k.high,T.low=k.low;for(var R=0;R<5;R++)for(var F=0;F<5;F++){var z=R+5*F,B=x[z],G=f[z],V=f[(R+1)%5+5*F],Z=f[(R+2)%5+5*F];B.high=G.high^~V.high&Z.high,B.low=G.low^~V.low&Z.low}var B=x[0],M=p[D];B.high^=M.high,B.low^=M.low}},_doFinalize:function(){var v=this._data,C=v.words;this._nDataBytes*8;var x=v.sigBytes*8,h=this.blockSize*32;C[x>>>5]|=1<<24-x%32,C[(n.ceil((x+1)/h)*h>>>5)-1]|=128,v.sigBytes=C.length*4,this._process();for(var g=this._state,b=this.cfg.outputLength/8,A=b/8,B=[],D=0;D<A;D++){var R=g[D],m=R.high,y=R.low;m=(m<<8|m>>>24)&16711935|(m<<24|m>>>8)&4278255360,y=(y<<8|y>>>24)&16711935|(y<<24|y>>>8)&4278255360,B.push(y),B.push(m)}return new o.init(B,b)},clone:function(){for(var v=u.clone.call(this),C=v._state=this._state.slice(0),x=0;x<25;x++)C[x]=C[x].clone();return v}});i.SHA3=u._createHelper(_),i.HmacSHA3=u._createHmacHelper(_)}(Math),t.SHA3})}(Nt)),Nt.exports}var Lt={exports:{}},Xr;function si(){return Xr||(Xr=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){/** @preserve
				(c) 2012 by Cédric Mesnil. All rights reserved.

				Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

				    - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
				    - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

				THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
				*/return function(n){var i=t,s=i.lib,o=s.WordArray,u=s.Hasher,c=i.algo,a=o.create([0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,7,4,13,1,10,6,15,3,12,0,9,5,2,14,11,8,3,10,14,4,9,15,8,1,2,7,0,6,13,11,5,12,1,9,11,10,0,8,12,4,13,3,7,15,14,5,6,2,4,0,5,9,7,12,2,10,14,1,3,8,11,6,15,13]),l=o.create([5,14,7,0,9,2,11,4,13,6,15,8,1,10,3,12,6,11,3,7,0,13,5,10,14,15,8,12,4,9,1,2,15,5,1,3,7,14,6,9,11,8,12,2,10,0,4,13,8,6,4,1,3,11,15,0,5,12,2,13,9,7,10,14,12,15,10,4,1,5,8,7,6,2,13,14,0,3,9,11]),E=o.create([11,14,15,12,5,8,7,9,11,13,14,15,6,7,9,8,7,6,8,13,11,9,7,15,7,12,15,9,11,7,13,12,11,13,6,7,14,9,13,15,14,8,13,6,5,12,7,5,11,12,14,15,14,15,9,8,9,14,5,6,8,6,5,12,9,15,5,11,6,8,13,12,5,12,13,14,11,8,5,6]),d=o.create([8,9,9,11,13,15,15,5,7,7,8,11,14,14,12,6,9,13,15,7,12,8,9,11,7,7,12,7,6,15,13,11,9,7,15,11,8,6,6,14,12,13,5,14,13,13,7,5,15,5,8,11,14,14,6,14,6,9,12,9,12,5,15,8,8,5,12,9,12,5,14,6,8,13,6,5,15,13,11,11]),p=o.create([0,1518500249,1859775393,2400959708,2840853838]),f=o.create([1352829926,1548603684,1836072691,2053994217,0]),_=c.RIPEMD160=u.extend({_doReset:function(){this._hash=o.create([1732584193,4023233417,2562383102,271733878,3285377520])},_doProcessBlock:function(A,B){for(var D=0;D<16;D++){var R=B+D,m=A[R];A[R]=(m<<8|m>>>24)&16711935|(m<<24|m>>>8)&4278255360}var y=this._hash.words,F=p.words,I=f.words,P=a.words,O=l.words,H=E.words,j=d.words,z,q,W,w,S,T,k,G,V,Z;T=z=y[0],k=q=y[1],G=W=y[2],V=w=y[3],Z=S=y[4];for(var M,D=0;D<80;D+=1)M=z+A[B+P[D]]|0,D<16?M+=v(q,W,w)+F[0]:D<32?M+=C(q,W,w)+F[1]:D<48?M+=x(q,W,w)+F[2]:D<64?M+=h(q,W,w)+F[3]:M+=g(q,W,w)+F[4],M=M|0,M=b(M,H[D]),M=M+S|0,z=S,S=w,w=b(W,10),W=q,q=M,M=T+A[B+O[D]]|0,D<16?M+=g(k,G,V)+I[0]:D<32?M+=h(k,G,V)+I[1]:D<48?M+=x(k,G,V)+I[2]:D<64?M+=C(k,G,V)+I[3]:M+=v(k,G,V)+I[4],M=M|0,M=b(M,j[D]),M=M+Z|0,T=Z,Z=V,V=b(G,10),G=k,k=M;M=y[1]+W+V|0,y[1]=y[2]+w+Z|0,y[2]=y[3]+S+T|0,y[3]=y[4]+z+k|0,y[4]=y[0]+q+G|0,y[0]=M},_doFinalize:function(){var A=this._data,B=A.words,D=this._nDataBytes*8,R=A.sigBytes*8;B[R>>>5]|=128<<24-R%32,B[(R+64>>>9<<4)+14]=(D<<8|D>>>24)&16711935|(D<<24|D>>>8)&4278255360,A.sigBytes=(B.length+1)*4,this._process();for(var m=this._hash,y=m.words,F=0;F<5;F++){var I=y[F];y[F]=(I<<8|I>>>24)&16711935|(I<<24|I>>>8)&4278255360}return m},clone:function(){var A=u.clone.call(this);return A._hash=this._hash.clone(),A}});function v(A,B,D){return A^B^D}function C(A,B,D){return A&B|~A&D}function x(A,B,D){return(A|~B)^D}function h(A,B,D){return A&D|B&~D}function g(A,B,D){return A^(B|~D)}function b(A,B){return A<<B|A>>>32-B}i.RIPEMD160=u._createHelper(_),i.HmacRIPEMD160=u._createHmacHelper(_)}(),t.RIPEMD160})}(Lt)),Lt.exports}var Ht={exports:{}},Yr;function Mt(){return Yr||(Yr=1,function(r,e){(function(t,n){r.exports=n(U())})(L,function(t){(function(){var n=t,i=n.lib,s=i.Base,o=n.enc,u=o.Utf8,c=n.algo;c.HMAC=s.extend({init:function(a,l){a=this._hasher=new a.init,typeof l=="string"&&(l=u.parse(l));var E=a.blockSize,d=E*4;l.sigBytes>d&&(l=a.finalize(l)),l.clamp();for(var p=this._oKey=l.clone(),f=this._iKey=l.clone(),_=p.words,v=f.words,C=0;C<E;C++)_[C]^=1549556828,v[C]^=909522486;p.sigBytes=f.sigBytes=d,this.reset()},reset:function(){var a=this._hasher;a.reset(),a.update(this._iKey)},update:function(a){return this._hasher.update(a),this},finalize:function(a){var l=this._hasher,E=l.finalize(a);l.reset();var d=l.finalize(this._oKey.clone().concat(E));return d}})})()})}(Ht)),Ht.exports}var Ut={exports:{}},Zr;function ai(){return Zr||(Zr=1,function(r,e){(function(t,n,i){r.exports=n(U(),Tt(),Mt())})(L,function(t){return function(){var n=t,i=n.lib,s=i.Base,o=i.WordArray,u=n.algo,c=u.SHA256,a=u.HMAC,l=u.PBKDF2=s.extend({cfg:s.extend({keySize:128/32,hasher:c,iterations:25e4}),init:function(E){this.cfg=this.cfg.extend(E)},compute:function(E,d){for(var p=this.cfg,f=a.create(p.hasher,E),_=o.create(),v=o.create([1]),C=_.words,x=v.words,h=p.keySize,g=p.iterations;C.length<h;){var b=f.update(d).finalize(v);f.reset();for(var A=b.words,B=A.length,D=b,R=1;R<g;R++){D=f.finalize(D),f.reset();for(var m=D.words,y=0;y<B;y++)A[y]^=m[y]}_.concat(b),x[0]++}return _.sigBytes=h*4,_}});n.PBKDF2=function(E,d,p){return l.create(p).compute(E,d)}}(),t.PBKDF2})}(Ut)),Ut.exports}var zt={exports:{}},Qr;function xe(){return Qr||(Qr=1,function(r,e){(function(t,n,i){r.exports=n(U(),Wr(),Mt())})(L,function(t){return function(){var n=t,i=n.lib,s=i.Base,o=i.WordArray,u=n.algo,c=u.MD5,a=u.EvpKDF=s.extend({cfg:s.extend({keySize:128/32,hasher:c,iterations:1}),init:function(l){this.cfg=this.cfg.extend(l)},compute:function(l,E){for(var d,p=this.cfg,f=p.hasher.create(),_=o.create(),v=_.words,C=p.keySize,x=p.iterations;v.length<C;){d&&f.update(d),d=f.update(l).finalize(E),f.reset();for(var h=1;h<x;h++)d=f.finalize(d),f.reset();_.concat(d)}return _.sigBytes=C*4,_}});n.EvpKDF=function(l,E,d){return a.create(d).compute(l,E)}}(),t.EvpKDF})}(zt)),zt.exports}var Wt={exports:{}},Jr;function X(){return Jr||(Jr=1,function(r,e){(function(t,n,i){r.exports=n(U(),xe())})(L,function(t){t.lib.Cipher||function(n){var i=t,s=i.lib,o=s.Base,u=s.WordArray,c=s.BufferedBlockAlgorithm,a=i.enc;a.Utf8;var l=a.Base64,E=i.algo,d=E.EvpKDF,p=s.Cipher=c.extend({cfg:o.extend(),createEncryptor:function(m,y){return this.create(this._ENC_XFORM_MODE,m,y)},createDecryptor:function(m,y){return this.create(this._DEC_XFORM_MODE,m,y)},init:function(m,y,F){this.cfg=this.cfg.extend(F),this._xformMode=m,this._key=y,this.reset()},reset:function(){c.reset.call(this),this._doReset()},process:function(m){return this._append(m),this._process()},finalize:function(m){m&&this._append(m);var y=this._doFinalize();return y},keySize:128/32,ivSize:128/32,_ENC_XFORM_MODE:1,_DEC_XFORM_MODE:2,_createHelper:function(){function m(y){return typeof y=="string"?R:A}return function(y){return{encrypt:function(F,I,P){return m(I).encrypt(y,F,I,P)},decrypt:function(F,I,P){return m(I).decrypt(y,F,I,P)}}}}()});s.StreamCipher=p.extend({_doFinalize:function(){var m=this._process(!0);return m},blockSize:1});var f=i.mode={},_=s.BlockCipherMode=o.extend({createEncryptor:function(m,y){return this.Encryptor.create(m,y)},createDecryptor:function(m,y){return this.Decryptor.create(m,y)},init:function(m,y){this._cipher=m,this._iv=y}}),v=f.CBC=function(){var m=_.extend();m.Encryptor=m.extend({processBlock:function(F,I){var P=this._cipher,O=P.blockSize;y.call(this,F,I,O),P.encryptBlock(F,I),this._prevBlock=F.slice(I,I+O)}}),m.Decryptor=m.extend({processBlock:function(F,I){var P=this._cipher,O=P.blockSize,H=F.slice(I,I+O);P.decryptBlock(F,I),y.call(this,F,I,O),this._prevBlock=H}});function y(F,I,P){var O,H=this._iv;H?(O=H,this._iv=n):O=this._prevBlock;for(var j=0;j<P;j++)F[I+j]^=O[j]}return m}(),C=i.pad={},x=C.Pkcs7={pad:function(m,y){for(var F=y*4,I=F-m.sigBytes%F,P=I<<24|I<<16|I<<8|I,O=[],H=0;H<I;H+=4)O.push(P);var j=u.create(O,I);m.concat(j)},unpad:function(m){var y=m.words[m.sigBytes-1>>>2]&255;m.sigBytes-=y}};s.BlockCipher=p.extend({cfg:p.cfg.extend({mode:v,padding:x}),reset:function(){var m;p.reset.call(this);var y=this.cfg,F=y.iv,I=y.mode;this._xformMode==this._ENC_XFORM_MODE?m=I.createEncryptor:(m=I.createDecryptor,this._minBufferSize=1),this._mode&&this._mode.__creator==m?this._mode.init(this,F&&F.words):(this._mode=m.call(I,this,F&&F.words),this._mode.__creator=m)},_doProcessBlock:function(m,y){this._mode.processBlock(m,y)},_doFinalize:function(){var m,y=this.cfg.padding;return this._xformMode==this._ENC_XFORM_MODE?(y.pad(this._data,this.blockSize),m=this._process(!0)):(m=this._process(!0),y.unpad(m)),m},blockSize:128/32});var h=s.CipherParams=o.extend({init:function(m){this.mixIn(m)},toString:function(m){return(m||this.formatter).stringify(this)}}),g=i.format={},b=g.OpenSSL={stringify:function(m){var y,F=m.ciphertext,I=m.salt;return I?y=u.create([1398893684,1701076831]).concat(I).concat(F):y=F,y.toString(l)},parse:function(m){var y,F=l.parse(m),I=F.words;return I[0]==1398893684&&I[1]==1701076831&&(y=u.create(I.slice(2,4)),I.splice(0,4),F.sigBytes-=16),h.create({ciphertext:F,salt:y})}},A=s.SerializableCipher=o.extend({cfg:o.extend({format:b}),encrypt:function(m,y,F,I){I=this.cfg.extend(I);var P=m.createEncryptor(F,I),O=P.finalize(y),H=P.cfg;return h.create({ciphertext:O,key:F,iv:H.iv,algorithm:m,mode:H.mode,padding:H.padding,blockSize:m.blockSize,formatter:I.format})},decrypt:function(m,y,F,I){I=this.cfg.extend(I),y=this._parse(y,I.format);var P=m.createDecryptor(F,I).finalize(y.ciphertext);return P},_parse:function(m,y){return typeof m=="string"?y.parse(m,this):m}}),B=i.kdf={},D=B.OpenSSL={execute:function(m,y,F,I,P){if(I||(I=u.random(64/8)),P)var O=d.create({keySize:y+F,hasher:P}).compute(m,I);else var O=d.create({keySize:y+F}).compute(m,I);var H=u.create(O.words.slice(y),F*4);return O.sigBytes=y*4,h.create({key:O,iv:H,salt:I})}},R=s.PasswordBasedCipher=A.extend({cfg:A.cfg.extend({kdf:D}),encrypt:function(m,y,F,I){I=this.cfg.extend(I);var P=I.kdf.execute(F,m.keySize,m.ivSize,I.salt,I.hasher);I.iv=P.iv;var O=A.encrypt.call(this,m,y,P.key,I);return O.mixIn(P),O},decrypt:function(m,y,F,I){I=this.cfg.extend(I),y=this._parse(y,I.format);var P=I.kdf.execute(F,m.keySize,m.ivSize,y.salt,I.hasher);I.iv=P.iv;var O=A.decrypt.call(this,m,y,P.key,I);return O}})}()})}(Wt)),Wt.exports}var $t={exports:{}},e0;function oi(){return e0||(e0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return t.mode.CFB=function(){var n=t.lib.BlockCipherMode.extend();n.Encryptor=n.extend({processBlock:function(s,o){var u=this._cipher,c=u.blockSize;i.call(this,s,o,c,u),this._prevBlock=s.slice(o,o+c)}}),n.Decryptor=n.extend({processBlock:function(s,o){var u=this._cipher,c=u.blockSize,a=s.slice(o,o+c);i.call(this,s,o,c,u),this._prevBlock=a}});function i(s,o,u,c){var a,l=this._iv;l?(a=l.slice(0),this._iv=void 0):a=this._prevBlock,c.encryptBlock(a,0);for(var E=0;E<u;E++)s[o+E]^=a[E]}return n}(),t.mode.CFB})}($t)),$t.exports}var qt={exports:{}},t0;function ci(){return t0||(t0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return t.mode.CTR=function(){var n=t.lib.BlockCipherMode.extend(),i=n.Encryptor=n.extend({processBlock:function(s,o){var u=this._cipher,c=u.blockSize,a=this._iv,l=this._counter;a&&(l=this._counter=a.slice(0),this._iv=void 0);var E=l.slice(0);u.encryptBlock(E,0),l[c-1]=l[c-1]+1|0;for(var d=0;d<c;d++)s[o+d]^=E[d]}});return n.Decryptor=i,n}(),t.mode.CTR})}(qt)),qt.exports}var Vt={exports:{}},r0;function li(){return r0||(r0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){/** @preserve
 * Counter block mode compatible with  Dr Brian Gladman fileenc.c
 * derived from CryptoJS.mode.CTR
 * Jan Hruby jhruby.web@gmail.com
 */return t.mode.CTRGladman=function(){var n=t.lib.BlockCipherMode.extend();function i(u){if((u>>24&255)===255){var c=u>>16&255,a=u>>8&255,l=u&255;c===255?(c=0,a===255?(a=0,l===255?l=0:++l):++a):++c,u=0,u+=c<<16,u+=a<<8,u+=l}else u+=1<<24;return u}function s(u){return(u[0]=i(u[0]))===0&&(u[1]=i(u[1])),u}var o=n.Encryptor=n.extend({processBlock:function(u,c){var a=this._cipher,l=a.blockSize,E=this._iv,d=this._counter;E&&(d=this._counter=E.slice(0),this._iv=void 0),s(d);var p=d.slice(0);a.encryptBlock(p,0);for(var f=0;f<l;f++)u[c+f]^=p[f]}});return n.Decryptor=o,n}(),t.mode.CTRGladman})}(Vt)),Vt.exports}var jt={exports:{}},n0;function ui(){return n0||(n0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return t.mode.OFB=function(){var n=t.lib.BlockCipherMode.extend(),i=n.Encryptor=n.extend({processBlock:function(s,o){var u=this._cipher,c=u.blockSize,a=this._iv,l=this._keystream;a&&(l=this._keystream=a.slice(0),this._iv=void 0),u.encryptBlock(l,0);for(var E=0;E<c;E++)s[o+E]^=l[E]}});return n.Decryptor=i,n}(),t.mode.OFB})}(jt)),jt.exports}var Gt={exports:{}},i0;function di(){return i0||(i0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return t.mode.ECB=function(){var n=t.lib.BlockCipherMode.extend();return n.Encryptor=n.extend({processBlock:function(i,s){this._cipher.encryptBlock(i,s)}}),n.Decryptor=n.extend({processBlock:function(i,s){this._cipher.decryptBlock(i,s)}}),n}(),t.mode.ECB})}(Gt)),Gt.exports}var Kt={exports:{}},s0;function xi(){return s0||(s0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return t.pad.AnsiX923={pad:function(n,i){var s=n.sigBytes,o=i*4,u=o-s%o,c=s+u-1;n.clamp(),n.words[c>>>2]|=u<<24-c%4*8,n.sigBytes+=u},unpad:function(n){var i=n.words[n.sigBytes-1>>>2]&255;n.sigBytes-=i}},t.pad.Ansix923})}(Kt)),Kt.exports}var Xt={exports:{}},a0;function hi(){return a0||(a0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return t.pad.Iso10126={pad:function(n,i){var s=i*4,o=s-n.sigBytes%s;n.concat(t.lib.WordArray.random(o-1)).concat(t.lib.WordArray.create([o<<24],1))},unpad:function(n){var i=n.words[n.sigBytes-1>>>2]&255;n.sigBytes-=i}},t.pad.Iso10126})}(Xt)),Xt.exports}var Yt={exports:{}},o0;function fi(){return o0||(o0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return t.pad.Iso97971={pad:function(n,i){n.concat(t.lib.WordArray.create([2147483648],1)),t.pad.ZeroPadding.pad(n,i)},unpad:function(n){t.pad.ZeroPadding.unpad(n),n.sigBytes--}},t.pad.Iso97971})}(Yt)),Yt.exports}var Zt={exports:{}},c0;function pi(){return c0||(c0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return t.pad.ZeroPadding={pad:function(n,i){var s=i*4;n.clamp(),n.sigBytes+=s-(n.sigBytes%s||s)},unpad:function(n){for(var i=n.words,s=n.sigBytes-1,s=n.sigBytes-1;s>=0;s--)if(i[s>>>2]>>>24-s%4*8&255){n.sigBytes=s+1;break}}},t.pad.ZeroPadding})}(Zt)),Zt.exports}var Qt={exports:{}},l0;function vi(){return l0||(l0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return t.pad.NoPadding={pad:function(){},unpad:function(){}},t.pad.NoPadding})}(Qt)),Qt.exports}var Jt={exports:{}},u0;function gi(){return u0||(u0=1,function(r,e){(function(t,n,i){r.exports=n(U(),X())})(L,function(t){return function(n){var i=t,s=i.lib,o=s.CipherParams,u=i.enc,c=u.Hex,a=i.format;a.Hex={stringify:function(l){return l.ciphertext.toString(c)},parse:function(l){var E=c.parse(l);return o.create({ciphertext:E})}}}(),t.format.Hex})}(Jt)),Jt.exports}var er={exports:{}},d0;function _i(){return d0||(d0=1,function(r,e){(function(t,n,i){r.exports=n(U(),ye(),De(),xe(),X())})(L,function(t){return function(){var n=t,i=n.lib,s=i.BlockCipher,o=n.algo,u=[],c=[],a=[],l=[],E=[],d=[],p=[],f=[],_=[],v=[];(function(){for(var h=[],g=0;g<256;g++)g<128?h[g]=g<<1:h[g]=g<<1^283;for(var b=0,A=0,g=0;g<256;g++){var B=A^A<<1^A<<2^A<<3^A<<4;B=B>>>8^B&255^99,u[b]=B,c[B]=b;var D=h[b],R=h[D],m=h[R],y=h[B]*257^B*16843008;a[b]=y<<24|y>>>8,l[b]=y<<16|y>>>16,E[b]=y<<8|y>>>24,d[b]=y;var y=m*16843009^R*65537^D*257^b*16843008;p[B]=y<<24|y>>>8,f[B]=y<<16|y>>>16,_[B]=y<<8|y>>>24,v[B]=y,b?(b=D^h[h[h[m^D]]],A^=h[h[A]]):b=A=1}})();var C=[0,1,2,4,8,16,32,64,128,27,54],x=o.AES=s.extend({_doReset:function(){var h;if(!(this._nRounds&&this._keyPriorReset===this._key)){for(var g=this._keyPriorReset=this._key,b=g.words,A=g.sigBytes/4,B=this._nRounds=A+6,D=(B+1)*4,R=this._keySchedule=[],m=0;m<D;m++)m<A?R[m]=b[m]:(h=R[m-1],m%A?A>6&&m%A==4&&(h=u[h>>>24]<<24|u[h>>>16&255]<<16|u[h>>>8&255]<<8|u[h&255]):(h=h<<8|h>>>24,h=u[h>>>24]<<24|u[h>>>16&255]<<16|u[h>>>8&255]<<8|u[h&255],h^=C[m/A|0]<<24),R[m]=R[m-A]^h);for(var y=this._invKeySchedule=[],F=0;F<D;F++){var m=D-F;if(F%4)var h=R[m];else var h=R[m-4];F<4||m<=4?y[F]=h:y[F]=p[u[h>>>24]]^f[u[h>>>16&255]]^_[u[h>>>8&255]]^v[u[h&255]]}}},encryptBlock:function(h,g){this._doCryptBlock(h,g,this._keySchedule,a,l,E,d,u)},decryptBlock:function(h,g){var b=h[g+1];h[g+1]=h[g+3],h[g+3]=b,this._doCryptBlock(h,g,this._invKeySchedule,p,f,_,v,c);var b=h[g+1];h[g+1]=h[g+3],h[g+3]=b},_doCryptBlock:function(h,g,b,A,B,D,R,m){for(var y=this._nRounds,F=h[g]^b[0],I=h[g+1]^b[1],P=h[g+2]^b[2],O=h[g+3]^b[3],H=4,j=1;j<y;j++){var z=A[F>>>24]^B[I>>>16&255]^D[P>>>8&255]^R[O&255]^b[H++],q=A[I>>>24]^B[P>>>16&255]^D[O>>>8&255]^R[F&255]^b[H++],W=A[P>>>24]^B[O>>>16&255]^D[F>>>8&255]^R[I&255]^b[H++],w=A[O>>>24]^B[F>>>16&255]^D[I>>>8&255]^R[P&255]^b[H++];F=z,I=q,P=W,O=w}var z=(m[F>>>24]<<24|m[I>>>16&255]<<16|m[P>>>8&255]<<8|m[O&255])^b[H++],q=(m[I>>>24]<<24|m[P>>>16&255]<<16|m[O>>>8&255]<<8|m[F&255])^b[H++],W=(m[P>>>24]<<24|m[O>>>16&255]<<16|m[F>>>8&255]<<8|m[I&255])^b[H++],w=(m[O>>>24]<<24|m[F>>>16&255]<<16|m[I>>>8&255]<<8|m[P&255])^b[H++];h[g]=z,h[g+1]=q,h[g+2]=W,h[g+3]=w},keySize:256/32});n.AES=s._createHelper(x)}(),t.AES})}(er)),er.exports}var tr={exports:{}},x0;function Ei(){return x0||(x0=1,function(r,e){(function(t,n,i){r.exports=n(U(),ye(),De(),xe(),X())})(L,function(t){return function(){var n=t,i=n.lib,s=i.WordArray,o=i.BlockCipher,u=n.algo,c=[57,49,41,33,25,17,9,1,58,50,42,34,26,18,10,2,59,51,43,35,27,19,11,3,60,52,44,36,63,55,47,39,31,23,15,7,62,54,46,38,30,22,14,6,61,53,45,37,29,21,13,5,28,20,12,4],a=[14,17,11,24,1,5,3,28,15,6,21,10,23,19,12,4,26,8,16,7,27,20,13,2,41,52,31,37,47,55,30,40,51,45,33,48,44,49,39,56,34,53,46,42,50,36,29,32],l=[1,2,4,6,8,10,12,14,15,17,19,21,23,25,27,28],E=[{0:8421888,268435456:32768,536870912:8421378,805306368:2,1073741824:512,1342177280:8421890,1610612736:8389122,1879048192:8388608,2147483648:514,2415919104:8389120,2684354560:33280,2952790016:8421376,3221225472:32770,3489660928:8388610,3758096384:0,4026531840:33282,134217728:0,402653184:8421890,671088640:33282,939524096:32768,1207959552:8421888,1476395008:512,1744830464:8421378,2013265920:2,2281701376:8389120,2550136832:33280,2818572288:8421376,3087007744:8389122,3355443200:8388610,3623878656:32770,3892314112:514,4160749568:8388608,1:32768,268435457:2,536870913:8421888,805306369:8388608,1073741825:8421378,1342177281:33280,1610612737:512,1879048193:8389122,2147483649:8421890,2415919105:8421376,2684354561:8388610,2952790017:33282,3221225473:514,3489660929:8389120,3758096385:32770,4026531841:0,134217729:8421890,402653185:8421376,671088641:8388608,939524097:512,1207959553:32768,1476395009:8388610,1744830465:2,2013265921:33282,2281701377:32770,2550136833:8389122,2818572289:514,3087007745:8421888,3355443201:8389120,3623878657:0,3892314113:33280,4160749569:8421378},{0:1074282512,16777216:16384,33554432:524288,50331648:1074266128,67108864:1073741840,83886080:1074282496,100663296:1073758208,117440512:16,134217728:540672,150994944:1073758224,167772160:1073741824,184549376:540688,201326592:524304,218103808:0,234881024:16400,251658240:1074266112,8388608:1073758208,25165824:540688,41943040:16,58720256:1073758224,75497472:1074282512,92274688:1073741824,109051904:524288,125829120:1074266128,142606336:524304,159383552:0,176160768:16384,192937984:1074266112,209715200:1073741840,226492416:540672,243269632:1074282496,260046848:16400,268435456:0,285212672:1074266128,301989888:1073758224,318767104:1074282496,335544320:1074266112,352321536:16,369098752:540688,385875968:16384,402653184:16400,419430400:524288,436207616:524304,452984832:1073741840,469762048:540672,486539264:1073758208,503316480:1073741824,520093696:1074282512,276824064:540688,293601280:524288,310378496:1074266112,327155712:16384,343932928:1073758208,360710144:1074282512,377487360:16,394264576:1073741824,411041792:1074282496,427819008:1073741840,444596224:1073758224,461373440:524304,478150656:0,494927872:16400,511705088:1074266128,528482304:540672},{0:260,1048576:0,2097152:67109120,3145728:65796,4194304:65540,5242880:67108868,6291456:67174660,7340032:67174400,8388608:67108864,9437184:67174656,10485760:65792,11534336:67174404,12582912:67109124,13631488:65536,14680064:4,15728640:256,524288:67174656,1572864:67174404,2621440:0,3670016:67109120,4718592:67108868,5767168:65536,6815744:65540,7864320:260,8912896:4,9961472:256,11010048:67174400,12058624:65796,13107200:65792,14155776:67109124,15204352:67174660,16252928:67108864,16777216:67174656,17825792:65540,18874368:65536,19922944:67109120,20971520:256,22020096:67174660,23068672:67108868,24117248:0,25165824:67109124,26214400:67108864,27262976:4,28311552:65792,29360128:67174400,30408704:260,31457280:65796,32505856:67174404,17301504:67108864,18350080:260,19398656:67174656,20447232:0,21495808:65540,22544384:67109120,23592960:256,24641536:67174404,25690112:65536,26738688:67174660,27787264:65796,28835840:67108868,29884416:67109124,30932992:67174400,31981568:4,33030144:65792},{0:2151682048,65536:2147487808,131072:4198464,196608:2151677952,262144:0,327680:4198400,393216:2147483712,458752:4194368,524288:2147483648,589824:4194304,655360:64,720896:2147487744,786432:2151678016,851968:4160,917504:4096,983040:2151682112,32768:2147487808,98304:64,163840:2151678016,229376:2147487744,294912:4198400,360448:2151682112,425984:0,491520:2151677952,557056:4096,622592:2151682048,688128:4194304,753664:4160,819200:2147483648,884736:4194368,950272:4198464,1015808:2147483712,1048576:4194368,1114112:4198400,1179648:2147483712,1245184:0,1310720:4160,1376256:2151678016,1441792:2151682048,1507328:2147487808,1572864:2151682112,1638400:2147483648,1703936:2151677952,1769472:4198464,1835008:2147487744,1900544:4194304,1966080:64,2031616:4096,1081344:2151677952,1146880:2151682112,1212416:0,1277952:4198400,1343488:4194368,1409024:2147483648,1474560:2147487808,1540096:64,1605632:2147483712,1671168:4096,1736704:2147487744,1802240:2151678016,1867776:4160,1933312:2151682048,1998848:4194304,2064384:4198464},{0:128,4096:17039360,8192:262144,12288:536870912,16384:537133184,20480:16777344,24576:553648256,28672:262272,32768:16777216,36864:537133056,40960:536871040,45056:553910400,49152:553910272,53248:0,57344:17039488,61440:553648128,2048:17039488,6144:553648256,10240:128,14336:17039360,18432:262144,22528:537133184,26624:553910272,30720:536870912,34816:537133056,38912:0,43008:553910400,47104:16777344,51200:536871040,55296:553648128,59392:16777216,63488:262272,65536:262144,69632:128,73728:536870912,77824:553648256,81920:16777344,86016:553910272,90112:537133184,94208:16777216,98304:553910400,102400:553648128,106496:17039360,110592:537133056,114688:262272,118784:536871040,122880:0,126976:17039488,67584:553648256,71680:16777216,75776:17039360,79872:537133184,83968:536870912,88064:17039488,92160:128,96256:553910272,100352:262272,104448:553910400,108544:0,112640:553648128,116736:16777344,120832:262144,124928:537133056,129024:536871040},{0:268435464,256:8192,512:270532608,768:270540808,1024:268443648,1280:2097152,1536:2097160,1792:268435456,2048:0,2304:268443656,2560:2105344,2816:8,3072:270532616,3328:2105352,3584:8200,3840:270540800,128:270532608,384:270540808,640:8,896:2097152,1152:2105352,1408:268435464,1664:268443648,1920:8200,2176:2097160,2432:8192,2688:268443656,2944:270532616,3200:0,3456:270540800,3712:2105344,3968:268435456,4096:268443648,4352:270532616,4608:270540808,4864:8200,5120:2097152,5376:268435456,5632:268435464,5888:2105344,6144:2105352,6400:0,6656:8,6912:270532608,7168:8192,7424:268443656,7680:270540800,7936:2097160,4224:8,4480:2105344,4736:2097152,4992:268435464,5248:268443648,5504:8200,5760:270540808,6016:270532608,6272:270540800,6528:270532616,6784:8192,7040:2105352,7296:2097160,7552:0,7808:268435456,8064:268443656},{0:1048576,16:33555457,32:1024,48:1049601,64:34604033,80:0,96:1,112:34603009,128:33555456,144:1048577,160:33554433,176:34604032,192:34603008,208:1025,224:1049600,240:33554432,8:34603009,24:0,40:33555457,56:34604032,72:1048576,88:33554433,104:33554432,120:1025,136:1049601,152:33555456,168:34603008,184:1048577,200:1024,216:34604033,232:1,248:1049600,256:33554432,272:1048576,288:33555457,304:34603009,320:1048577,336:33555456,352:34604032,368:1049601,384:1025,400:34604033,416:1049600,432:1,448:0,464:34603008,480:33554433,496:1024,264:1049600,280:33555457,296:34603009,312:1,328:33554432,344:1048576,360:1025,376:34604032,392:33554433,408:34603008,424:0,440:34604033,456:1049601,472:1024,488:33555456,504:1048577},{0:134219808,1:131072,2:134217728,3:32,4:131104,5:134350880,6:134350848,7:2048,8:134348800,9:134219776,10:133120,11:134348832,12:2080,13:0,14:134217760,15:133152,2147483648:2048,2147483649:134350880,2147483650:134219808,2147483651:134217728,2147483652:134348800,2147483653:133120,2147483654:133152,2147483655:32,2147483656:134217760,2147483657:2080,2147483658:131104,2147483659:134350848,2147483660:0,2147483661:134348832,2147483662:134219776,2147483663:131072,16:133152,17:134350848,18:32,19:2048,20:134219776,21:134217760,22:134348832,23:131072,24:0,25:131104,26:134348800,27:134219808,28:134350880,29:133120,30:2080,31:134217728,2147483664:131072,2147483665:2048,2147483666:134348832,2147483667:133152,2147483668:32,2147483669:134348800,2147483670:134217728,2147483671:134219808,2147483672:134350880,2147483673:134217760,2147483674:134219776,2147483675:0,2147483676:133120,2147483677:2080,2147483678:131104,2147483679:134350848}],d=[4160749569,528482304,33030144,2064384,129024,8064,504,2147483679],p=u.DES=o.extend({_doReset:function(){for(var C=this._key,x=C.words,h=[],g=0;g<56;g++){var b=c[g]-1;h[g]=x[b>>>5]>>>31-b%32&1}for(var A=this._subKeys=[],B=0;B<16;B++){for(var D=A[B]=[],R=l[B],g=0;g<24;g++)D[g/6|0]|=h[(a[g]-1+R)%28]<<31-g%6,D[4+(g/6|0)]|=h[28+(a[g+24]-1+R)%28]<<31-g%6;D[0]=D[0]<<1|D[0]>>>31;for(var g=1;g<7;g++)D[g]=D[g]>>>(g-1)*4+3;D[7]=D[7]<<5|D[7]>>>27}for(var m=this._invSubKeys=[],g=0;g<16;g++)m[g]=A[15-g]},encryptBlock:function(C,x){this._doCryptBlock(C,x,this._subKeys)},decryptBlock:function(C,x){this._doCryptBlock(C,x,this._invSubKeys)},_doCryptBlock:function(C,x,h){this._lBlock=C[x],this._rBlock=C[x+1],f.call(this,4,252645135),f.call(this,16,65535),_.call(this,2,858993459),_.call(this,8,16711935),f.call(this,1,1431655765);for(var g=0;g<16;g++){for(var b=h[g],A=this._lBlock,B=this._rBlock,D=0,R=0;R<8;R++)D|=E[R][((B^b[R])&d[R])>>>0];this._lBlock=B,this._rBlock=A^D}var m=this._lBlock;this._lBlock=this._rBlock,this._rBlock=m,f.call(this,1,1431655765),_.call(this,8,16711935),_.call(this,2,858993459),f.call(this,16,65535),f.call(this,4,252645135),C[x]=this._lBlock,C[x+1]=this._rBlock},keySize:64/32,ivSize:64/32,blockSize:64/32});function f(C,x){var h=(this._lBlock>>>C^this._rBlock)&x;this._rBlock^=h,this._lBlock^=h<<C}function _(C,x){var h=(this._rBlock>>>C^this._lBlock)&x;this._lBlock^=h,this._rBlock^=h<<C}n.DES=o._createHelper(p);var v=u.TripleDES=o.extend({_doReset:function(){var C=this._key,x=C.words;if(x.length!==2&&x.length!==4&&x.length<6)throw new Error("Invalid key length - 3DES requires the key length to be 64, 128, 192 or >192.");var h=x.slice(0,2),g=x.length<4?x.slice(0,2):x.slice(2,4),b=x.length<6?x.slice(0,2):x.slice(4,6);this._des1=p.createEncryptor(s.create(h)),this._des2=p.createEncryptor(s.create(g)),this._des3=p.createEncryptor(s.create(b))},encryptBlock:function(C,x){this._des1.encryptBlock(C,x),this._des2.decryptBlock(C,x),this._des3.encryptBlock(C,x)},decryptBlock:function(C,x){this._des3.decryptBlock(C,x),this._des2.encryptBlock(C,x),this._des1.decryptBlock(C,x)},keySize:192/32,ivSize:64/32,blockSize:64/32});n.TripleDES=o._createHelper(v)}(),t.TripleDES})}(tr)),tr.exports}var rr={exports:{}},h0;function mi(){return h0||(h0=1,function(r,e){(function(t,n,i){r.exports=n(U(),ye(),De(),xe(),X())})(L,function(t){return function(){var n=t,i=n.lib,s=i.StreamCipher,o=n.algo,u=o.RC4=s.extend({_doReset:function(){for(var l=this._key,E=l.words,d=l.sigBytes,p=this._S=[],f=0;f<256;f++)p[f]=f;for(var f=0,_=0;f<256;f++){var v=f%d,C=E[v>>>2]>>>24-v%4*8&255;_=(_+p[f]+C)%256;var x=p[f];p[f]=p[_],p[_]=x}this._i=this._j=0},_doProcessBlock:function(l,E){l[E]^=c.call(this)},keySize:256/32,ivSize:0});function c(){for(var l=this._S,E=this._i,d=this._j,p=0,f=0;f<4;f++){E=(E+1)%256,d=(d+l[E])%256;var _=l[E];l[E]=l[d],l[d]=_,p|=l[(l[E]+l[d])%256]<<24-f*8}return this._i=E,this._j=d,p}n.RC4=s._createHelper(u);var a=o.RC4Drop=u.extend({cfg:u.cfg.extend({drop:192}),_doReset:function(){u._doReset.call(this);for(var l=this.cfg.drop;l>0;l--)c.call(this)}});n.RC4Drop=s._createHelper(a)}(),t.RC4})}(rr)),rr.exports}var nr={exports:{}},f0;function Ci(){return f0||(f0=1,function(r,e){(function(t,n,i){r.exports=n(U(),ye(),De(),xe(),X())})(L,function(t){return function(){var n=t,i=n.lib,s=i.StreamCipher,o=n.algo,u=[],c=[],a=[],l=o.Rabbit=s.extend({_doReset:function(){for(var d=this._key.words,p=this.cfg.iv,f=0;f<4;f++)d[f]=(d[f]<<8|d[f]>>>24)&16711935|(d[f]<<24|d[f]>>>8)&4278255360;var _=this._X=[d[0],d[3]<<16|d[2]>>>16,d[1],d[0]<<16|d[3]>>>16,d[2],d[1]<<16|d[0]>>>16,d[3],d[2]<<16|d[1]>>>16],v=this._C=[d[2]<<16|d[2]>>>16,d[0]&4294901760|d[1]&65535,d[3]<<16|d[3]>>>16,d[1]&4294901760|d[2]&65535,d[0]<<16|d[0]>>>16,d[2]&4294901760|d[3]&65535,d[1]<<16|d[1]>>>16,d[3]&4294901760|d[0]&65535];this._b=0;for(var f=0;f<4;f++)E.call(this);for(var f=0;f<8;f++)v[f]^=_[f+4&7];if(p){var C=p.words,x=C[0],h=C[1],g=(x<<8|x>>>24)&16711935|(x<<24|x>>>8)&4278255360,b=(h<<8|h>>>24)&16711935|(h<<24|h>>>8)&4278255360,A=g>>>16|b&4294901760,B=b<<16|g&65535;v[0]^=g,v[1]^=A,v[2]^=b,v[3]^=B,v[4]^=g,v[5]^=A,v[6]^=b,v[7]^=B;for(var f=0;f<4;f++)E.call(this)}},_doProcessBlock:function(d,p){var f=this._X;E.call(this),u[0]=f[0]^f[5]>>>16^f[3]<<16,u[1]=f[2]^f[7]>>>16^f[5]<<16,u[2]=f[4]^f[1]>>>16^f[7]<<16,u[3]=f[6]^f[3]>>>16^f[1]<<16;for(var _=0;_<4;_++)u[_]=(u[_]<<8|u[_]>>>24)&16711935|(u[_]<<24|u[_]>>>8)&4278255360,d[p+_]^=u[_]},blockSize:128/32,ivSize:64/32});function E(){for(var d=this._X,p=this._C,f=0;f<8;f++)c[f]=p[f];p[0]=p[0]+1295307597+this._b|0,p[1]=p[1]+3545052371+(p[0]>>>0<c[0]>>>0?1:0)|0,p[2]=p[2]+886263092+(p[1]>>>0<c[1]>>>0?1:0)|0,p[3]=p[3]+1295307597+(p[2]>>>0<c[2]>>>0?1:0)|0,p[4]=p[4]+3545052371+(p[3]>>>0<c[3]>>>0?1:0)|0,p[5]=p[5]+886263092+(p[4]>>>0<c[4]>>>0?1:0)|0,p[6]=p[6]+1295307597+(p[5]>>>0<c[5]>>>0?1:0)|0,p[7]=p[7]+3545052371+(p[6]>>>0<c[6]>>>0?1:0)|0,this._b=p[7]>>>0<c[7]>>>0?1:0;for(var f=0;f<8;f++){var _=d[f]+p[f],v=_&65535,C=_>>>16,x=((v*v>>>17)+v*C>>>15)+C*C,h=((_&4294901760)*_|0)+((_&65535)*_|0);a[f]=x^h}d[0]=a[0]+(a[7]<<16|a[7]>>>16)+(a[6]<<16|a[6]>>>16)|0,d[1]=a[1]+(a[0]<<8|a[0]>>>24)+a[7]|0,d[2]=a[2]+(a[1]<<16|a[1]>>>16)+(a[0]<<16|a[0]>>>16)|0,d[3]=a[3]+(a[2]<<8|a[2]>>>24)+a[1]|0,d[4]=a[4]+(a[3]<<16|a[3]>>>16)+(a[2]<<16|a[2]>>>16)|0,d[5]=a[5]+(a[4]<<8|a[4]>>>24)+a[3]|0,d[6]=a[6]+(a[5]<<16|a[5]>>>16)+(a[4]<<16|a[4]>>>16)|0,d[7]=a[7]+(a[6]<<8|a[6]>>>24)+a[5]|0}n.Rabbit=s._createHelper(l)}(),t.Rabbit})}(nr)),nr.exports}var ir={exports:{}},p0;function Ai(){return p0||(p0=1,function(r,e){(function(t,n,i){r.exports=n(U(),ye(),De(),xe(),X())})(L,function(t){return function(){var n=t,i=n.lib,s=i.StreamCipher,o=n.algo,u=[],c=[],a=[],l=o.RabbitLegacy=s.extend({_doReset:function(){var d=this._key.words,p=this.cfg.iv,f=this._X=[d[0],d[3]<<16|d[2]>>>16,d[1],d[0]<<16|d[3]>>>16,d[2],d[1]<<16|d[0]>>>16,d[3],d[2]<<16|d[1]>>>16],_=this._C=[d[2]<<16|d[2]>>>16,d[0]&4294901760|d[1]&65535,d[3]<<16|d[3]>>>16,d[1]&4294901760|d[2]&65535,d[0]<<16|d[0]>>>16,d[2]&4294901760|d[3]&65535,d[1]<<16|d[1]>>>16,d[3]&4294901760|d[0]&65535];this._b=0;for(var v=0;v<4;v++)E.call(this);for(var v=0;v<8;v++)_[v]^=f[v+4&7];if(p){var C=p.words,x=C[0],h=C[1],g=(x<<8|x>>>24)&16711935|(x<<24|x>>>8)&4278255360,b=(h<<8|h>>>24)&16711935|(h<<24|h>>>8)&4278255360,A=g>>>16|b&4294901760,B=b<<16|g&65535;_[0]^=g,_[1]^=A,_[2]^=b,_[3]^=B,_[4]^=g,_[5]^=A,_[6]^=b,_[7]^=B;for(var v=0;v<4;v++)E.call(this)}},_doProcessBlock:function(d,p){var f=this._X;E.call(this),u[0]=f[0]^f[5]>>>16^f[3]<<16,u[1]=f[2]^f[7]>>>16^f[5]<<16,u[2]=f[4]^f[1]>>>16^f[7]<<16,u[3]=f[6]^f[3]>>>16^f[1]<<16;for(var _=0;_<4;_++)u[_]=(u[_]<<8|u[_]>>>24)&16711935|(u[_]<<24|u[_]>>>8)&4278255360,d[p+_]^=u[_]},blockSize:128/32,ivSize:64/32});function E(){for(var d=this._X,p=this._C,f=0;f<8;f++)c[f]=p[f];p[0]=p[0]+1295307597+this._b|0,p[1]=p[1]+3545052371+(p[0]>>>0<c[0]>>>0?1:0)|0,p[2]=p[2]+886263092+(p[1]>>>0<c[1]>>>0?1:0)|0,p[3]=p[3]+1295307597+(p[2]>>>0<c[2]>>>0?1:0)|0,p[4]=p[4]+3545052371+(p[3]>>>0<c[3]>>>0?1:0)|0,p[5]=p[5]+886263092+(p[4]>>>0<c[4]>>>0?1:0)|0,p[6]=p[6]+1295307597+(p[5]>>>0<c[5]>>>0?1:0)|0,p[7]=p[7]+3545052371+(p[6]>>>0<c[6]>>>0?1:0)|0,this._b=p[7]>>>0<c[7]>>>0?1:0;for(var f=0;f<8;f++){var _=d[f]+p[f],v=_&65535,C=_>>>16,x=((v*v>>>17)+v*C>>>15)+C*C,h=((_&4294901760)*_|0)+((_&65535)*_|0);a[f]=x^h}d[0]=a[0]+(a[7]<<16|a[7]>>>16)+(a[6]<<16|a[6]>>>16)|0,d[1]=a[1]+(a[0]<<8|a[0]>>>24)+a[7]|0,d[2]=a[2]+(a[1]<<16|a[1]>>>16)+(a[0]<<16|a[0]>>>16)|0,d[3]=a[3]+(a[2]<<8|a[2]>>>24)+a[1]|0,d[4]=a[4]+(a[3]<<16|a[3]>>>16)+(a[2]<<16|a[2]>>>16)|0,d[5]=a[5]+(a[4]<<8|a[4]>>>24)+a[3]|0,d[6]=a[6]+(a[5]<<16|a[5]>>>16)+(a[4]<<16|a[4]>>>16)|0,d[7]=a[7]+(a[6]<<8|a[6]>>>24)+a[5]|0}n.RabbitLegacy=s._createHelper(l)}(),t.RabbitLegacy})}(ir)),ir.exports}var sr={exports:{}},v0;function bi(){return v0||(v0=1,function(r,e){(function(t,n,i){r.exports=n(U(),ye(),De(),xe(),X())})(L,function(t){return function(){var n=t,i=n.lib,s=i.BlockCipher,o=n.algo;const u=16,c=[608135816,2242054355,320440878,57701188,2752067618,698298832,137296536,3964562569,1160258022,953160567,3193202383,887688300,3232508343,3380367581,1065670069,3041331479,2450970073,2306472731],a=[[3509652390,2564797868,805139163,3491422135,3101798381,1780907670,3128725573,4046225305,614570311,3012652279,134345442,2240740374,1667834072,1901547113,2757295779,4103290238,227898511,1921955416,1904987480,2182433518,2069144605,3260701109,2620446009,720527379,3318853667,677414384,3393288472,3101374703,2390351024,1614419982,1822297739,2954791486,3608508353,3174124327,2024746970,1432378464,3864339955,2857741204,1464375394,1676153920,1439316330,715854006,3033291828,289532110,2706671279,2087905683,3018724369,1668267050,732546397,1947742710,3462151702,2609353502,2950085171,1814351708,2050118529,680887927,999245976,1800124847,3300911131,1713906067,1641548236,4213287313,1216130144,1575780402,4018429277,3917837745,3693486850,3949271944,596196993,3549867205,258830323,2213823033,772490370,2760122372,1774776394,2652871518,566650946,4142492826,1728879713,2882767088,1783734482,3629395816,2517608232,2874225571,1861159788,326777828,3124490320,2130389656,2716951837,967770486,1724537150,2185432712,2364442137,1164943284,2105845187,998989502,3765401048,2244026483,1075463327,1455516326,1322494562,910128902,469688178,1117454909,936433444,3490320968,3675253459,1240580251,122909385,2157517691,634681816,4142456567,3825094682,3061402683,2540495037,79693498,3249098678,1084186820,1583128258,426386531,1761308591,1047286709,322548459,995290223,1845252383,2603652396,3431023940,2942221577,3202600964,3727903485,1712269319,422464435,3234572375,1170764815,3523960633,3117677531,1434042557,442511882,3600875718,1076654713,1738483198,4213154764,2393238008,3677496056,1014306527,4251020053,793779912,2902807211,842905082,4246964064,1395751752,1040244610,2656851899,3396308128,445077038,3742853595,3577915638,679411651,2892444358,2354009459,1767581616,3150600392,3791627101,3102740896,284835224,4246832056,1258075500,768725851,2589189241,3069724005,3532540348,1274779536,3789419226,2764799539,1660621633,3471099624,4011903706,913787905,3497959166,737222580,2514213453,2928710040,3937242737,1804850592,3499020752,2949064160,2386320175,2390070455,2415321851,4061277028,2290661394,2416832540,1336762016,1754252060,3520065937,3014181293,791618072,3188594551,3933548030,2332172193,3852520463,3043980520,413987798,3465142937,3030929376,4245938359,2093235073,3534596313,375366246,2157278981,2479649556,555357303,3870105701,2008414854,3344188149,4221384143,3956125452,2067696032,3594591187,2921233993,2428461,544322398,577241275,1471733935,610547355,4027169054,1432588573,1507829418,2025931657,3646575487,545086370,48609733,2200306550,1653985193,298326376,1316178497,3007786442,2064951626,458293330,2589141269,3591329599,3164325604,727753846,2179363840,146436021,1461446943,4069977195,705550613,3059967265,3887724982,4281599278,3313849956,1404054877,2845806497,146425753,1854211946],[1266315497,3048417604,3681880366,3289982499,290971e4,1235738493,2632868024,2414719590,3970600049,1771706367,1449415276,3266420449,422970021,1963543593,2690192192,3826793022,1062508698,1531092325,1804592342,2583117782,2714934279,4024971509,1294809318,4028980673,1289560198,2221992742,1669523910,35572830,157838143,1052438473,1016535060,1802137761,1753167236,1386275462,3080475397,2857371447,1040679964,2145300060,2390574316,1461121720,2956646967,4031777805,4028374788,33600511,2920084762,1018524850,629373528,3691585981,3515945977,2091462646,2486323059,586499841,988145025,935516892,3367335476,2599673255,2839830854,265290510,3972581182,2759138881,3795373465,1005194799,847297441,406762289,1314163512,1332590856,1866599683,4127851711,750260880,613907577,1450815602,3165620655,3734664991,3650291728,3012275730,3704569646,1427272223,778793252,1343938022,2676280711,2052605720,1946737175,3164576444,3914038668,3967478842,3682934266,1661551462,3294938066,4011595847,840292616,3712170807,616741398,312560963,711312465,1351876610,322626781,1910503582,271666773,2175563734,1594956187,70604529,3617834859,1007753275,1495573769,4069517037,2549218298,2663038764,504708206,2263041392,3941167025,2249088522,1514023603,1998579484,1312622330,694541497,2582060303,2151582166,1382467621,776784248,2618340202,3323268794,2497899128,2784771155,503983604,4076293799,907881277,423175695,432175456,1378068232,4145222326,3954048622,3938656102,3820766613,2793130115,2977904593,26017576,3274890735,3194772133,1700274565,1756076034,4006520079,3677328699,720338349,1533947780,354530856,688349552,3973924725,1637815568,332179504,3949051286,53804574,2852348879,3044236432,1282449977,3583942155,3416972820,4006381244,1617046695,2628476075,3002303598,1686838959,431878346,2686675385,1700445008,1080580658,1009431731,832498133,3223435511,2605976345,2271191193,2516031870,1648197032,4164389018,2548247927,300782431,375919233,238389289,3353747414,2531188641,2019080857,1475708069,455242339,2609103871,448939670,3451063019,1395535956,2413381860,1841049896,1491858159,885456874,4264095073,4001119347,1565136089,3898914787,1108368660,540939232,1173283510,2745871338,3681308437,4207628240,3343053890,4016749493,1699691293,1103962373,3625875870,2256883143,3830138730,1031889488,3479347698,1535977030,4236805024,3251091107,2132092099,1774941330,1199868427,1452454533,157007616,2904115357,342012276,595725824,1480756522,206960106,497939518,591360097,863170706,2375253569,3596610801,1814182875,2094937945,3421402208,1082520231,3463918190,2785509508,435703966,3908032597,1641649973,2842273706,3305899714,1510255612,2148256476,2655287854,3276092548,4258621189,236887753,3681803219,274041037,1734335097,3815195456,3317970021,1899903192,1026095262,4050517792,356393447,2410691914,3873677099,3682840055],[3913112168,2491498743,4132185628,2489919796,1091903735,1979897079,3170134830,3567386728,3557303409,857797738,1136121015,1342202287,507115054,2535736646,337727348,3213592640,1301675037,2528481711,1895095763,1721773893,3216771564,62756741,2142006736,835421444,2531993523,1442658625,3659876326,2882144922,676362277,1392781812,170690266,3921047035,1759253602,3611846912,1745797284,664899054,1329594018,3901205900,3045908486,2062866102,2865634940,3543621612,3464012697,1080764994,553557557,3656615353,3996768171,991055499,499776247,1265440854,648242737,3940784050,980351604,3713745714,1749149687,3396870395,4211799374,3640570775,1161844396,3125318951,1431517754,545492359,4268468663,3499529547,1437099964,2702547544,3433638243,2581715763,2787789398,1060185593,1593081372,2418618748,4260947970,69676912,2159744348,86519011,2512459080,3838209314,1220612927,3339683548,133810670,1090789135,1078426020,1569222167,845107691,3583754449,4072456591,1091646820,628848692,1613405280,3757631651,526609435,236106946,48312990,2942717905,3402727701,1797494240,859738849,992217954,4005476642,2243076622,3870952857,3732016268,765654824,3490871365,2511836413,1685915746,3888969200,1414112111,2273134842,3281911079,4080962846,172450625,2569994100,980381355,4109958455,2819808352,2716589560,2568741196,3681446669,3329971472,1835478071,660984891,3704678404,4045999559,3422617507,3040415634,1762651403,1719377915,3470491036,2693910283,3642056355,3138596744,1364962596,2073328063,1983633131,926494387,3423689081,2150032023,4096667949,1749200295,3328846651,309677260,2016342300,1779581495,3079819751,111262694,1274766160,443224088,298511866,1025883608,3806446537,1145181785,168956806,3641502830,3584813610,1689216846,3666258015,3200248200,1692713982,2646376535,4042768518,1618508792,1610833997,3523052358,4130873264,2001055236,3610705100,2202168115,4028541809,2961195399,1006657119,2006996926,3186142756,1430667929,3210227297,1314452623,4074634658,4101304120,2273951170,1399257539,3367210612,3027628629,1190975929,2062231137,2333990788,2221543033,2438960610,1181637006,548689776,2362791313,3372408396,3104550113,3145860560,296247880,1970579870,3078560182,3769228297,1714227617,3291629107,3898220290,166772364,1251581989,493813264,448347421,195405023,2709975567,677966185,3703036547,1463355134,2715995803,1338867538,1343315457,2802222074,2684532164,233230375,2599980071,2000651841,3277868038,1638401717,4028070440,3237316320,6314154,819756386,300326615,590932579,1405279636,3267499572,3150704214,2428286686,3959192993,3461946742,1862657033,1266418056,963775037,2089974820,2263052895,1917689273,448879540,3550394620,3981727096,150775221,3627908307,1303187396,508620638,2975983352,2726630617,1817252668,1876281319,1457606340,908771278,3720792119,3617206836,2455994898,1729034894,1080033504],[976866871,3556439503,2881648439,1522871579,1555064734,1336096578,3548522304,2579274686,3574697629,3205460757,3593280638,3338716283,3079412587,564236357,2993598910,1781952180,1464380207,3163844217,3332601554,1699332808,1393555694,1183702653,3581086237,1288719814,691649499,2847557200,2895455976,3193889540,2717570544,1781354906,1676643554,2592534050,3230253752,1126444790,2770207658,2633158820,2210423226,2615765581,2414155088,3127139286,673620729,2805611233,1269405062,4015350505,3341807571,4149409754,1057255273,2012875353,2162469141,2276492801,2601117357,993977747,3918593370,2654263191,753973209,36408145,2530585658,25011837,3520020182,2088578344,530523599,2918365339,1524020338,1518925132,3760827505,3759777254,1202760957,3985898139,3906192525,674977740,4174734889,2031300136,2019492241,3983892565,4153806404,3822280332,352677332,2297720250,60907813,90501309,3286998549,1016092578,2535922412,2839152426,457141659,509813237,4120667899,652014361,1966332200,2975202805,55981186,2327461051,676427537,3255491064,2882294119,3433927263,1307055953,942726286,933058658,2468411793,3933900994,4215176142,1361170020,2001714738,2830558078,3274259782,1222529897,1679025792,2729314320,3714953764,1770335741,151462246,3013232138,1682292957,1483529935,471910574,1539241949,458788160,3436315007,1807016891,3718408830,978976581,1043663428,3165965781,1927990952,4200891579,2372276910,3208408903,3533431907,1412390302,2931980059,4132332400,1947078029,3881505623,4168226417,2941484381,1077988104,1320477388,886195818,18198404,3786409e3,2509781533,112762804,3463356488,1866414978,891333506,18488651,661792760,1628790961,3885187036,3141171499,876946877,2693282273,1372485963,791857591,2686433993,3759982718,3167212022,3472953795,2716379847,445679433,3561995674,3504004811,3574258232,54117162,3331405415,2381918588,3769707343,4154350007,1140177722,4074052095,668550556,3214352940,367459370,261225585,2610173221,4209349473,3468074219,3265815641,314222801,3066103646,3808782860,282218597,3406013506,3773591054,379116347,1285071038,846784868,2669647154,3771962079,3550491691,2305946142,453669953,1268987020,3317592352,3279303384,3744833421,2610507566,3859509063,266596637,3847019092,517658769,3462560207,3443424879,370717030,4247526661,2224018117,4143653529,4112773975,2788324899,2477274417,1456262402,2901442914,1517677493,1846949527,2295493580,3734397586,2176403920,1280348187,1908823572,3871786941,846861322,1172426758,3287448474,3383383037,1655181056,3139813346,901632758,1897031941,2986607138,3066810236,3447102507,1393639104,373351379,950779232,625454576,3124240540,4148612726,2007998917,544563296,2244738638,2330496472,2058025392,1291430526,424198748,50039436,29584100,3605783033,2429876329,2791104160,1057563949,3255363231,3075367218,3463963227,1469046755,985887462]];var l={pbox:[],sbox:[]};function E(v,C){let x=C>>24&255,h=C>>16&255,g=C>>8&255,b=C&255,A=v.sbox[0][x]+v.sbox[1][h];return A=A^v.sbox[2][g],A=A+v.sbox[3][b],A}function d(v,C,x){let h=C,g=x,b;for(let A=0;A<u;++A)h=h^v.pbox[A],g=E(v,h)^g,b=h,h=g,g=b;return b=h,h=g,g=b,g=g^v.pbox[u],h=h^v.pbox[u+1],{left:h,right:g}}function p(v,C,x){let h=C,g=x,b;for(let A=u+1;A>1;--A)h=h^v.pbox[A],g=E(v,h)^g,b=h,h=g,g=b;return b=h,h=g,g=b,g=g^v.pbox[1],h=h^v.pbox[0],{left:h,right:g}}function f(v,C,x){for(let B=0;B<4;B++){v.sbox[B]=[];for(let D=0;D<256;D++)v.sbox[B][D]=a[B][D]}let h=0;for(let B=0;B<u+2;B++)v.pbox[B]=c[B]^C[h],h++,h>=x&&(h=0);let g=0,b=0,A=0;for(let B=0;B<u+2;B+=2)A=d(v,g,b),g=A.left,b=A.right,v.pbox[B]=g,v.pbox[B+1]=b;for(let B=0;B<4;B++)for(let D=0;D<256;D+=2)A=d(v,g,b),g=A.left,b=A.right,v.sbox[B][D]=g,v.sbox[B][D+1]=b;return!0}var _=o.Blowfish=s.extend({_doReset:function(){if(this._keyPriorReset!==this._key){var v=this._keyPriorReset=this._key,C=v.words,x=v.sigBytes/4;f(l,C,x)}},encryptBlock:function(v,C){var x=d(l,v[C],v[C+1]);v[C]=x.left,v[C+1]=x.right},decryptBlock:function(v,C){var x=p(l,v[C],v[C+1]);v[C]=x.left,v[C+1]=x.right},blockSize:64/32,keySize:128/32,ivSize:64/32});n.Blowfish=s._createHelper(_)}(),t.Blowfish})}(sr)),sr.exports}(function(r,e){(function(t,n,i){r.exports=n(U(),nt(),Jn(),ei(),ye(),ti(),De(),Wr(),Tt(),ri(),jr(),ni(),ii(),si(),Mt(),ai(),xe(),X(),oi(),ci(),li(),ui(),di(),xi(),hi(),fi(),pi(),vi(),gi(),_i(),Ei(),mi(),Ci(),Ai(),bi())})(L,function(t){return t})})(Rr);var Bi=Rr.exports;const K=Xn(Bi);function yi(r,e,t){const n=K.enc.Hex.parse(r);return K.AES.encrypt(K.enc.Hex.parse(e),n,{mode:g0(t),padding:K.pad.NoPadding}).ciphertext.toString(K.enc.Hex)}function Di(r,e,t){var n=K.enc.Hex.parse(r),i=K.enc.Hex.parse(e),s=K.enc.Base64.stringify(i),o=K.AES.decrypt(s,n,{mode:g0(t),padding:K.pad.NoPadding});return o.toString(K.enc.Hex)}function g0(r){if(r==="ecb")return K.mode.ECB;if(r==="cbc")return K.mode.CBC;if(r==="cfb")return K.mode.CFB;if(r==="ctr")return K.mode.CTR;if(r==="ofb")return K.mode.OFB;throw new Error(`AES mode ${r} not supported.`)}function wi(r){const e=K.enc.Hex.parse(r);return K.MD5(e).toString(K.enc.Hex)}async function Fi(r,e){const t=await e.arrayBuffer(),n=new Blob([t]),i=URL.createObjectURL(n),s=document.createElement("a");s.href=i,s.download=r??"",s.click(),s.remove(),URL.revokeObjectURL(i)}function Ii(){window.encryptAes=yi,window.decryptAes=Di,window.md5Hash=wi,window.downloadFileFromStream=Fi,window.getWidth=()=>window.innerWidth,window.hasPreferenceForDarkTheme=()=>window.matchMedia&&window.matchMedia("(prefers-color-scheme: dark)").matches,window.addEventListener("resize",async()=>{await DotNet.invokeMethodAsync("PKHeX.Web","OnWindowResized",window.innerWidth)}),screen.orientation.addEventListener("change",async r=>{await DotNet.invokeMethodAsync("PKHeX.Web","OnWindowResized",window.innerWidth)})}/**
 * @license
 * Copyright 2017 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *//**
 * @license
 * Copyright 2017 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const _0=function(r){const e=[];let t=0;for(let n=0;n<r.length;n++){let i=r.charCodeAt(n);i<128?e[t++]=i:i<2048?(e[t++]=i>>6|192,e[t++]=i&63|128):(i&64512)===55296&&n+1<r.length&&(r.charCodeAt(n+1)&64512)===56320?(i=65536+((i&1023)<<10)+(r.charCodeAt(++n)&1023),e[t++]=i>>18|240,e[t++]=i>>12&63|128,e[t++]=i>>6&63|128,e[t++]=i&63|128):(e[t++]=i>>12|224,e[t++]=i>>6&63|128,e[t++]=i&63|128)}return e},ki=function(r){const e=[];let t=0,n=0;for(;t<r.length;){const i=r[t++];if(i<128)e[n++]=String.fromCharCode(i);else if(i>191&&i<224){const s=r[t++];e[n++]=String.fromCharCode((i&31)<<6|s&63)}else if(i>239&&i<365){const s=r[t++],o=r[t++],u=r[t++],c=((i&7)<<18|(s&63)<<12|(o&63)<<6|u&63)-65536;e[n++]=String.fromCharCode(55296+(c>>10)),e[n++]=String.fromCharCode(56320+(c&1023))}else{const s=r[t++],o=r[t++];e[n++]=String.fromCharCode((i&15)<<12|(s&63)<<6|o&63)}}return e.join("")},E0={byteToCharMap_:null,charToByteMap_:null,byteToCharMapWebSafe_:null,charToByteMapWebSafe_:null,ENCODED_VALS_BASE:"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",get ENCODED_VALS(){return this.ENCODED_VALS_BASE+"+/="},get ENCODED_VALS_WEBSAFE(){return this.ENCODED_VALS_BASE+"-_."},HAS_NATIVE_SUPPORT:typeof atob=="function",encodeByteArray(r,e){if(!Array.isArray(r))throw Error("encodeByteArray takes an array as a parameter");this.init_();const t=e?this.byteToCharMapWebSafe_:this.byteToCharMap_,n=[];for(let i=0;i<r.length;i+=3){const s=r[i],o=i+1<r.length,u=o?r[i+1]:0,c=i+2<r.length,a=c?r[i+2]:0,l=s>>2,E=(s&3)<<4|u>>4;let d=(u&15)<<2|a>>6,p=a&63;c||(p=64,o||(d=64)),n.push(t[l],t[E],t[d],t[p])}return n.join("")},encodeString(r,e){return this.HAS_NATIVE_SUPPORT&&!e?btoa(r):this.encodeByteArray(_0(r),e)},decodeString(r,e){return this.HAS_NATIVE_SUPPORT&&!e?atob(r):ki(this.decodeStringToByteArray(r,e))},decodeStringToByteArray(r,e){this.init_();const t=e?this.charToByteMapWebSafe_:this.charToByteMap_,n=[];for(let i=0;i<r.length;){const s=t[r.charAt(i++)],u=i<r.length?t[r.charAt(i)]:0;++i;const a=i<r.length?t[r.charAt(i)]:64;++i;const E=i<r.length?t[r.charAt(i)]:64;if(++i,s==null||u==null||a==null||E==null)throw new Si;const d=s<<2|u>>4;if(n.push(d),a!==64){const p=u<<4&240|a>>2;if(n.push(p),E!==64){const f=a<<6&192|E;n.push(f)}}}return n},init_(){if(!this.byteToCharMap_){this.byteToCharMap_={},this.charToByteMap_={},this.byteToCharMapWebSafe_={},this.charToByteMapWebSafe_={};for(let r=0;r<this.ENCODED_VALS.length;r++)this.byteToCharMap_[r]=this.ENCODED_VALS.charAt(r),this.charToByteMap_[this.byteToCharMap_[r]]=r,this.byteToCharMapWebSafe_[r]=this.ENCODED_VALS_WEBSAFE.charAt(r),this.charToByteMapWebSafe_[this.byteToCharMapWebSafe_[r]]=r,r>=this.ENCODED_VALS_BASE.length&&(this.charToByteMap_[this.ENCODED_VALS_WEBSAFE.charAt(r)]=r,this.charToByteMapWebSafe_[this.ENCODED_VALS.charAt(r)]=r)}}};class Si extends Error{constructor(){super(...arguments),this.name="DecodeBase64StringError"}}const Ti=function(r){const e=_0(r);return E0.encodeByteArray(e,!0)},m0=function(r){return Ti(r).replace(/\./g,"")},C0=function(r){try{return E0.decodeString(r,!0)}catch(e){console.error("base64Decode failed: ",e)}return null};/**
 * @license
 * Copyright 2022 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function Ri(){if(typeof self<"u")return self;if(typeof window<"u")return window;if(typeof global<"u")return global;throw new Error("Unable to locate global object.")}/**
 * @license
 * Copyright 2022 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const Pi=()=>Ri().__FIREBASE_DEFAULTS__,Oi=()=>{if(typeof process>"u"||typeof process.env>"u")return;const r=process.env.__FIREBASE_DEFAULTS__;if(r)return JSON.parse(r)},Ni=()=>{if(typeof document>"u")return;let r;try{r=document.cookie.match(/__FIREBASE_DEFAULTS__=([^;]+)/)}catch{return}const e=r&&C0(r[1]);return e&&JSON.parse(e)},ar=()=>{try{return Pi()||Oi()||Ni()}catch(r){console.info(`Unable to get __FIREBASE_DEFAULTS__ due to: ${r}`);return}},Li=r=>{var e,t;return(t=(e=ar())===null||e===void 0?void 0:e.emulatorHosts)===null||t===void 0?void 0:t[r]},A0=()=>{var r;return(r=ar())===null||r===void 0?void 0:r.config},b0=r=>{var e;return(e=ar())===null||e===void 0?void 0:e[`_${r}`]};/**
 * @license
 * Copyright 2017 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Hi{constructor(){this.reject=()=>{},this.resolve=()=>{},this.promise=new Promise((e,t)=>{this.resolve=e,this.reject=t})}wrapCallback(e){return(t,n)=>{t?this.reject(t):this.resolve(n),typeof e=="function"&&(this.promise.catch(()=>{}),e.length===1?e(t):e(t,n))}}}/**
 * @license
 * Copyright 2017 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function Y(){return typeof navigator<"u"&&typeof navigator.userAgent=="string"?navigator.userAgent:""}function Mi(){return typeof window<"u"&&!!(window.cordova||window.phonegap||window.PhoneGap)&&/ios|iphone|ipod|ipad|android|blackberry|iemobile/i.test(Y())}function Ui(){return typeof navigator<"u"&&navigator.userAgent==="Cloudflare-Workers"}function zi(){const r=typeof chrome=="object"?chrome.runtime:typeof browser=="object"?browser.runtime:void 0;return typeof r=="object"&&r.id!==void 0}function Wi(){return typeof navigator=="object"&&navigator.product==="ReactNative"}function $i(){const r=Y();return r.indexOf("MSIE ")>=0||r.indexOf("Trident/")>=0}function qi(){try{return typeof indexedDB=="object"}catch{return!1}}function Vi(){return new Promise((r,e)=>{try{let t=!0;const n="validate-browser-context-for-indexeddb-analytics-module",i=self.indexedDB.open(n);i.onsuccess=()=>{i.result.close(),t||self.indexedDB.deleteDatabase(n),r(!0)},i.onupgradeneeded=()=>{t=!1},i.onerror=()=>{var s;e(((s=i.error)===null||s===void 0?void 0:s.message)||"")}}catch(t){e(t)}})}/**
 * @license
 * Copyright 2017 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const ji="FirebaseError";class he extends Error{constructor(e,t,n){super(t),this.code=e,this.customData=n,this.name=ji,Object.setPrototypeOf(this,he.prototype),Error.captureStackTrace&&Error.captureStackTrace(this,Ue.prototype.create)}}class Ue{constructor(e,t,n){this.service=e,this.serviceName=t,this.errors=n}create(e,...t){const n=t[0]||{},i=`${this.service}/${e}`,s=this.errors[e],o=s?Gi(s,n):"Error",u=`${this.serviceName}: ${o} (${i}).`;return new he(i,u,n)}}function Gi(r,e){return r.replace(Ki,(t,n)=>{const i=e[n];return i!=null?String(i):`<${n}?>`})}const Ki=/\{\$([^}]+)}/g;function Xi(r){for(const e in r)if(Object.prototype.hasOwnProperty.call(r,e))return!1;return!0}function it(r,e){if(r===e)return!0;const t=Object.keys(r),n=Object.keys(e);for(const i of t){if(!n.includes(i))return!1;const s=r[i],o=e[i];if(B0(s)&&B0(o)){if(!it(s,o))return!1}else if(s!==o)return!1}for(const i of n)if(!t.includes(i))return!1;return!0}function B0(r){return r!==null&&typeof r=="object"}/**
 * @license
 * Copyright 2017 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function ze(r){const e=[];for(const[t,n]of Object.entries(r))Array.isArray(n)?n.forEach(i=>{e.push(encodeURIComponent(t)+"="+encodeURIComponent(i))}):e.push(encodeURIComponent(t)+"="+encodeURIComponent(n));return e.length?"&"+e.join("&"):""}function Yi(r,e){const t=new Zi(r,e);return t.subscribe.bind(t)}class Zi{constructor(e,t){this.observers=[],this.unsubscribes=[],this.observerCount=0,this.task=Promise.resolve(),this.finalized=!1,this.onNoObservers=t,this.task.then(()=>{e(this)}).catch(n=>{this.error(n)})}next(e){this.forEachObserver(t=>{t.next(e)})}error(e){this.forEachObserver(t=>{t.error(e)}),this.close(e)}complete(){this.forEachObserver(e=>{e.complete()}),this.close()}subscribe(e,t,n){let i;if(e===void 0&&t===void 0&&n===void 0)throw new Error("Missing Observer.");Qi(e,["next","error","complete"])?i=e:i={next:e,error:t,complete:n},i.next===void 0&&(i.next=or),i.error===void 0&&(i.error=or),i.complete===void 0&&(i.complete=or);const s=this.unsubscribeOne.bind(this,this.observers.length);return this.finalized&&this.task.then(()=>{try{this.finalError?i.error(this.finalError):i.complete()}catch{}}),this.observers.push(i),s}unsubscribeOne(e){this.observers===void 0||this.observers[e]===void 0||(delete this.observers[e],this.observerCount-=1,this.observerCount===0&&this.onNoObservers!==void 0&&this.onNoObservers(this))}forEachObserver(e){if(!this.finalized)for(let t=0;t<this.observers.length;t++)this.sendOne(t,e)}sendOne(e,t){this.task.then(()=>{if(this.observers!==void 0&&this.observers[e]!==void 0)try{t(this.observers[e])}catch(n){typeof console<"u"&&console.error&&console.error(n)}})}close(e){this.finalized||(this.finalized=!0,e!==void 0&&(this.finalError=e),this.task.then(()=>{this.observers=void 0,this.onNoObservers=void 0}))}}function Qi(r,e){if(typeof r!="object"||r===null)return!1;for(const t of e)if(t in r&&typeof r[t]=="function")return!0;return!1}function or(){}/**
 * @license
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function Ie(r){return r&&r._delegate?r._delegate:r}class ke{constructor(e,t,n){this.name=e,this.instanceFactory=t,this.type=n,this.multipleInstances=!1,this.serviceProps={},this.instantiationMode="LAZY",this.onInstanceCreated=null}setInstantiationMode(e){return this.instantiationMode=e,this}setMultipleInstances(e){return this.multipleInstances=e,this}setServiceProps(e){return this.serviceProps=e,this}setInstanceCreatedCallback(e){return this.onInstanceCreated=e,this}}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const we="[DEFAULT]";/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Ji{constructor(e,t){this.name=e,this.container=t,this.component=null,this.instances=new Map,this.instancesDeferred=new Map,this.instancesOptions=new Map,this.onInitCallbacks=new Map}get(e){const t=this.normalizeInstanceIdentifier(e);if(!this.instancesDeferred.has(t)){const n=new Hi;if(this.instancesDeferred.set(t,n),this.isInitialized(t)||this.shouldAutoInitialize())try{const i=this.getOrInitializeService({instanceIdentifier:t});i&&n.resolve(i)}catch{}}return this.instancesDeferred.get(t).promise}getImmediate(e){var t;const n=this.normalizeInstanceIdentifier(e==null?void 0:e.identifier),i=(t=e==null?void 0:e.optional)!==null&&t!==void 0?t:!1;if(this.isInitialized(n)||this.shouldAutoInitialize())try{return this.getOrInitializeService({instanceIdentifier:n})}catch(s){if(i)return null;throw s}else{if(i)return null;throw Error(`Service ${this.name} is not available`)}}getComponent(){return this.component}setComponent(e){if(e.name!==this.name)throw Error(`Mismatching Component ${e.name} for Provider ${this.name}.`);if(this.component)throw Error(`Component for ${this.name} has already been provided`);if(this.component=e,!!this.shouldAutoInitialize()){if(ts(e))try{this.getOrInitializeService({instanceIdentifier:we})}catch{}for(const[t,n]of this.instancesDeferred.entries()){const i=this.normalizeInstanceIdentifier(t);try{const s=this.getOrInitializeService({instanceIdentifier:i});n.resolve(s)}catch{}}}}clearInstance(e=we){this.instancesDeferred.delete(e),this.instancesOptions.delete(e),this.instances.delete(e)}async delete(){const e=Array.from(this.instances.values());await Promise.all([...e.filter(t=>"INTERNAL"in t).map(t=>t.INTERNAL.delete()),...e.filter(t=>"_delete"in t).map(t=>t._delete())])}isComponentSet(){return this.component!=null}isInitialized(e=we){return this.instances.has(e)}getOptions(e=we){return this.instancesOptions.get(e)||{}}initialize(e={}){const{options:t={}}=e,n=this.normalizeInstanceIdentifier(e.instanceIdentifier);if(this.isInitialized(n))throw Error(`${this.name}(${n}) has already been initialized`);if(!this.isComponentSet())throw Error(`Component ${this.name} has not been registered yet`);const i=this.getOrInitializeService({instanceIdentifier:n,options:t});for(const[s,o]of this.instancesDeferred.entries()){const u=this.normalizeInstanceIdentifier(s);n===u&&o.resolve(i)}return i}onInit(e,t){var n;const i=this.normalizeInstanceIdentifier(t),s=(n=this.onInitCallbacks.get(i))!==null&&n!==void 0?n:new Set;s.add(e),this.onInitCallbacks.set(i,s);const o=this.instances.get(i);return o&&e(o,i),()=>{s.delete(e)}}invokeOnInitCallbacks(e,t){const n=this.onInitCallbacks.get(t);if(n)for(const i of n)try{i(e,t)}catch{}}getOrInitializeService({instanceIdentifier:e,options:t={}}){let n=this.instances.get(e);if(!n&&this.component&&(n=this.component.instanceFactory(this.container,{instanceIdentifier:es(e),options:t}),this.instances.set(e,n),this.instancesOptions.set(e,t),this.invokeOnInitCallbacks(n,e),this.component.onInstanceCreated))try{this.component.onInstanceCreated(this.container,e,n)}catch{}return n||null}normalizeInstanceIdentifier(e=we){return this.component?this.component.multipleInstances?e:we:e}shouldAutoInitialize(){return!!this.component&&this.component.instantiationMode!=="EXPLICIT"}}function es(r){return r===we?void 0:r}function ts(r){return r.instantiationMode==="EAGER"}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class rs{constructor(e){this.name=e,this.providers=new Map}addComponent(e){const t=this.getProvider(e.name);if(t.isComponentSet())throw new Error(`Component ${e.name} has already been registered with ${this.name}`);t.setComponent(e)}addOrOverwriteComponent(e){this.getProvider(e.name).isComponentSet()&&this.providers.delete(e.name),this.addComponent(e)}getProvider(e){if(this.providers.has(e))return this.providers.get(e);const t=new Ji(e,this);return this.providers.set(e,t),t}getProviders(){return Array.from(this.providers.values())}}/**
 * @license
 * Copyright 2017 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */var $;(function(r){r[r.DEBUG=0]="DEBUG",r[r.VERBOSE=1]="VERBOSE",r[r.INFO=2]="INFO",r[r.WARN=3]="WARN",r[r.ERROR=4]="ERROR",r[r.SILENT=5]="SILENT"})($||($={}));const ns={debug:$.DEBUG,verbose:$.VERBOSE,info:$.INFO,warn:$.WARN,error:$.ERROR,silent:$.SILENT},is=$.INFO,ss={[$.DEBUG]:"log",[$.VERBOSE]:"log",[$.INFO]:"info",[$.WARN]:"warn",[$.ERROR]:"error"},as=(r,e,...t)=>{if(e<r.logLevel)return;const n=new Date().toISOString(),i=ss[e];if(i)console[i](`[${n}]  ${r.name}:`,...t);else throw new Error(`Attempted to log a message with an invalid logType (value: ${e})`)};class y0{constructor(e){this.name=e,this._logLevel=is,this._logHandler=as,this._userLogHandler=null}get logLevel(){return this._logLevel}set logLevel(e){if(!(e in $))throw new TypeError(`Invalid value "${e}" assigned to \`logLevel\``);this._logLevel=e}setLogLevel(e){this._logLevel=typeof e=="string"?ns[e]:e}get logHandler(){return this._logHandler}set logHandler(e){if(typeof e!="function")throw new TypeError("Value assigned to `logHandler` must be a function");this._logHandler=e}get userLogHandler(){return this._userLogHandler}set userLogHandler(e){this._userLogHandler=e}debug(...e){this._userLogHandler&&this._userLogHandler(this,$.DEBUG,...e),this._logHandler(this,$.DEBUG,...e)}log(...e){this._userLogHandler&&this._userLogHandler(this,$.VERBOSE,...e),this._logHandler(this,$.VERBOSE,...e)}info(...e){this._userLogHandler&&this._userLogHandler(this,$.INFO,...e),this._logHandler(this,$.INFO,...e)}warn(...e){this._userLogHandler&&this._userLogHandler(this,$.WARN,...e),this._logHandler(this,$.WARN,...e)}error(...e){this._userLogHandler&&this._userLogHandler(this,$.ERROR,...e),this._logHandler(this,$.ERROR,...e)}}const os=(r,e)=>e.some(t=>r instanceof t);let D0,w0;function cs(){return D0||(D0=[IDBDatabase,IDBObjectStore,IDBIndex,IDBCursor,IDBTransaction])}function ls(){return w0||(w0=[IDBCursor.prototype.advance,IDBCursor.prototype.continue,IDBCursor.prototype.continuePrimaryKey])}const F0=new WeakMap,cr=new WeakMap,I0=new WeakMap,lr=new WeakMap,ur=new WeakMap;function us(r){const e=new Promise((t,n)=>{const i=()=>{r.removeEventListener("success",s),r.removeEventListener("error",o)},s=()=>{t(fe(r.result)),i()},o=()=>{n(r.error),i()};r.addEventListener("success",s),r.addEventListener("error",o)});return e.then(t=>{t instanceof IDBCursor&&F0.set(t,r)}).catch(()=>{}),ur.set(e,r),e}function ds(r){if(cr.has(r))return;const e=new Promise((t,n)=>{const i=()=>{r.removeEventListener("complete",s),r.removeEventListener("error",o),r.removeEventListener("abort",o)},s=()=>{t(),i()},o=()=>{n(r.error||new DOMException("AbortError","AbortError")),i()};r.addEventListener("complete",s),r.addEventListener("error",o),r.addEventListener("abort",o)});cr.set(r,e)}let dr={get(r,e,t){if(r instanceof IDBTransaction){if(e==="done")return cr.get(r);if(e==="objectStoreNames")return r.objectStoreNames||I0.get(r);if(e==="store")return t.objectStoreNames[1]?void 0:t.objectStore(t.objectStoreNames[0])}return fe(r[e])},set(r,e,t){return r[e]=t,!0},has(r,e){return r instanceof IDBTransaction&&(e==="done"||e==="store")?!0:e in r}};function xs(r){dr=r(dr)}function hs(r){return r===IDBDatabase.prototype.transaction&&!("objectStoreNames"in IDBTransaction.prototype)?function(e,...t){const n=r.call(xr(this),e,...t);return I0.set(n,e.sort?e.sort():[e]),fe(n)}:ls().includes(r)?function(...e){return r.apply(xr(this),e),fe(F0.get(this))}:function(...e){return fe(r.apply(xr(this),e))}}function fs(r){return typeof r=="function"?hs(r):(r instanceof IDBTransaction&&ds(r),os(r,cs())?new Proxy(r,dr):r)}function fe(r){if(r instanceof IDBRequest)return us(r);if(lr.has(r))return lr.get(r);const e=fs(r);return e!==r&&(lr.set(r,e),ur.set(e,r)),e}const xr=r=>ur.get(r);function ps(r,e,{blocked:t,upgrade:n,blocking:i,terminated:s}={}){const o=indexedDB.open(r,e),u=fe(o);return n&&o.addEventListener("upgradeneeded",c=>{n(fe(o.result),c.oldVersion,c.newVersion,fe(o.transaction),c)}),t&&o.addEventListener("blocked",c=>t(c.oldVersion,c.newVersion,c)),u.then(c=>{s&&c.addEventListener("close",()=>s()),i&&c.addEventListener("versionchange",a=>i(a.oldVersion,a.newVersion,a))}).catch(()=>{}),u}const vs=["get","getKey","getAll","getAllKeys","count"],gs=["put","add","delete","clear"],hr=new Map;function k0(r,e){if(!(r instanceof IDBDatabase&&!(e in r)&&typeof e=="string"))return;if(hr.get(e))return hr.get(e);const t=e.replace(/FromIndex$/,""),n=e!==t,i=gs.includes(t);if(!(t in(n?IDBIndex:IDBObjectStore).prototype)||!(i||vs.includes(t)))return;const s=async function(o,...u){const c=this.transaction(o,i?"readwrite":"readonly");let a=c.store;return n&&(a=a.index(u.shift())),(await Promise.all([a[t](...u),i&&c.done]))[0]};return hr.set(e,s),s}xs(r=>({...r,get:(e,t,n)=>k0(e,t)||r.get(e,t,n),has:(e,t)=>!!k0(e,t)||r.has(e,t)}));/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class _s{constructor(e){this.container=e}getPlatformInfoString(){return this.container.getProviders().map(t=>{if(Es(t)){const n=t.getImmediate();return`${n.library}/${n.version}`}else return null}).filter(t=>t).join(" ")}}function Es(r){const e=r.getComponent();return(e==null?void 0:e.type)==="VERSION"}const fr="@firebase/app",S0="0.10.15";/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const se=new y0("@firebase/app"),ms="@firebase/app-compat",Cs="@firebase/analytics-compat",As="@firebase/analytics",bs="@firebase/app-check-compat",Bs="@firebase/app-check",ys="@firebase/auth",Ds="@firebase/auth-compat",ws="@firebase/database",Fs="@firebase/data-connect",Is="@firebase/database-compat",ks="@firebase/functions",Ss="@firebase/functions-compat",Ts="@firebase/installations",Rs="@firebase/installations-compat",Ps="@firebase/messaging",Os="@firebase/messaging-compat",Ns="@firebase/performance",Ls="@firebase/performance-compat",Hs="@firebase/remote-config",Ms="@firebase/remote-config-compat",Us="@firebase/storage",zs="@firebase/storage-compat",Ws="@firebase/firestore",$s="@firebase/vertexai",qs="@firebase/firestore-compat",Vs="firebase",js="11.0.1";/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const pr="[DEFAULT]",Gs={[fr]:"fire-core",[ms]:"fire-core-compat",[As]:"fire-analytics",[Cs]:"fire-analytics-compat",[Bs]:"fire-app-check",[bs]:"fire-app-check-compat",[ys]:"fire-auth",[Ds]:"fire-auth-compat",[ws]:"fire-rtdb",[Fs]:"fire-data-connect",[Is]:"fire-rtdb-compat",[ks]:"fire-fn",[Ss]:"fire-fn-compat",[Ts]:"fire-iid",[Rs]:"fire-iid-compat",[Ps]:"fire-fcm",[Os]:"fire-fcm-compat",[Ns]:"fire-perf",[Ls]:"fire-perf-compat",[Hs]:"fire-rc",[Ms]:"fire-rc-compat",[Us]:"fire-gcs",[zs]:"fire-gcs-compat",[Ws]:"fire-fst",[qs]:"fire-fst-compat",[$s]:"fire-vertex","fire-js":"fire-js",[Vs]:"fire-js-all"};/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const st=new Map,Ks=new Map,vr=new Map;function T0(r,e){try{r.container.addComponent(e)}catch(t){se.debug(`Component ${e.name} failed to register with FirebaseApp ${r.name}`,t)}}function We(r){const e=r.name;if(vr.has(e))return se.debug(`There were multiple attempts to register component ${e}.`),!1;vr.set(e,r);for(const t of st.values())T0(t,r);for(const t of Ks.values())T0(t,r);return!0}function R0(r,e){const t=r.container.getProvider("heartbeat").getImmediate({optional:!0});return t&&t.triggerHeartbeat(),r.container.getProvider(e)}function ae(r){return r.settings!==void 0}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const Xs={"no-app":"No Firebase App '{$appName}' has been created - call initializeApp() first","bad-app-name":"Illegal App name: '{$appName}'","duplicate-app":"Firebase App named '{$appName}' already exists with different options or config","app-deleted":"Firebase App named '{$appName}' already deleted","server-app-deleted":"Firebase Server App has been deleted","no-options":"Need to provide options, when not being deployed to hosting via source.","invalid-app-argument":"firebase.{$appName}() takes either no argument or a Firebase App instance.","invalid-log-argument":"First argument to `onLog` must be null or a function.","idb-open":"Error thrown when opening IndexedDB. Original error: {$originalErrorMessage}.","idb-get":"Error thrown when reading from IndexedDB. Original error: {$originalErrorMessage}.","idb-set":"Error thrown when writing to IndexedDB. Original error: {$originalErrorMessage}.","idb-delete":"Error thrown when deleting from IndexedDB. Original error: {$originalErrorMessage}.","finalization-registry-not-supported":"FirebaseServerApp deleteOnDeref field defined but the JS runtime does not support FinalizationRegistry.","invalid-server-app-environment":"FirebaseServerApp is not for use in browser environments."},pe=new Ue("app","Firebase",Xs);/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Ys{constructor(e,t,n){this._isDeleted=!1,this._options=Object.assign({},e),this._config=Object.assign({},t),this._name=t.name,this._automaticDataCollectionEnabled=t.automaticDataCollectionEnabled,this._container=n,this.container.addComponent(new ke("app",()=>this,"PUBLIC"))}get automaticDataCollectionEnabled(){return this.checkDestroyed(),this._automaticDataCollectionEnabled}set automaticDataCollectionEnabled(e){this.checkDestroyed(),this._automaticDataCollectionEnabled=e}get name(){return this.checkDestroyed(),this._name}get options(){return this.checkDestroyed(),this._options}get config(){return this.checkDestroyed(),this._config}get container(){return this._container}get isDeleted(){return this._isDeleted}set isDeleted(e){this._isDeleted=e}checkDestroyed(){if(this.isDeleted)throw pe.create("app-deleted",{appName:this._name})}}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const $e=js;function P0(r,e={}){let t=r;typeof e!="object"&&(e={name:e});const n=Object.assign({name:pr,automaticDataCollectionEnabled:!1},e),i=n.name;if(typeof i!="string"||!i)throw pe.create("bad-app-name",{appName:String(i)});if(t||(t=A0()),!t)throw pe.create("no-options");const s=st.get(i);if(s){if(it(t,s.options)&&it(n,s.config))return s;throw pe.create("duplicate-app",{appName:i})}const o=new rs(i);for(const c of vr.values())o.addComponent(c);const u=new Ys(t,n,o);return st.set(i,u),u}function Zs(r=pr){const e=st.get(r);if(!e&&r===pr&&A0())return P0();if(!e)throw pe.create("no-app",{appName:r});return e}function Se(r,e,t){var n;let i=(n=Gs[r])!==null&&n!==void 0?n:r;t&&(i+=`-${t}`);const s=i.match(/\s|\//),o=e.match(/\s|\//);if(s||o){const u=[`Unable to register library "${i}" with version "${e}":`];s&&u.push(`library name "${i}" contains illegal characters (whitespace or "/")`),s&&o&&u.push("and"),o&&u.push(`version name "${e}" contains illegal characters (whitespace or "/")`),se.warn(u.join(" "));return}We(new ke(`${i}-version`,()=>({library:i,version:e}),"VERSION"))}/**
 * @license
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const Qs="firebase-heartbeat-database",Js=1,qe="firebase-heartbeat-store";let gr=null;function O0(){return gr||(gr=ps(Qs,Js,{upgrade:(r,e)=>{switch(e){case 0:try{r.createObjectStore(qe)}catch(t){console.warn(t)}}}}).catch(r=>{throw pe.create("idb-open",{originalErrorMessage:r.message})})),gr}async function ea(r){try{const t=(await O0()).transaction(qe),n=await t.objectStore(qe).get(L0(r));return await t.done,n}catch(e){if(e instanceof he)se.warn(e.message);else{const t=pe.create("idb-get",{originalErrorMessage:e==null?void 0:e.message});se.warn(t.message)}}}async function N0(r,e){try{const n=(await O0()).transaction(qe,"readwrite");await n.objectStore(qe).put(e,L0(r)),await n.done}catch(t){if(t instanceof he)se.warn(t.message);else{const n=pe.create("idb-set",{originalErrorMessage:t==null?void 0:t.message});se.warn(n.message)}}}function L0(r){return`${r.name}!${r.options.appId}`}/**
 * @license
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const ta=1024,ra=30*24*60*60*1e3;class na{constructor(e){this.container=e,this._heartbeatsCache=null;const t=this.container.getProvider("app").getImmediate();this._storage=new sa(t),this._heartbeatsCachePromise=this._storage.read().then(n=>(this._heartbeatsCache=n,n))}async triggerHeartbeat(){var e,t;try{const i=this.container.getProvider("platform-logger").getImmediate().getPlatformInfoString(),s=H0();return((e=this._heartbeatsCache)===null||e===void 0?void 0:e.heartbeats)==null&&(this._heartbeatsCache=await this._heartbeatsCachePromise,((t=this._heartbeatsCache)===null||t===void 0?void 0:t.heartbeats)==null)||this._heartbeatsCache.lastSentHeartbeatDate===s||this._heartbeatsCache.heartbeats.some(o=>o.date===s)?void 0:(this._heartbeatsCache.heartbeats.push({date:s,agent:i}),this._heartbeatsCache.heartbeats=this._heartbeatsCache.heartbeats.filter(o=>{const u=new Date(o.date).valueOf();return Date.now()-u<=ra}),this._storage.overwrite(this._heartbeatsCache))}catch(n){se.warn(n)}}async getHeartbeatsHeader(){var e;try{if(this._heartbeatsCache===null&&await this._heartbeatsCachePromise,((e=this._heartbeatsCache)===null||e===void 0?void 0:e.heartbeats)==null||this._heartbeatsCache.heartbeats.length===0)return"";const t=H0(),{heartbeatsToSend:n,unsentEntries:i}=ia(this._heartbeatsCache.heartbeats),s=m0(JSON.stringify({version:2,heartbeats:n}));return this._heartbeatsCache.lastSentHeartbeatDate=t,i.length>0?(this._heartbeatsCache.heartbeats=i,await this._storage.overwrite(this._heartbeatsCache)):(this._heartbeatsCache.heartbeats=[],this._storage.overwrite(this._heartbeatsCache)),s}catch(t){return se.warn(t),""}}}function H0(){return new Date().toISOString().substring(0,10)}function ia(r,e=ta){const t=[];let n=r.slice();for(const i of r){const s=t.find(o=>o.agent===i.agent);if(s){if(s.dates.push(i.date),M0(t)>e){s.dates.pop();break}}else if(t.push({agent:i.agent,dates:[i.date]}),M0(t)>e){t.pop();break}n=n.slice(1)}return{heartbeatsToSend:t,unsentEntries:n}}class sa{constructor(e){this.app=e,this._canUseIndexedDBPromise=this.runIndexedDBEnvironmentCheck()}async runIndexedDBEnvironmentCheck(){return qi()?Vi().then(()=>!0).catch(()=>!1):!1}async read(){if(await this._canUseIndexedDBPromise){const t=await ea(this.app);return t!=null&&t.heartbeats?t:{heartbeats:[]}}else return{heartbeats:[]}}async overwrite(e){var t;if(await this._canUseIndexedDBPromise){const i=await this.read();return N0(this.app,{lastSentHeartbeatDate:(t=e.lastSentHeartbeatDate)!==null&&t!==void 0?t:i.lastSentHeartbeatDate,heartbeats:e.heartbeats})}else return}async add(e){var t;if(await this._canUseIndexedDBPromise){const i=await this.read();return N0(this.app,{lastSentHeartbeatDate:(t=e.lastSentHeartbeatDate)!==null&&t!==void 0?t:i.lastSentHeartbeatDate,heartbeats:[...i.heartbeats,...e.heartbeats]})}else return}}function M0(r){return m0(JSON.stringify({version:2,heartbeats:r})).length}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function aa(r){We(new ke("platform-logger",e=>new _s(e),"PRIVATE")),We(new ke("heartbeat",e=>new na(e),"PRIVATE")),Se(fr,S0,r),Se(fr,S0,"esm2017"),Se("fire-js","")}aa("");var oa="firebase",ca="11.0.1";/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */Se(oa,ca,"app");function _r(r,e){var t={};for(var n in r)Object.prototype.hasOwnProperty.call(r,n)&&e.indexOf(n)<0&&(t[n]=r[n]);if(r!=null&&typeof Object.getOwnPropertySymbols=="function")for(var i=0,n=Object.getOwnPropertySymbols(r);i<n.length;i++)e.indexOf(n[i])<0&&Object.prototype.propertyIsEnumerable.call(r,n[i])&&(t[n[i]]=r[n[i]]);return t}typeof SuppressedError=="function"&&SuppressedError;function U0(){return{"dependent-sdk-initialized-before-auth":"Another Firebase SDK was initialized and is trying to use Auth before Auth is initialized. Please be sure to call `initializeAuth` or `getAuth` before starting any other Firebase SDK."}}const la=U0,z0=new Ue("auth","Firebase",U0());/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const at=new y0("@firebase/auth");function ua(r,...e){at.logLevel<=$.WARN&&at.warn(`Auth (${$e}): ${r}`,...e)}function ot(r,...e){at.logLevel<=$.ERROR&&at.error(`Auth (${$e}): ${r}`,...e)}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function oe(r,...e){throw Er(r,...e)}function ne(r,...e){return Er(r,...e)}function W0(r,e,t){const n=Object.assign(Object.assign({},la()),{[e]:t});return new Ue("auth","Firebase",n).create(e,{appName:r.name})}function ve(r){return W0(r,"operation-not-supported-in-this-environment","Operations that alter the current user are not supported in conjunction with FirebaseServerApp")}function Er(r,...e){if(typeof r!="string"){const t=e[0],n=[...e.slice(1)];return n[0]&&(n[0].appName=r.name),r._errorFactory.create(t,...n)}return z0.create(r,...e)}function N(r,e,...t){if(!r)throw Er(e,...t)}function ce(r){const e="INTERNAL ASSERTION FAILED: "+r;throw ot(e),new Error(e)}function le(r,e){r||ce(e)}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function mr(){var r;return typeof self<"u"&&((r=self.location)===null||r===void 0?void 0:r.href)||""}function da(){return $0()==="http:"||$0()==="https:"}function $0(){var r;return typeof self<"u"&&((r=self.location)===null||r===void 0?void 0:r.protocol)||null}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function xa(){return typeof navigator<"u"&&navigator&&"onLine"in navigator&&typeof navigator.onLine=="boolean"&&(da()||zi()||"connection"in navigator)?navigator.onLine:!0}function ha(){if(typeof navigator>"u")return null;const r=navigator;return r.languages&&r.languages[0]||r.language||null}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Ve{constructor(e,t){this.shortDelay=e,this.longDelay=t,le(t>e,"Short delay should be less than long delay!"),this.isMobile=Mi()||Wi()}get(){return xa()?this.isMobile?this.longDelay:this.shortDelay:Math.min(5e3,this.shortDelay)}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function Cr(r,e){le(r.emulator,"Emulator should always be set here");const{url:t}=r.emulator;return e?`${t}${e.startsWith("/")?e.slice(1):e}`:t}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class q0{static initialize(e,t,n){this.fetchImpl=e,t&&(this.headersImpl=t),n&&(this.responseImpl=n)}static fetch(){if(this.fetchImpl)return this.fetchImpl;if(typeof self<"u"&&"fetch"in self)return self.fetch;if(typeof globalThis<"u"&&globalThis.fetch)return globalThis.fetch;if(typeof fetch<"u")return fetch;ce("Could not find fetch implementation, make sure you call FetchProvider.initialize() with an appropriate polyfill")}static headers(){if(this.headersImpl)return this.headersImpl;if(typeof self<"u"&&"Headers"in self)return self.Headers;if(typeof globalThis<"u"&&globalThis.Headers)return globalThis.Headers;if(typeof Headers<"u")return Headers;ce("Could not find Headers implementation, make sure you call FetchProvider.initialize() with an appropriate polyfill")}static response(){if(this.responseImpl)return this.responseImpl;if(typeof self<"u"&&"Response"in self)return self.Response;if(typeof globalThis<"u"&&globalThis.Response)return globalThis.Response;if(typeof Response<"u")return Response;ce("Could not find Response implementation, make sure you call FetchProvider.initialize() with an appropriate polyfill")}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const fa={CREDENTIAL_MISMATCH:"custom-token-mismatch",MISSING_CUSTOM_TOKEN:"internal-error",INVALID_IDENTIFIER:"invalid-email",MISSING_CONTINUE_URI:"internal-error",INVALID_PASSWORD:"wrong-password",MISSING_PASSWORD:"missing-password",INVALID_LOGIN_CREDENTIALS:"invalid-credential",EMAIL_EXISTS:"email-already-in-use",PASSWORD_LOGIN_DISABLED:"operation-not-allowed",INVALID_IDP_RESPONSE:"invalid-credential",INVALID_PENDING_TOKEN:"invalid-credential",FEDERATED_USER_ID_ALREADY_LINKED:"credential-already-in-use",MISSING_REQ_TYPE:"internal-error",EMAIL_NOT_FOUND:"user-not-found",RESET_PASSWORD_EXCEED_LIMIT:"too-many-requests",EXPIRED_OOB_CODE:"expired-action-code",INVALID_OOB_CODE:"invalid-action-code",MISSING_OOB_CODE:"internal-error",CREDENTIAL_TOO_OLD_LOGIN_AGAIN:"requires-recent-login",INVALID_ID_TOKEN:"invalid-user-token",TOKEN_EXPIRED:"user-token-expired",USER_NOT_FOUND:"user-token-expired",TOO_MANY_ATTEMPTS_TRY_LATER:"too-many-requests",PASSWORD_DOES_NOT_MEET_REQUIREMENTS:"password-does-not-meet-requirements",INVALID_CODE:"invalid-verification-code",INVALID_SESSION_INFO:"invalid-verification-id",INVALID_TEMPORARY_PROOF:"invalid-credential",MISSING_SESSION_INFO:"missing-verification-id",SESSION_EXPIRED:"code-expired",MISSING_ANDROID_PACKAGE_NAME:"missing-android-pkg-name",UNAUTHORIZED_DOMAIN:"unauthorized-continue-uri",INVALID_OAUTH_CLIENT_ID:"invalid-oauth-client-id",ADMIN_ONLY_OPERATION:"admin-restricted-operation",INVALID_MFA_PENDING_CREDENTIAL:"invalid-multi-factor-session",MFA_ENROLLMENT_NOT_FOUND:"multi-factor-info-not-found",MISSING_MFA_ENROLLMENT_ID:"missing-multi-factor-info",MISSING_MFA_PENDING_CREDENTIAL:"missing-multi-factor-session",SECOND_FACTOR_EXISTS:"second-factor-already-in-use",SECOND_FACTOR_LIMIT_EXCEEDED:"maximum-second-factor-count-exceeded",BLOCKING_FUNCTION_ERROR_RESPONSE:"internal-error",RECAPTCHA_NOT_ENABLED:"recaptcha-not-enabled",MISSING_RECAPTCHA_TOKEN:"missing-recaptcha-token",INVALID_RECAPTCHA_TOKEN:"invalid-recaptcha-token",INVALID_RECAPTCHA_ACTION:"invalid-recaptcha-action",MISSING_CLIENT_TYPE:"missing-client-type",MISSING_RECAPTCHA_VERSION:"missing-recaptcha-version",INVALID_RECAPTCHA_VERSION:"invalid-recaptcha-version",INVALID_REQ_TYPE:"invalid-req-type"};/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const pa=new Ve(3e4,6e4);function ct(r,e){return r.tenantId&&!e.tenantId?Object.assign(Object.assign({},e),{tenantId:r.tenantId}):e}async function Te(r,e,t,n,i={}){return V0(r,i,async()=>{let s={},o={};n&&(e==="GET"?o=n:s={body:JSON.stringify(n)});const u=ze(Object.assign({key:r.config.apiKey},o)).slice(1),c=await r._getAdditionalHeaders();c["Content-Type"]="application/json",r.languageCode&&(c["X-Firebase-Locale"]=r.languageCode);const a=Object.assign({method:e,headers:c},s);return Ui()||(a.referrerPolicy="no-referrer"),q0.fetch()(G0(r,r.config.apiHost,t,u),a)})}async function V0(r,e,t){r._canInitEmulator=!1;const n=Object.assign(Object.assign({},fa),e);try{const i=new va(r),s=await Promise.race([t(),i.promise]);i.clearNetworkTimeout();const o=await s.json();if("needConfirmation"in o)throw lt(r,"account-exists-with-different-credential",o);if(s.ok&&!("errorMessage"in o))return o;{const u=s.ok?o.errorMessage:o.error.message,[c,a]=u.split(" : ");if(c==="FEDERATED_USER_ID_ALREADY_LINKED")throw lt(r,"credential-already-in-use",o);if(c==="EMAIL_EXISTS")throw lt(r,"email-already-in-use",o);if(c==="USER_DISABLED")throw lt(r,"user-disabled",o);const l=n[c]||c.toLowerCase().replace(/[_\s]+/g,"-");if(a)throw W0(r,l,a);oe(r,l)}}catch(i){if(i instanceof he)throw i;oe(r,"network-request-failed",{message:String(i)})}}async function j0(r,e,t,n,i={}){const s=await Te(r,e,t,n,i);return"mfaPendingCredential"in s&&oe(r,"multi-factor-auth-required",{_serverResponse:s}),s}function G0(r,e,t,n){const i=`${e}${t}?${n}`;return r.config.emulator?Cr(r.config,i):`${r.config.apiScheme}://${i}`}class va{constructor(e){this.auth=e,this.timer=null,this.promise=new Promise((t,n)=>{this.timer=setTimeout(()=>n(ne(this.auth,"network-request-failed")),pa.get())})}clearNetworkTimeout(){clearTimeout(this.timer)}}function lt(r,e,t){const n={appName:r.name};t.email&&(n.email=t.email),t.phoneNumber&&(n.phoneNumber=t.phoneNumber);const i=ne(r,e,n);return i.customData._tokenResponse=t,i}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function ga(r,e){return Te(r,"POST","/v1/accounts:delete",e)}async function K0(r,e){return Te(r,"POST","/v1/accounts:lookup",e)}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function je(r){if(r)try{const e=new Date(Number(r));if(!isNaN(e.getTime()))return e.toUTCString()}catch{}}async function _a(r,e=!1){const t=Ie(r),n=await t.getIdToken(e),i=br(n);N(i&&i.exp&&i.auth_time&&i.iat,t.auth,"internal-error");const s=typeof i.firebase=="object"?i.firebase:void 0,o=s==null?void 0:s.sign_in_provider;return{claims:i,token:n,authTime:je(Ar(i.auth_time)),issuedAtTime:je(Ar(i.iat)),expirationTime:je(Ar(i.exp)),signInProvider:o||null,signInSecondFactor:(s==null?void 0:s.sign_in_second_factor)||null}}function Ar(r){return Number(r)*1e3}function br(r){const[e,t,n]=r.split(".");if(e===void 0||t===void 0||n===void 0)return ot("JWT malformed, contained fewer than 3 sections"),null;try{const i=C0(t);return i?JSON.parse(i):(ot("Failed to decode base64 JWT payload"),null)}catch(i){return ot("Caught error parsing JWT payload as JSON",i==null?void 0:i.toString()),null}}function X0(r){const e=br(r);return N(e,"internal-error"),N(typeof e.exp<"u","internal-error"),N(typeof e.iat<"u","internal-error"),Number(e.exp)-Number(e.iat)}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function Ge(r,e,t=!1){if(t)return e;try{return await e}catch(n){throw n instanceof he&&Ea(n)&&r.auth.currentUser===r&&await r.auth.signOut(),n}}function Ea({code:r}){return r==="auth/user-disabled"||r==="auth/user-token-expired"}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class ma{constructor(e){this.user=e,this.isRunning=!1,this.timerId=null,this.errorBackoff=3e4}_start(){this.isRunning||(this.isRunning=!0,this.schedule())}_stop(){this.isRunning&&(this.isRunning=!1,this.timerId!==null&&clearTimeout(this.timerId))}getInterval(e){var t;if(e){const n=this.errorBackoff;return this.errorBackoff=Math.min(this.errorBackoff*2,96e4),n}else{this.errorBackoff=3e4;const i=((t=this.user.stsTokenManager.expirationTime)!==null&&t!==void 0?t:0)-Date.now()-3e5;return Math.max(0,i)}}schedule(e=!1){if(!this.isRunning)return;const t=this.getInterval(e);this.timerId=setTimeout(async()=>{await this.iteration()},t)}async iteration(){try{await this.user.getIdToken(!0)}catch(e){(e==null?void 0:e.code)==="auth/network-request-failed"&&this.schedule(!0);return}this.schedule()}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Br{constructor(e,t){this.createdAt=e,this.lastLoginAt=t,this._initializeTime()}_initializeTime(){this.lastSignInTime=je(this.lastLoginAt),this.creationTime=je(this.createdAt)}_copy(e){this.createdAt=e.createdAt,this.lastLoginAt=e.lastLoginAt,this._initializeTime()}toJSON(){return{createdAt:this.createdAt,lastLoginAt:this.lastLoginAt}}}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function ut(r){var e;const t=r.auth,n=await r.getIdToken(),i=await Ge(r,K0(t,{idToken:n}));N(i==null?void 0:i.users.length,t,"internal-error");const s=i.users[0];r._notifyReloadListener(s);const o=!((e=s.providerUserInfo)===null||e===void 0)&&e.length?Y0(s.providerUserInfo):[],u=Aa(r.providerData,o),c=r.isAnonymous,a=!(r.email&&s.passwordHash)&&!(u!=null&&u.length),l=c?a:!1,E={uid:s.localId,displayName:s.displayName||null,photoURL:s.photoUrl||null,email:s.email||null,emailVerified:s.emailVerified||!1,phoneNumber:s.phoneNumber||null,tenantId:s.tenantId||null,providerData:u,metadata:new Br(s.createdAt,s.lastLoginAt),isAnonymous:l};Object.assign(r,E)}async function Ca(r){const e=Ie(r);await ut(e),await e.auth._persistUserIfCurrent(e),e.auth._notifyListenersIfCurrent(e)}function Aa(r,e){return[...r.filter(n=>!e.some(i=>i.providerId===n.providerId)),...e]}function Y0(r){return r.map(e=>{var{providerId:t}=e,n=_r(e,["providerId"]);return{providerId:t,uid:n.rawId||"",displayName:n.displayName||null,email:n.email||null,phoneNumber:n.phoneNumber||null,photoURL:n.photoUrl||null}})}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function ba(r,e){const t=await V0(r,{},async()=>{const n=ze({grant_type:"refresh_token",refresh_token:e}).slice(1),{tokenApiHost:i,apiKey:s}=r.config,o=G0(r,i,"/v1/token",`key=${s}`),u=await r._getAdditionalHeaders();return u["Content-Type"]="application/x-www-form-urlencoded",q0.fetch()(o,{method:"POST",headers:u,body:n})});return{accessToken:t.access_token,expiresIn:t.expires_in,refreshToken:t.refresh_token}}async function Ba(r,e){return Te(r,"POST","/v2/accounts:revokeToken",ct(r,e))}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Re{constructor(){this.refreshToken=null,this.accessToken=null,this.expirationTime=null}get isExpired(){return!this.expirationTime||Date.now()>this.expirationTime-3e4}updateFromServerResponse(e){N(e.idToken,"internal-error"),N(typeof e.idToken<"u","internal-error"),N(typeof e.refreshToken<"u","internal-error");const t="expiresIn"in e&&typeof e.expiresIn<"u"?Number(e.expiresIn):X0(e.idToken);this.updateTokensAndExpiration(e.idToken,e.refreshToken,t)}updateFromIdToken(e){N(e.length!==0,"internal-error");const t=X0(e);this.updateTokensAndExpiration(e,null,t)}async getToken(e,t=!1){return!t&&this.accessToken&&!this.isExpired?this.accessToken:(N(this.refreshToken,e,"user-token-expired"),this.refreshToken?(await this.refresh(e,this.refreshToken),this.accessToken):null)}clearRefreshToken(){this.refreshToken=null}async refresh(e,t){const{accessToken:n,refreshToken:i,expiresIn:s}=await ba(e,t);this.updateTokensAndExpiration(n,i,Number(s))}updateTokensAndExpiration(e,t,n){this.refreshToken=t||null,this.accessToken=e||null,this.expirationTime=Date.now()+n*1e3}static fromJSON(e,t){const{refreshToken:n,accessToken:i,expirationTime:s}=t,o=new Re;return n&&(N(typeof n=="string","internal-error",{appName:e}),o.refreshToken=n),i&&(N(typeof i=="string","internal-error",{appName:e}),o.accessToken=i),s&&(N(typeof s=="number","internal-error",{appName:e}),o.expirationTime=s),o}toJSON(){return{refreshToken:this.refreshToken,accessToken:this.accessToken,expirationTime:this.expirationTime}}_assign(e){this.accessToken=e.accessToken,this.refreshToken=e.refreshToken,this.expirationTime=e.expirationTime}_clone(){return Object.assign(new Re,this.toJSON())}_performRefresh(){return ce("not implemented")}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function ge(r,e){N(typeof r=="string"||typeof r>"u","internal-error",{appName:e})}class ue{constructor(e){var{uid:t,auth:n,stsTokenManager:i}=e,s=_r(e,["uid","auth","stsTokenManager"]);this.providerId="firebase",this.proactiveRefresh=new ma(this),this.reloadUserInfo=null,this.reloadListener=null,this.uid=t,this.auth=n,this.stsTokenManager=i,this.accessToken=i.accessToken,this.displayName=s.displayName||null,this.email=s.email||null,this.emailVerified=s.emailVerified||!1,this.phoneNumber=s.phoneNumber||null,this.photoURL=s.photoURL||null,this.isAnonymous=s.isAnonymous||!1,this.tenantId=s.tenantId||null,this.providerData=s.providerData?[...s.providerData]:[],this.metadata=new Br(s.createdAt||void 0,s.lastLoginAt||void 0)}async getIdToken(e){const t=await Ge(this,this.stsTokenManager.getToken(this.auth,e));return N(t,this.auth,"internal-error"),this.accessToken!==t&&(this.accessToken=t,await this.auth._persistUserIfCurrent(this),this.auth._notifyListenersIfCurrent(this)),t}getIdTokenResult(e){return _a(this,e)}reload(){return Ca(this)}_assign(e){this!==e&&(N(this.uid===e.uid,this.auth,"internal-error"),this.displayName=e.displayName,this.photoURL=e.photoURL,this.email=e.email,this.emailVerified=e.emailVerified,this.phoneNumber=e.phoneNumber,this.isAnonymous=e.isAnonymous,this.tenantId=e.tenantId,this.providerData=e.providerData.map(t=>Object.assign({},t)),this.metadata._copy(e.metadata),this.stsTokenManager._assign(e.stsTokenManager))}_clone(e){const t=new ue(Object.assign(Object.assign({},this),{auth:e,stsTokenManager:this.stsTokenManager._clone()}));return t.metadata._copy(this.metadata),t}_onReload(e){N(!this.reloadListener,this.auth,"internal-error"),this.reloadListener=e,this.reloadUserInfo&&(this._notifyReloadListener(this.reloadUserInfo),this.reloadUserInfo=null)}_notifyReloadListener(e){this.reloadListener?this.reloadListener(e):this.reloadUserInfo=e}_startProactiveRefresh(){this.proactiveRefresh._start()}_stopProactiveRefresh(){this.proactiveRefresh._stop()}async _updateTokensIfNecessary(e,t=!1){let n=!1;e.idToken&&e.idToken!==this.stsTokenManager.accessToken&&(this.stsTokenManager.updateFromServerResponse(e),n=!0),t&&await ut(this),await this.auth._persistUserIfCurrent(this),n&&this.auth._notifyListenersIfCurrent(this)}async delete(){if(ae(this.auth.app))return Promise.reject(ve(this.auth));const e=await this.getIdToken();return await Ge(this,ga(this.auth,{idToken:e})),this.stsTokenManager.clearRefreshToken(),this.auth.signOut()}toJSON(){return Object.assign(Object.assign({uid:this.uid,email:this.email||void 0,emailVerified:this.emailVerified,displayName:this.displayName||void 0,isAnonymous:this.isAnonymous,photoURL:this.photoURL||void 0,phoneNumber:this.phoneNumber||void 0,tenantId:this.tenantId||void 0,providerData:this.providerData.map(e=>Object.assign({},e)),stsTokenManager:this.stsTokenManager.toJSON(),_redirectEventId:this._redirectEventId},this.metadata.toJSON()),{apiKey:this.auth.config.apiKey,appName:this.auth.name})}get refreshToken(){return this.stsTokenManager.refreshToken||""}static _fromJSON(e,t){var n,i,s,o,u,c,a,l;const E=(n=t.displayName)!==null&&n!==void 0?n:void 0,d=(i=t.email)!==null&&i!==void 0?i:void 0,p=(s=t.phoneNumber)!==null&&s!==void 0?s:void 0,f=(o=t.photoURL)!==null&&o!==void 0?o:void 0,_=(u=t.tenantId)!==null&&u!==void 0?u:void 0,v=(c=t._redirectEventId)!==null&&c!==void 0?c:void 0,C=(a=t.createdAt)!==null&&a!==void 0?a:void 0,x=(l=t.lastLoginAt)!==null&&l!==void 0?l:void 0,{uid:h,emailVerified:g,isAnonymous:b,providerData:A,stsTokenManager:B}=t;N(h&&B,e,"internal-error");const D=Re.fromJSON(this.name,B);N(typeof h=="string",e,"internal-error"),ge(E,e.name),ge(d,e.name),N(typeof g=="boolean",e,"internal-error"),N(typeof b=="boolean",e,"internal-error"),ge(p,e.name),ge(f,e.name),ge(_,e.name),ge(v,e.name),ge(C,e.name),ge(x,e.name);const R=new ue({uid:h,auth:e,email:d,emailVerified:g,displayName:E,isAnonymous:b,photoURL:f,phoneNumber:p,tenantId:_,stsTokenManager:D,createdAt:C,lastLoginAt:x});return A&&Array.isArray(A)&&(R.providerData=A.map(m=>Object.assign({},m))),v&&(R._redirectEventId=v),R}static async _fromIdTokenResponse(e,t,n=!1){const i=new Re;i.updateFromServerResponse(t);const s=new ue({uid:t.localId,auth:e,stsTokenManager:i,isAnonymous:n});return await ut(s),s}static async _fromGetAccountInfoResponse(e,t,n){const i=t.users[0];N(i.localId!==void 0,"internal-error");const s=i.providerUserInfo!==void 0?Y0(i.providerUserInfo):[],o=!(i.email&&i.passwordHash)&&!(s!=null&&s.length),u=new Re;u.updateFromIdToken(n);const c=new ue({uid:i.localId,auth:e,stsTokenManager:u,isAnonymous:o}),a={uid:i.localId,displayName:i.displayName||null,photoURL:i.photoUrl||null,email:i.email||null,emailVerified:i.emailVerified||!1,phoneNumber:i.phoneNumber||null,tenantId:i.tenantId||null,providerData:s,metadata:new Br(i.createdAt,i.lastLoginAt),isAnonymous:!(i.email&&i.passwordHash)&&!(s!=null&&s.length)};return Object.assign(c,a),c}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const Z0=new Map;function de(r){le(r instanceof Function,"Expected a class definition");let e=Z0.get(r);return e?(le(e instanceof r,"Instance stored in cache mismatched with class"),e):(e=new r,Z0.set(r,e),e)}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Q0{constructor(){this.type="NONE",this.storage={}}async _isAvailable(){return!0}async _set(e,t){this.storage[e]=t}async _get(e){const t=this.storage[e];return t===void 0?null:t}async _remove(e){delete this.storage[e]}_addListener(e,t){}_removeListener(e,t){}}Q0.type="NONE";const J0=Q0;/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function dt(r,e,t){return`firebase:${r}:${e}:${t}`}class Pe{constructor(e,t,n){this.persistence=e,this.auth=t,this.userKey=n;const{config:i,name:s}=this.auth;this.fullUserKey=dt(this.userKey,i.apiKey,s),this.fullPersistenceKey=dt("persistence",i.apiKey,s),this.boundEventHandler=t._onStorageEvent.bind(t),this.persistence._addListener(this.fullUserKey,this.boundEventHandler)}setCurrentUser(e){return this.persistence._set(this.fullUserKey,e.toJSON())}async getCurrentUser(){const e=await this.persistence._get(this.fullUserKey);return e?ue._fromJSON(this.auth,e):null}removeCurrentUser(){return this.persistence._remove(this.fullUserKey)}savePersistenceForRedirect(){return this.persistence._set(this.fullPersistenceKey,this.persistence.type)}async setPersistence(e){if(this.persistence===e)return;const t=await this.getCurrentUser();if(await this.removeCurrentUser(),this.persistence=e,t)return this.setCurrentUser(t)}delete(){this.persistence._removeListener(this.fullUserKey,this.boundEventHandler)}static async create(e,t,n="authUser"){if(!t.length)return new Pe(de(J0),e,n);const i=(await Promise.all(t.map(async a=>{if(await a._isAvailable())return a}))).filter(a=>a);let s=i[0]||de(J0);const o=dt(n,e.config.apiKey,e.name);let u=null;for(const a of t)try{const l=await a._get(o);if(l){const E=ue._fromJSON(e,l);a!==s&&(u=E),s=a;break}}catch{}const c=i.filter(a=>a._shouldAllowMigration);return!s._shouldAllowMigration||!c.length?new Pe(s,e,n):(s=c[0],u&&await s._set(o,u.toJSON()),await Promise.all(t.map(async a=>{if(a!==s)try{await a._remove(o)}catch{}})),new Pe(s,e,n))}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function en(r){const e=r.toLowerCase();if(e.includes("opera/")||e.includes("opr/")||e.includes("opios/"))return"Opera";if(sn(e))return"IEMobile";if(e.includes("msie")||e.includes("trident/"))return"IE";if(e.includes("edge/"))return"Edge";if(tn(e))return"Firefox";if(e.includes("silk/"))return"Silk";if(on(e))return"Blackberry";if(cn(e))return"Webos";if(rn(e))return"Safari";if((e.includes("chrome/")||nn(e))&&!e.includes("edge/"))return"Chrome";if(an(e))return"Android";{const t=/([a-zA-Z\d\.]+)\/[a-zA-Z\d\.]*$/,n=r.match(t);if((n==null?void 0:n.length)===2)return n[1]}return"Other"}function tn(r=Y()){return/firefox\//i.test(r)}function rn(r=Y()){const e=r.toLowerCase();return e.includes("safari/")&&!e.includes("chrome/")&&!e.includes("crios/")&&!e.includes("android")}function nn(r=Y()){return/crios\//i.test(r)}function sn(r=Y()){return/iemobile/i.test(r)}function an(r=Y()){return/android/i.test(r)}function on(r=Y()){return/blackberry/i.test(r)}function cn(r=Y()){return/webos/i.test(r)}function yr(r=Y()){return/iphone|ipad|ipod/i.test(r)||/macintosh/i.test(r)&&/mobile/i.test(r)}function ya(r=Y()){var e;return yr(r)&&!!(!((e=window.navigator)===null||e===void 0)&&e.standalone)}function Da(){return $i()&&document.documentMode===10}function ln(r=Y()){return yr(r)||an(r)||cn(r)||on(r)||/windows phone/i.test(r)||sn(r)}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function un(r,e=[]){let t;switch(r){case"Browser":t=en(Y());break;case"Worker":t=`${en(Y())}-${r}`;break;default:t=r}const n=e.length?e.join(","):"FirebaseCore-web";return`${t}/JsCore/${$e}/${n}`}/**
 * @license
 * Copyright 2022 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class wa{constructor(e){this.auth=e,this.queue=[]}pushCallback(e,t){const n=s=>new Promise((o,u)=>{try{const c=e(s);o(c)}catch(c){u(c)}});n.onAbort=t,this.queue.push(n);const i=this.queue.length-1;return()=>{this.queue[i]=()=>Promise.resolve()}}async runMiddleware(e){if(this.auth.currentUser===e)return;const t=[];try{for(const n of this.queue)await n(e),n.onAbort&&t.push(n.onAbort)}catch(n){t.reverse();for(const i of t)try{i()}catch{}throw this.auth._errorFactory.create("login-blocked",{originalMessage:n==null?void 0:n.message})}}}/**
 * @license
 * Copyright 2023 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function Fa(r,e={}){return Te(r,"GET","/v2/passwordPolicy",ct(r,e))}/**
 * @license
 * Copyright 2023 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const Ia=6;class ka{constructor(e){var t,n,i,s;const o=e.customStrengthOptions;this.customStrengthOptions={},this.customStrengthOptions.minPasswordLength=(t=o.minPasswordLength)!==null&&t!==void 0?t:Ia,o.maxPasswordLength&&(this.customStrengthOptions.maxPasswordLength=o.maxPasswordLength),o.containsLowercaseCharacter!==void 0&&(this.customStrengthOptions.containsLowercaseLetter=o.containsLowercaseCharacter),o.containsUppercaseCharacter!==void 0&&(this.customStrengthOptions.containsUppercaseLetter=o.containsUppercaseCharacter),o.containsNumericCharacter!==void 0&&(this.customStrengthOptions.containsNumericCharacter=o.containsNumericCharacter),o.containsNonAlphanumericCharacter!==void 0&&(this.customStrengthOptions.containsNonAlphanumericCharacter=o.containsNonAlphanumericCharacter),this.enforcementState=e.enforcementState,this.enforcementState==="ENFORCEMENT_STATE_UNSPECIFIED"&&(this.enforcementState="OFF"),this.allowedNonAlphanumericCharacters=(i=(n=e.allowedNonAlphanumericCharacters)===null||n===void 0?void 0:n.join(""))!==null&&i!==void 0?i:"",this.forceUpgradeOnSignin=(s=e.forceUpgradeOnSignin)!==null&&s!==void 0?s:!1,this.schemaVersion=e.schemaVersion}validatePassword(e){var t,n,i,s,o,u;const c={isValid:!0,passwordPolicy:this};return this.validatePasswordLengthOptions(e,c),this.validatePasswordCharacterOptions(e,c),c.isValid&&(c.isValid=(t=c.meetsMinPasswordLength)!==null&&t!==void 0?t:!0),c.isValid&&(c.isValid=(n=c.meetsMaxPasswordLength)!==null&&n!==void 0?n:!0),c.isValid&&(c.isValid=(i=c.containsLowercaseLetter)!==null&&i!==void 0?i:!0),c.isValid&&(c.isValid=(s=c.containsUppercaseLetter)!==null&&s!==void 0?s:!0),c.isValid&&(c.isValid=(o=c.containsNumericCharacter)!==null&&o!==void 0?o:!0),c.isValid&&(c.isValid=(u=c.containsNonAlphanumericCharacter)!==null&&u!==void 0?u:!0),c}validatePasswordLengthOptions(e,t){const n=this.customStrengthOptions.minPasswordLength,i=this.customStrengthOptions.maxPasswordLength;n&&(t.meetsMinPasswordLength=e.length>=n),i&&(t.meetsMaxPasswordLength=e.length<=i)}validatePasswordCharacterOptions(e,t){this.updatePasswordCharacterOptionsStatuses(t,!1,!1,!1,!1);let n;for(let i=0;i<e.length;i++)n=e.charAt(i),this.updatePasswordCharacterOptionsStatuses(t,n>="a"&&n<="z",n>="A"&&n<="Z",n>="0"&&n<="9",this.allowedNonAlphanumericCharacters.includes(n))}updatePasswordCharacterOptionsStatuses(e,t,n,i,s){this.customStrengthOptions.containsLowercaseLetter&&(e.containsLowercaseLetter||(e.containsLowercaseLetter=t)),this.customStrengthOptions.containsUppercaseLetter&&(e.containsUppercaseLetter||(e.containsUppercaseLetter=n)),this.customStrengthOptions.containsNumericCharacter&&(e.containsNumericCharacter||(e.containsNumericCharacter=i)),this.customStrengthOptions.containsNonAlphanumericCharacter&&(e.containsNonAlphanumericCharacter||(e.containsNonAlphanumericCharacter=s))}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Sa{constructor(e,t,n,i){this.app=e,this.heartbeatServiceProvider=t,this.appCheckServiceProvider=n,this.config=i,this.currentUser=null,this.emulatorConfig=null,this.operations=Promise.resolve(),this.authStateSubscription=new dn(this),this.idTokenSubscription=new dn(this),this.beforeStateQueue=new wa(this),this.redirectUser=null,this.isProactiveRefreshEnabled=!1,this.EXPECTED_PASSWORD_POLICY_SCHEMA_VERSION=1,this._canInitEmulator=!0,this._isInitialized=!1,this._deleted=!1,this._initializationPromise=null,this._popupRedirectResolver=null,this._errorFactory=z0,this._agentRecaptchaConfig=null,this._tenantRecaptchaConfigs={},this._projectPasswordPolicy=null,this._tenantPasswordPolicies={},this.lastNotifiedUid=void 0,this.languageCode=null,this.tenantId=null,this.settings={appVerificationDisabledForTesting:!1},this.frameworks=[],this.name=e.name,this.clientVersion=i.sdkClientVersion}_initializeWithPersistence(e,t){return t&&(this._popupRedirectResolver=de(t)),this._initializationPromise=this.queue(async()=>{var n,i;if(!this._deleted&&(this.persistenceManager=await Pe.create(this,e),!this._deleted)){if(!((n=this._popupRedirectResolver)===null||n===void 0)&&n._shouldInitProactively)try{await this._popupRedirectResolver._initialize(this)}catch{}await this.initializeCurrentUser(t),this.lastNotifiedUid=((i=this.currentUser)===null||i===void 0?void 0:i.uid)||null,!this._deleted&&(this._isInitialized=!0)}}),this._initializationPromise}async _onStorageEvent(){if(this._deleted)return;const e=await this.assertedPersistence.getCurrentUser();if(!(!this.currentUser&&!e)){if(this.currentUser&&e&&this.currentUser.uid===e.uid){this._currentUser._assign(e),await this.currentUser.getIdToken();return}await this._updateCurrentUser(e,!0)}}async initializeCurrentUserFromIdToken(e){try{const t=await K0(this,{idToken:e}),n=await ue._fromGetAccountInfoResponse(this,t,e);await this.directlySetCurrentUser(n)}catch(t){console.warn("FirebaseServerApp could not login user with provided authIdToken: ",t),await this.directlySetCurrentUser(null)}}async initializeCurrentUser(e){var t;if(ae(this.app)){const o=this.app.settings.authIdToken;return o?new Promise(u=>{setTimeout(()=>this.initializeCurrentUserFromIdToken(o).then(u,u))}):this.directlySetCurrentUser(null)}const n=await this.assertedPersistence.getCurrentUser();let i=n,s=!1;if(e&&this.config.authDomain){await this.getOrInitRedirectPersistenceManager();const o=(t=this.redirectUser)===null||t===void 0?void 0:t._redirectEventId,u=i==null?void 0:i._redirectEventId,c=await this.tryRedirectSignIn(e);(!o||o===u)&&(c!=null&&c.user)&&(i=c.user,s=!0)}if(!i)return this.directlySetCurrentUser(null);if(!i._redirectEventId){if(s)try{await this.beforeStateQueue.runMiddleware(i)}catch(o){i=n,this._popupRedirectResolver._overrideRedirectResult(this,()=>Promise.reject(o))}return i?this.reloadAndSetCurrentUserOrClear(i):this.directlySetCurrentUser(null)}return N(this._popupRedirectResolver,this,"argument-error"),await this.getOrInitRedirectPersistenceManager(),this.redirectUser&&this.redirectUser._redirectEventId===i._redirectEventId?this.directlySetCurrentUser(i):this.reloadAndSetCurrentUserOrClear(i)}async tryRedirectSignIn(e){let t=null;try{t=await this._popupRedirectResolver._completeRedirectFn(this,e,!0)}catch{await this._setRedirectUser(null)}return t}async reloadAndSetCurrentUserOrClear(e){try{await ut(e)}catch(t){if((t==null?void 0:t.code)!=="auth/network-request-failed")return this.directlySetCurrentUser(null)}return this.directlySetCurrentUser(e)}useDeviceLanguage(){this.languageCode=ha()}async _delete(){this._deleted=!0}async updateCurrentUser(e){if(ae(this.app))return Promise.reject(ve(this));const t=e?Ie(e):null;return t&&N(t.auth.config.apiKey===this.config.apiKey,this,"invalid-user-token"),this._updateCurrentUser(t&&t._clone(this))}async _updateCurrentUser(e,t=!1){if(!this._deleted)return e&&N(this.tenantId===e.tenantId,this,"tenant-id-mismatch"),t||await this.beforeStateQueue.runMiddleware(e),this.queue(async()=>{await this.directlySetCurrentUser(e),this.notifyAuthListeners()})}async signOut(){return ae(this.app)?Promise.reject(ve(this)):(await this.beforeStateQueue.runMiddleware(null),(this.redirectPersistenceManager||this._popupRedirectResolver)&&await this._setRedirectUser(null),this._updateCurrentUser(null,!0))}setPersistence(e){return ae(this.app)?Promise.reject(ve(this)):this.queue(async()=>{await this.assertedPersistence.setPersistence(de(e))})}_getRecaptchaConfig(){return this.tenantId==null?this._agentRecaptchaConfig:this._tenantRecaptchaConfigs[this.tenantId]}async validatePassword(e){this._getPasswordPolicyInternal()||await this._updatePasswordPolicy();const t=this._getPasswordPolicyInternal();return t.schemaVersion!==this.EXPECTED_PASSWORD_POLICY_SCHEMA_VERSION?Promise.reject(this._errorFactory.create("unsupported-password-policy-schema-version",{})):t.validatePassword(e)}_getPasswordPolicyInternal(){return this.tenantId===null?this._projectPasswordPolicy:this._tenantPasswordPolicies[this.tenantId]}async _updatePasswordPolicy(){const e=await Fa(this),t=new ka(e);this.tenantId===null?this._projectPasswordPolicy=t:this._tenantPasswordPolicies[this.tenantId]=t}_getPersistence(){return this.assertedPersistence.persistence.type}_updateErrorMap(e){this._errorFactory=new Ue("auth","Firebase",e())}onAuthStateChanged(e,t,n){return this.registerStateListener(this.authStateSubscription,e,t,n)}beforeAuthStateChanged(e,t){return this.beforeStateQueue.pushCallback(e,t)}onIdTokenChanged(e,t,n){return this.registerStateListener(this.idTokenSubscription,e,t,n)}authStateReady(){return new Promise((e,t)=>{if(this.currentUser)e();else{const n=this.onAuthStateChanged(()=>{n(),e()},t)}})}async revokeAccessToken(e){if(this.currentUser){const t=await this.currentUser.getIdToken(),n={providerId:"apple.com",tokenType:"ACCESS_TOKEN",token:e,idToken:t};this.tenantId!=null&&(n.tenantId=this.tenantId),await Ba(this,n)}}toJSON(){var e;return{apiKey:this.config.apiKey,authDomain:this.config.authDomain,appName:this.name,currentUser:(e=this._currentUser)===null||e===void 0?void 0:e.toJSON()}}async _setRedirectUser(e,t){const n=await this.getOrInitRedirectPersistenceManager(t);return e===null?n.removeCurrentUser():n.setCurrentUser(e)}async getOrInitRedirectPersistenceManager(e){if(!this.redirectPersistenceManager){const t=e&&de(e)||this._popupRedirectResolver;N(t,this,"argument-error"),this.redirectPersistenceManager=await Pe.create(this,[de(t._redirectPersistence)],"redirectUser"),this.redirectUser=await this.redirectPersistenceManager.getCurrentUser()}return this.redirectPersistenceManager}async _redirectUserForId(e){var t,n;return this._isInitialized&&await this.queue(async()=>{}),((t=this._currentUser)===null||t===void 0?void 0:t._redirectEventId)===e?this._currentUser:((n=this.redirectUser)===null||n===void 0?void 0:n._redirectEventId)===e?this.redirectUser:null}async _persistUserIfCurrent(e){if(e===this.currentUser)return this.queue(async()=>this.directlySetCurrentUser(e))}_notifyListenersIfCurrent(e){e===this.currentUser&&this.notifyAuthListeners()}_key(){return`${this.config.authDomain}:${this.config.apiKey}:${this.name}`}_startProactiveRefresh(){this.isProactiveRefreshEnabled=!0,this.currentUser&&this._currentUser._startProactiveRefresh()}_stopProactiveRefresh(){this.isProactiveRefreshEnabled=!1,this.currentUser&&this._currentUser._stopProactiveRefresh()}get _currentUser(){return this.currentUser}notifyAuthListeners(){var e,t;if(!this._isInitialized)return;this.idTokenSubscription.next(this.currentUser);const n=(t=(e=this.currentUser)===null||e===void 0?void 0:e.uid)!==null&&t!==void 0?t:null;this.lastNotifiedUid!==n&&(this.lastNotifiedUid=n,this.authStateSubscription.next(this.currentUser))}registerStateListener(e,t,n,i){if(this._deleted)return()=>{};const s=typeof t=="function"?t:t.next.bind(t);let o=!1;const u=this._isInitialized?Promise.resolve():this._initializationPromise;if(N(u,this,"internal-error"),u.then(()=>{o||s(this.currentUser)}),typeof t=="function"){const c=e.addObserver(t,n,i);return()=>{o=!0,c()}}else{const c=e.addObserver(t);return()=>{o=!0,c()}}}async directlySetCurrentUser(e){this.currentUser&&this.currentUser!==e&&this._currentUser._stopProactiveRefresh(),e&&this.isProactiveRefreshEnabled&&e._startProactiveRefresh(),this.currentUser=e,e?await this.assertedPersistence.setCurrentUser(e):await this.assertedPersistence.removeCurrentUser()}queue(e){return this.operations=this.operations.then(e,e),this.operations}get assertedPersistence(){return N(this.persistenceManager,this,"internal-error"),this.persistenceManager}_logFramework(e){!e||this.frameworks.includes(e)||(this.frameworks.push(e),this.frameworks.sort(),this.clientVersion=un(this.config.clientPlatform,this._getFrameworks()))}_getFrameworks(){return this.frameworks}async _getAdditionalHeaders(){var e;const t={"X-Client-Version":this.clientVersion};this.app.options.appId&&(t["X-Firebase-gmpid"]=this.app.options.appId);const n=await((e=this.heartbeatServiceProvider.getImmediate({optional:!0}))===null||e===void 0?void 0:e.getHeartbeatsHeader());n&&(t["X-Firebase-Client"]=n);const i=await this._getAppCheckToken();return i&&(t["X-Firebase-AppCheck"]=i),t}async _getAppCheckToken(){var e;const t=await((e=this.appCheckServiceProvider.getImmediate({optional:!0}))===null||e===void 0?void 0:e.getToken());return t!=null&&t.error&&ua(`Error while retrieving App Check token: ${t.error}`),t==null?void 0:t.token}}function xt(r){return Ie(r)}class dn{constructor(e){this.auth=e,this.observer=null,this.addObserver=Yi(t=>this.observer=t)}get next(){return N(this.observer,this.auth,"internal-error"),this.observer.next.bind(this.observer)}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */let Dr={async loadJS(){throw new Error("Unable to load external scripts")},recaptchaV2Script:"",recaptchaEnterpriseScript:"",gapiScript:""};function Ta(r){Dr=r}function Ra(r){return Dr.loadJS(r)}function Pa(){return Dr.gapiScript}function Oa(r){return`__${r}${Math.floor(Math.random()*1e6)}`}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function Na(r,e){const t=R0(r,"auth");if(t.isInitialized()){const i=t.getImmediate(),s=t.getOptions();if(it(s,e??{}))return i;oe(i,"already-initialized")}return t.initialize({options:e})}function La(r,e){const t=(e==null?void 0:e.persistence)||[],n=(Array.isArray(t)?t:[t]).map(de);e!=null&&e.errorMap&&r._updateErrorMap(e.errorMap),r._initializeWithPersistence(n,e==null?void 0:e.popupRedirectResolver)}function Ha(r,e,t){const n=xt(r);N(n._canInitEmulator,n,"emulator-config-failed"),N(/^https?:\/\//.test(e),n,"invalid-emulator-scheme");const i=!1,s=xn(e),{host:o,port:u}=Ma(e),c=u===null?"":`:${u}`;n.config.emulator={url:`${s}//${o}${c}/`},n.settings.appVerificationDisabledForTesting=!0,n.emulatorConfig=Object.freeze({host:o,port:u,protocol:s.replace(":",""),options:Object.freeze({disableWarnings:i})}),Ua()}function xn(r){const e=r.indexOf(":");return e<0?"":r.substr(0,e+1)}function Ma(r){const e=xn(r),t=/(\/\/)?([^?#/]+)/.exec(r.substr(e.length));if(!t)return{host:"",port:null};const n=t[2].split("@").pop()||"",i=/^(\[[^\]]+\])(:|$)/.exec(n);if(i){const s=i[1];return{host:s,port:hn(n.substr(s.length+1))}}else{const[s,o]=n.split(":");return{host:s,port:hn(o)}}}function hn(r){if(!r)return null;const e=Number(r);return isNaN(e)?null:e}function Ua(){function r(){const e=document.createElement("p"),t=e.style;e.innerText="Running in emulator mode. Do not use with production credentials.",t.position="fixed",t.width="100%",t.backgroundColor="#ffffff",t.border=".1em solid #000000",t.color="#b50000",t.bottom="0px",t.left="0px",t.margin="0px",t.zIndex="10000",t.textAlign="center",e.classList.add("firebase-emulator-warning"),document.body.appendChild(e)}typeof console<"u"&&typeof console.info=="function"&&console.info("WARNING: You are using the Auth Emulator, which is intended for local testing only.  Do not use with production credentials."),typeof window<"u"&&typeof document<"u"&&(document.readyState==="loading"?window.addEventListener("DOMContentLoaded",r):r())}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class fn{constructor(e,t){this.providerId=e,this.signInMethod=t}toJSON(){return ce("not implemented")}_getIdTokenResponse(e){return ce("not implemented")}_linkToIdToken(e,t){return ce("not implemented")}_getReauthenticationResolver(e){return ce("not implemented")}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function Oe(r,e){return j0(r,"POST","/v1/accounts:signInWithIdp",ct(r,e))}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const za="http://localhost";class Fe extends fn{constructor(){super(...arguments),this.pendingToken=null}static _fromParams(e){const t=new Fe(e.providerId,e.signInMethod);return e.idToken||e.accessToken?(e.idToken&&(t.idToken=e.idToken),e.accessToken&&(t.accessToken=e.accessToken),e.nonce&&!e.pendingToken&&(t.nonce=e.nonce),e.pendingToken&&(t.pendingToken=e.pendingToken)):e.oauthToken&&e.oauthTokenSecret?(t.accessToken=e.oauthToken,t.secret=e.oauthTokenSecret):oe("argument-error"),t}toJSON(){return{idToken:this.idToken,accessToken:this.accessToken,secret:this.secret,nonce:this.nonce,pendingToken:this.pendingToken,providerId:this.providerId,signInMethod:this.signInMethod}}static fromJSON(e){const t=typeof e=="string"?JSON.parse(e):e,{providerId:n,signInMethod:i}=t,s=_r(t,["providerId","signInMethod"]);if(!n||!i)return null;const o=new Fe(n,i);return o.idToken=s.idToken||void 0,o.accessToken=s.accessToken||void 0,o.secret=s.secret,o.nonce=s.nonce,o.pendingToken=s.pendingToken||null,o}_getIdTokenResponse(e){const t=this.buildRequest();return Oe(e,t)}_linkToIdToken(e,t){const n=this.buildRequest();return n.idToken=t,Oe(e,n)}_getReauthenticationResolver(e){const t=this.buildRequest();return t.autoCreate=!1,Oe(e,t)}buildRequest(){const e={requestUri:za,returnSecureToken:!0};if(this.pendingToken)e.pendingToken=this.pendingToken;else{const t={};this.idToken&&(t.id_token=this.idToken),this.accessToken&&(t.access_token=this.accessToken),this.secret&&(t.oauth_token_secret=this.secret),t.providerId=this.providerId,this.nonce&&!this.pendingToken&&(t.nonce=this.nonce),e.postBody=ze(t)}return e}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class pn{constructor(e){this.providerId=e,this.defaultLanguageCode=null,this.customParameters={}}setDefaultLanguage(e){this.defaultLanguageCode=e}setCustomParameters(e){return this.customParameters=e,this}getCustomParameters(){return this.customParameters}}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Ke extends pn{constructor(){super(...arguments),this.scopes=[]}addScope(e){return this.scopes.includes(e)||this.scopes.push(e),this}getScopes(){return[...this.scopes]}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class _e extends Ke{constructor(){super("facebook.com")}static credential(e){return Fe._fromParams({providerId:_e.PROVIDER_ID,signInMethod:_e.FACEBOOK_SIGN_IN_METHOD,accessToken:e})}static credentialFromResult(e){return _e.credentialFromTaggedObject(e)}static credentialFromError(e){return _e.credentialFromTaggedObject(e.customData||{})}static credentialFromTaggedObject({_tokenResponse:e}){if(!e||!("oauthAccessToken"in e)||!e.oauthAccessToken)return null;try{return _e.credential(e.oauthAccessToken)}catch{return null}}}_e.FACEBOOK_SIGN_IN_METHOD="facebook.com",_e.PROVIDER_ID="facebook.com";/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Ee extends Ke{constructor(){super("google.com"),this.addScope("profile")}static credential(e,t){return Fe._fromParams({providerId:Ee.PROVIDER_ID,signInMethod:Ee.GOOGLE_SIGN_IN_METHOD,idToken:e,accessToken:t})}static credentialFromResult(e){return Ee.credentialFromTaggedObject(e)}static credentialFromError(e){return Ee.credentialFromTaggedObject(e.customData||{})}static credentialFromTaggedObject({_tokenResponse:e}){if(!e)return null;const{oauthIdToken:t,oauthAccessToken:n}=e;if(!t&&!n)return null;try{return Ee.credential(t,n)}catch{return null}}}Ee.GOOGLE_SIGN_IN_METHOD="google.com",Ee.PROVIDER_ID="google.com";/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class me extends Ke{constructor(){super("github.com")}static credential(e){return Fe._fromParams({providerId:me.PROVIDER_ID,signInMethod:me.GITHUB_SIGN_IN_METHOD,accessToken:e})}static credentialFromResult(e){return me.credentialFromTaggedObject(e)}static credentialFromError(e){return me.credentialFromTaggedObject(e.customData||{})}static credentialFromTaggedObject({_tokenResponse:e}){if(!e||!("oauthAccessToken"in e)||!e.oauthAccessToken)return null;try{return me.credential(e.oauthAccessToken)}catch{return null}}}me.GITHUB_SIGN_IN_METHOD="github.com",me.PROVIDER_ID="github.com";/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Ce extends Ke{constructor(){super("twitter.com")}static credential(e,t){return Fe._fromParams({providerId:Ce.PROVIDER_ID,signInMethod:Ce.TWITTER_SIGN_IN_METHOD,oauthToken:e,oauthTokenSecret:t})}static credentialFromResult(e){return Ce.credentialFromTaggedObject(e)}static credentialFromError(e){return Ce.credentialFromTaggedObject(e.customData||{})}static credentialFromTaggedObject({_tokenResponse:e}){if(!e)return null;const{oauthAccessToken:t,oauthTokenSecret:n}=e;if(!t||!n)return null;try{return Ce.credential(t,n)}catch{return null}}}Ce.TWITTER_SIGN_IN_METHOD="twitter.com",Ce.PROVIDER_ID="twitter.com";/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function Wa(r,e){return j0(r,"POST","/v1/accounts:signUp",ct(r,e))}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Ae{constructor(e){this.user=e.user,this.providerId=e.providerId,this._tokenResponse=e._tokenResponse,this.operationType=e.operationType}static async _fromIdTokenResponse(e,t,n,i=!1){const s=await ue._fromIdTokenResponse(e,n,i),o=vn(n);return new Ae({user:s,providerId:o,_tokenResponse:n,operationType:t})}static async _forOperation(e,t,n){await e._updateTokensIfNecessary(n,!0);const i=vn(n);return new Ae({user:e,providerId:i,_tokenResponse:n,operationType:t})}}function vn(r){return r.providerId?r.providerId:"phoneNumber"in r?"phone":null}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function $a(r){var e;if(ae(r.app))return Promise.reject(ve(r));const t=xt(r);if(await t._initializationPromise,!((e=t.currentUser)===null||e===void 0)&&e.isAnonymous)return new Ae({user:t.currentUser,providerId:null,operationType:"signIn"});const n=await Wa(t,{returnSecureToken:!0}),i=await Ae._fromIdTokenResponse(t,"signIn",n,!0);return await t._updateCurrentUser(i.user),i}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class ht extends he{constructor(e,t,n,i){var s;super(t.code,t.message),this.operationType=n,this.user=i,Object.setPrototypeOf(this,ht.prototype),this.customData={appName:e.name,tenantId:(s=e.tenantId)!==null&&s!==void 0?s:void 0,_serverResponse:t.customData._serverResponse,operationType:n}}static _fromErrorAndOperation(e,t,n,i){return new ht(e,t,n,i)}}function gn(r,e,t,n){return(e==="reauthenticate"?t._getReauthenticationResolver(r):t._getIdTokenResponse(r)).catch(s=>{throw s.code==="auth/multi-factor-auth-required"?ht._fromErrorAndOperation(r,s,e,n):s})}async function qa(r,e,t=!1){const n=await Ge(r,e._linkToIdToken(r.auth,await r.getIdToken()),t);return Ae._forOperation(r,"link",n)}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function Va(r,e,t=!1){const{auth:n}=r;if(ae(n.app))return Promise.reject(ve(n));const i="reauthenticate";try{const s=await Ge(r,gn(n,i,e,r),t);N(s.idToken,n,"internal-error");const o=br(s.idToken);N(o,n,"internal-error");const{sub:u}=o;return N(r.uid===u,n,"user-mismatch"),Ae._forOperation(r,i,s)}catch(s){throw(s==null?void 0:s.code)==="auth/user-not-found"&&oe(n,"user-mismatch"),s}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function ja(r,e,t=!1){if(ae(r.app))return Promise.reject(ve(r));const n="signIn",i=await gn(r,n,e),s=await Ae._fromIdTokenResponse(r,n,i);return t||await r._updateCurrentUser(s.user),s}function Ga(r,e,t,n){return Ie(r).onIdTokenChanged(e,t,n)}function Ka(r,e,t){return Ie(r).beforeAuthStateChanged(e,t)}const ft="__sak";/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class _n{constructor(e,t){this.storageRetriever=e,this.type=t}_isAvailable(){try{return this.storage?(this.storage.setItem(ft,"1"),this.storage.removeItem(ft),Promise.resolve(!0)):Promise.resolve(!1)}catch{return Promise.resolve(!1)}}_set(e,t){return this.storage.setItem(e,JSON.stringify(t)),Promise.resolve()}_get(e){const t=this.storage.getItem(e);return Promise.resolve(t?JSON.parse(t):null)}_remove(e){return this.storage.removeItem(e),Promise.resolve()}get storage(){return this.storageRetriever()}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const Xa=1e3,Ya=10;class En extends _n{constructor(){super(()=>window.localStorage,"LOCAL"),this.boundEventHandler=(e,t)=>this.onStorageEvent(e,t),this.listeners={},this.localCache={},this.pollTimer=null,this.fallbackToPolling=ln(),this._shouldAllowMigration=!0}forAllChangedKeys(e){for(const t of Object.keys(this.listeners)){const n=this.storage.getItem(t),i=this.localCache[t];n!==i&&e(t,i,n)}}onStorageEvent(e,t=!1){if(!e.key){this.forAllChangedKeys((o,u,c)=>{this.notifyListeners(o,c)});return}const n=e.key;t?this.detachListener():this.stopPolling();const i=()=>{const o=this.storage.getItem(n);!t&&this.localCache[n]===o||this.notifyListeners(n,o)},s=this.storage.getItem(n);Da()&&s!==e.newValue&&e.newValue!==e.oldValue?setTimeout(i,Ya):i()}notifyListeners(e,t){this.localCache[e]=t;const n=this.listeners[e];if(n)for(const i of Array.from(n))i(t&&JSON.parse(t))}startPolling(){this.stopPolling(),this.pollTimer=setInterval(()=>{this.forAllChangedKeys((e,t,n)=>{this.onStorageEvent(new StorageEvent("storage",{key:e,oldValue:t,newValue:n}),!0)})},Xa)}stopPolling(){this.pollTimer&&(clearInterval(this.pollTimer),this.pollTimer=null)}attachListener(){window.addEventListener("storage",this.boundEventHandler)}detachListener(){window.removeEventListener("storage",this.boundEventHandler)}_addListener(e,t){Object.keys(this.listeners).length===0&&(this.fallbackToPolling?this.startPolling():this.attachListener()),this.listeners[e]||(this.listeners[e]=new Set,this.localCache[e]=this.storage.getItem(e)),this.listeners[e].add(t)}_removeListener(e,t){this.listeners[e]&&(this.listeners[e].delete(t),this.listeners[e].size===0&&delete this.listeners[e]),Object.keys(this.listeners).length===0&&(this.detachListener(),this.stopPolling())}async _set(e,t){await super._set(e,t),this.localCache[e]=JSON.stringify(t)}async _get(e){const t=await super._get(e);return this.localCache[e]=JSON.stringify(t),t}async _remove(e){await super._remove(e),delete this.localCache[e]}}En.type="LOCAL";const Za=En;/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class mn extends _n{constructor(){super(()=>window.sessionStorage,"SESSION")}_addListener(e,t){}_removeListener(e,t){}}mn.type="SESSION";const Cn=mn;/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function Qa(r){return Promise.all(r.map(async e=>{try{return{fulfilled:!0,value:await e}}catch(t){return{fulfilled:!1,reason:t}}}))}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class pt{constructor(e){this.eventTarget=e,this.handlersMap={},this.boundEventHandler=this.handleEvent.bind(this)}static _getInstance(e){const t=this.receivers.find(i=>i.isListeningto(e));if(t)return t;const n=new pt(e);return this.receivers.push(n),n}isListeningto(e){return this.eventTarget===e}async handleEvent(e){const t=e,{eventId:n,eventType:i,data:s}=t.data,o=this.handlersMap[i];if(!(o!=null&&o.size))return;t.ports[0].postMessage({status:"ack",eventId:n,eventType:i});const u=Array.from(o).map(async a=>a(t.origin,s)),c=await Qa(u);t.ports[0].postMessage({status:"done",eventId:n,eventType:i,response:c})}_subscribe(e,t){Object.keys(this.handlersMap).length===0&&this.eventTarget.addEventListener("message",this.boundEventHandler),this.handlersMap[e]||(this.handlersMap[e]=new Set),this.handlersMap[e].add(t)}_unsubscribe(e,t){this.handlersMap[e]&&t&&this.handlersMap[e].delete(t),(!t||this.handlersMap[e].size===0)&&delete this.handlersMap[e],Object.keys(this.handlersMap).length===0&&this.eventTarget.removeEventListener("message",this.boundEventHandler)}}pt.receivers=[];/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function wr(r="",e=10){let t="";for(let n=0;n<e;n++)t+=Math.floor(Math.random()*10);return r+t}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Ja{constructor(e){this.target=e,this.handlers=new Set}removeMessageHandler(e){e.messageChannel&&(e.messageChannel.port1.removeEventListener("message",e.onMessage),e.messageChannel.port1.close()),this.handlers.delete(e)}async _send(e,t,n=50){const i=typeof MessageChannel<"u"?new MessageChannel:null;if(!i)throw new Error("connection_unavailable");let s,o;return new Promise((u,c)=>{const a=wr("",20);i.port1.start();const l=setTimeout(()=>{c(new Error("unsupported_event"))},n);o={messageChannel:i,onMessage(E){const d=E;if(d.data.eventId===a)switch(d.data.status){case"ack":clearTimeout(l),s=setTimeout(()=>{c(new Error("timeout"))},3e3);break;case"done":clearTimeout(s),u(d.data.response);break;default:clearTimeout(l),clearTimeout(s),c(new Error("invalid_response"));break}}},this.handlers.add(o),i.port1.addEventListener("message",o.onMessage),this.target.postMessage({eventType:e,eventId:a,data:t},[i.port2])}).finally(()=>{o&&this.removeMessageHandler(o)})}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function ie(){return window}function eo(r){ie().location.href=r}/**
 * @license
 * Copyright 2020 Google LLC.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function An(){return typeof ie().WorkerGlobalScope<"u"&&typeof ie().importScripts=="function"}async function to(){if(!(navigator!=null&&navigator.serviceWorker))return null;try{return(await navigator.serviceWorker.ready).active}catch{return null}}function ro(){var r;return((r=navigator==null?void 0:navigator.serviceWorker)===null||r===void 0?void 0:r.controller)||null}function no(){return An()?self:null}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const bn="firebaseLocalStorageDb",io=1,vt="firebaseLocalStorage",Bn="fbase_key";class Xe{constructor(e){this.request=e}toPromise(){return new Promise((e,t)=>{this.request.addEventListener("success",()=>{e(this.request.result)}),this.request.addEventListener("error",()=>{t(this.request.error)})})}}function gt(r,e){return r.transaction([vt],e?"readwrite":"readonly").objectStore(vt)}function so(){const r=indexedDB.deleteDatabase(bn);return new Xe(r).toPromise()}function Fr(){const r=indexedDB.open(bn,io);return new Promise((e,t)=>{r.addEventListener("error",()=>{t(r.error)}),r.addEventListener("upgradeneeded",()=>{const n=r.result;try{n.createObjectStore(vt,{keyPath:Bn})}catch(i){t(i)}}),r.addEventListener("success",async()=>{const n=r.result;n.objectStoreNames.contains(vt)?e(n):(n.close(),await so(),e(await Fr()))})})}async function yn(r,e,t){const n=gt(r,!0).put({[Bn]:e,value:t});return new Xe(n).toPromise()}async function ao(r,e){const t=gt(r,!1).get(e),n=await new Xe(t).toPromise();return n===void 0?null:n.value}function Dn(r,e){const t=gt(r,!0).delete(e);return new Xe(t).toPromise()}const oo=800,co=3;class wn{constructor(){this.type="LOCAL",this._shouldAllowMigration=!0,this.listeners={},this.localCache={},this.pollTimer=null,this.pendingWrites=0,this.receiver=null,this.sender=null,this.serviceWorkerReceiverAvailable=!1,this.activeServiceWorker=null,this._workerInitializationPromise=this.initializeServiceWorkerMessaging().then(()=>{},()=>{})}async _openDb(){return this.db?this.db:(this.db=await Fr(),this.db)}async _withRetries(e){let t=0;for(;;)try{const n=await this._openDb();return await e(n)}catch(n){if(t++>co)throw n;this.db&&(this.db.close(),this.db=void 0)}}async initializeServiceWorkerMessaging(){return An()?this.initializeReceiver():this.initializeSender()}async initializeReceiver(){this.receiver=pt._getInstance(no()),this.receiver._subscribe("keyChanged",async(e,t)=>({keyProcessed:(await this._poll()).includes(t.key)})),this.receiver._subscribe("ping",async(e,t)=>["keyChanged"])}async initializeSender(){var e,t;if(this.activeServiceWorker=await to(),!this.activeServiceWorker)return;this.sender=new Ja(this.activeServiceWorker);const n=await this.sender._send("ping",{},800);n&&!((e=n[0])===null||e===void 0)&&e.fulfilled&&!((t=n[0])===null||t===void 0)&&t.value.includes("keyChanged")&&(this.serviceWorkerReceiverAvailable=!0)}async notifyServiceWorker(e){if(!(!this.sender||!this.activeServiceWorker||ro()!==this.activeServiceWorker))try{await this.sender._send("keyChanged",{key:e},this.serviceWorkerReceiverAvailable?800:50)}catch{}}async _isAvailable(){try{if(!indexedDB)return!1;const e=await Fr();return await yn(e,ft,"1"),await Dn(e,ft),!0}catch{}return!1}async _withPendingWrite(e){this.pendingWrites++;try{await e()}finally{this.pendingWrites--}}async _set(e,t){return this._withPendingWrite(async()=>(await this._withRetries(n=>yn(n,e,t)),this.localCache[e]=t,this.notifyServiceWorker(e)))}async _get(e){const t=await this._withRetries(n=>ao(n,e));return this.localCache[e]=t,t}async _remove(e){return this._withPendingWrite(async()=>(await this._withRetries(t=>Dn(t,e)),delete this.localCache[e],this.notifyServiceWorker(e)))}async _poll(){const e=await this._withRetries(i=>{const s=gt(i,!1).getAll();return new Xe(s).toPromise()});if(!e)return[];if(this.pendingWrites!==0)return[];const t=[],n=new Set;if(e.length!==0)for(const{fbase_key:i,value:s}of e)n.add(i),JSON.stringify(this.localCache[i])!==JSON.stringify(s)&&(this.notifyListeners(i,s),t.push(i));for(const i of Object.keys(this.localCache))this.localCache[i]&&!n.has(i)&&(this.notifyListeners(i,null),t.push(i));return t}notifyListeners(e,t){this.localCache[e]=t;const n=this.listeners[e];if(n)for(const i of Array.from(n))i(t)}startPolling(){this.stopPolling(),this.pollTimer=setInterval(async()=>this._poll(),oo)}stopPolling(){this.pollTimer&&(clearInterval(this.pollTimer),this.pollTimer=null)}_addListener(e,t){Object.keys(this.listeners).length===0&&this.startPolling(),this.listeners[e]||(this.listeners[e]=new Set,this._get(e)),this.listeners[e].add(t)}_removeListener(e,t){this.listeners[e]&&(this.listeners[e].delete(t),this.listeners[e].size===0&&delete this.listeners[e]),Object.keys(this.listeners).length===0&&this.stopPolling()}}wn.type="LOCAL";const lo=wn;new Ve(3e4,6e4);/**
 * @license
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function uo(r,e){return e?de(e):(N(r._popupRedirectResolver,r,"argument-error"),r._popupRedirectResolver)}/**
 * @license
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Ir extends fn{constructor(e){super("custom","custom"),this.params=e}_getIdTokenResponse(e){return Oe(e,this._buildIdpRequest())}_linkToIdToken(e,t){return Oe(e,this._buildIdpRequest(t))}_getReauthenticationResolver(e){return Oe(e,this._buildIdpRequest())}_buildIdpRequest(e){const t={requestUri:this.params.requestUri,sessionId:this.params.sessionId,postBody:this.params.postBody,tenantId:this.params.tenantId,pendingToken:this.params.pendingToken,returnSecureToken:!0,returnIdpCredential:!0};return e&&(t.idToken=e),t}}function xo(r){return ja(r.auth,new Ir(r),r.bypassAuthState)}function ho(r){const{auth:e,user:t}=r;return N(t,e,"internal-error"),Va(t,new Ir(r),r.bypassAuthState)}async function fo(r){const{auth:e,user:t}=r;return N(t,e,"internal-error"),qa(t,new Ir(r),r.bypassAuthState)}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class Fn{constructor(e,t,n,i,s=!1){this.auth=e,this.resolver=n,this.user=i,this.bypassAuthState=s,this.pendingPromise=null,this.eventManager=null,this.filter=Array.isArray(t)?t:[t]}execute(){return new Promise(async(e,t)=>{this.pendingPromise={resolve:e,reject:t};try{this.eventManager=await this.resolver._initialize(this.auth),await this.onExecution(),this.eventManager.registerConsumer(this)}catch(n){this.reject(n)}})}async onAuthEvent(e){const{urlResponse:t,sessionId:n,postBody:i,tenantId:s,error:o,type:u}=e;if(o){this.reject(o);return}const c={auth:this.auth,requestUri:t,sessionId:n,tenantId:s||void 0,postBody:i||void 0,user:this.user,bypassAuthState:this.bypassAuthState};try{this.resolve(await this.getIdpTask(u)(c))}catch(a){this.reject(a)}}onError(e){this.reject(e)}getIdpTask(e){switch(e){case"signInViaPopup":case"signInViaRedirect":return xo;case"linkViaPopup":case"linkViaRedirect":return fo;case"reauthViaPopup":case"reauthViaRedirect":return ho;default:oe(this.auth,"internal-error")}}resolve(e){le(this.pendingPromise,"Pending promise was never set"),this.pendingPromise.resolve(e),this.unregisterAndCleanUp()}reject(e){le(this.pendingPromise,"Pending promise was never set"),this.pendingPromise.reject(e),this.unregisterAndCleanUp()}unregisterAndCleanUp(){this.eventManager&&this.eventManager.unregisterConsumer(this),this.pendingPromise=null,this.cleanUp()}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const po=new Ve(2e3,1e4);class Ne extends Fn{constructor(e,t,n,i,s){super(e,t,i,s),this.provider=n,this.authWindow=null,this.pollId=null,Ne.currentPopupAction&&Ne.currentPopupAction.cancel(),Ne.currentPopupAction=this}async executeNotNull(){const e=await this.execute();return N(e,this.auth,"internal-error"),e}async onExecution(){le(this.filter.length===1,"Popup operations only handle one event");const e=wr();this.authWindow=await this.resolver._openPopup(this.auth,this.provider,this.filter[0],e),this.authWindow.associatedEvent=e,this.resolver._originValidation(this.auth).catch(t=>{this.reject(t)}),this.resolver._isIframeWebStorageSupported(this.auth,t=>{t||this.reject(ne(this.auth,"web-storage-unsupported"))}),this.pollUserCancellation()}get eventId(){var e;return((e=this.authWindow)===null||e===void 0?void 0:e.associatedEvent)||null}cancel(){this.reject(ne(this.auth,"cancelled-popup-request"))}cleanUp(){this.authWindow&&this.authWindow.close(),this.pollId&&window.clearTimeout(this.pollId),this.authWindow=null,this.pollId=null,Ne.currentPopupAction=null}pollUserCancellation(){const e=()=>{var t,n;if(!((n=(t=this.authWindow)===null||t===void 0?void 0:t.window)===null||n===void 0)&&n.closed){this.pollId=window.setTimeout(()=>{this.pollId=null,this.reject(ne(this.auth,"popup-closed-by-user"))},8e3);return}this.pollId=window.setTimeout(e,po.get())};e()}}Ne.currentPopupAction=null;/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const vo="pendingRedirect",_t=new Map;class go extends Fn{constructor(e,t,n=!1){super(e,["signInViaRedirect","linkViaRedirect","reauthViaRedirect","unknown"],t,void 0,n),this.eventId=null}async execute(){let e=_t.get(this.auth._key());if(!e){try{const n=await _o(this.resolver,this.auth)?await super.execute():null;e=()=>Promise.resolve(n)}catch(t){e=()=>Promise.reject(t)}_t.set(this.auth._key(),e)}return this.bypassAuthState||_t.set(this.auth._key(),()=>Promise.resolve(null)),e()}async onAuthEvent(e){if(e.type==="signInViaRedirect")return super.onAuthEvent(e);if(e.type==="unknown"){this.resolve(null);return}if(e.eventId){const t=await this.auth._redirectUserForId(e.eventId);if(t)return this.user=t,super.onAuthEvent(e);this.resolve(null)}}async onExecution(){}cleanUp(){}}async function _o(r,e){const t=Co(e),n=mo(r);if(!await n._isAvailable())return!1;const i=await n._get(t)==="true";return await n._remove(t),i}function Eo(r,e){_t.set(r._key(),e)}function mo(r){return de(r._redirectPersistence)}function Co(r){return dt(vo,r.config.apiKey,r.name)}async function Ao(r,e,t=!1){if(ae(r.app))return Promise.reject(ve(r));const n=xt(r),i=uo(n,e),o=await new go(n,i,t).execute();return o&&!t&&(delete o.user._redirectEventId,await n._persistUserIfCurrent(o.user),await n._setRedirectUser(null,e)),o}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const bo=10*60*1e3;class Bo{constructor(e){this.auth=e,this.cachedEventUids=new Set,this.consumers=new Set,this.queuedRedirectEvent=null,this.hasHandledPotentialRedirect=!1,this.lastProcessedEventTime=Date.now()}registerConsumer(e){this.consumers.add(e),this.queuedRedirectEvent&&this.isEventForConsumer(this.queuedRedirectEvent,e)&&(this.sendToConsumer(this.queuedRedirectEvent,e),this.saveEventToCache(this.queuedRedirectEvent),this.queuedRedirectEvent=null)}unregisterConsumer(e){this.consumers.delete(e)}onEvent(e){if(this.hasEventBeenHandled(e))return!1;let t=!1;return this.consumers.forEach(n=>{this.isEventForConsumer(e,n)&&(t=!0,this.sendToConsumer(e,n),this.saveEventToCache(e))}),this.hasHandledPotentialRedirect||!yo(e)||(this.hasHandledPotentialRedirect=!0,t||(this.queuedRedirectEvent=e,t=!0)),t}sendToConsumer(e,t){var n;if(e.error&&!kn(e)){const i=((n=e.error.code)===null||n===void 0?void 0:n.split("auth/")[1])||"internal-error";t.onError(ne(this.auth,i))}else t.onAuthEvent(e)}isEventForConsumer(e,t){const n=t.eventId===null||!!e.eventId&&e.eventId===t.eventId;return t.filter.includes(e.type)&&n}hasEventBeenHandled(e){return Date.now()-this.lastProcessedEventTime>=bo&&this.cachedEventUids.clear(),this.cachedEventUids.has(In(e))}saveEventToCache(e){this.cachedEventUids.add(In(e)),this.lastProcessedEventTime=Date.now()}}function In(r){return[r.type,r.eventId,r.sessionId,r.tenantId].filter(e=>e).join("-")}function kn({type:r,error:e}){return r==="unknown"&&(e==null?void 0:e.code)==="auth/no-auth-event"}function yo(r){switch(r.type){case"signInViaRedirect":case"linkViaRedirect":case"reauthViaRedirect":return!0;case"unknown":return kn(r);default:return!1}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */async function Do(r,e={}){return Te(r,"GET","/v1/projects",e)}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const wo=/^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$/,Fo=/^https?/;async function Io(r){if(r.config.emulator)return;const{authorizedDomains:e}=await Do(r);for(const t of e)try{if(ko(t))return}catch{}oe(r,"unauthorized-domain")}function ko(r){const e=mr(),{protocol:t,hostname:n}=new URL(e);if(r.startsWith("chrome-extension://")){const o=new URL(r);return o.hostname===""&&n===""?t==="chrome-extension:"&&r.replace("chrome-extension://","")===e.replace("chrome-extension://",""):t==="chrome-extension:"&&o.hostname===n}if(!Fo.test(t))return!1;if(wo.test(r))return n===r;const i=r.replace(/\./g,"\\.");return new RegExp("^(.+\\."+i+"|"+i+")$","i").test(n)}/**
 * @license
 * Copyright 2020 Google LLC.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const So=new Ve(3e4,6e4);function Sn(){const r=ie().___jsl;if(r!=null&&r.H){for(const e of Object.keys(r.H))if(r.H[e].r=r.H[e].r||[],r.H[e].L=r.H[e].L||[],r.H[e].r=[...r.H[e].L],r.CP)for(let t=0;t<r.CP.length;t++)r.CP[t]=null}}function To(r){return new Promise((e,t)=>{var n,i,s;function o(){Sn(),gapi.load("gapi.iframes",{callback:()=>{e(gapi.iframes.getContext())},ontimeout:()=>{Sn(),t(ne(r,"network-request-failed"))},timeout:So.get()})}if(!((i=(n=ie().gapi)===null||n===void 0?void 0:n.iframes)===null||i===void 0)&&i.Iframe)e(gapi.iframes.getContext());else if(!((s=ie().gapi)===null||s===void 0)&&s.load)o();else{const u=Oa("iframefcb");return ie()[u]=()=>{gapi.load?o():t(ne(r,"network-request-failed"))},Ra(`${Pa()}?onload=${u}`).catch(c=>t(c))}}).catch(e=>{throw Et=null,e})}let Et=null;function Ro(r){return Et=Et||To(r),Et}/**
 * @license
 * Copyright 2020 Google LLC.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const Po=new Ve(5e3,15e3),Oo="__/auth/iframe",No="emulator/auth/iframe",Lo={style:{position:"absolute",top:"-100px",width:"1px",height:"1px"},"aria-hidden":"true",tabindex:"-1"},Ho=new Map([["identitytoolkit.googleapis.com","p"],["staging-identitytoolkit.sandbox.googleapis.com","s"],["test-identitytoolkit.sandbox.googleapis.com","t"]]);function Mo(r){const e=r.config;N(e.authDomain,r,"auth-domain-config-required");const t=e.emulator?Cr(e,No):`https://${r.config.authDomain}/${Oo}`,n={apiKey:e.apiKey,appName:r.name,v:$e},i=Ho.get(r.config.apiHost);i&&(n.eid=i);const s=r._getFrameworks();return s.length&&(n.fw=s.join(",")),`${t}?${ze(n).slice(1)}`}async function Uo(r){const e=await Ro(r),t=ie().gapi;return N(t,r,"internal-error"),e.open({where:document.body,url:Mo(r),messageHandlersFilter:t.iframes.CROSS_ORIGIN_IFRAMES_FILTER,attributes:Lo,dontclear:!0},n=>new Promise(async(i,s)=>{await n.restyle({setHideOnLeave:!1});const o=ne(r,"network-request-failed"),u=ie().setTimeout(()=>{s(o)},Po.get());function c(){ie().clearTimeout(u),i(n)}n.ping(c).then(c,()=>{s(o)})}))}/**
 * @license
 * Copyright 2020 Google LLC.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const zo={location:"yes",resizable:"yes",statusbar:"yes",toolbar:"no"},Wo=500,$o=600,qo="_blank",Vo="http://localhost";class Tn{constructor(e){this.window=e,this.associatedEvent=null}close(){if(this.window)try{this.window.close()}catch{}}}function jo(r,e,t,n=Wo,i=$o){const s=Math.max((window.screen.availHeight-i)/2,0).toString(),o=Math.max((window.screen.availWidth-n)/2,0).toString();let u="";const c=Object.assign(Object.assign({},zo),{width:n.toString(),height:i.toString(),top:s,left:o}),a=Y().toLowerCase();t&&(u=nn(a)?qo:t),tn(a)&&(e=e||Vo,c.scrollbars="yes");const l=Object.entries(c).reduce((d,[p,f])=>`${d}${p}=${f},`,"");if(ya(a)&&u!=="_self")return Go(e||"",u),new Tn(null);const E=window.open(e||"",u,l);N(E,r,"popup-blocked");try{E.focus()}catch{}return new Tn(E)}function Go(r,e){const t=document.createElement("a");t.href=r,t.target=e;const n=document.createEvent("MouseEvent");n.initMouseEvent("click",!0,!0,window,1,0,0,0,0,!1,!1,!1,!1,1,null),t.dispatchEvent(n)}/**
 * @license
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const Ko="__/auth/handler",Xo="emulator/auth/handler",Yo=encodeURIComponent("fac");async function Rn(r,e,t,n,i,s){N(r.config.authDomain,r,"auth-domain-config-required"),N(r.config.apiKey,r,"invalid-api-key");const o={apiKey:r.config.apiKey,appName:r.name,authType:t,redirectUrl:n,v:$e,eventId:i};if(e instanceof pn){e.setDefaultLanguage(r.languageCode),o.providerId=e.providerId||"",Xi(e.getCustomParameters())||(o.customParameters=JSON.stringify(e.getCustomParameters()));for(const[l,E]of Object.entries({}))o[l]=E}if(e instanceof Ke){const l=e.getScopes().filter(E=>E!=="");l.length>0&&(o.scopes=l.join(","))}r.tenantId&&(o.tid=r.tenantId);const u=o;for(const l of Object.keys(u))u[l]===void 0&&delete u[l];const c=await r._getAppCheckToken(),a=c?`#${Yo}=${encodeURIComponent(c)}`:"";return`${Zo(r)}?${ze(u).slice(1)}${a}`}function Zo({config:r}){return r.emulator?Cr(r,Xo):`https://${r.authDomain}/${Ko}`}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const kr="webStorageSupport";class Qo{constructor(){this.eventManagers={},this.iframes={},this.originValidationPromises={},this._redirectPersistence=Cn,this._completeRedirectFn=Ao,this._overrideRedirectResult=Eo}async _openPopup(e,t,n,i){var s;le((s=this.eventManagers[e._key()])===null||s===void 0?void 0:s.manager,"_initialize() not called before _openPopup()");const o=await Rn(e,t,n,mr(),i);return jo(e,o,wr())}async _openRedirect(e,t,n,i){await this._originValidation(e);const s=await Rn(e,t,n,mr(),i);return eo(s),new Promise(()=>{})}_initialize(e){const t=e._key();if(this.eventManagers[t]){const{manager:i,promise:s}=this.eventManagers[t];return i?Promise.resolve(i):(le(s,"If manager is not set, promise should be"),s)}const n=this.initAndGetManager(e);return this.eventManagers[t]={promise:n},n.catch(()=>{delete this.eventManagers[t]}),n}async initAndGetManager(e){const t=await Uo(e),n=new Bo(e);return t.register("authEvent",i=>(N(i==null?void 0:i.authEvent,e,"invalid-auth-event"),{status:n.onEvent(i.authEvent)?"ACK":"ERROR"}),gapi.iframes.CROSS_ORIGIN_IFRAMES_FILTER),this.eventManagers[e._key()]={manager:n},this.iframes[e._key()]=t,n}_isIframeWebStorageSupported(e,t){this.iframes[e._key()].send(kr,{type:kr},i=>{var s;const o=(s=i==null?void 0:i[0])===null||s===void 0?void 0:s[kr];o!==void 0&&t(!!o),oe(e,"internal-error")},gapi.iframes.CROSS_ORIGIN_IFRAMES_FILTER)}_originValidation(e){const t=e._key();return this.originValidationPromises[t]||(this.originValidationPromises[t]=Io(e)),this.originValidationPromises[t]}get _shouldInitProactively(){return ln()||rn()||yr()}}const Jo=Qo;var Pn="@firebase/auth",On="1.8.0";/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */class ec{constructor(e){this.auth=e,this.internalListeners=new Map}getUid(){var e;return this.assertAuthConfigured(),((e=this.auth.currentUser)===null||e===void 0?void 0:e.uid)||null}async getToken(e){return this.assertAuthConfigured(),await this.auth._initializationPromise,this.auth.currentUser?{accessToken:await this.auth.currentUser.getIdToken(e)}:null}addAuthTokenListener(e){if(this.assertAuthConfigured(),this.internalListeners.has(e))return;const t=this.auth.onIdTokenChanged(n=>{e((n==null?void 0:n.stsTokenManager.accessToken)||null)});this.internalListeners.set(e,t),this.updateProactiveRefresh()}removeAuthTokenListener(e){this.assertAuthConfigured();const t=this.internalListeners.get(e);t&&(this.internalListeners.delete(e),t(),this.updateProactiveRefresh())}assertAuthConfigured(){N(this.auth._initializationPromise,"dependent-sdk-initialized-before-auth")}updateProactiveRefresh(){this.internalListeners.size>0?this.auth._startProactiveRefresh():this.auth._stopProactiveRefresh()}}/**
 * @license
 * Copyright 2020 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */function tc(r){switch(r){case"Node":return"node";case"ReactNative":return"rn";case"Worker":return"webworker";case"Cordova":return"cordova";case"WebExtension":return"web-extension";default:return}}function rc(r){We(new ke("auth",(e,{options:t})=>{const n=e.getProvider("app").getImmediate(),i=e.getProvider("heartbeat"),s=e.getProvider("app-check-internal"),{apiKey:o,authDomain:u}=n.options;N(o&&!o.includes(":"),"invalid-api-key",{appName:n.name});const c={apiKey:o,authDomain:u,clientPlatform:r,apiHost:"identitytoolkit.googleapis.com",tokenApiHost:"securetoken.googleapis.com",apiScheme:"https",sdkClientVersion:un(r)},a=new Sa(n,i,s,c);return La(a,t),a},"PUBLIC").setInstantiationMode("EXPLICIT").setInstanceCreatedCallback((e,t,n)=>{e.getProvider("auth-internal").initialize()})),We(new ke("auth-internal",e=>{const t=xt(e.getProvider("auth").getImmediate());return(n=>new ec(n))(t)},"PRIVATE").setInstantiationMode("EXPLICIT")),Se(Pn,On,tc(r)),Se(Pn,On,"esm2017")}/**
 * @license
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */const nc=5*60,ic=b0("authIdTokenMaxAge")||nc;let Nn=null;const sc=r=>async e=>{const t=e&&await e.getIdTokenResult(),n=t&&(new Date().getTime()-Date.parse(t.issuedAtTime))/1e3;if(n&&n>ic)return;const i=t==null?void 0:t.token;Nn!==i&&(Nn=i,await fetch(r,{method:i?"POST":"DELETE",headers:i?{Authorization:`Bearer ${i}`}:{}}))};function ac(r=Zs()){const e=R0(r,"auth");if(e.isInitialized())return e.getImmediate();const t=Na(r,{popupRedirectResolver:Jo,persistence:[lo,Za,Cn]}),n=b0("authTokenSyncURL");if(n&&typeof isSecureContext=="boolean"&&isSecureContext){const s=new URL(n,location.origin);if(location.origin===s.origin){const o=sc(s.toString());Ka(t,o,()=>o(t.currentUser)),Ga(t,u=>o(u))}}const i=Li("auth");return i&&Ha(t,`http://${i}`),t}function oc(){var r,e;return(e=(r=document.getElementsByTagName("head"))===null||r===void 0?void 0:r[0])!==null&&e!==void 0?e:document}Ta({loadJS(r){return new Promise((e,t)=>{const n=document.createElement("script");n.setAttribute("src",r),n.onload=e,n.onerror=i=>{const s=ne("internal-error");s.customData=i,t(s)},n.type="text/javascript",n.charset="UTF-8",oc().appendChild(n)})},gapiScript:"https://apis.google.com/js/api.js",recaptchaV2Script:"https://www.google.com/recaptcha/api.js",recaptchaEnterpriseScript:"https://www.google.com/recaptcha/enterprise.js?render="}),rc("Browser");const cc={apiKey:"AIzaSyC1EfCSuT-CvxV8Frmdj4WZ4ZKQs6SB34k",authDomain:"pkhex-everywhere.firebaseapp.com",projectId:"pkhex-everywhere",storageBucket:"pkhex-everywhere.firebasestorage.app",messagingSenderId:"989834045132",appId:"1:989834045132:web:709ee8b8cca43fc216c469"};async function lc(){const r=P0(cc),e=ac(r);window.getAuthToken=async()=>{const t=e.currentUser;if(!t)throw new Error("No user found");return await t.getIdToken()},await $a(e)}async function uc(){Ii(),await lc()}uc()})();
