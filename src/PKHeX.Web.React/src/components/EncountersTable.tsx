import { Button, Table, type TableColumnsType } from 'antd'
import type { EncounterRow } from '@pkhex-everywhere/engine'
import { ItemIcon } from './ItemIcon'
import { PokemonSprite } from './PokemonSprite'
import { NumberFilter } from './filters/NumberFilter'
import { containsText } from './filters/containsText'
import { TextFilter } from './filters/TextFilter'

const levels = (encounter: EncounterRow) => encounter.levelRange.split('-').map(Number)

interface EncountersTableProps {
  encounters: EncounterRow[]
  adding?: number
  onAdd: (encounter: EncounterRow) => void
}

export function EncountersTable({ encounters, adding, onAdd }: EncountersTableProps) {
  const columns: TableColumnsType<EncounterRow> = [
    {
      key: 'sprite',
      width: 80,
      render: (_, encounter) => <PokemonSprite pokemon={encounter} />,
    },
    {
      title: 'Species',
      dataIndex: 'species',
      sorter: (a, b) => a.species.localeCompare(b.species),
      filterDropdown: (props) => <TextFilter {...props} placeholder="Species" />,
      onFilter: (value, encounter) => containsText(encounter.species, value),
    },
    {
      title: 'Encounter',
      dataIndex: 'name',
      sorter: (a, b) => a.name.localeCompare(b.name),
      filterDropdown: (props) => <TextFilter {...props} placeholder="Encounter" />,
      onFilter: (value, encounter) => containsText(encounter.name, value),
    },
    {
      title: 'Ball/Egg',
      key: 'ball',
      render: (_, encounter) => (
        <>
          {encounter.isEgg && '🥚 Egg'}
          {encounter.ball && <ItemIcon name={encounter.ball} />}
        </>
      ),
    },
    {
      title: 'Lvl',
      dataIndex: 'levelRange',
      sorter: (a, b) => levels(a)[0] - levels(b)[0],
      filterDropdown: (props) => <NumberFilter {...props} placeholder="Level" min={1} max={100} />,
      onFilter: (value, encounter) => {
        const [min, max = min] = levels(encounter)
        return min <= Number(value) && Number(value) <= max
      },
    },
    {
      title: 'Location',
      dataIndex: 'location',
      sorter: (a, b) => a.location.localeCompare(b.location),
      filterDropdown: (props) => <TextFilter {...props} placeholder="Location" />,
      onFilter: (value, encounter) => containsText(encounter.location, value),
    },
    {
      title: 'Game',
      dataIndex: 'version',
    },
    {
      title: 'Action',
      key: 'action',
      render: (_, encounter) => (
        <Button
          type="link"
          loading={adding === encounter.index}
          disabled={adding !== undefined}
          onClick={() => onAdd(encounter)}
        >
          Add to box
        </Button>
      ),
    },
  ]

  return (
    <Table
      rowKey="index"
      dataSource={encounters}
      columns={columns}
      size="small"
      scroll={{ x: 'max-content' }}
      pagination={{ hideOnSinglePage: true }}
    />
  )
}
