---
'@pkhex-everywhere/engine': patch
---

The Engine reads the version of the `PKHeX.Everywhere.PlugIns` reference to tell SDK 3 plug-ins from SDK 2. It runs only SDK 2 for now, so `plugins.register` lists an SDK 3 plug-in with `needsReinstall`, and `plugins.isSupported` returns `false` for it.
