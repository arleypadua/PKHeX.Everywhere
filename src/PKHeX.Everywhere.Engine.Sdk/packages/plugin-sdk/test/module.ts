import type { Mount } from '@pkhex-everywhere/plugin-sdk'

export const mount: Mount = (element, ctx) => {
  element.textContent = `${ctx.plugInId} (${ctx.theme})`
  void ctx.getSave().then((save) => save && ctx.loadSave(save.bytes, save.fileName))
  void ctx.getSetting('ROM').then((rom) => rom instanceof Uint8Array && ctx.navigate('/'))
  return () => element.replaceChildren()
}
