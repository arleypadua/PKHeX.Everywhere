import { CopyOutlined } from '@ant-design/icons'
import { Button } from 'antd'
import { useCopyShowdown } from '../hooks/useCopyShowdown'
import { openCalculator } from '../host'

interface ShowdownActionsProps {
  showdown: () => Promise<string>
  description: string
}

export function ShowdownActions({ showdown, description }: ShowdownActionsProps) {
  const copyShowdown = useCopyShowdown()
  return (
    <>
      <Button type="primary" onClick={async () => openCalculator(await showdown())}>
        Calculator
      </Button>
      <Button type="link" icon={<CopyOutlined />} onClick={async () => copyShowdown(await showdown(), description)}>
        Showdown
      </Button>
    </>
  )
}
