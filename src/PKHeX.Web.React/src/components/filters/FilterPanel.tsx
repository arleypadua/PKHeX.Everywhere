import type { ReactNode } from 'react'
import { Button, Flex } from 'antd'

interface FilterPanelProps {
  onConfirm: () => void
  onReset: () => void
  children: ReactNode
}

export function FilterPanel({ onConfirm, onReset, children }: FilterPanelProps) {
  return (
    <Flex vertical gap="small" style={{ padding: 8 }} onKeyDown={(e) => e.stopPropagation()}>
      {children}
      <Flex justify="space-between" gap="small">
        <Button size="small" onClick={onReset}>
          Reset
        </Button>
        <Button size="small" type="primary" onClick={onConfirm}>
          OK
        </Button>
      </Flex>
    </Flex>
  )
}
