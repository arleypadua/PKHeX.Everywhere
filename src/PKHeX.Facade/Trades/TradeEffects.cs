using PKHeX.Core;

namespace PKHeX.Facade.Trades;

internal static class TradeEffects
{
    public const ushort GriseousOrb = 112;
    private const ushort Everstone = 229;
    private const byte ReceivedFriendship = 70;
    // VAR_ARCEUS_EVENT_STATE in pret/pokeplatinum.
    public const int ArceusEventWork = 86;
    private const ushort HallOfOrigin = 86;

    private static readonly ushort[] RotomFormMoves =
        [(ushort)Move.Overheat, (ushort)Move.HydroPump, (ushort)Move.Blizzard, (ushort)Move.AirSlash, (ushort)Move.LeafStorm];

    /// <returns>Whether it took a Griseous Orb, which goes back to the sender's bag.</returns>
    public static bool RevertPlatinumForm(PKM pk)
    {
        var tookOrb = pk.HeldItem == GriseousOrb;
        if (tookOrb) pk.HeldItem = 0;
        if (pk.Form == 0) return tookOrb;

        switch ((Species)pk.Species)
        {
            case Species.Giratina or Species.Shaymin:
                break;
            case Species.Rotom:
                ForgetRotomFormMoves(pk);
                break;
            default:
                return tookOrb;
        }

        pk.Form = 0;
        RefreshAbility(pk);
        RecalculateStats(pk);
        return tookOrb;
    }

    public static void Receive(PKM pk, TradeRoute route)
    {
        if (route == TradeRoute.Link && pk.Context is EntityContext.Gen2 or EntityContext.Gen3 or EntityContext.Gen4 && !pk.IsEgg)
            pk.CurrentFriendship = ReceivedFriendship;
    }

    public static void Evolve(PKM pk, Game to, TradeRoute route)
    {
        if (pk.IsEgg || route is not (TradeRoute.Link or TradeRoute.TimeCapsule)) return;

        var context = pk.Context;
        var held = ItemConverter.GetItemDisplay(pk.HeldItem, context);
        // pret/pokeplatinum skips the Everstone check for Kadabra.
        if (held == Everstone && !(context == EntityContext.Gen4 && pk.Species == (ushort)Species.Kadabra)) return;

        foreach (var evolution in EvolutionTree.GetEvolutionTree(context).Forward.GetForward(pk.Species, pk.Form).Span)
        {
            if (evolution.Method is not (EvolutionType.Trade or EvolutionType.TradeHeldItem)) continue;

            var item = RequiredItem(evolution, pk.Species, context);
            if (item is not null && (route == TradeRoute.TimeCapsule || item != held)) continue;

            var form = evolution.GetDestinationForm(pk.Form);
            if (!to.SaveFile.Personal.IsPresentInGame(evolution.Species, form)) return;

            var nicknamed = pk.IsNicknamed;
            pk.Species = evolution.Species;
            pk.Form = form;
            if (item is not null) pk.HeldItem = 0;
            if (!nicknamed) pk.ClearNickname();
            RefreshAbility(pk);
            RecalculateStats(pk);
            return;
        }
    }

    public static bool StartsArceusEvent(PKM pk, Game to) =>
        to.SaveFile is SAV4Pt platinum
        && pk.Species == (ushort)Species.Arceus
        && (pk.FatefulEncounter || pk.MetLocation == HallOfOrigin)
        && platinum.GetWork(ArceusEventWork) == 0;

    // PKHeX's Gen 2 data lists the held-item trade evolutions as plain trades, so the item comes from Gen 4's.
    private static int? RequiredItem(EvolutionMethod evolution, ushort species, EntityContext context)
    {
        if (evolution.Method == EvolutionType.TradeHeldItem) return ItemConverter.GetItemDisplay(evolution.Argument, context);
        if (context != EntityContext.Gen2) return null;

        foreach (var modern in EvolutionTree.Evolves4.Forward.GetForward(species, 0).Span)
            if (modern.Species == evolution.Species && modern.Method == EvolutionType.TradeHeldItem)
                return modern.Argument;
        return null;
    }

    // pret/pokeplatinum Pokemon_SetRotomForm, reverting to the base form.
    private static void ForgetRotomFormMoves(PKM pk)
    {
        for (var slot = 0; slot < 4; slot++)
            if (RotomFormMoves.Contains(pk.GetMove(slot))) pk.SetMove(slot, 0);
        pk.FixMoves();

        if (pk.Move1 != 0) return;
        pk.Move1 = (ushort)Move.ThunderShock;
        pk.Move1_PP = pk.GetMovePP(pk.Move1, 0);
        pk.Move1_PPUps = 0;
    }

    // Gen 3 works the ability out from the species, so only later formats store it.
    private static void RefreshAbility(PKM pk)
    {
        if (pk.Format < 4) return;

        var personal = pk.PersonalInfo;
        var index = Math.Min(pk.AbilityNumber >> 1, personal.AbilityCount - 1);
        var ability = personal.GetAbilityAtIndex(index);
        pk.Ability = ability != 0 ? ability : personal.GetAbilityAtIndex(0);
    }

    // As the games do, the HP lost stays lost, and a fainted Pokémon stays fainted.
    private static void RecalculateStats(PKM pk)
    {
        if (!pk.PartyStatsPresent) return;

        var (maxBefore, current) = (pk.Stat_HPMax, pk.Stat_HPCurrent);
        pk.SetStats(pk.GetStats(pk.PersonalInfo));
        pk.Stat_HPCurrent = current == 0 ? 0 : Math.Max(1, current + pk.Stat_HPMax - maxBefore);
    }
}
