import { Button, Flex, Table, Typography } from 'antd'
import { useQuery } from '@pkhex-everywhere/react'
import { useNavigate } from '../host'

export default function PartyNext() {
  const party = useQuery('party.get')
  const navigate = useNavigate()

  return (
    <Flex vertical gap="middle">
      <Flex justify="space-between" align="center">
        <Typography.Title level={3} style={{ margin: 0 }}>
          Party (preview)
        </Typography.Title>
        <Button onClick={() => navigate('/party')}>Open Blazor party</Button>
      </Flex>
      <Table
        rowKey="id"
        dataSource={party}
        pagination={false}
        columns={[
          { title: 'Species', dataIndex: 'species' },
          { title: 'Level', dataIndex: 'level' },
        ]}
      />
    </Flex>
  )
}
