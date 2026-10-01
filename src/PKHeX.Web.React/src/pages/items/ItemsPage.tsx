import { useState } from 'react'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Flex, Tabs } from 'antd'
import type { OwnedItem } from '@pkhex-everywhere/engine'
import { useInventory } from '@pkhex-everywhere/react'
import { ItemsTable } from '../../components/ItemsTable'
import { PageHeader } from '../../components/PageHeader'
import { AddItemModal } from './AddItemModal'
import { EditItemModal } from './EditItemModal'

export default function ItemsPage() {
  const { inventory } = useInventory()
  const [activeKey, setActiveKey] = useState<string>()
  const [editing, setEditing] = useState<OwnedItem>()
  const [adding, setAdding] = useState(false)

  const active = inventory.find((pouch) => pouch.name === activeKey) ?? inventory[0]

  return (
    <Flex vertical gap={20}>
      <PageHeader
        title="Items"
        extra={
          <Button type="primary" icon={<PlusOutlined />} disabled={!active} onClick={() => setAdding(true)}>
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
          children: <ItemsTable items={pouch.items} onEdit={setEditing} />,
        }))}
      />
      {active && editing && <EditItemModal pouch={active.name} item={editing} onClose={() => setEditing(undefined)} />}
      {active && adding && <AddItemModal pouch={active} onClose={() => setAdding(false)} />}
    </Flex>
  )
}
