import { Flex, Tabs } from 'antd'
import { useInventory } from '@pkhex-everywhere/react'
import { ItemsTable } from '../../components/ItemsTable'
import { PageHeader } from '../../components/PageHeader'

export default function ItemsPage() {
  const { inventory } = useInventory()

  return (
    <Flex vertical gap={20}>
      <PageHeader title="Items" />
      <Tabs items={inventory.map((pouch) => ({ key: pouch.name, label: pouch.name, children: <ItemsTable items={pouch.items} /> }))} />
    </Flex>
  )
}
