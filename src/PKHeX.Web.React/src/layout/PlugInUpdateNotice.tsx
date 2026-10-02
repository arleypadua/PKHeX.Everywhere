import { useEffect } from 'react'
import { App } from 'antd'
import { plugInsRefreshed } from '../app'

export function PlugInUpdateNotice() {
  const { notification } = App.useApp()

  useEffect(() => {
    let active = true
    void plugInsRefreshed.then(({ hasNewerVersions }) => {
      if (active && hasNewerVersions)
        notification.info({ title: 'New plug-in version available', description: 'Update it from the Plug-ins page.' })
    })
    return () => {
      active = false
    }
  }, [notification])

  return null
}
