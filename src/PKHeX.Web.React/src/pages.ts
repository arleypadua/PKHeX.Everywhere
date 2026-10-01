import { lazy, type ComponentType, type LazyExoticComponent } from 'react'

export const pages: Record<string, LazyExoticComponent<ComponentType<Record<string, unknown>>>> = {
  box: lazy(() => import('./pages/box/BoxPage')),
  credits: lazy(() => import('./pages/static/CreditsPage')),
  encounters: lazy(() => import('./pages/encounters/EncountersPage')),
  items: lazy(() => import('./pages/items/ItemsPage')),
  party: lazy(() => import('./pages/party/PartyPage')),
  'privacy-policy': lazy(() => import('./pages/static/PrivacyPolicyPage')),
  'terms-of-use': lazy(() => import('./pages/static/TermsOfUsePage')),
}
