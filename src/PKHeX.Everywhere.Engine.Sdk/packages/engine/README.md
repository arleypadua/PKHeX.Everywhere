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
const input = document.querySelector<HTMLInputElement>('input[type=file]')!

input.onchange = async () => {
  await engine.game.load(input.files![0])
  const party = await engine.party.get()
  console.log(party.map((pokemon) => `${pokemon.species} Lv. ${pokemon.level}`))
}
```

Binary inputs take a `Uint8Array`, `ArrayBuffer`, `Blob` or `File`, and binary outputs come back as `Uint8Array`. `game.load` takes the file name from a `File` when you leave it out. Hover a method to see whether it needs a loaded save or an open draft. Commands that fail reject with an `EngineError`, whose `code` is one of `errorCodes`. Use `engine.subscribe(topics, callback)` to get a callback when a command changes the save.

## React

With [`@pkhex-everywhere/react`](https://www.npmjs.com/package/@pkhex-everywhere/react), this app opens a save from a file input, lists the party and downloads the edited save:

```tsx
import { Suspense, type ChangeEvent } from 'react'
import { createEngine } from '@pkhex-everywhere/engine'
import { EngineProvider, RequireGame, useEngineStatus, useLoadedGame, useParty } from '@pkhex-everywhere/react'

const engine = createEngine()

function Booting() {
  const { loaded, total } = useEngineStatus()
  return <progress value={loaded} max={total || 1} />
}

function OpenSave() {
  const { load } = useLoadedGame()
  const open = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (file) void load(file)
  }
  return <input type="file" onChange={open} />
}

function Party() {
  const { party } = useParty()
  return (
    <ul id="party">
      {party.map((pokemon) => (
        <li key={pokemon.id}>{pokemon.species}</li>
      ))}
    </ul>
  )
}

function Download() {
  const { export: exportSave } = useLoadedGame()
  const download = async () => {
    const { bytes, fileName } = await exportSave()
    const url = URL.createObjectURL(new Blob([bytes]))
    const link = document.createElement('a')
    link.href = url
    link.download = fileName
    link.click()
    setTimeout(() => URL.revokeObjectURL(url))
  }
  return <button onClick={download}>Download</button>
}

export function App() {
  return (
    <EngineProvider engine={engine}>
      <Suspense fallback={<Booting />}>
        <RequireGame fallback={<OpenSave />}>
          <Party />
          <Download />
        </RequireGame>
      </Suspense>
    </EngineProvider>
  )
}
```

`<RequireGame>` renders its fallback until a save is loaded. Without a save, `useEngineStatus()`, `useEngine()` and `useLoadedGame()` work, and the other hooks throw `no-save`. CI builds this app from the packed packages and runs it.

## Boot status

In a browser, `createEngine()` starts downloading the runtime right away. Pass `{ lazy: true }` to wait for the first call instead. On a server it does nothing until called.

`engine.status` is `{ state, loaded, total, error? }`, where `state` is `idle`, `booting`, `ready` or `failed`, and `loaded` and `total` count downloaded files. `engine.onStatusChange(listener)` returns a function that stops listening:

```ts
engine.onStatusChange(({ state, loaded, total }) => {
  if (state === 'booting') progress.value = total ? loaded / total : 0
})
```

If the runtime fails to load, `state` becomes `failed` and every call rejects with the error.

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

A page has one runtime. Every `wasmHost()` call returns it, so engines created on the same page share it. The first call picks the URL, and a later call with a different `dotnetUrl` logs a warning.

## License

GPL-3.0-or-later, because the runtime contains PKHeX.Core.
