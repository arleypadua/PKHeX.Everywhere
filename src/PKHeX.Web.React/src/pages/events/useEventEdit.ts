import { useCallback } from 'react'
import { App } from 'antd'
import { EngineError } from '@pkhex-everywhere/engine'

export function useEventEdit() {
  const { notification } = App.useApp()

  return useCallback(
    async (edit: () => Promise<void>) => {
      try {
        await edit()
      } catch (error) {
        if (!(error instanceof EngineError) || (error.code !== 'out-of-range' && error.code !== 'not-found')) throw error
        notification.error({ title: "Couldn't change the event", description: error.message })
      }
    },
    [notification],
  )
}
