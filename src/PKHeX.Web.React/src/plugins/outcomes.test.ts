import { describe, expect, it } from 'vitest'
import type { DeclaredPage, PlugInRan } from '@pkhex-everywhere/engine'
import { outcomeOf } from './outcomes'

const ran = (patch: Partial<PlugInRan>): PlugInRan => ({
  type: 'plugInRan',
  plugInId: 'LiveRun',
  hookId: 'LiveRun.GoToLiveRun',
  outcome: null,
  failure: null,
  ...patch,
})

const pages: DeclaredPage[] = [
  { plugInId: 'LiveRun', path: 'live-run', title: 'Live Run', layout: 'empty' },
  { plugInId: 'LiveRun', path: 'stats', title: null, layout: 'standard' },
]

describe('outcomeOf', () => {
  it('reports a failed run', () => {
    expect(outcomeOf(ran({ failure: { type: 'InvalidOperationException', message: 'Boom' } }), pages)).toEqual({
      kind: 'notify',
      type: 'error',
      message: 'Plugin failed to execute',
      description: 'Visit /plugins/errors for details.',
    })
  })

  it("shows the plug-in's notification", () => {
    const outcome = ran({ outcome: { kind: 'notify', message: 'Done', description: 'All set', type: 'success' } })

    expect(outcomeOf(outcome, pages)).toEqual({ kind: 'notify', type: 'success', message: 'Done', description: 'All set' })
  })

  it('shows a notification without a type as a plain one', () => {
    expect(outcomeOf(ran({ outcome: { kind: 'notify', message: 'Done' } }), pages)).toEqual({
      kind: 'notify',
      type: 'none',
      message: 'Done',
      description: null,
    })
  })

  it('opens the requested page with its layout', () => {
    expect(outcomeOf(ran({ outcome: { kind: 'openPage', path: 'live-run' } }), pages)).toEqual({
      kind: 'navigate',
      url: '/plugins/LiveRun/live-run/empty',
    })
    expect(outcomeOf(ran({ outcome: { kind: 'openPage', path: 'stats' } }), pages)).toEqual({
      kind: 'navigate',
      url: '/plugins/LiveRun/stats/standard',
    })
  })

  it('ignores a page the plug-in did not declare', () => {
    expect(outcomeOf(ran({ outcome: { kind: 'openPage', path: 'missing' } }), pages)).toBeNull()
  })

  it('ignores a run without an outcome', () => {
    expect(outcomeOf(ran({ outcome: { kind: 'void' } }), pages)).toBeNull()
    expect(outcomeOf(ran({}), pages)).toBeNull()
  })
})
