import { describe, expect, it } from 'vitest'
import { failToLoad, render } from '../testing/render'
import { SpeciesOption } from './SpeciesSelect'

describe('SpeciesOption', () => {
  it('shows the placeholder when the sprite fails to load', () => {
    const { container } = render(<SpeciesOption id={62146} name="Missing" />)
    failToLoad(container.querySelector('img')!)
    expect(container.querySelector('img')).toBeNull()
    expect(container.querySelector('[role="img"]')?.getAttribute('aria-label')).toBe('Missing')
    expect(container.textContent).toContain('Missing')
  })
})
