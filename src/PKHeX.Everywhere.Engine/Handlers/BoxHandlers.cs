using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;
using Transfers = PKHeX.Facade.Transfers;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class BoxHandlers
{
    [Query("box.get", Topics.Box)]
    public static PokemonSummary[] Get(Game game) => game.Trainer.PokemonBox.Boxed()
        .Select(boxed => boxed.Pokemon.ToSummary(PokemonSlots.BoxHandle(game.SaveFile, boxed.Index)))
        .ToArray();

    /// <summary>
    /// Lists every box in the save in order, empty ones included, with its name and slot count.
    /// </summary>
    [Query("box.list", Topics.Box)]
    public static BoxEntry[] List(Game game) => game.Trainer.PokemonBox.Boxes.Select(box => box.ToEntry()).ToArray();

    [Query("box.showdown", Topics.Box)]
    public static string Showdown(Game game) => game.Trainer.PokemonBox.Showdown();

    /// <summary>
    /// Shows how <c>box.addFromFile</c> would add a Pokémon file to the loaded save. Nothing is written.
    /// Fails with <c>unparseable</c> when the bytes aren't a Pokémon, <c>not-in-game</c> when the game doesn't have its species,
    /// and <c>conversion-failed</c> when PKHeX can't convert it at all.
    /// </summary>
    [Query("box.previewFile", Topics.Box, Topics.Trainer)]
    public static FilePreview PreviewFile(Game game, byte[] bytes)
    {
        var import = Import(game, bytes);
        return new FilePreview(
            import.Arrives.ToPreview(),
            import.Unofficial,
            import.Changes.Select(change => change.ToDto()).ToArray(),
            import.Legality() is { } legality ? new Legality(legality.Valid, legality.Messages.ToArray()) : null);
    }

    /// <summary>
    /// Adds a Pokémon file to the first empty box slot, converted to the loaded save's format.
    /// Fails with <c>unparseable</c>, <c>not-in-game</c> and <c>conversion-failed</c> as <c>box.previewFile</c> does, and with <c>box-full</c> when no slot is empty.
    /// A file no game can move into the save fails with <c>conversion-failed</c> unless <c>allowUnofficial</c> is set.
    /// </summary>
    [Command("box.addFromFile", Topics.Box)]
    public static AddedPokemon AddFromFile(Session session, Game game, byte[] bytes, AddFromFileOptions? options = null)
    {
        var import = Import(game, bytes);
        if (import.Unofficial && options?.AllowUnofficial != true)
            throw new EngineException(ErrorCodes.ConversionFailed, $"No game can move {import.Arrives.Species.Name} to {game.SaveVersion.Name}. Pass allowUnofficial to add it anyway.");

        return AddToBox(session, game, import.Arrives, PokemonAddSource.File);
    }

    [Command("box.addEncounter", Topics.Box)]
    public static AddedPokemon AddEncounter(Session session, Game game, int index)
    {
        game.Require(Capability.Encounters);
        if (session.Encounters is not { } encounters || index < 0 || index >= encounters.Count)
            throw new EngineException(ErrorCodes.NotFound, "That encounter isn't in the latest search. Search again.");

        return AddToBox(session, game, encounters[index].ConvertToPokemon(), PokemonAddSource.Encounter);
    }

    private static Transfers.PokemonImport Import(Game game, byte[] bytes)
    {
        Pokemon loaded;
        try
        {
            loaded = Pokemon.LoadFrom(bytes, game);
        }
        catch (InvalidOperationException e)
        {
            throw new EngineException(ErrorCodes.Unparseable, "The file isn't a Pokémon.", e);
        }

        return Converting(loaded, game, () => Transfers.Transfer.Import(loaded, game));
    }

    internal static Transfers.PokemonImport Converting(Pokemon loaded, Game to, Func<Transfers.PokemonImport> convert)
    {
        try
        {
            return convert();
        }
        catch (Transfers.PokemonRefusedException e) when (e.Reason == Transfers.TransferRefusal.SpeciesNotInGame)
        {
            throw new EngineException(ErrorCodes.NotInGame, $"{loaded.Species.Name} doesn't exist in {to.SaveVersion.Name}.", e);
        }
        catch (Transfers.PokemonRefusedException e)
        {
            throw new EngineException(ErrorCodes.ConversionFailed, $"Can't convert {loaded.Species.Name} to {to.SaveVersion.Name} ({e.Reason}).", e);
        }
    }

    private static AddedPokemon AddToBox(Session session, Game game, Pokemon pokemon, PokemonAddSource source)
    {
        var box = game.Trainer.PokemonBox;
        if (!box.AddOnEmptySlot(pokemon, out var index))
            throw new EngineException(ErrorCodes.BoxFull, "Your box is full.");

        var at = PokemonSlots.BoxHandle(game.SaveFile, index);
        session.Raise(new PokemonAdded(at, source, box.All[index].ToOverview()));
        return new AddedPokemon(new PokemonId(box.All[index].UniqueId.Value), at);
    }
}
