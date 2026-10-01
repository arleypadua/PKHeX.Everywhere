import { Dropdown } from 'antd'
import type { PokemonSummary } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'
import { openCalculator, useNavigate } from '../host'
import { useCopyShowdown } from '../hooks/useCopyShowdown'

export function PokemonActions({ pokemon }: { pokemon: PokemonSummary }) {
  const engine = useEngine()
  const navigate = useNavigate()
  const copyShowdown = useCopyShowdown()
  const showdown = () => engine.pokemon.showdown(pokemon.at)

  return (
    <Dropdown.Button
      type="link"
      trigger={['click']}
      onClick={() => navigate(`/pokemon/${pokemon.at.source}/${pokemon.id}`)}
      menu={{
        items: [
          { key: 'clone', label: 'Clone', onClick: () => navigate(`/pokemon/${pokemon.id}/clone`) },
          { key: 'calculator', label: 'Calculator', onClick: async () => openCalculator(await showdown()) },
          { key: 'showdown', label: 'Showdown', onClick: async () => copyShowdown(await showdown(), pokemon.species) },
        ],
      }}
    >
      View
    </Dropdown.Button>
  )
}
