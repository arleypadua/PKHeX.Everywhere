---
'@pkhex-everywhere/engine': minor
'@pkhex-everywhere/react': minor
---

`createEngine()` starts downloading the runtime as soon as it's created in a browser (`{ lazy: true }` opts out) and reports `engine.status` with file download progress through `engine.onStatusChange`. `wasmHost()` returns one shared runtime per page, so several engines on a page all get changes and events.
