import { Table, type TableColumnsType } from 'antd'
import type { PokemonSummary } from '@pkhex-everywhere/engine'
import { PokemonActions } from './PokemonActions'
import { PokemonSprite } from './PokemonSprite'
import { NumberFilter } from './filters/NumberFilter'
import { containsText, TextFilter } from './filters/TextFilter'

const columns: TableColumnsType<PokemonSummary> = [
  {
    key: 'sprite',
    width: 80,
    render: (_, pokemon) => <PokemonSprite pokemon={pokemon} />,
  },
  {
    title: 'Name',
    dataIndex: 'species',
    sorter: (a, b) => a.species.localeCompare(b.species),
    filterDropdown: (props) => <TextFilter {...props} placeholder="Name" />,
    onFilter: (value, pokemon) => containsText(pokemon.species, value),
  },
  {
    title: 'Level',
    dataIndex: 'level',
    sorter: (a, b) => a.level - b.level,
    filterDropdown: (props) => <NumberFilter {...props} placeholder="Level" min={1} max={100} />,
    onFilter: (value, pokemon) => pokemon.level === value,
  },
  {
    title: 'Action',
    key: 'action',
    render: (_, pokemon) => <PokemonActions pokemon={pokemon} />,
  },
]

export function PokemonTable({ pokemons }: { pokemons: PokemonSummary[] }) {
  return (
    <Table
      rowKey={(pokemon) => JSON.stringify(pokemon.at)}
      dataSource={pokemons}
      columns={columns}
      size="small"
      pagination={{ hideOnSinglePage: true }}
    />
  )
}
