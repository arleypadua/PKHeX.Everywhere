import { CopyOutlined } from '@ant-design/icons'
import { useCopyShowdown } from '../hooks/useCopyShowdown'
import { openCalculator } from '../host'
import { ButtonOrMenu } from './ButtonOrMenu'

interface ShowdownActionsProps {
  showdown: () => Promise<string>
  description: string
}

export function ShowdownActions({ showdown, description }: ShowdownActionsProps) {
  const copyShowdown = useCopyShowdown()
  return (
    <ButtonOrMenu
      actions={[
        {
          key: 'calculator',
          label: 'Calculator',
          type: 'primary',
          onClick: async () => openCalculator(await showdown()),
        },
        {
          key: 'showdown',
          label: 'Showdown',
          icon: <CopyOutlined />,
          onClick: async () => copyShowdown(await showdown(), description),
        },
      ]}
    />
  )
}
