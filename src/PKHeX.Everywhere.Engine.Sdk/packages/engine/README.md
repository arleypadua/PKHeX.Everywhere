# @pkhex-everywhere/engine

Runs PKHeX in the browser. Load a Pokémon save file, read and edit it, and export it. The package contains the .NET runtime with PKHeX.Core.

## Install

```sh
npm install @pkhex-everywhere/engine
```

## Example

```ts
import { createEngine } from '@pkhex-everywhere/engine'

const engine = createEngine()

await engine.game.load(await (await fetch('/emerald.sav')).blob(), 'emerald.sav')

const party = await engine.party.get()
console.log(party.map((pokemon) => `${pokemon.species} Lv. ${pokemon.level}`))
```

Binary inputs take a `Uint8Array`, `ArrayBuffer`, `Blob` or `File`, and binary outputs come back as `Uint8Array`. `game.load` takes the file name from a `File` when you leave it out. Hover a method to see whether it needs a loaded save or an open draft. Commands that fail reject with an `EngineError`, whose `code` is one of `errorCodes`. Use `engine.subscribe(topics, callback)` to get a callback when a command changes the save.

## Where the runtime loads from

By default the browser loads `_framework/dotnet.js` from jsDelivr, pinned to the installed version of this package.

### Vite

To serve the runtime from your own site, add the plugin:

```ts
import { defineConfig } from 'vite'
import pkhexEngine from '@pkhex-everywhere/engine/vite'

export default defineConfig({
  plugins: [pkhexEngine()],
})
```

In dev it serves `/_framework`, and at build it copies `_framework` into the output. `createEngine()` then loads `/_framework/dotnet.js` from your site and makes no request to jsDelivr. The plugin serves the `_framework` inside this package. Pass `frameworkDir` to serve another folder:

```ts
pkhexEngine({ frameworkDir: '/path/to/_framework' })
```

### Other bundlers

Copy `node_modules/@pkhex-everywhere/engine/_framework` to your site and pass the URL of `dotnet.js`:

```ts
import { createEngine, wasmHost } from '@pkhex-everywhere/engine'

const engine = createEngine({ host: wasmHost({ dotnetUrl: '/_framework/dotnet.js' }) })
```

`dotnetUrl` also works with the Vite plugin, for example to load the runtime from your own CDN.

## License

GPL-3.0-or-later, because the runtime contains PKHeX.Core.
