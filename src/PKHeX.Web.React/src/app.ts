import { createEngine } from '@pkhex-everywhere/engine'
import { createPlugIns } from './plugins/plugIns'
import { createPlugInStore } from './plugins/store'

export const engine = createEngine()

export const plugIns = createPlugIns(engine, createPlugInStore())

export const plugInsLoaded = engine.ready.then(() =>
  plugIns.registerStored().catch((error) => console.error("Couldn't load plug-ins.", error)),
)

export const plugInsRefreshed = plugInsLoaded.then(() =>
  plugIns.refresh().catch((error) => {
    console.error("Couldn't refresh plug-ins.", error)
    return { hasNewerVersions: false }
  }),
)
