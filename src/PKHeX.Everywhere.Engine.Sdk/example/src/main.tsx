import { Suspense } from 'react'
import { createRoot } from 'react-dom/client'
import { createEngine } from '@pkhex-everywhere/engine'
import { EngineProvider, useParty } from '@pkhex-everywhere/react'
import saveUrl from '../../../PKHeX.Web.React/public/data/emerald.sav?url'

const engine = createEngine()

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

const bytes = new Uint8Array(await (await fetch(saveUrl)).arrayBuffer())
let binary = ''
for (const byte of bytes) binary += String.fromCharCode(byte)
await engine.game.load(btoa(binary), 'emerald.sav')

createRoot(document.getElementById('root')!).render(
  <EngineProvider engine={engine}>
    <Suspense fallback="Loading…">
      <Party />
    </Suspense>
  </EngineProvider>,
)
