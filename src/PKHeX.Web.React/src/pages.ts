import { lazy, type ComponentType, type LazyExoticComponent } from 'react'

export const pages: Record<string, LazyExoticComponent<ComponentType<Record<string, unknown>>>> = {
  encounters: lazy(() => import('./pages/encounters/EncountersPage')),
  items: lazy(() => import('./pages/items/ItemsPage')),
  party: lazy(() => import('./pages/party/PartyPage')),
}
