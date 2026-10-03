import { useState } from 'react'
import { Select } from 'antd'
import type { Choice } from '@pkhex-everywhere/engine'

interface ChoiceSelectProps {
  label: string
  choices: Choice[]
  value: number
  disabled?: boolean
  onChange: (id: number) => void
}

export function ChoiceSelect({ label, choices, value, disabled, onChange }: ChoiceSelectProps) {
  const [search, setSearch] = useState('')

  return (
    <Select<number>
      value={value}
      disabled={disabled}
      aria-label={label}
      placeholder={label}
      style={{ width: '100%' }}
      showSearch={{ searchValue: search, onSearch: setSearch, optionFilterProp: 'label' }}
      virtual
      options={choices.map((choice) => ({ value: choice.id, label: choice.name }))}
      onSelect={() => setSearch('')}
      onChange={onChange}
    />
  )
}
