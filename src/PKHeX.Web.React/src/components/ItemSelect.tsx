import { useState } from 'react'
import { Select, Space } from 'antd'
import { ItemIcon } from './ItemIcon'

interface Item {
  id: number
  name: string
  isUnknown?: boolean
}

const hasIcon = (item: Item) => item.id !== 0 && !item.isUnknown

interface ItemSelectProps<T extends Item> {
  items: T[]
  label?: string
  value?: number
  placeholder?: string
  onChange: (item: T) => void
}

export function ItemSelect<T extends Item>({ items, label, value, placeholder = 'Select an item', onChange }: ItemSelectProps<T>) {
  const [search, setSearch] = useState('')

  return (
    <Select<number, { value: number; label: string; icon: boolean }>
      value={value}
      aria-label={label}
      placeholder={placeholder}
      style={{ width: '100%' }}
      showSearch={{ searchValue: search, onSearch: setSearch, optionFilterProp: 'label' }}
      virtual
      options={items.map((item) => ({ value: item.id, label: item.name, icon: hasIcon(item) }))}
      optionRender={({ data }) => <ItemOption icon={data.icon} name={data.label} />}
      labelRender={({ value, label }) => (
        <ItemOption icon={items.some((item) => item.id === Number(value) && hasIcon(item))} name={String(label)} />
      )}
      onSelect={() => setSearch('')}
      onChange={(id) => {
        const item = items.find((item) => item.id === id)
        if (item) onChange(item)
      }}
    />
  )
}

function ItemOption({ icon, name }: { icon: boolean; name: string }) {
  return (
    <Space size={5}>
      {icon && <ItemIcon name={name} />}
      <span>{name}</span>
    </Space>
  )
}
