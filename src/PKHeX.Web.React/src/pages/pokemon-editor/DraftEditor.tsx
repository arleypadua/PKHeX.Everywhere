import { App, Button, Descriptions, Flex, InputNumber, Typography } from 'antd'
import { draftHandle, EngineError, type PokemonPatch } from '@pkhex-everywhere/engine'
import { usePokemon, usePokemonDetails } from '@pkhex-everywhere/react'
import { PageHeader } from '../../components/PageHeader'
import { PokemonSprite } from '../../components/PokemonSprite'
import { LegalityBanner } from './LegalityBanner'

interface DraftEditorProps {
  onSave: () => void
}

export function DraftEditor({ onSave }: DraftEditorProps) {
  const { pokemon } = usePokemon(draftHandle)
  const { details, update } = usePokemonDetails(draftHandle)
  const { notification } = App.useApp()

  const submit = async (patch: PokemonPatch) => {
    try {
      await update(patch)
    } catch (error) {
      if (!(error instanceof EngineError) || error.code !== 'invalid-patch') throw error
      notification.error({ title: "Couldn't change the Pokémon", description: error.message })
    }
  }

  return (
    <Flex vertical gap={20}>
      <PageHeader title="Pokemon" extra={<PokemonSprite pokemon={pokemon} />} />
      <LegalityBanner legality={details.legality} />
      <Descriptions
        bordered
        column={1}
        items={[
          {
            key: 'nickname',
            label: 'Nickname',
            children: (
              <Typography.Text editable={{ text: details.nickname, onChange: (nickname) => submit({ nickname }) }}>
                {details.nickname}
              </Typography.Text>
            ),
          },
          {
            key: 'level',
            label: 'Level',
            children: (
              <InputNumber
                min={1}
                max={100}
                value={details.level}
                onChange={(level) => level !== null && submit({ level })}
              />
            ),
          },
        ]}
      />
      <Flex justify="end">
        <Button type="primary" onClick={onSave}>
          Save
        </Button>
      </Flex>
    </Flex>
  )
}
