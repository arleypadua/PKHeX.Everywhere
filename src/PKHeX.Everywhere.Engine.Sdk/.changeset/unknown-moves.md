---
'@pkhex-everywhere/engine': minor
---

Move slots have a required `isUnknown`. In a ROM hack save, a move PKHeX has no id for now shows in its slot as `Unknown move #n` instead of an empty slot. It can stay in that slot, be replaced or be cleared, and clearing it removes it from the save.
