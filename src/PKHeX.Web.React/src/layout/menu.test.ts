import { describe, expect, it } from 'vitest'
import type { InstalledPlugIn, SaveCapability, SaveSummary } from '@pkhex-everywhere/engine'
import { menuEntries } from './menu'

const allCapabilities: SaveCapability[] = ['legality', 'autoLegality', 'encounters', 'showdown', 'events', 'plugIns']

const save = (hasEvents: boolean, capabilities = allCapabilities): SaveSummary => ({
  fileName: 'emerald.sav',
  version: 'Emerald',
  generation: 3,
  hasEvents,
  format: null,
  capabilities,
})

const plugIn = (id: string, needsReinstall = false): InstalledPlugIn => ({
  id,
  name: `${id} plug-in`,
  version: '1.0.0',
  enabled: true,
  hasNewerVersion: false,
  needsReinstall,
})

const labels = (entries: ReturnType<typeof menuEntries>) => entries.map((entry) => entry.label)

describe('menuEntries', () => {
  it('shows only Home, Plug-Ins, Analytics and Save without a save', () => {
    expect(labels(menuEntries(null, []))).toEqual(['Home', 'Plug-Ins', 'Analytics', 'Save'])
  })

  it('shows Party, Pokemon Box and Items with a save', () => {
    expect(labels(menuEntries(save(false), []))).toEqual(['Home', 'Party', 'Pokemon Box', 'Items', 'Plug-Ins', 'Analytics', 'Save'])
  })

  it('shows Events when the save has events', () => {
    expect(labels(menuEntries(save(true), []))).toEqual([
      'Home',
      'Party',
      'Pokemon Box',
      'Items',
      'Events',
      'Plug-Ins',
      'Analytics',
      'Save',
    ])
  })

  it('hides Events when the save does not support them', () => {
    expect(labels(menuEntries(save(true, []), []))).not.toContain('Events')
  })

  it('links Plug-Ins straight to the plug-ins page when the save does not support plug-ins', () => {
    expect(menuEntries(save(false, []), [plugIn('A')]).find((entry) => entry.label === 'Plug-Ins')).toEqual({
      route: '/plugins',
      label: 'Plug-Ins',
      icon: 'api',
    })
  })

  it('links Plug-Ins straight to the plug-ins page when none is installed', () => {
    expect(menuEntries(null, []).find((entry) => entry.label === 'Plug-Ins')).toEqual({
      route: '/plugins',
      label: 'Plug-Ins',
      icon: 'api',
    })
  })

  it('lists each installed plug-in and Manage Plug-Ins under Plug-Ins', () => {
    const plugIns = menuEntries(null, [plugIn('A'), plugIn('B', true), plugIn('C')]).find((entry) => entry.label === 'Plug-Ins')

    expect(plugIns?.children).toEqual([
      { route: '/plugins/A', label: 'A plug-in' },
      { route: '/plugins/C', label: 'C plug-in' },
      { route: '/plugins', label: 'Manage Plug-Ins' },
    ])
  })
})
