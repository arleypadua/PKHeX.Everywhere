import { useCallback } from 'react'
import { App } from 'antd'
import { EngineError } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'
import { useNavigate } from '../../host'
import { routes } from '../../routes'

async function toBase64(file: File) {
  let binary = ''
  for (const byte of new Uint8Array(await file.arrayBuffer())) binary += String.fromCharCode(byte)
  return btoa(binary)
}

export function useLoadPokemonFile(): (file: File) => Promise<void> {
  const engine = useEngine()
  const navigate = useNavigate()
  const { notification } = App.useApp()

  return useCallback(
    async (file: File) => {
      try {
        const added = await engine.box.addFromFile(await toBase64(file))
        const pokemon = await engine.pokemon.get(added.at)
        notification.success({ title: `${pokemon.species} Created`, description: `${pokemon.species} added to your box` })
        navigate(routes.pokemon(pokemon))
      } catch (error) {
        if (!(error instanceof EngineError)) throw error
        notification.error({ title: error.message })
      }
    },
    [engine, navigate, notification],
  )
}
