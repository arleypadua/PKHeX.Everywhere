import { Button, Tooltip } from 'antd'
import { EngineError, type PlugInAction, type PokemonHandle } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'

interface PlugInActionButtonProps {
  action: PlugInAction
  target?: PokemonHandle
}

export function PlugInActionButton({ action, target }: PlugInActionButtonProps) {
  const engine = useEngine()

  const run = async () => {
    try {
      await engine.plugins.run(action.id, target ?? null)
    } catch (error) {
      // PlugInOutcomes already reports failures from the PlugInRan event.
      if (!(error instanceof EngineError) || error.code !== 'plugin-failed') throw error
    }
  }

  const button = (
    <Button type="link" disabled={action.disabled} onClick={run}>
      {action.label}
    </Button>
  )

  return action.disabled && action.reason ? <Tooltip title={action.reason}>{button}</Tooltip> : button
}
