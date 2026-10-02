import type { DeclaredPage, PlugInNotificationType, PlugInRan } from '@pkhex-everywhere/engine'
import { routes } from '../routes'

export type Outcome =
  | { kind: 'notify'; type: PlugInNotificationType; message: string; description: string | null }
  | { kind: 'navigate'; url: string }

export function outcomeOf(ran: PlugInRan, pages: DeclaredPage[]): Outcome | null {
  if (ran.failure)
    return { kind: 'notify', type: 'error', message: 'Plugin failed to execute', description: `Visit ${routes.plugInErrors} for details.` }

  const outcome = ran.outcome
  if (outcome?.kind === 'notify')
    return { kind: 'notify', type: outcome.type ?? 'none', message: outcome.message ?? '', description: outcome.description ?? null }

  if (outcome?.kind === 'openPage') {
    const page = pages.find((p) => p.plugInId === ran.plugInId && p.path === outcome.path)
    return page ? { kind: 'navigate', url: routes.plugInPage(page) } : null
  }

  return null
}
