import { useState } from 'react'
import { Select, Space } from 'antd'
import { ItemIcon } from './ItemIcon'

interface Item {
  id: number
  name: string
}

interface ItemSelectProps<T extends Item> {
  items: T[]
  value?: number
  onChange: (item: T) => void
}

export function ItemSelect<T extends Item>({ items, value, onChange }: ItemSelectProps<T>) {
  const [search, setSearch] = useState('')

  return (
    <Select<number, { value: number; label: string }>
      value={value}
      placeholder="Select an item"
      style={{ width: '100%' }}
      showSearch={{ searchValue: search, onSearch: setSearch, optionFilterProp: 'label' }}
      virtual
      options={items.map((item) => ({ value: item.id, label: item.name }))}
      optionRender={({ data }) => <ItemOption name={data.label} />}
      labelRender={({ label }) => <ItemOption name={String(label)} />}
      onSelect={() => setSearch('')}
      onChange={(id) => {
        const item = items.find((item) => item.id === id)
        if (item) onChange(item)
      }}
    />
  )
}

function ItemOption({ name }: { name: string }) {
  return (
    <Space size={5}>
      <ItemIcon name={name} />
      <span>{name}</span>
    </Space>
  )
}
