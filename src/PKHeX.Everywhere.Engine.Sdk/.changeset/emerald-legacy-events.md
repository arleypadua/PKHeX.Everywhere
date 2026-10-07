---
"@pkhex-everywhere/engine": minor
---

A save format can now name its own event flags and work values, and Emerald Legacy does: it supports `Events`, and `events.flags()` and `events.work()` label what the hack means rather than what Emerald means. Legacy's larger trainer flag range moves everything from `SYSTEM_FLAGS` up by `0x60`, so the first badge is flag `0x8C7` rather than Emerald's `0x867`.
