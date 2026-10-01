import { Input } from 'antd'
import type { FilterDropdownProps } from 'antd/es/table/interface'
import { FilterPanel } from './FilterPanel'

interface TextFilterProps extends FilterDropdownProps {
  placeholder?: string
}

export function TextFilter({ setSelectedKeys, selectedKeys, confirm, clearFilters, placeholder }: TextFilterProps) {
  return (
    <FilterPanel onConfirm={() => confirm()} onReset={() => clearFilters?.({ confirm: true })}>
      <Input
        autoFocus
        placeholder={placeholder}
        value={selectedKeys[0] as string | undefined}
        onChange={(e) => setSelectedKeys(e.target.value ? [e.target.value] : [])}
        onPressEnter={() => confirm()}
      />
    </FilterPanel>
  )
}
