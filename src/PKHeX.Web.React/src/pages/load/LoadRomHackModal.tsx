import { useRef, useState, type ChangeEvent } from 'react'
import { Modal, Select } from 'antd'
import { useQuery } from '@pkhex-everywhere/react'
import { useLoadSave } from './useLoadSave'

export function LoadRomHackModal({ onClose }: { onClose: () => void }) {
  const formats = useQuery('game.formats')
  const { openFile } = useLoadSave()
  const [formatId, setFormatId] = useState<string>()
  const fileInput = useRef<HTMLInputElement>(null)

  const loadFile = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (file && formatId && (await openFile(file, formatId))) onClose()
  }

  return (
    <Modal
      open
      title="Load ROM Hack"
      okText="Open save file..."
      onOk={() => fileInput.current?.click()}
      onCancel={onClose}
      okButtonProps={{ disabled: !formatId }}
    >
      <Select
        showSearch={{ optionFilterProp: 'label' }}
        placeholder="Search ROM hacks"
        aria-label="ROM hack"
        options={formats.map((format) => ({ value: format.id, label: format.name }))}
        value={formatId}
        onChange={setFormatId}
        style={{ width: '100%' }}
      />
      <input ref={fileInput} type="file" hidden onChange={loadFile} />
    </Modal>
  )
}
