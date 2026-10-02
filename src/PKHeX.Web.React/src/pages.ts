import { lazy, type ComponentType, type LazyExoticComponent } from 'react'

export const pages: Record<string, LazyExoticComponent<ComponentType<Record<string, unknown>>>> = {
  analytics: lazy(() => import('./pages/analytics/AnalyticsPage')),
  box: lazy(() => import('./pages/box/BoxPage')),
  credits: lazy(() => import('./pages/static/CreditsPage')),
  encounters: lazy(() => import('./pages/encounters/EncountersPage')),
  events: lazy(() => import('./pages/events/EventsPage')),
  home: lazy(() => import('./pages/home/HomePage')),
  items: lazy(() => import('./pages/items/ItemsPage')),
  load: lazy(() => import('./pages/load/LoadPage')),
  party: lazy(() => import('./pages/party/PartyPage')),
  'pokemon-clone': lazy(() => import('./pages/pokemon-editor/PokemonClonePage')),
  'pokemon-editor': lazy(() => import('./pages/pokemon-editor/PokemonEditorPage')),
  'privacy-policy': lazy(() => import('./pages/static/PrivacyPolicyPage')),
  'release-notes': lazy(() => import('./pages/release-notes/ReleaseNotesPage')),
  settings: lazy(() => import('./pages/settings/SettingsPage')),
  'terms-of-use': lazy(() => import('./pages/static/TermsOfUsePage')),
}
