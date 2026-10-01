import { useCallback } from 'react'
import { App } from 'antd'
import { EngineError } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'
import { toBase64 } from '../../base64'

const maxFileSize = 6 * 1024 * 1024

export function useLoadSave() {
  const engine = useEngine()
  const { notification } = App.useApp()

  const load = useCallback(
    async (bytes: ArrayBuffer, fileName: string) => engine.game.load(toBase64(new Uint8Array(bytes)), fileName),
    [engine],
  )

  const openFile = useCallback(
    async (file: File) => {
      if (file.size > maxFileSize) {
        notification.error({ title: 'File is too large', description: 'The file should not exceed 6 MB.' })
        return
      }
      try {
        await load(await file.arrayBuffer(), file.name)
      } catch (error) {
        if (!(error instanceof EngineError) || error.code !== 'invalid-save') throw error
        notification.error({ title: error.message, description: 'The file is not a valid save file.' })
      }
    },
    [load, notification],
  )

  const openDemo = useCallback(async () => {
    try {
      const response = await fetch('/data/emerald.sav')
      if (!response.ok) throw new Error(`The demo save returned ${response.status}.`)
      await load(await response.arrayBuffer(), 'emerald.sav')
    } catch (error) {
      if (error instanceof EngineError && error.code !== 'invalid-save') throw error
      notification.error({ title: 'Could not load the demo', description: 'Try again, or open your own save file.' })
    }
  }, [load, notification])

  return { openFile, openDemo }
}
