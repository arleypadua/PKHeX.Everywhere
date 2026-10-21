import { afterEach, describe, expect, it } from 'vitest'
import { render } from '../testing/render'
import { AdSlot } from './AdSlot'

describe('AdSlot', () => {
  const original = globalThis.adsbygoogle

  afterEach(() => {
    globalThis.adsbygoogle = original
  })

  it('keeps the slot when AdSense has no ad that fits', () => {
    globalThis.adsbygoogle = Object.assign([], {
      push: () => {
        throw new Error('No slot size for availableWidth=216')
      },
    })

    const { container } = render(<AdSlot slot="1" />)

    expect(container.querySelector('ins.adsbygoogle')).not.toBeNull()
  })
})
