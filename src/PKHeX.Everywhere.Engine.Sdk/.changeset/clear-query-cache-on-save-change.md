---
"@pkhex-everywhere/react": patch
---

Clear the query cache when a save loads, blanks or closes instead of refetching cached queries with the old save's arguments. A leftover query whose arguments only made sense for the previous save no longer runs against the new one.
