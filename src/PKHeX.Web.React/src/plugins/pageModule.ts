import type { PlugInSetting } from '@pkhex-everywhere/engine'
import type { Mount, PageContext, SettingValue, Unmount } from '@pkhex-everywhere/plugin-sdk'
import { fromBase64 } from '../base64'

type PageModule = { mount: Mount }

export function settingValue(setting: PlugInSetting | null): SettingValue | null {
  if (!setting) return null
  if (setting.fileName !== null) return fromBase64(setting.file ?? '')
  return setting.integerValue ?? setting.booleanValue ?? setting.stringValue ?? null
}

async function importFromBlob(source: string): Promise<PageModule> {
  const url = URL.createObjectURL(new Blob([source], { type: 'text/javascript' }))
  try {
    return await import(/* @vite-ignore */ url)
  } finally {
    URL.revokeObjectURL(url)
  }
}

export function mountPageModule(
  element: HTMLElement,
  source: Promise<string>,
  ctx: PageContext,
  load: (source: string) => Promise<PageModule> = importFromBlob,
): Unmount {
  let unmounted = false
  const mounted = source.then(load).then((module) => (unmounted ? undefined : module.mount(element, ctx)))
  mounted.catch((error) => console.error("Couldn't mount the plug-in page.", error))

  return () => {
    unmounted = true
    void mounted.then((unmount) => unmount?.()).catch(() => {})
  }
}
