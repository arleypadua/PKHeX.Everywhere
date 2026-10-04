---
"@pkhex-everywhere/engine": patch
---

A Gen 3 save whose party or boxes hold a species or move the base game doesn't have, such as a Run & Bun save, now fails to load with `invalid-save` instead of loading as the base game. Loading it with the `pkhex` format still works.
