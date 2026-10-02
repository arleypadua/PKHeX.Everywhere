# @pkhex-everywhere/engine

Runs PKHeX in the browser. Load a Pokémon save file, read and edit it, and export it. The package contains the .NET runtime with PKHeX.Core.

**[Documentation](https://arley-space--docs.wawesome.app/docs/)** · [Getting started](https://arley-space--docs.wawesome.app/docs/getting-started/) · [API reference](https://arley-space--docs.wawesome.app/docs/reference/engine/)

## Install

```sh
npm install @pkhex-everywhere/engine
```

## Example

```ts
import { createEngine } from '@pkhex-everywhere/engine'

const engine = createEngine()
const input = document.querySelector<HTMLInputElement>('input[type=file]')!

input.onchange = async () => {
  await engine.game.load(input.files![0])
  const party = await engine.party.get()
  console.log(party.map((pokemon) => `${pokemon.species} Lv. ${pokemon.level}`))
}
```

For React, use [`@pkhex-everywhere/react`](https://www.npmjs.com/package/@pkhex-everywhere/react).

## License

GPL-3.0-or-later, because the runtime contains PKHeX.Core.
