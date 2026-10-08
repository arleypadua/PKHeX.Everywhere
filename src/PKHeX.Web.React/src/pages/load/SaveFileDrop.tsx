import { useCallback, useEffect, useRef, useState, type DragEvent, type ReactNode } from 'react'
import { InboxOutlined } from '@ant-design/icons'
import { Modal, Typography, theme } from 'antd'

interface SaveFileDropProps {
  onOpen: (file: File) => void
  disabled?: boolean
  children: ReactNode
}

export function SaveFileDrop({ onOpen, disabled = false, children }: SaveFileDropProps) {
  const [dragging, setDragging] = useState(false)
  const depth = useRef(0)
  const { token } = theme.useToken()

  const reset = useCallback(() => {
    depth.current = 0
    setDragging(false)
  }, [])

  useEffect(() => {
    window.addEventListener('blur', reset)
    return () => window.removeEventListener('blur', reset)
  }, [reset])

  // modal portals bubble through React even when their DOM is outside this element.
  const isFileDragInside = (event: DragEvent<HTMLDivElement>) =>
    event.currentTarget.contains(event.target as Node) && event.dataTransfer.types.includes('Files')

  const enter = (event: DragEvent<HTMLDivElement>) => {
    if (!isFileDragInside(event)) return
    event.preventDefault()
    if (disabled) return
    depth.current += 1
    setDragging(true)
  }

  const over = (event: DragEvent<HTMLDivElement>) => {
    if (!isFileDragInside(event)) return
    event.preventDefault()
    event.dataTransfer.dropEffect = disabled ? 'none' : 'copy'
  }

  const leave = (event: DragEvent<HTMLDivElement>) => {
    if (!event.currentTarget.contains(event.target as Node)) return
    depth.current = Math.max(0, depth.current - 1)
    if (depth.current === 0) setDragging(false)
  }

  const drop = (event: DragEvent<HTMLDivElement>) => {
    if (!isFileDragInside(event)) return
    event.preventDefault()
    reset()
    if (disabled) return
    const file = event.dataTransfer.files[0]
    if (file) onOpen(file)
  }

  return (
    <div style={{ width: '100%' }} onDragEnter={enter} onDragOver={over} onDragLeave={leave} onDrop={drop} onDragEnd={reset}>
      {children}
      {/* Inside this element, dragging over the overlay doesn't count as leaving the drop area. */}
      {dragging && (
        <Modal open centered title="Drop to open save" footer={null} closable={false} keyboard={false} getContainer={false}>
          <div style={{ textAlign: 'center', padding: '24px 0' }}>
            <InboxOutlined aria-hidden style={{ fontSize: 48, color: token.colorPrimary }} />
            <Typography.Paragraph style={{ marginTop: 20, marginBottom: 0 }}>
              Drop your save file here.
            </Typography.Paragraph>
          </div>
        </Modal>
      )}
    </div>
  )
}
