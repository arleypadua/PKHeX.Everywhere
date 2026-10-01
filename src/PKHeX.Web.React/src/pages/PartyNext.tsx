import { Suspense, useState } from 'react'
import { Button, Flex, InputNumber, Space, Table, Typography } from 'antd'
import type { PokemonHandle } from '@pkhex-everywhere/engine'
import { useParty, usePokemon } from '@pkhex-everywhere/react'
import { useNavigate } from '../host'

export default function PartyNext() {
  const { party } = useParty()
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
          ...(import.meta.env.DEV
            ? [
                {
                  title: 'Set level (dev)',
                  key: 'set-level',
                  render: (_: unknown, { at }: { at: PokemonHandle }) => (
                    <Suspense fallback={null}>
                      <DevSetLevel at={at} />
                    </Suspense>
                  ),
                },
              ]
            : []),
        ]}
      />
    </Flex>
  )
}

// Temporary control that proves commands update the page. Remove it once the Party page ships.
function DevSetLevel({ at }: { at: PokemonHandle }) {
  const { pokemon, setLevel } = usePokemon(at)
  const [level, setLevelInput] = useState<number | null>(pokemon.level)

  return (
    <Space.Compact>
      <InputNumber min={1} max={100} value={level} onChange={setLevelInput} />
      <Button onClick={() => level !== null && setLevel(level)}>Set</Button>
    </Space.Compact>
  )
}
