import type { ReactNode } from 'react'
import { act } from 'react'
import { createRoot } from 'react-dom/client'
import { afterEach, expect } from 'vitest'

declare global {
  var IS_REACT_ACT_ENVIRONMENT: boolean
}
globalThis.IS_REACT_ACT_ENVIRONMENT = true

const unmounts: (() => void)[] = []

afterEach(() => {
  unmounts.splice(0).forEach((unmount) => unmount())
})

export function render(element: ReactNode) {
  const container = document.createElement('div')
  document.body.appendChild(container)
  const root = createRoot(container)
  act(() => root.render(element))
  unmounts.push(() => {
    act(() => root.unmount())
    container.remove()
  })
  return { container, rerender: (next: ReactNode) => act(() => root.render(next)) }
}

export function failToLoad(image: HTMLImageElement) {
  act(() => {
    image.dispatchEvent(new Event('error'))
  })
}

export function expectPlaceholder(container: HTMLElement, name: string) {
  expect(container.querySelector('img')).toBeNull()
  expect(container.querySelector('[role="img"]')?.getAttribute('aria-label')).toBe(name)
}
