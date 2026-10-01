export type Theme = 'light' | 'dark'

export interface SaveFile {
  bytes: Uint8Array
  fileName: string
  /** The PKHeX `GameVersion` name of the save, such as `E` or `FRLG`. */
  version: string
}

/** File settings come back as their bytes. */
export type SettingValue = string | number | boolean | Uint8Array

export interface PageContext {
  readonly plugInId: string
  readonly theme: Theme
  getSave(): Promise<SaveFile | null>
  getSetting(key: string): Promise<SettingValue | null>
  /** Loads the save into the app and navigates to Home. */
  loadSave(bytes: Uint8Array, fileName: string): Promise<void>
  navigate(url: string): void
}

export type Unmount = () => void

/** The signature of the `mount` function a page module exports. */
export type Mount = (element: HTMLElement, ctx: PageContext) => Unmount | Promise<Unmount>
