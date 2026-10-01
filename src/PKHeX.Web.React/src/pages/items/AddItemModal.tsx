import { useState } from 'react'
import { Flex, InputNumber, Modal } from 'antd'
import type { AddableItem, Pouch } from '@pkhex-everywhere/engine'
import { ItemSelect } from '../../components/ItemSelect'
import { useSetItem } from './useSetItem'

interface AddItemModalProps {
  pouch: Pouch
  onClose: () => void
}

export function AddItemModal({ pouch, onClose }: AddItemModalProps) {
  const [item, setItem] = useState<AddableItem>()
  const [count, setCount] = useState<number | null>(null)
  const { submit, saving } = useSetItem()

  const selectItem = (selected: AddableItem) => {
    setItem(selected)
    setCount((current) => (current === null ? null : Math.min(current, selected.maxCount)))
  }

  const handleOk = async () => {
    if (!item || count === null) return
    if (await submit({ pouch: pouch.name, itemId: item.id }, count)) onClose()
  }

  return (
    <Modal
      open
      title={`Add item to "${pouch.name}"`}
      onOk={handleOk}
      onCancel={onClose}
      confirmLoading={saving}
      okButtonProps={{ disabled: !item || count === null }}
    >
      <Flex vertical align="center" gap={20}>
        <ItemSelect items={pouch.addable} value={item?.id} onChange={selectItem} />
        <InputNumber<number>
          placeholder="Quantity"
          min={1}
          max={item?.maxCount}
          precision={0}
          value={count}
          onChange={setCount}
          style={{ width: '100%', maxWidth: 110 }}
        />
      </Flex>
    </Modal>
  )
}
