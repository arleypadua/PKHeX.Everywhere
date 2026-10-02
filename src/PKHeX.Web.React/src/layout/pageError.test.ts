import { afterEach, describe, expect, it, vi } from 'vitest'
import { EngineError, type SaveVersion } from '@pkhex-everywhere/engine'
import { issueLink, reportPageError, toPageError } from './pageError'
import { startGoogleAnalytics } from '../googleAnalytics'
import { captureError } from '../sentry'

vi.mock('../sentry', async (original) => ({ ...(await original<typeof import('../sentry')>()), captureError: vi.fn() }))

const emerald: SaveVersion = { version: 'Emerald', versionId: 3, generation: 'Gen3', generationId: 3, formatId: null }

function boom() {
  const error = new TypeError('Cannot read properties of undefined')
  error.stack = 'TypeError: Cannot read properties of undefined\n    at Party (party.tsx:1:1)'
  return error
}

describe('page errors', () => {
  afterEach(() => {
    globalThis.gtag = undefined
    vi.mocked(captureError).mockClear()
  })

  it('gives every error a tracking id', () => {
    const first = toPageError(boom())
    const second = toPageError(boom())

    expect(first.id).toMatch(/^[0-9a-f-]{36}$/)
    expect(second.id).not.toBe(first.id)
    expect(first).toMatchObject({ type: 'TypeError', message: 'Cannot read properties of undefined' })
  })

  it('describes values that are not errors', () => {
    expect(toPageError('Boom')).toMatchObject({ type: 'string', message: 'Boom', stack: '' })
  })

  it('links to a new bug issue with the id, game and error', () => {
    const error = toPageError(boom(), 'b7a9f2c4')

    const url = new URL(issueLink(error, emerald)!)

    expect(url.origin + url.pathname).toBe('https://github.com/arleypadua/PKHeX.Everywhere/issues/new')
    expect(url.searchParams.get('title')).toBe('Error on PKHeX.Web: Cannot read properties of undefined')
    expect(url.searchParams.get('labels')).toBe('bug')
    expect(url.searchParams.get('body')).toBe(
      [
        '# Error on PKHeX.Web',
        '',
        '* **Id**: b7a9f2c4',
        '* **Game version**: Emerald',
        '* **Generation**: Gen3',
        '## Type',
        '```',
        'TypeError',
        '```',
        '',
        '## Message',
        '```',
        'Cannot read properties of undefined',
        '```',
        '',
        '## Stack trace',
        '```',
        'TypeError: Cannot read properties of undefined\n    at Party (party.tsx:1:1)',
        '```',
      ].join('\n'),
    )
  })

  it('leaves the game blank in the link when none is loaded', () => {
    const body = new URL(issueLink(toPageError(boom(), 'id'), null)!).searchParams.get('body')

    expect(body).toContain('* **Game version**: \n* **Generation**: \n')
  })

  it('has no link for a Pokémon format the save does not support', () => {
    const error = toPageError(new EngineError('unexpected', 'PKM Format needs to be PK3 when setting to this Save File.'))

    expect(error.unsupportedFormat).toBe(true)
    expect(issueLink(error, emerald)).toBeNull()
  })

  it('sends unexpected_error to Google Analytics', () => {
    const gtag = vi.fn()
    globalThis.gtag = gtag
    startGoogleAnalytics({ onEvent: () => () => {} }, true)
    gtag.mockClear()
    document.head.innerHTML = '<base href="/" />'
    history.replaceState(null, '', '/party')
    const error = boom()

    reportPageError(error, toPageError(error, 'b7a9f2c4'), emerald)

    expect(gtag).toHaveBeenCalledWith('event', 'unexpected_error', {
      current_route: 'party',
      exception_message: 'Cannot read properties of undefined',
      exception_stack_trace: error.stack,
      exception_type: 'TypeError',
      exception_id: 'b7a9f2c4',
      version_name: 'Emerald',
      version_id: 3,
      generation_name: 'Gen3',
      generation_id: 3,
      format_id: null,
    })
  })

  it('captures the error in Sentry with its tracking id', () => {
    const error = boom()

    reportPageError(error, toPageError(error, 'b7a9f2c4'), null)

    expect(captureError).toHaveBeenCalledWith(error, { exception_id: 'b7a9f2c4' })
  })

  it('leaves Engine errors to the Engine watcher in Sentry', () => {
    const error = new EngineError('no-save', 'No save is loaded.')

    reportPageError(error, toPageError(error), null)

    expect(captureError).not.toHaveBeenCalled()
  })
})
