import { useEffect, useState } from 'react'
import { InboxOutlined } from '@ant-design/icons'
import { Modal, Typography, theme } from 'antd'
import { FormatPickerModal } from '../pages/load/FormatPickerModal'
import { useLoadSave } from '../pages/load/useLoadSave'

export function SaveFileDrop() {
  const { openFile, formatChoice, chooseFormat, cancelFormatChoice } = useLoadSave()
  const [dragging, setDragging] = useState(false)
  const { token } = theme.useToken()

  useEffect(() => {
    let depth = 0
    const hasFiles = (event: DragEvent) => event.dataTransfer?.types.includes('Files')
    const reset = () => {
      depth = 0
      setDragging(false)
    }
    const enter = (event: DragEvent) => {
      if (!hasFiles(event)) return
      event.preventDefault()
      depth += 1
      setDragging(true)
    }
    const over = (event: DragEvent) => {
      if (!hasFiles(event)) return
      event.preventDefault()
      event.dataTransfer!.dropEffect = 'copy'
    }
    const leave = () => {
      depth = Math.max(0, depth - 1)
      if (depth === 0) setDragging(false)
    }
    const drop = (event: DragEvent) => {
      if (!hasFiles(event)) return
      event.preventDefault()
      reset()
      const file = event.dataTransfer?.files[0]
      if (file) void openFile(file)
    }

    window.addEventListener('dragenter', enter)
    window.addEventListener('dragover', over)
    window.addEventListener('dragleave', leave)
    window.addEventListener('drop', drop)
    window.addEventListener('dragend', reset)
    window.addEventListener('blur', reset)
    return () => {
      window.removeEventListener('dragenter', enter)
      window.removeEventListener('dragover', over)
      window.removeEventListener('dragleave', leave)
      window.removeEventListener('drop', drop)
      window.removeEventListener('dragend', reset)
      window.removeEventListener('blur', reset)
    }
  }, [openFile])

  return (
    <>
      {dragging && (
        <Modal open centered title="Drop to open save" footer={null} closable={false} keyboard={false}>
          <div style={{ textAlign: 'center', padding: '24px 0' }}>
            <InboxOutlined aria-hidden style={{ fontSize: 48, color: token.colorPrimary }} />
            <Typography.Paragraph style={{ marginTop: 20, marginBottom: 0 }}>
              Drop your save file anywhere in this window.
            </Typography.Paragraph>
          </div>
        </Modal>
      )}
      {formatChoice && <FormatPickerModal choice={formatChoice} onChoose={chooseFormat} onCancel={cancelFormatChoice} />}
    </>
  )
}
