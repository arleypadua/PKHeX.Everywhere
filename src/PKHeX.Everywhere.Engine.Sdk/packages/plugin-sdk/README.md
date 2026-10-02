# @pkhex-everywhere/plugin-sdk

Types for the page modules of [PKHeX.Everywhere](https://pkhex-web.github.io) plug-ins.

**[Writing a plugin](https://arley-space--docs.wawesome.app/docs/guides/plugins/)** · [API reference](https://arley-space--docs.wawesome.app/docs/reference/plugin-sdk/)

## Install

```sh
npm install --save-dev @pkhex-everywhere/plugin-sdk
```

## Example

A page module exports a `mount` function. The app calls it with the element to render into and a `PageContext`.

```ts
import type { Mount } from '@pkhex-everywhere/plugin-sdk'

export const mount: Mount = (element, ctx) => {
  element.textContent = `Hello from ${ctx.plugInId}`
  return () => element.replaceChildren()
}
```

## License

GPL-3.0-or-later.
