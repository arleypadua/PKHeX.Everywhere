import type { InstalledPlugIn, SaveSummary } from '@pkhex-everywhere/engine'
import { supports } from '../capabilities'
import { routes } from '../routes'

export type MenuIcon = 'home' | 'team' | 'inbox' | 'shop' | 'flag' | 'api' | 'line-chart' | 'save'

export interface MenuEntry {
  route: string
  label: string
  icon?: MenuIcon
  children?: MenuEntry[]
}

export function menuEntries(game: SaveSummary | null, installed: InstalledPlugIn[]): MenuEntry[] {
  const plugIns: MenuEntry[] = installed
    .filter((plugIn) => !plugIn.needsReinstall && supports(game, 'plugIns'))
    .map((plugIn) => ({ route: routes.plugIn(plugIn.id), label: plugIn.name }))
  if (plugIns.length) plugIns.push({ route: routes.plugIns, label: 'Manage Plug-Ins' })

  return [
    { route: routes.home, label: 'Home', icon: 'home' },
    ...(game
      ? [
          { route: routes.party, label: 'Party', icon: 'team' as const },
          { route: routes.box, label: 'Pokemon Box', icon: 'inbox' as const },
          { route: routes.items, label: 'Items', icon: 'shop' as const },
          ...(game.hasEvents && supports(game, 'events') ? [{ route: routes.events, label: 'Events', icon: 'flag' as const }] : []),
        ]
      : []),
    { route: routes.plugIns, label: 'Plug-Ins', icon: 'api', ...(plugIns.length && { children: plugIns }) },
    { route: routes.analytics, label: 'Analytics', icon: 'line-chart' },
    { route: routes.save, label: 'Save', icon: 'save' },
  ]
}
