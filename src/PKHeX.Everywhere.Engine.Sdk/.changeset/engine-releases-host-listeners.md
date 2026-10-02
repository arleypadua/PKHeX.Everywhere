---
'@pkhex-everywhere/engine': patch
---

Engines hold host change and event listeners only while they have subscribers, and release the progress listener once boot settles, so engines dropped by StrictMode remounts or HMR no longer stay registered on the shared runtime.
