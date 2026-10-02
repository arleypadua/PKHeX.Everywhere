# @pkhex-everywhere/react

React hooks for [`@pkhex-everywhere/engine`](https://www.npmjs.com/package/@pkhex-everywhere/engine). Hooks suspend until their data arrives and refetch when a command changes it.

## Install

```sh
npm install @pkhex-everywhere/engine @pkhex-everywhere/react
```

## Example

```tsx
import { Suspense } from 'react'
import { createEngine } from '@pkhex-everywhere/engine'
import { EngineProvider, useParty } from '@pkhex-everywhere/react'

const engine = createEngine()

function Party() {
  const { party } = useParty()
  return (
    <ul>
      {party.map((pokemon) => (
        <li key={pokemon.id}>
          {pokemon.species} Lv. {pokemon.level}
        </li>
      ))}
    </ul>
  )
}

export function App() {
  return (
    <EngineProvider engine={engine}>
      <Suspense fallback="Loading…">
        <Party />
      </Suspense>
    </EngineProvider>
  )
}
```

Load a save with `useLoadedGame().load(base64, fileName)` or `engine.game.load` before rendering hooks that read it.

## License

GPL-3.0-or-later.
