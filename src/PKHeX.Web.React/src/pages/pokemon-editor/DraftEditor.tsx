import type { ReactNode } from 'react'
import { Flex, Tabs } from 'antd'
import { draftHandle } from '@pkhex-everywhere/engine'
import { usePokemon, usePokemonDetails } from '@pkhex-everywhere/react'
import { PageHeader } from '../../components/PageHeader'
import { PokemonImage } from '../../components/PokemonImage'
import { DescriptionTab } from './DescriptionTab'
import { LegalityBanner } from './LegalityBanner'
import { MovesTab } from './MovesTab'
import { TrainerTab } from './TrainerTab'
import { MetConditionsTab } from './MetConditionsTab'
import { StatsTab } from './StatsTab'

interface DraftEditorProps {
  actions: ReactNode
}

export function DraftEditor({ actions }: DraftEditorProps) {
  const { pokemon } = usePokemon(draftHandle)
  const { details } = usePokemonDetails(draftHandle)

  return (
    <Flex vertical gap={20}>
      <PageHeader title="Pokemon" extra={<PokemonImage pokemon={pokemon} />} />
      {details.legality && <LegalityBanner legality={details.legality} />}
      <Tabs
        items={[
          { key: 'description', label: 'Description', children: <DescriptionTab /> },
          { key: 'metConditions', label: 'Met Conditions', children: <MetConditionsTab /> },
          { key: 'stats', label: 'Stats', children: <StatsTab /> },
          { key: 'moves', label: 'Moves', children: <MovesTab /> },
          { key: 'trainer', label: 'Trainer', children: <TrainerTab /> },
        ]}
      />
      <Flex justify="end">{actions}</Flex>
    </Flex>
  )
}
