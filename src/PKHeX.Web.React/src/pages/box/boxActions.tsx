import { CalculatorOutlined, CopyOutlined, FolderOpenOutlined, SwapOutlined } from '@ant-design/icons'
import type { ButtonOrMenuAction } from '../../components/ButtonOrMenu'

type Handler = ButtonOrMenuAction['onClick']

interface BoxActionHandlers {
  add: Handler
  load: Handler
  calculator: Handler
  showdown: Handler
  transfer: Handler
}

export function boxActions(
  supports: { encounters: boolean; showdown: boolean },
  handlers: BoxActionHandlers,
): ButtonOrMenuAction[] {
  return [
    ...(supports.encounters ? [{ key: 'add', label: 'Add', type: 'primary' as const, onClick: handlers.add }] : []),
    { key: 'load', label: 'Load *.pk', icon: <FolderOpenOutlined />, onClick: handlers.load },
    ...(supports.showdown
      ? [
          { key: 'calculator', label: 'Calculator', icon: <CalculatorOutlined />, onClick: handlers.calculator },
          { key: 'showdown', label: 'Showdown', icon: <CopyOutlined />, onClick: handlers.showdown },
        ]
      : []),
    { key: 'transfer', label: 'Transfer', icon: <SwapOutlined />, onClick: handlers.transfer },
  ]
}
