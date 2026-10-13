import { useMemo } from 'react'
import type { Binary } from '@pkhex-everywhere/engine'
import { useEngine } from './EngineProvider'
import { useQuery } from './useQuery'

/**
 * The save's Wonder Card album, from `mysteryGift.status`, and the commands that change it.
 * Only Generation 4 and 5 saves have an album, so check `hasMysteryGift` from `useLoadedGame()` first: on other saves it throws `not-supported`.
 * Requires a loaded save. Wrap in `<RequireGame>`.
 */
export function useMysteryGift() {
  const engine = useEngine()
  const album = useQuery('mysteryGift.status')
  const commands = useMemo(
    () => ({
      /** Puts an event's card in a slot as a real reception. Resolves to the slot the card landed in. */
      insertEvent: (id: string, slot: number) => engine.mysteryGift.insertEvent(id, slot),
      /** Puts a `.pgt`, `.pcd`, `.wc4` or `.pgf` file's card in a slot as a real reception. Resolves to the slot the card landed in. */
      insertFile: (bytes: Binary, slot: number) => engine.mysteryGift.insertFile(bytes, slot),
      /** Empties a slot and moves the cards after it up. */
      remove: (slot: number) => engine.mysteryGift.remove(slot),
      /** Shows or hides Mystery Gift in a Generation 4 game's menu. */
      setUnlocked: (value: boolean) => engine.mysteryGift.setUnlocked(value),
      /** Sets or clears the save's mark that it received a card number. */
      setReceived: (cardId: number, value: boolean) => engine.mysteryGift.setReceived(cardId, value),
    }),
    [engine],
  )
  return { album, ...commands }
}
