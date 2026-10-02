import { useEffect } from 'react'
import { App } from 'antd'
import { useEngine } from '@pkhex-everywhere/react'
import { useNavigate } from '../host'
import { outcomeOf } from '../plugins/outcomes'

export function PlugInOutcomes() {
  const engine = useEngine()
  const navigate = useNavigate()
  const { notification } = App.useApp()

  useEffect(
    () =>
      engine.onEvent(async (event) => {
        if (event.type !== 'plugInRan') return
        const pages = event.outcome?.kind === 'openPage' ? await engine.plugins.pages() : []
        const outcome = outcomeOf(event, pages)
        if (outcome?.kind === 'navigate') await navigate(outcome.url)
        if (outcome?.kind === 'notify')
          notification.open({
            type: outcome.type === 'none' ? undefined : outcome.type,
            title: outcome.message,
            description: outcome.description,
          })
      }),
    [engine, navigate, notification],
  )

  return null
}
