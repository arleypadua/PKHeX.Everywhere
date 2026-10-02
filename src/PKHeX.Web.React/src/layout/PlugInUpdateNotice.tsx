import { useEffect } from 'react'
import { App } from 'antd'
import { plugInsRefreshed } from '../app'

export function PlugInUpdateNotice() {
  const { notification } = App.useApp()

  useEffect(() => {
    let mounted = true
    void plugInsRefreshed.then(({ hasNewerVersions }) => {
      if (mounted && hasNewerVersions)
        notification.info({ title: 'New plug-in version available', description: 'Visit the plug-in page and update them.' })
    })
    return () => {
      mounted = false
    }
  }, [notification])

  return null
}
