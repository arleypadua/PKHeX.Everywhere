const spritesUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items'

// PokeAPI only has per-type TM sprites and no TR sprites; the TM's move type isn't known here
const numberedTmOrTr = /^T[MR]\d+$/

const itemSpriteName = (name: string) =>
  numberedTmOrTr.test(name) ? 'tm-normal' : name.toLowerCase().replaceAll('é', 'e').split(' ').join('-')

interface ItemIconProps {
  name: string
}

export function ItemIcon({ name }: ItemIconProps) {
  if (!name) return null
  return <img alt={name} src={`${spritesUrl}/${itemSpriteName(name)}.png`} style={{ width: 30 }} />
}
