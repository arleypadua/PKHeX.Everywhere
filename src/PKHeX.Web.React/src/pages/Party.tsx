import { useState, type ReactNode } from 'react'
import { ArrowLeftOutlined, CopyOutlined } from '@ant-design/icons'
import { Button, Dropdown, Flex, Input, InputNumber, notification, Space, Table, Typography, type TableColumnsType } from 'antd'
import type { FilterDropdownProps } from 'antd/es/table/interface'
import type { PokemonSummary } from '@pkhex-everywhere/engine'
import { useEngine, useParty } from '@pkhex-everywhere/react'
import { openCalculator, useNavigate } from '../host'

export default function Party() {
  const { party } = useParty()
  const engine = useEngine()
  const navigate = useNavigate()
  const [api, contextHolder] = notification.useNotification()

  const copyShowdown = async (showdown: string, description: string) => {
    await navigator.clipboard.writeText(showdown)
    api.success({ title: 'Showdown copied to clipboard', description })
  }

  const columns: TableColumnsType<PokemonSummary> = [
    {
      key: 'sprite',
      width: 80,
      render: (_, pokemon) => <Sprite pokemon={pokemon} />,
    },
    {
      title: 'Name',
      dataIndex: 'species',
      sorter: (a, b) => a.species.localeCompare(b.species),
      filterDropdown: (props) => <NameFilter {...props} />,
      onFilter: (value, pokemon) => pokemon.species.toLowerCase().includes(String(value).toLowerCase()),
    },
    {
      title: 'Level',
      dataIndex: 'level',
      sorter: (a, b) => a.level - b.level,
      filterDropdown: (props) => <LevelFilter {...props} />,
      onFilter: (value, pokemon) => pokemon.level === value,
    },
    {
      title: 'Action',
      key: 'action',
      render: (_, pokemon) => (
        <Dropdown.Button
          type="link"
          trigger={['click']}
          onClick={() => navigate(`/pokemon/${pokemon.at.source}/${pokemon.id}`)}
          menu={{
            items: [
              { key: 'clone', label: 'Clone', onClick: () => navigate(`/pokemon/${pokemon.id}/clone`) },
              {
                key: 'calculator',
                label: 'Calculator',
                onClick: async () => openCalculator(await engine.pokemon.showdown(pokemon.at)),
              },
              {
                key: 'showdown',
                label: 'Showdown',
                onClick: async () => copyShowdown(await engine.pokemon.showdown(pokemon.at), pokemon.species),
              },
            ],
          }}
        >
          View
        </Dropdown.Button>
      ),
    },
  ]

  return (
    <Flex vertical gap={20}>
      {contextHolder}
      <Flex justify="space-between" align="center" wrap gap="small">
        <Space>
          <Button type="text" icon={<ArrowLeftOutlined />} aria-label="Back" onClick={() => history.back()} />
          <Typography.Title level={4} style={{ margin: 0 }}>
            Party
          </Typography.Title>
        </Space>
        <Space>
          <Button type="primary" onClick={async () => openCalculator(await engine.party.showdown())}>
            Calculator
          </Button>
          <Button
            type="link"
            icon={<CopyOutlined />}
            onClick={async () => copyShowdown(await engine.party.showdown(), `${party.length} entries`)}
          >
            Showdown
          </Button>
        </Space>
      </Flex>
      <Table
        rowKey={(pokemon) => `${pokemon.at.slot}:${pokemon.id}`}
        dataSource={party}
        columns={columns}
        size="small"
        pagination={{ hideOnSinglePage: true }}
      />
    </Flex>
  )
}

function Sprite({ pokemon }: { pokemon: PokemonSummary }) {
  const form = pokemon.form.name.toLowerCase()
  const suffix = form === 'alola' ? '-alola' : form === 'galar' ? '-galar' : ''
  return (
    <img
      alt={pokemon.species}
      src={`https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/versions/generation-viii/icons/${pokemon.speciesId}${suffix}.png`}
    />
  )
}

function NameFilter({ setSelectedKeys, selectedKeys, confirm, clearFilters }: FilterDropdownProps) {
  return (
    <FilterPanel confirm={confirm} clear={() => clearFilters?.({ confirm: true })}>
      <Input
        autoFocus
        placeholder="Name"
        value={selectedKeys[0] as string | undefined}
        onChange={(e) => setSelectedKeys(e.target.value ? [e.target.value] : [])}
        onPressEnter={() => confirm()}
      />
    </FilterPanel>
  )
}

function LevelFilter({ setSelectedKeys, selectedKeys, confirm, clearFilters }: FilterDropdownProps) {
  const [level, setLevel] = useState<number | null>((selectedKeys[0] as number | undefined) ?? null)
  return (
    <FilterPanel
      confirm={confirm}
      clear={() => {
        setLevel(null)
        clearFilters?.({ confirm: true })
      }}
    >
      <InputNumber
        autoFocus
        placeholder="Level"
        min={1}
        max={100}
        value={level}
        onChange={(value) => {
          setLevel(value)
          setSelectedKeys(value === null ? [] : [value])
        }}
        onPressEnter={() => confirm()}
        style={{ width: '100%' }}
      />
    </FilterPanel>
  )
}

function FilterPanel({ confirm, clear, children }: { confirm: () => void; clear: () => void; children: ReactNode }) {
  return (
    <Flex vertical gap="small" style={{ padding: 8 }} onKeyDown={(e) => e.stopPropagation()}>
      {children}
      <Flex justify="space-between" gap="small">
        <Button size="small" onClick={clear}>
          Reset
        </Button>
        <Button size="small" type="primary" onClick={() => confirm()}>
          OK
        </Button>
      </Flex>
    </Flex>
  )
}
