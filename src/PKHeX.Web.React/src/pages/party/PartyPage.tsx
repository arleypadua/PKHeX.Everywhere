import { CopyOutlined } from '@ant-design/icons'
import { Button, Flex } from 'antd'
import { useEngine, useParty } from '@pkhex-everywhere/react'
import { PageHeader } from '../../components/PageHeader'
import { PokemonTable } from '../../components/PokemonTable'
import { useCopyShowdown } from '../../hooks/useCopyShowdown'
import { openCalculator } from '../../host'

export default function PartyPage() {
  const { party } = useParty()
  const engine = useEngine()
  const copyShowdown = useCopyShowdown()

  return (
    <Flex vertical gap={20}>
      <PageHeader
        title="Party"
        extra={
          <>
            <Button type="primary" onClick={async () => openCalculator(await engine.party.showdown())}>
              Calculator
            </Button>
            <Button
              type="link"
              icon={<CopyOutlined />}
              onClick={async () => copyShowdown(await engine.party.showdown(), `${party.length} entries`)}
            >
              Showdown
            </Button>
          </>
        }
      />
      <PokemonTable pokemons={party} />
    </Flex>
  )
}
