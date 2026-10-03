import { useEffect, useState } from 'react'
import { App, Button, Result, Spin } from 'antd'
import { EngineError } from '@pkhex-everywhere/engine'
import { useBox, useEngine, useParty } from '@pkhex-everywhere/react'
import { useNavigate } from '../../host'
import { routes } from '../../routes'
import { DraftEditor } from './DraftEditor'
import { editorView } from './editorView'

interface PokemonClonePageProps {
  source?: string
  id?: string
}

export default function PokemonClonePage({ source, id }: PokemonClonePageProps) {
  const { party } = useParty()
  const { box } = useBox()
  const engine = useEngine()
  const navigate = useNavigate()
  const { notification } = App.useApp()
  const [phase, setPhase] = useState<'opening' | 'editing' | 'adding'>('opening')

  const saved = (source === 'party' ? party : box).find((pokemon) => pokemon.id === id)
  const key = saved && JSON.stringify(saved.at)

  useEffect(() => {
    if (!saved || editorView(saved) === 'read-only') return
    let current = true
    void engine.pokemon.clone(saved.at).then(() => current && setPhase('editing'))
    return () => {
      current = false
    }
  }, [engine, key])

  // addToBox clears the draft, so the editor unmounts first instead of reading a draft that's gone.
  const addToBox = async () => {
    if (!saved) return
    setPhase('adding')
    try {
      const added = await engine.pokemon.addToBox()
      notification.success({ title: `${saved.species} cloned` })
      await navigate(routes.box, { replace: true })
      await navigate(routes.pokemon(added))
    } catch (error) {
      setPhase('editing')
      if (!(error instanceof EngineError)) throw error
      notification.error({ title: "Couldn't add the clone", description: error.message })
    }
  }

  if (phase === 'adding') return <Spin />
  if (!saved) return <Result status="404" title="Pokémon not found" />
  if (editorView(saved) === 'read-only') return <Result status="warning" title="This Pokémon's species is unknown, so it can't be cloned." />
  if (phase === 'opening') return <Spin />
  return (
    <DraftEditor
      actions={
        <Button type="primary" onClick={addToBox}>
          Add to box
        </Button>
      }
    />
  )
}
