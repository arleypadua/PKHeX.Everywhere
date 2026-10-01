import { useState } from 'react'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Flex, Tabs } from 'antd'
import type { ItemHandle } from '@pkhex-everywhere/engine'
import { useInventory } from '@pkhex-everywhere/react'
import { ItemsTable } from '../../components/ItemsTable'
import { PageHeader } from '../../components/PageHeader'
import { AddItemModal } from './AddItemModal'
import { EditItemModal } from './EditItemModal'

export default function ItemsPage() {
  const { inventory } = useInventory()
  const [activeKey, setActiveKey] = useState<string>()
  const [editing, setEditing] = useState<ItemHandle>()
  const [addingTo, setAddingTo] = useState<string>()

  const active = inventory.find((pouch) => pouch.name === activeKey) ?? inventory[0]
  const findPouch = (name?: string) => inventory.find((pouch) => pouch.name === name)
  const editingItem = findPouch(editing?.pouch)?.items.find((item) => item.id === editing?.itemId)
  const addingPouch = findPouch(addingTo)

  return (
    <Flex vertical gap={20}>
      <PageHeader
        title="Items"
        extra={
          <Button type="primary" icon={<PlusOutlined />} disabled={!active} onClick={() => setAddingTo(active?.name)}>
            Add
          </Button>
        }
      />
      <Tabs
        activeKey={active?.name}
        onChange={setActiveKey}
        items={inventory.map((pouch) => ({
          key: pouch.name,
          label: pouch.name,
          children: <ItemsTable items={pouch.items} onEdit={(item) => setEditing({ pouch: pouch.name, itemId: item.id })} />,
        }))}
      />
      {editing && editingItem && (
        <EditItemModal pouchName={editing.pouch} item={editingItem} onClose={() => setEditing(undefined)} />
      )}
      {addingPouch && <AddItemModal pouch={addingPouch} onClose={() => setAddingTo(undefined)} />}
    </Flex>
  )
}
