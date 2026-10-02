import { blazorHost, createEngine } from '@pkhex-everywhere/engine'
import { createPlugIns } from './plugins/plugIns'
import { createPlugInStore } from './plugins/store'

export const engine = createEngine({ host: blazorHost() })

export const plugIns = createPlugIns(engine, createPlugInStore())

export const plugInsLoaded = engine.ready.then(async () => {
  await plugIns.registerStored().catch((error) => console.error("Couldn't load plug-ins.", error))
  plugIns.refresh().catch((error) => console.error("Couldn't refresh plug-ins.", error))
})
