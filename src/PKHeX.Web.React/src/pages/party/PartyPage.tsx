import { Flex } from 'antd'
import { useEngine, useParty } from '@pkhex-everywhere/react'
import { PageHeader } from '../../components/PageHeader'
import { PokemonTable } from '../../components/PokemonTable'
import { ShowdownActions } from '../../components/ShowdownActions'

export default function PartyPage() {
  const { party } = useParty()
  const engine = useEngine()

  return (
    <Flex vertical gap={20}>
      <PageHeader
        title="Party"
        extra={<ShowdownActions showdown={() => engine.party.showdown()} description={`${party.length} entries`} />}
      />
      <PokemonTable pokemon={party} />
    </Flex>
  )
}
