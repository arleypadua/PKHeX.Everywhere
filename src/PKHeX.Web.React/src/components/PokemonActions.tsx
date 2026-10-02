import { Dropdown } from 'antd'
import type { PokemonSummary } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'
import { openCalculator, useNavigate } from '../host'
import { useCopyShowdown } from '../hooks/useCopyShowdown'
import { routes } from '../routes'

interface PokemonActionsProps {
  pokemon: PokemonSummary
}

export function PokemonActions({ pokemon }: PokemonActionsProps) {
  const engine = useEngine()
  const navigate = useNavigate()
  const copyShowdown = useCopyShowdown()
  const showdown = () => engine.pokemon.showdown(pokemon.at)

  return (
    <Dropdown.Button
      type="link"
      trigger={['click']}
      onClick={() => navigate(routes.pokemon(pokemon))}
      menu={{
        items: [
          { key: 'clone', label: 'Clone', onClick: () => navigate(routes.clonePokemon(pokemon)) },
          { key: 'calculator', label: 'Calculator', onClick: async () => openCalculator(await showdown()) },
          { key: 'showdown', label: 'Showdown', onClick: async () => copyShowdown(await showdown(), pokemon.species) },
        ],
      }}
    >
      View
    </Dropdown.Button>
  )
}
