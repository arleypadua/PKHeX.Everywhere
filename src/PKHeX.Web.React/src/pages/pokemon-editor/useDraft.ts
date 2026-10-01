import { App } from 'antd'
import { draftHandle, EngineError, type PokemonPatch } from '@pkhex-everywhere/engine'
import { usePokemonDetails } from '@pkhex-everywhere/react'

export function useDraft() {
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

  return { details, submit }
}
