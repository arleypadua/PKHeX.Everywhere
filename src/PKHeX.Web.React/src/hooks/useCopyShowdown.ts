import { useCallback } from 'react'
import { App } from 'antd'

export function useCopyShowdown(): (showdown: string, description: string) => Promise<void> {
  const { notification } = App.useApp()
  return useCallback(
    async (showdown: string, description: string) => {
      await navigator.clipboard.writeText(showdown)
      notification.success({ title: 'Showdown copied to clipboard', description })
    },
    [notification],
  )
}
