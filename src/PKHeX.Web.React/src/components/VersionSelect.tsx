import { Select } from 'antd'
import type { VersionEntry } from '@pkhex-everywhere/engine'
import { useQuery } from '@pkhex-everywhere/react'

interface VersionSelectProps {
  value?: number
  onChange: (version: VersionEntry) => void
}

export function VersionSelect({ value, onChange }: VersionSelectProps) {
  const { versions } = useQuery('encounters.versions')

  return (
    <Select<number>
      value={value}
      aria-label="Version"
      placeholder="Version"
      style={{ width: '100%' }}
      showSearch={{ optionFilterProp: 'label' }}
      options={versions.map((version) => ({ value: version.id, label: version.name }))}
      onChange={(id) => {
        const version = versions.find((version) => version.id === id)
        if (version) onChange(version)
      }}
    />
  )
}
