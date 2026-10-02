import { useState } from 'react'
import { Select, Space } from 'antd'
import { ItemIcon } from './ItemIcon'

interface Item {
  id: number
  name: string
}

interface ItemSelectProps<T extends Item> {
  items: T[]
  label?: string
  value?: number
  onChange: (item: T) => void
}

export function ItemSelect<T extends Item>({ items, label, value, onChange }: ItemSelectProps<T>) {
  const [search, setSearch] = useState('')

  return (
    <Select<number, { value: number; label: string }>
      value={value}
      aria-label={label}
      placeholder="Select an item"
      style={{ width: '100%' }}
      showSearch={{ searchValue: search, onSearch: setSearch, optionFilterProp: 'label' }}
      virtual
      options={items.map((item) => ({ value: item.id, label: item.name }))}
      optionRender={({ data }) => <ItemOption id={data.value} name={data.label} />}
      labelRender={({ value, label }) => <ItemOption id={Number(value)} name={String(label)} />}
      onSelect={() => setSearch('')}
      onChange={(id) => {
        const item = items.find((item) => item.id === id)
        if (item) onChange(item)
      }}
    />
  )
}

function ItemOption({ id, name }: { id: number; name: string }) {
  return (
    <Space size={5}>
      {id !== 0 && <ItemIcon name={name} />}
      <span>{name}</span>
    </Space>
  )
}
