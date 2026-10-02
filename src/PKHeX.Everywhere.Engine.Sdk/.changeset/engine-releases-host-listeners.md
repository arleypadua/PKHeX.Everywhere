---
'@pkhex-everywhere/engine': patch
---

Engines release their host listeners when nothing subscribes to them and once boot settles, so engines dropped by StrictMode or HMR no longer stay registered.
