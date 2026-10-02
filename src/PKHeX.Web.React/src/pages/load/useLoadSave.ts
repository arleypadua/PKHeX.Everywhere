import { useCallback } from 'react'
import { App } from 'antd'
import { EngineError } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'

const maxFileSize = 6 * 1024 * 1024

export function useLoadSave() {
  const engine = useEngine()
  const { notification } = App.useApp()

  const openFile = useCallback(
    async (file: File) => {
      if (file.size > maxFileSize) {
        notification.error({ title: 'File is too large', description: 'The file should not exceed 6 MB.' })
        return
      }
      try {
        await engine.game.load(file)
      } catch (error) {
        if (!(error instanceof EngineError) || error.code !== 'invalid-save') throw error
        notification.error({ title: error.message, description: 'The file is not a valid save file.' })
      }
    },
    [engine, notification],
  )

  const openDemo = useCallback(async () => {
    try {
      const response = await fetch('/data/emerald.sav')
      if (!response.ok) throw new Error(`The demo save returned ${response.status}.`)
      await engine.game.load(await response.blob(), 'emerald.sav')
    } catch (error) {
      if (error instanceof EngineError && error.code !== 'invalid-save') throw error
      notification.error({ title: 'Could not load the demo', description: 'Try again, or open your own save file.' })
    }
  }, [engine, notification])

  return { openFile, openDemo }
}
