using System.Text.Json;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;

namespace PKHeX.Everywhere.Engine.Tests;

internal static class EngineCalls
{
    // Let's Go keeps party members in box storage, so their box slots alias party slots. The Eevee save has no other box Pokémon.
    internal static (PokemonHandle At, int Index)? FirstBoxPokemon(Game game)
    {
        var all = game.Trainer.PokemonBox.All;
        var index = Enumerable.Range(0, all.Count).FirstOrDefault(i => all[i].Pkm.Species != 0 && !InParty(game, i), -1);
        var slots = game.SaveFile.BoxSlotCount;
        return index < 0 ? null : (PokemonHandle.InBox(index / slots, index % slots), index);
    }

    private static bool InParty(Game game, int boxIndex) =>
        game.SaveFile is SAV7b { Blocks.Storage: var storage } && storage.IsParty(boxIndex);

    internal static string Args(params object[] args) => JsonSerializer.Serialize(args.Select(Arg));

    private static object Arg(object arg) => arg is PokemonHandle at
        ? new { source = at.Source == SlotSource.Party ? "party" : "box", slot = at.Slot, box = at.Box }
        : arg;
}
