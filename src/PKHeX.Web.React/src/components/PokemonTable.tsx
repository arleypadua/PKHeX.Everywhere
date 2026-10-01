import { Table, type TableColumnsType } from 'antd'
import type { PokemonSummary } from '@pkhex-everywhere/engine'
import { PokemonActions } from './PokemonActions'
import { PokemonSprite } from './PokemonSprite'
import { NumberFilter } from './filters/NumberFilter'
import { containsText } from './filters/containsText'
import { TextFilter } from './filters/TextFilter'

const leadingColumns: TableColumnsType<PokemonSummary> = [
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
]

const actionColumn: TableColumnsType<PokemonSummary>[number] = {
  title: 'Action',
  key: 'action',
  render: (_, pokemon) => <PokemonActions pokemon={pokemon} />,
}

const rowKey = (pokemon: PokemonSummary) => JSON.stringify(pokemon.at)

interface PokemonTableProps {
  pokemon: PokemonSummary[]
  extraColumns?: TableColumnsType<PokemonSummary>
  selected?: PokemonSummary[]
  onSelectedChange?: (selected: PokemonSummary[]) => void
}

export function PokemonTable({ pokemon, extraColumns = [], selected = [], onSelectedChange }: PokemonTableProps) {
  return (
    <Table<PokemonSummary>
      rowKey={rowKey}
      dataSource={pokemon}
      columns={[...leadingColumns, ...extraColumns, actionColumn]}
      rowSelection={
        onSelectedChange && {
          selectedRowKeys: selected.map(rowKey),
          onChange: (_, rows) => onSelectedChange(rows),
        }
      }
      size="small"
      scroll={{ x: 'max-content' }}
      pagination={{ hideOnSinglePage: true }}
    />
  )
}
