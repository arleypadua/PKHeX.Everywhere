# @pkhex-everywhere/engine

PKHeX in the browser. Load a Pokémon save file, read and edit it, and export it. The package contains the .NET runtime with PKHeX.Core.

## Install

```sh
npm install @pkhex-everywhere/engine
```

## Example

```ts
import { createEngine } from '@pkhex-everywhere/engine'

const engine = createEngine()

const bytes = new Uint8Array(await (await fetch('/emerald.sav')).arrayBuffer())
let binary = ''
for (const byte of bytes) binary += String.fromCharCode(byte)
await engine.game.load(btoa(binary), 'emerald.sav')

const party = await engine.party.get()
console.log(party.map((pokemon) => `${pokemon.species} Lv. ${pokemon.level}`))
```

Saves travel as base64. Commands that fail reject with an `EngineError`, whose `code` is one of `errorCodes`. Use `engine.subscribe(topics, callback)` to hear when a command changes the save.

## Where the runtime loads from

By default the browser loads `_framework/dotnet.js` from jsDelivr, pinned to the installed version of this package. To serve the runtime yourself, copy `node_modules/@pkhex-everywhere/engine/_framework` to your site and pass its URL:

```ts
import { createEngine, wasmHost } from '@pkhex-everywhere/engine'

const engine = createEngine({ host: wasmHost({ dotnetUrl: '/_framework/dotnet.js' }) })
```

## License

GPL-3.0-or-later, because the runtime contains PKHeX.Core.
