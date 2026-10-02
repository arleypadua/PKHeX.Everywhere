import { useCallback, useState } from 'react'
import { App } from 'antd'
import { EngineError, type FormatEntry } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'
import { romHacksEnabled } from '../../host'
import { loadFailure } from './loadFailure'

const maxFileSize = 6 * 1024 * 1024

export interface FormatChoice {
  file: File
  candidates: FormatEntry[]
}

export function useLoadSave() {
  const engine = useEngine()
  const { notification } = App.useApp()
  const [formatChoice, setFormatChoice] = useState<FormatChoice>()

  const openFile = useCallback(
    async (file: File, formatId?: string) => {
      if (file.size > maxFileSize) {
        notification.error({ title: 'File is too large', description: 'The file should not exceed 6 MB.' })
        return false
      }
      try {
        await engine.game.load(file, undefined, formatId)
        return true
      } catch (error) {
        const failure = loadFailure(error, romHacksEnabled)
        if (!failure) throw error
        if (failure.kind === 'formatChoice') setFormatChoice({ file, candidates: failure.candidates })
        else notification.error({ title: `'${file.name}' is not a supported save file.`, description: 'The file is not a valid save file.' })
        return false
      }
    },
    [engine, notification],
  )

  const chooseFormat = useCallback(
    async (formatId: string) => {
      if (!formatChoice) return
      setFormatChoice(undefined)
      await openFile(formatChoice.file, formatId)
    },
    [formatChoice, openFile],
  )

  const cancelFormatChoice = useCallback(() => setFormatChoice(undefined), [])

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

  return { openFile, openDemo, formatChoice, chooseFormat, cancelFormatChoice }
}
