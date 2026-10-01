import { InputNumber } from 'antd'
import type { FilterDropdownProps } from 'antd/es/table/interface'
import { FilterPanel } from './FilterPanel'

interface NumberFilterProps extends FilterDropdownProps {
  placeholder?: string
  min?: number
  max?: number
}

export function NumberFilter({ setSelectedKeys, selectedKeys, confirm, clearFilters, placeholder, min, max }: NumberFilterProps) {
  return (
    <FilterPanel onConfirm={() => confirm()} onReset={() => clearFilters?.({ confirm: true })}>
      <InputNumber<number>
        autoFocus
        placeholder={placeholder}
        min={min}
        max={max}
        value={(selectedKeys[0] as number | undefined) ?? null}
        onChange={(value) => setSelectedKeys(value === null ? [] : [value])}
        onPressEnter={() => confirm()}
        style={{ width: '100%' }}
      />
    </FilterPanel>
  )
}
