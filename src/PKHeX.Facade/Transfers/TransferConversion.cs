using System.Reflection;
using PKHeX.Core;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade.Transfers;

internal static class TransferConversion
{
    public static TransferRefusal? Convert(Pokemon pokemon, Game to, bool unofficial, out Pokemon? arrives)
    {
        arrives = null;
        if (unofficial && !to.IsAwareOf(pokemon)) return TransferRefusal.SpeciesNotInGame;

        var converted = unofficial
            ? ConvertUnofficially(pokemon.Pkm, to, out var result)
            : EntityConverter.ConvertToType(pokemon.Pkm.Clone(), to.SaveFile.PKMType, out result);
        if (converted is null)
            return result is EntityConverterResult.IncompatibleSpecies or EntityConverterResult.IncompatibleForm
                ? TransferRefusal.SpeciesNotInGame
                : TransferRefusal.ConversionFailed;

        to.SaveFile.AdaptToSaveFile(converted, isParty: false);
        arrives = new Pokemon(converted, to);
        return to.IsAwareOf(arrives) ? null : TransferRefusal.SpeciesNotInGame;
    }

    public static bool HasBall(Game game, byte ball) => game.Options.Balls.Any(choice => choice.Id == ball);

    public static bool CanHaveAbility(PKM pk, int ability) => pk.PersonalInfo.GetIndexOfAbility(ability) >= 0;

    private static PKM? ConvertUnofficially(PKM pk, Game to, out EntityConverterResult result)
    {
        var converted = ConvertSane(pk.Clone(), to, out result);
        if (converted is null)
        {
            var stripped = pk.Clone();
            StripMoves(stripped, to);
            stripped.HeldItem = 0;
            if (!HasBall(to, stripped.Ball)) stripped.Ball = (byte)Ball.Poke;
            converted = ConvertSane(stripped, to, out result);
        }

        if (converted is null) return null;

        // Reflection copies the held item's raw value, and Generations 1 to 3 number their items differently from the rest.
        if (result == EntityConverterResult.SuccessIncompatibleReflection) converted.HeldItem = ItemIn(to, pk);
        if (converted.HeldItem != 0 && to.Options.HeldItems.All(item => item.Id != converted.HeldItem)) converted.HeldItem = 0;
        if (to.SaveFile.Generation >= 3 && !HasBall(to, converted.Ball)) converted.Ball = (byte)Ball.Poke;
        if (!Enum.IsDefined(converted.Version) || !converted.Version.IsValidSavedVersion()) converted.Version = DestinationVersion(to);
        if (converted.Format >= 4 && !CanHaveAbility(converted, converted.Ability)) TransferEffects.RefreshAbility(converted);

        StripMoves(converted, to);
        if (converted.Move1 == 0 && FirstLevelUpMove(converted, to) is { } move)
        {
            converted.Move1 = move;
            converted.Move1_PPUps = 0;
            converted.Move1_PP = converted.GetMovePP(move, 0);
        }

        return converted;
    }

    // EntityConverter reads the setting from a static, so it is on only while this conversion runs.
    private static PKM? ConvertSane(PKM pk, Game to, out EntityConverterResult result)
    {
        var previous = EntityConverter.AllowIncompatibleConversion;
        EntityConverter.AllowIncompatibleConversion = EntityCompatibilitySetting.AllowIncompatibleSane;
        try
        {
            return EntityConverter.ConvertToType(pk, to.SaveFile.PKMType, out result);
        }
        catch (TargetInvocationException)
        {
            result = EntityConverterResult.NoTransferRoute;
            return null;
        }
        finally
        {
            EntityConverter.AllowIncompatibleConversion = previous;
        }
    }

    private static void StripMoves(PKM pk, Game to)
    {
        var moves = to.Options.Moves.Select(move => move.Id).ToHashSet();
        for (var slot = 0; slot < 4; slot++)
            if (!moves.Contains(pk.GetMove(slot))) pk.SetMove(slot, 0);
        pk.FixMoves();
    }

    private static int ItemIn(Game to, PKM from)
    {
        var item = ItemConverter.GetItemDisplay(from.HeldItem, from.Context);
        if (from.Format <= 3 && !ItemConverter.IsItemTransferable34((ushort)item)) return 0;

        return to.SaveFile.Generation switch
        {
            1 => 0,
            2 => ItemConverter.GetItemOld2((ushort)item),
            3 => ItemConverter.GetItemOld3((ushort)item),
            _ => item,
        };
    }

    private static GameVersion DestinationVersion(Game to) => to.SaveFile.Version.IsValidSavedVersion()
        ? to.SaveFile.Version
        : to.SaveFile.Context.GetSingleGameVersion();

    private static ushort? FirstLevelUpMove(PKM pk, Game to)
    {
        var version = DestinationVersion(to);
        var moves = to.Options.Moves.Select(move => move.Id).ToHashSet();
        foreach (var move in GameData.GetLearnSource(version).GetLearnset(pk.Species, pk.Form).GetAllMoves())
            if (moves.Contains(move)) return move;
        return null;
    }
}
