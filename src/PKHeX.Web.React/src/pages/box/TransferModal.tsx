import { useRef, useState, type ChangeEvent } from 'react'
import { App, Modal, Typography } from 'antd'
import type { SaveSummary, TransferSummary } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'
import { gameName } from '../../capabilities'
import { FormatPickerModal } from '../load/FormatPickerModal'
import { SaveFileDrop } from '../load/SaveFileDrop'
import type { FormatChoice } from '../load/useLoadSave'
import { openTransfer } from './openTransfer'
import { TransferPicker } from './TransferPicker'

export function TransferModal({ game, onClose }: { game: SaveSummary; onClose: () => void }) {
  const engine = useEngine()
  const { notification } = App.useApp()
  const [formatChoice, setFormatChoice] = useState<FormatChoice>()
  const [opened, setOpened] = useState<TransferSummary>()
  const fileInput = useRef<HTMLInputElement>(null)

  const openSave = async (file: File, formatId?: string) => {
    const opening = await openTransfer(engine.transfer, file, formatId)
    if (opening.kind === 'opened') setOpened(opening.summary)
    else if (opening.kind === 'formatChoice')
      setFormatChoice({ file, candidates: opening.candidates, pkhexGame: opening.pkhexGame })
    else if (opening.kind === 'noRoute')
      notification.error({ title: `Pokémon can't move from ${gameName(opening.partner)} to ${gameName(game)}.` })
    else notification.error({ title: `'${file.name}' is not a supported save file.`, description: 'The file is not a valid save file.' })
  }

  const chooseFormat = async (formatId: string) => {
    if (!formatChoice) return
    setFormatChoice(undefined)
    await openSave(formatChoice.file, formatId)
  }

  const loadFile = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (file) await openSave(file)
  }

  const close = async () => {
    await engine.transfer.close()
    onClose()
  }

  if (opened) return <TransferPicker transfer={opened} onClose={close} />

  return (
    <>
      <Modal
        open
        title="Transfer"
        okText="Open save file..."
        onOk={() => fileInput.current?.click()}
        onCancel={close}
        modalRender={(content) => <SaveFileDrop onOpen={openSave}>{content}</SaveFileDrop>}
      >
        <Typography.Paragraph>{`Open another ${gameName(game)} save to move its Pokémon into this one.`}</Typography.Paragraph>
        <input ref={fileInput} type="file" hidden onChange={loadFile} />
      </Modal>
      {formatChoice && (
        <FormatPickerModal choice={formatChoice} onChoose={chooseFormat} onCancel={() => setFormatChoice(undefined)} />
      )}
    </>
  )
}
