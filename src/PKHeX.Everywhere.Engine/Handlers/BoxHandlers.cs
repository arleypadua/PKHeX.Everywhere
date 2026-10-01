using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using Pokemon = PKHeX.Facade.Pokemons.Pokemon;

namespace PKHeX.Everywhere.Engine.Handlers;

public static class BoxHandlers
{
    [Command("box.addFromFile", Topics.Box)]
    public static AddedPokemon AddFromFile(Session session, Game game, byte[] bytes)
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

        var pokemon = loaded.ConvertTo(game, out var result)
            ?? throw new EngineException(ErrorCodes.ConversionFailed, $"Can't convert {loaded.Species.Name} to {game.SaveVersion.Name} ({result}).");

        if (!game.IsAwareOf(pokemon))
            throw new EngineException(ErrorCodes.NotInGame, $"{pokemon.Species.Name} doesn't exist in {game.SaveVersion.Name}.");

        return AddToBox(session, game, pokemon, PokemonAddSource.File);
    }

    [Command("box.addEncounter", Topics.Box)]
    public static AddedPokemon AddEncounter(Session session, Game game, int index)
    {
        if (session.Encounters is not { } encounters || index < 0 || index >= encounters.Count)
            throw new EngineException(ErrorCodes.NotFound, "That encounter isn't in the latest search. Search again.");

        return AddToBox(session, game, encounters[index].ConvertToPokemon(), PokemonAddSource.Encounter);
    }

    private static AddedPokemon AddToBox(Session session, Game game, Pokemon pokemon, PokemonAddSource source)
    {
        var box = game.Trainer.PokemonBox;
        if (!box.AddOnEmptySlot(pokemon, out var index))
            throw new EngineException(ErrorCodes.BoxFull, "Your box is full.");

        var slots = game.SaveFile.BoxSlotCount;
        var at = PokemonHandle.InBox(index / slots, index % slots);
        session.Raise(new PokemonAdded(at, source));
        return new AddedPokemon(new PokemonId(box.All[index].UniqueId.Value), at);
    }
}
