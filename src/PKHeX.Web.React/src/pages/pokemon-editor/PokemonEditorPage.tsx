import { useEffect, useState } from 'react'
import { Result, Spin } from 'antd'
import { useBox, useEngine, useParty } from '@pkhex-everywhere/react'
import { useNavigate } from '../../host'
import { routes } from '../../routes'
import { DraftEditor } from './DraftEditor'
import { SaveDropdown } from './SaveDropdown'

interface PokemonEditorPageProps {
  source?: string
  id?: string
}

export default function PokemonEditorPage({ source, id }: PokemonEditorPageProps) {
  const { party } = useParty()
  const { box } = useBox()
  const engine = useEngine()
  const navigate = useNavigate()
  const [phase, setPhase] = useState<'opening' | 'editing' | 'saving'>('opening')

  const saved = (source === 'party' ? party : box).find((pokemon) => pokemon.id === id)
  const key = saved && JSON.stringify(saved.at)

  useEffect(() => {
    if (!saved) return
    let current = true
    void engine.pokemon.edit(saved.at).then(() => current && setPhase('editing'))
    return () => {
      current = false
    }
  }, [engine, key])

  // Commit clears the draft, so the editor unmounts first instead of reading a draft that's gone.
  const save = async () => {
    if (!saved) return
    setPhase('saving')
    try {
      const committed = await engine.pokemon.commit()
      if (committed !== id) await navigate(routes.pokemonEditorPreview({ at: saved.at, id: committed }), { replace: true })
      history.back()
    } catch (error) {
      setPhase('editing')
      throw error
    }
  }

  if (phase === 'saving') return <Spin />
  if (!saved) return <Result status="404" title="Pokémon not found" />
  if (phase === 'opening') return <Spin />
  return <DraftEditor actions={<SaveDropdown onSave={save} />} />
}
