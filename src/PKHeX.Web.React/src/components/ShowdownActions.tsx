import { CopyOutlined, EllipsisOutlined } from '@ant-design/icons'
import { Button, Dropdown, Space } from 'antd'
import { useCopyShowdown } from '../hooks/useCopyShowdown'
import { openCalculator } from '../host'

interface ShowdownActionsProps {
  showdown: () => Promise<string>
  description: string
}

export function ShowdownActions({ showdown, description }: ShowdownActionsProps) {
  const copyShowdown = useCopyShowdown()
  return (
    <Space.Compact>
      <Button type="primary" onClick={async () => openCalculator(await showdown())}>
        Calculator
      </Button>
      <Dropdown
        trigger={['click']}
        menu={{
          items: [{ key: 'showdown', icon: <CopyOutlined />, label: 'Showdown' }],
          onClick: async () => copyShowdown(await showdown(), description),
        }}
      >
        <Button type="primary" icon={<EllipsisOutlined />} aria-label="More actions" />
      </Dropdown>
    </Space.Compact>
  )
}
