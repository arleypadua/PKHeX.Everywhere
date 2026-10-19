import type { TableColumnsType } from 'antd'
import type { PokemonSummary } from '@pkhex-everywhere/engine'

const boxNumber = (pokemon: PokemonSummary) => (pokemon.at.box ?? 0) + 1

export const boxColumns: TableColumnsType<PokemonSummary> = [
  {
    title: 'Box',
    key: 'box',
    render: (_, pokemon) => boxNumber(pokemon),
    sorter: (a, b) => boxNumber(a) - boxNumber(b) || a.at.slot - b.at.slot,
  },
]
