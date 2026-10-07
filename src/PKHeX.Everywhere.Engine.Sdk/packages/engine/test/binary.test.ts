import { describe, expect, it } from 'vitest'
import { createEngine } from '../src'
import { fakeHost } from './fakeHost'

const bytes = new Uint8Array([0, 1, 2, 250, 251, 255])
const base64 = 'AAEC+vv/'

function engineReturning(value: unknown) {
  const fake = fakeHost(() => ({ ok: true, value }))
  fake.signalReady()
  return { engine: createEngine({ host: fake.host }), calls: fake.calls }
}

const argsOf = (calls: { args: string }[]) => JSON.parse(calls[0].args) as unknown[]

describe('binary inputs', () => {
  it.each([
    ['Uint8Array', () => bytes],
    ['ArrayBuffer', () => bytes.slice().buffer],
    ['Blob', () => new Blob([bytes])],
    ['File', () => new File([bytes], 'emerald.sav')],
    ['a view into a larger buffer', () => new Uint8Array([9, ...bytes, 9]).subarray(1, bytes.length + 1)],
  ])('sends a %s to the host as base64', async (_, input) => {
    const { engine, calls } = engineReturning(null)

    await engine.box.addFromFile(input())

    expect(argsOf(calls)).toEqual([base64])
  })

  it('converts every binary parameter', async () => {
    const { engine, calls } = engineReturning(true)

    await engine.plugins.isSupported(new Blob([bytes]))
    await engine.plugins.register(bytes.slice().buffer, null)

    expect(calls.map((c) => JSON.parse(c.args))).toEqual([[base64], [base64, null]])
  })

  it('sends large inputs intact', async () => {
    const { engine, calls } = engineReturning(null)
    const large = Uint8Array.from({ length: 200_000 }, (_, i) => i % 256)

    await engine.box.addFromFile(large)

    expect(Uint8Array.from(atob(argsOf(calls)[0] as string), (c) => c.charCodeAt(0))).toEqual(large)
  })
})

describe('game.load', () => {
  it('takes the file name from a File', async () => {
    const { engine, calls } = engineReturning(null)

    await engine.game.load(new File([bytes], 'emerald.sav'))

    expect(argsOf(calls)).toEqual([base64, 'emerald.sav', null])
  })

  it('prefers the file name it is given', async () => {
    const { engine, calls } = engineReturning(null)

    await engine.game.load(new File([bytes], 'emerald.sav'), 'renamed.sav')

    expect(argsOf(calls)).toEqual([base64, 'renamed.sav', null])
  })

  it('leaves the default name to the engine for bytes without a name', async () => {
    const { engine, calls } = engineReturning(null)

    await engine.game.load(bytes)

    expect(argsOf(calls)).toEqual([base64, null, null])
  })

  it('passes the save format to load with', async () => {
    const { engine, calls } = engineReturning(null)

    await engine.game.load(new File([bytes], 'unbound.sav'), undefined, 'unbound')

    expect(argsOf(calls)).toEqual([base64, 'unbound.sav', 'unbound'])
  })
})

describe('binary outputs', () => {
  it('returns exported save bytes as a Uint8Array', async () => {
    const { engine } = engineReturning({ bytes: base64, fileName: 'emerald.sav' })

    const exported = await engine.game.export()

    expect(exported.bytes).toBeInstanceOf(Uint8Array)
    expect(exported).toEqual({ bytes, fileName: 'emerald.sav' })
  })

  it('returns exported Pokémon bytes as a Uint8Array', async () => {
    const { engine } = engineReturning({ bytes: base64, fileName: '025 - Pikachu.pk3' })

    const exported = await engine.pokemon.export({ source: 'party', slot: 0, box: null })

    expect(exported.bytes).toEqual(bytes)
  })

  it('returns both saves of a transfer as Uint8Arrays', async () => {
    const { engine } = engineReturning({
      save: { bytes: base64, fileName: 'firered.sav' },
      partner: { bytes: base64, fileName: 'emerald.sav' },
      arrived: [],
    })

    const done = await engine.transfer.commit({ send: [], receive: [] })

    expect(done).toEqual({ save: { bytes, fileName: 'firered.sav' }, partner: { bytes, fileName: 'emerald.sav' }, arrived: [] })
  })

  it('returns the loaded file as a Uint8Array, or null without a save', async () => {
    expect((await engineReturning({ bytes: base64, fileName: 'a.sav', version: 'E' }).engine.game.file())?.bytes).toEqual(bytes)
    expect(await engineReturning(null).engine.game.file()).toBeNull()
  })
})
