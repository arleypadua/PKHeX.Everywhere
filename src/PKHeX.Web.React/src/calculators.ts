export interface Calculator {
  name: string
  description: string
  url: string
}

export const smogon: Calculator = {
  name: 'Smogon Calculator',
  description: 'Pokemon Showdown default calculator',
  url: 'https://calc.pokemonshowdown.com',
}

export const calculators: Calculator[] = [
  smogon,
  {
    name: 'KinglerChamp Nuzlocke Calculator',
    description: 'Calculator with a database of all main line games included',
    url: 'https://kinglerchamp.github.io/VanillaNuzlockeCalc',
  },
]
