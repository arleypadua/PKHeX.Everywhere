---
"@pkhex-everywhere/engine": minor
"@pkhex-everywhere/react": minor
---

Add the `mysteryGift` namespace for the Wonder Card album of Generation 4 and 5 saves. `status()` returns the album and whether Mystery Gift is unlocked. `events()` lists the events from PKHeX's event database the save's game can receive. `previewEvent(id)` and `previewFile(bytes)` show a card, and `insertEvent(id, slot)` and `insertFile(bytes, slot)` put one in the album as a real reception, for the player to collect from the delivery person. `remove(slot)`, `setUnlocked(value)` and `setReceived(cardId, value)` manage the album. `game.get` gains `hasMysteryGift`, changes notify the new `mysteryGift` topic, and the new `wrong-generation` error code covers a card for another generation. The React package adds `useMysteryGift()`.
