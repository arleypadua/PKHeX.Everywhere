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
