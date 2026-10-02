import { createEngine, wasmHost } from '@pkhex-everywhere/engine'
import { blazorHost } from './blazorHost'
import { createPlugIns } from './plugins/plugIns'
import { createPlugInStore } from './plugins/store'

export const engine = createEngine({ host: import.meta.env.MODE === 'blazor' ? blazorHost() : wasmHost() })

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
