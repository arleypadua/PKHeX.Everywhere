import { useCallback } from 'react'
import { App } from 'antd'
import { EngineError } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'
import { useNavigate } from '../../host'
import { routes } from '../../routes'

export function useLoadPokemonFile(): (file: File) => Promise<void> {
  const engine = useEngine()
  const navigate = useNavigate()
  const { notification } = App.useApp()

  return useCallback(
    async (file: File) => {
      try {
        const added = await engine.box.addFromFile(file)
        notification.success({ title: 'Pokémon added to your box' })
        navigate(routes.pokemon(added))
      } catch (error) {
        if (!(error instanceof EngineError)) throw error
        notification.error({ title: error.message })
      }
    },
    [engine, navigate, notification],
  )
}
