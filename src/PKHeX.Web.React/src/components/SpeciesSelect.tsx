import { useState } from 'react'
import { Select, Space } from 'antd'
import type { SpeciesEntry } from '@pkhex-everywhere/engine'
import { useQuery } from '@pkhex-everywhere/react'
import { PokemonSprite } from './PokemonSprite'

interface SpeciesSelectProps {
  value?: number
  onChange: (species: SpeciesEntry) => void
}

export function SpeciesSelect({ value, onChange }: SpeciesSelectProps) {
  const species = useQuery('species.list')
  const [search, setSearch] = useState('')

  return (
    <Select<number, { value: number; label: string }>
      value={value}
      aria-label="Species"
      placeholder="Select a species"
      style={{ width: '100%' }}
      showSearch={{ searchValue: search, onSearch: setSearch, optionFilterProp: 'label' }}
      virtual
      options={species.map((entry) => ({ value: entry.id, label: entry.name }))}
      optionRender={({ data }) => <SpeciesOption id={data.value} name={data.label} />}
      labelRender={({ value, label }) => <SpeciesOption id={Number(value)} name={String(label)} />}
      onSelect={() => setSearch('')}
      onChange={(id) => {
        const entry = species.find((entry) => entry.id === id)
        if (entry) onChange(entry)
      }}
    />
  )
}

function SpeciesOption({ id, name }: { id: number; name: string }) {
  return (
    <Space size={5} align="center">
      <span style={{ display: 'inline-flex', width: 40, height: 30, overflow: 'hidden', alignItems: 'center' }}>
        <PokemonSprite pokemon={{ speciesId: id, species: name }} />
      </span>
      <span>{name}</span>
    </Space>
  )
}
