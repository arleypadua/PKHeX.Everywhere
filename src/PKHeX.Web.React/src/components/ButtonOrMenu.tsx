import type { ReactNode } from 'react'
import { EllipsisOutlined } from '@ant-design/icons'
import { Button, Dropdown, Space, type ButtonProps } from 'antd'

export interface ButtonOrMenuAction {
  key: string
  label: string
  icon?: ReactNode
  type?: ButtonProps['type']
  onClick: () => void | Promise<void>
  disabled?: boolean
}

interface ButtonOrMenuProps {
  actions: ButtonOrMenuAction[]
}

export function ButtonOrMenu({ actions }: ButtonOrMenuProps) {
  if (actions.length === 0) return null
  const [main, ...rest] = actions
  const mainButton = (
    <Button type={main.type} icon={main.icon} disabled={main.disabled} onClick={main.onClick}>
      {main.label}
    </Button>
  )
  if (rest.length === 0) return mainButton

  return (
    <Space.Compact>
      {mainButton}
      <Dropdown
        trigger={['click']}
        menu={{
          items: rest.map(({ key, label, icon, disabled, onClick }) => ({
            key,
            label,
            icon,
            disabled,
            onClick: () => void onClick(),
          })),
        }}
      >
        <Button type={main.type} icon={<EllipsisOutlined />} aria-label="More actions" />
      </Dropdown>
    </Space.Compact>
  )
}
