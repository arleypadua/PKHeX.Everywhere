import type { DeclaredPage, PokemonSummary } from '@pkhex-everywhere/engine'

export const routes = {
  home: '/',
  load: '/load',
  party: '/party',
  items: '/items',
  events: '/events',
  analytics: '/analytics',
  save: '/save',
  settings: '/settings',
  credits: '/credits',
  privacyPolicy: '/privacy-policy',
  termsOfUse: '/terms-of-use',
  plugInErrors: '/plugins/errors',
  releaseNotes: (since?: string | null) => (since === undefined ? '/release-notes' : `/release-notes?since=${since ?? ''}`),
  pokemon: ({ at, id }: Pick<PokemonSummary, 'at' | 'id'>) => `/pokemon/${at.source}/${id}`,
  box: '/pokemon-box',
  plugIns: '/plugins',
  plugIn: (id: string) => `/plugins/${id}`,
  plugInPage: ({ plugInId, path, layout }: Pick<DeclaredPage, 'plugInId' | 'path' | 'layout'>) =>
    `/plugins/${plugInId}/${path}/${layout}`,
  searchEncounter: '/pokemon/search-encounter',
  clonePokemon: ({ at, id }: Pick<PokemonSummary, 'at' | 'id'>) => `/pokemon/${at.source}/${id}/clone`,
  encounters: ({ version, species }: { version: number; species?: number }) =>
    `/pokemon/search-encounter?${new URLSearchParams({ version: String(version), ...(species && { species: String(species) }) })}`,
}
