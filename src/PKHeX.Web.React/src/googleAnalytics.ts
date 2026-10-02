import type { Engine, EngineEvent, GameOverview, PokemonOverview } from '@pkhex-everywhere/engine'

declare global {
  var gtag: ((...args: unknown[]) => void) | undefined
}

export const measurementId = 'G-BV586KEZM9'

type Params = Record<string, string | number | null>

let send: ((name: string, params: Params) => void) | undefined

// index.html defines gtag with the consent defaults, so loading gtag.js here keeps them.
export function startGoogleAnalytics(
  engine: Pick<Engine, 'onEvent'>,
  enabled = import.meta.env.VITE_GOOGLE_ANALYTICS === 'true',
) {
  const gtag = globalThis.gtag
  if (!enabled || !gtag) return

  const script = document.createElement('script')
  script.async = true
  script.src = `https://www.googletagmanager.com/gtag/js?id=${measurementId}`
  document.head.appendChild(script)

  gtag('js', new Date())
  gtag('config', measurementId)
  send = (name, params) => gtag('event', name, params)

  engine.onEvent((event) => {
    const tracked = toAnalyticsEvent(event)
    if (tracked) send?.(...tracked)
  })
}

export function track(name: string, params: Params) {
  send?.(name, params)
}

function toAnalyticsEvent(event: EngineEvent): [string, Params] | null {
  switch (event.type) {
    case 'itemChanged':
      return ['item_modified', { item_id: event.itemId, quantity: event.count }]
    case 'gameExported':
      return ['game_exported', gameParams(event.game)]
    case 'pokemonAdded':
      return [event.source === 'file' ? 'pokemon_loaded_from_file' : 'pokemon_loaded_from_encounter', pokemonParams(event.pokemon)]
    case 'pokemonSaved':
      return ['pokemon_saved', pokemonParams(event.pokemon, event.at.source === 'party' ? 'Party' : 'Box')]
    case 'plugInRan':
      return [
        'plugin_hook_executed',
        {
          hook_name: event.hookId.split(/[.+]/).at(-1) ?? event.hookId,
          exception_type: event.failure?.type ?? null,
          exception_message: event.failure?.message ?? null,
        },
      ]
    case 'plugInInstalled':
      return ['plug_in_installed', { id: event.plugInId, version: event.version }]
    case 'plugInUpdated':
      return ['plug_in_updated', { id: event.plugInId, version: event.version }]
    default:
      return null
  }
}

function gameParams(game: GameOverview): Params {
  const params: Params = {
    version_name: game.version,
    version_id: game.versionId,
    generation_name: game.generation,
    generation_id: game.generationId,
    gender: game.trainerGender,
    box_size: game.boxCount,
  }
  for (let i = 0; i < 6; i++) {
    const member = game.party[i]
    const n = String(i + 1).padStart(2, '0')
    params[`party_species_id_${n}`] = member?.speciesId ?? null
    params[`party_species_name_${n}`] = member?.species ?? null
    params[`party_level_${n}`] = member?.level ?? null
  }
  return params
}

function pokemonParams(pokemon: PokemonOverview, source: string | null = null): Params {
  return {
    species_id: pokemon.speciesId,
    species_name: pokemon.species,
    gender: pokemon.gender,
    ball_name: pokemon.ball,
    level: pokemon.level,
    source,
  }
}
