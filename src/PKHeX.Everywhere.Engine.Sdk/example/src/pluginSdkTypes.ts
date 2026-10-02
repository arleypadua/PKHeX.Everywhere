import type { Mount } from '@pkhex-everywhere/plugin-sdk'

export const mount: Mount = (element, ctx) => {
  element.textContent = ctx.plugInId
  return () => element.replaceChildren()
}
