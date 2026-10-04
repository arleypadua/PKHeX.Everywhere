import { createEngine } from '@pkhex-everywhere/engine'
import { readEnabledFormats } from './enabledFormats'
import { createPlugIns } from './plugins/plugIns'
import { createPlugInStore } from './plugins/store'

export const engine = createEngine()

export const plugIns = createPlugIns(engine, createPlugInStore())

const enabledFormats = readEnabledFormats(location.search, () => localStorage)

const formatsEnabled = engine.ready.then(() =>
  Promise.all(
    enabledFormats.map((id) =>
      engine.game.enableFormat(id).catch((error) => console.error(`Couldn't enable the save format '${id}'.`, error)),
    ),
  ),
)

// Every route waits for plug-ins, so waiting for the formats here enables them before the first load.
export const plugInsLoaded = formatsEnabled.then(() =>
  plugIns.registerStored().catch((error) => console.error("Couldn't load plug-ins.", error)),
)

export const plugInsRefreshed = plugInsLoaded.then(() =>
  plugIns.refresh().catch((error) => {
    console.error("Couldn't refresh plug-ins.", error)
    return { hasNewerVersions: false }
  }),
)
