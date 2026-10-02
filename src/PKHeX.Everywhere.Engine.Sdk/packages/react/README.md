# @pkhex-everywhere/react

React hooks for [`@pkhex-everywhere/engine`](https://www.npmjs.com/package/@pkhex-everywhere/engine). Hooks suspend until their data arrives and refetch when a command changes it.

## Install

```sh
npm install @pkhex-everywhere/engine @pkhex-everywhere/react
```

## Example

This app opens a save from a file input, lists the party and downloads the edited save:

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

The Suspense fallback shows the runtime download with `useEngineStatus()`. `<RequireGame>` renders its fallback until a save is loaded, then its children. If the runtime fails to load, the hooks throw the error to the nearest error boundary.

CI builds this app from the packed packages and runs it.

## Hooks that need a save

Most hooks throw `no-save` when no save is loaded, so render them inside `<RequireGame>`. Hover a hook to see what it needs. A hook that runs without a save fails with a message naming the call and the fix, and the error's `code` stays `no-save`. The same goes for `no-draft`.

These work without a save:

- `useEngineStatus()`
- `useEngine()`
- `useLoadedGame()`, where `game` is `null` until a save is loaded
- `useQuery` with `game.get`, `game.version`, `game.blankVersions`, `catalog.names` and the `plugins.*` queries

## License

GPL-3.0-or-later.
