import { useState } from 'react'
import { Flex, InputNumber, Modal, Typography } from 'antd'
import type { OwnedItem } from '@pkhex-everywhere/engine'
import { useSetItem } from './useSetItem'

interface EditItemModalProps {
  pouch: string
  item: OwnedItem
  onClose: () => void
}

export function EditItemModal({ pouch, item, onClose }: EditItemModalProps) {
  const [count, setCount] = useState<number | null>(item.count)
  const { save, saving } = useSetItem()

  const handleOk = async () => {
    if (count === null) return
    if (await save({ pouch, itemId: item.id }, count)) onClose()
  }

  return (
    <Modal open title={item.name} onOk={handleOk} onCancel={onClose} confirmLoading={saving} okButtonProps={{ disabled: count === null }}>
      <Flex vertical align="center" gap={20}>
        <InputNumber<number> min={0} max={item.maxCount} precision={0} value={count} onChange={setCount} />
        <Typography.Text italic>Setting to 0 will remove the item</Typography.Text>
      </Flex>
    </Modal>
  )
}
