import { Typography } from 'antd'

export default function CreditsPage() {
  return (
    <Typography>
      <h1>Credits</h1>

      <h2>PKHeX.Core</h2>
      <p>
        This app is built on the foundation of{' '}
        <a href="https://github.com/kwsch/PKHeX" target="_blank">PKHeX.Core</a>. Special thanks to{' '}
        <a href="https://github.com/kwsch" target="_blank">Kurt</a> for maintaining it.
      </p>

      <h2>PokeAPI/sprites</h2>
      <p>
        All image resources are taken from{' '}
        <a href="https://github.com/PokeAPI/sprites" target="_blank">PokeAPI/sprites</a>.
      </p>

      <h2>PKHeX-Plugins</h2>
      <p>
        Special thanks to <a href="https://github.com/santacrab2" target="_blank">santacrab2</a> for maintaining the{' '}
        <a href="https://github.com/santacrab2/PKHeX-Plugins" target="_blank">PKHeX-Plugins</a> fork, which includes the
        Auto-Legality mode.
      </p>

      <h2>OpenHome</h2>
      <p>
        The Pokémon Unbound and Radical Red species, item and move data comes from{' '}
        <a href="https://github.com/andrewbenington/OpenHome" target="_blank">OpenHome</a> by{' '}
        <a href="https://github.com/andrewbenington" target="_blank">Andrew Benington</a>. Thanks for making it available.
      </p>

      <h2>PKMN RBYGSC font</h2>
      <p>
        The pixel font is{' '}
        <a href="https://fontstruct.com/fontstructions/show/875033" target="_blank">PKMN RBYGSC</a> by David Fens.
      </p>
    </Typography>
  )
}
