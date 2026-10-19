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
      <Typography.Paragraph>
        {choice.candidates.length
          ? `'${choice.file.name}' might be from one of these games.`
          : `'${choice.file.name}' has Pokémon or moves that ${choice.pkhexGame} doesn't have. It might be from a ROM hack we don't support.`}
      </Typography.Paragraph>
      <Radio.Group
        vertical
        aria-label="Game"
        value={formatId}
        onChange={(event) => setFormatId(event.target.value as string)}
        options={[
          ...choice.candidates.map((format) => ({ value: format.id, label: format.name })),
          ...(choice.pkhexGame ? [{ value: pkhexFormatId, label: `No, it's ${choice.pkhexGame}` }] : []),
        ]}
      />
    </Modal>
  )
}
