/** The app's color theme. */
export type Theme = 'light' | 'dark'

/** The save currently loaded in the app. */
export interface SaveFile {
  /** The save's bytes, including any edits made in the app. */
  bytes: Uint8Array
  fileName: string
  /** The PKHeX `GameVersion` name of the save, such as `E` or `FRLG`. */
  version: string
}

/** A plug-in setting's value. File settings come back as their bytes. */
export type SettingValue = string | number | boolean | Uint8Array

/** What the app gives a page module when it mounts it. */
export interface PageContext {
  /** The plug-in's id, which is its assembly name. */
  readonly plugInId: string
  /** The app's current theme. Read it again when you need it, since the user can switch themes while the page is open. */
  readonly theme: Theme
  /** The loaded save, or `null` when none is loaded. */
  getSave(): Promise<SaveFile | null>
  /** The value of one of this plug-in's settings, or `null` when it has none. */
  getSetting(key: string): Promise<SettingValue | null>
  /** Loads the bytes into the app as the current save. Call `navigate` afterwards to leave the page. */
  loadSave(bytes: Uint8Array, fileName: string): Promise<void>
  /** Navigates the app to one of its routes, such as `/`. */
  navigate(url: string): void
}

/** Cleans up what `mount` rendered. The app calls it when the page closes. */
export type Unmount = () => void

/** The signature of the `mount` function a page module exports. */
export type Mount = (element: HTMLElement, ctx: PageContext) => Unmount | Promise<Unmount>
