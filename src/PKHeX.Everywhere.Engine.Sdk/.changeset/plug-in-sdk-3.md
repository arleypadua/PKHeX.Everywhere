---
'@pkhex-everywhere/engine': minor
---

The engine runs plug-ins built against plug-in SDK 3 only. `plugins.isSupported` returns false for an SDK 2 plug-in, `plugins.register` lists it with `needsReinstall`, and `plugins.newestCompatible` picks the newest version with `Sdk: 3`.
