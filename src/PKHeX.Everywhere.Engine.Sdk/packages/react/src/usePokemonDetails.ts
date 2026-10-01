import { useMemo } from 'react'
import type { PokemonHandle, PokemonPatch } from '@pkhex-everywhere/engine'
import { useEngine } from './EngineProvider'
import { useQuery } from './useQuery'

export function usePokemonDetails(at: PokemonHandle) {
  const engine = useEngine()
  const details = useQuery('pokemon.details', at)
  const key = JSON.stringify(at)
  const update = useMemo(() => (patch: PokemonPatch) => engine.pokemon.update(at, patch), [engine, key])
  return { details, update }
}
