import { useState } from 'react'
import { Modal, Radio, Typography } from 'antd'
import type { FormatChoice } from './useLoadSave'

const pkhexFormatId = 'pkhex'

interface FormatPickerModalProps {
  choice: FormatChoice
  onChoose: (formatId: string) => void
  onCancel: () => void
}

export function FormatPickerModal({ choice, onChoose, onCancel }: FormatPickerModalProps) {
  const [formatId, setFormatId] = useState<string>()

  return (
    <Modal
      open
      title="Is this save from a ROM hack?"
      okText="Open"
      onOk={() => formatId && onChoose(formatId)}
      onCancel={onCancel}
      okButtonProps={{ disabled: !formatId }}
    >
      <Typography.Paragraph>'{choice.file.name}' might be from one of these games.</Typography.Paragraph>
      <Radio.Group
        vertical
        aria-label="Game"
        value={formatId}
        onChange={(event) => setFormatId(event.target.value as string)}
        options={[
          ...choice.candidates.map((format) => ({ value: format.id, label: format.name })),
          { value: pkhexFormatId, label: "No, it's FireRed" },
        ]}
      />
    </Modal>
  )
}
