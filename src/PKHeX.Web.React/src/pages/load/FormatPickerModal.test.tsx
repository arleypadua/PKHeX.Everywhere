import { describe, expect, it } from 'vitest'
import { render } from '../../testing/render'
import { FormatPickerModal } from './FormatPickerModal'
import type { FormatChoice } from './useLoadSave'

const file = new File([new Uint8Array(8)], 'Pokemon Emerald.sav')

function renderPicker(choice: Omit<FormatChoice, 'file'>) {
  render(<FormatPickerModal choice={{ file, ...choice }} onChoose={() => {}} onCancel={() => {}} />)
  const dialog = document.querySelector('[role="dialog"]')!
  return {
    text: () => dialog.textContent,
    options: () => [...dialog.querySelectorAll('label')].map((label) => label.textContent),
  }
}

describe('FormatPickerModal', () => {
  it('asks whether a save with no candidates is from a ROM hack', () => {
    const picker = renderPicker({ candidates: [], pkhexGame: 'Emerald' })

    expect(picker.text()).toContain("moves that Emerald doesn't have")
    expect(picker.options()).toEqual(["No, it's Emerald"])
  })

  it('names the game PKHeX reads the save as after the candidates', () => {
    const picker = renderPicker({ candidates: [{ id: 'emerald-legacy', name: 'Pokémon Emerald Legacy' }], pkhexGame: 'Emerald' })

    expect(picker.options()).toEqual(['Pokémon Emerald Legacy', "No, it's Emerald"])
  })

  it('leaves out the PKHeX option when PKHeX cannot read the save', () => {
    const picker = renderPicker({ candidates: [{ id: 'radicalred', name: 'Pokémon Radical Red' }] })

    expect(picker.options()).toEqual(['Pokémon Radical Red'])
  })
})
