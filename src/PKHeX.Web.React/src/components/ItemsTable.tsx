import { Table, type TableColumnsType } from 'antd'
import type { OwnedItem } from '@pkhex-everywhere/engine'
import { ItemIcon } from './ItemIcon'
import { NumberFilter } from './filters/NumberFilter'
import { containsText } from './filters/containsText'
import { TextFilter } from './filters/TextFilter'

const columns: TableColumnsType<OwnedItem> = [
  {
    key: 'icon',
    width: 60,
    render: (_, item) => <ItemIcon name={item.name} />,
  },
  {
    title: 'Name',
    dataIndex: 'name',
    sorter: (a, b) => a.name.localeCompare(b.name),
    filterDropdown: (props) => <TextFilter {...props} placeholder="Name" />,
    onFilter: (value, item) => containsText(item.name, value),
  },
  {
    title: 'Amount',
    dataIndex: 'count',
    sorter: (a, b) => a.count - b.count,
    filterDropdown: (props) => <NumberFilter {...props} placeholder="Amount" min={0} />,
    onFilter: (value, item) => item.count === value,
  },
]

interface ItemsTableProps {
  items: OwnedItem[]
}

export function ItemsTable({ items }: ItemsTableProps) {
  return <Table rowKey="id" dataSource={items} columns={columns} size="small" scroll={{ x: 'max-content' }} pagination={{ hideOnSinglePage: true }} />
}
