import { useState } from 'react'
import { InputNumber } from 'antd'
import type { FilterDropdownProps } from 'antd/es/table/interface'
import { FilterPanel } from './FilterPanel'

interface NumberFilterProps extends FilterDropdownProps {
  placeholder?: string
  min?: number
  max?: number
}

export function NumberFilter({ setSelectedKeys, selectedKeys, confirm, clearFilters, placeholder, min, max }: NumberFilterProps) {
  const [value, setValue] = useState<number | null>((selectedKeys[0] as number | undefined) ?? null)
  return (
    <FilterPanel
      onConfirm={() => confirm()}
      onReset={() => {
        setValue(null)
        clearFilters?.({ confirm: true })
      }}
    >
      <InputNumber
        autoFocus
        placeholder={placeholder}
        min={min}
        max={max}
        value={value}
        onChange={(next) => {
          setValue(next)
          setSelectedKeys(next === null ? [] : [next])
        }}
        onPressEnter={() => confirm()}
        style={{ width: '100%' }}
      />
    </FilterPanel>
  )
}
