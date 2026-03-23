using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade;

public class Trainer
{
    private readonly Game _game;
    
    public Trainer(Game game)
    {
        _game = game;

        Money = new Money(_game);
        Inventories = new Inventories(_game);
        Party = new PokemonParty(_game);
        PokemonBox = new PokemonBox(_game);
    }

    public EntityId Id => new EntityId(_game.SaveFile.DisplayTID, _game.SaveFile.DisplaySID);

    public uint TID
    {
        get => _game.SaveFile.DisplayTID;
        set => _game.SaveFile.DisplayTID = value;
    }

    public uint SID
    {
        get => _game.SaveFile.DisplaySID;
        set => _game.SaveFile.DisplaySID = value;
    }

    public string Name
    {
        get => _game.SaveFile.OT;
        set => _game.SaveFile.OT = value;
    }

    public Gender Gender
    {
        get => Gender.FromByte(_game.SaveFile.Gender);
        set => _game.SaveFile.Gender = value.ToByte();
    }

    public Money Money { get; }
    public Inventories Inventories { get; private set; }
    public PokemonParty Party { get; private set; }
    public PokemonBox PokemonBox { get; private set; }

    public int PokemonInPartyCount => Party.Pokemons.Count(p => p.Species != SpeciesDefinition.None);
    public int PokemonInBoxCount => PokemonBox.All.Count(p => p.Species != SpeciesDefinition.None);

    public void ApplyOwnerToAll()
    {
        var allPokemon = Party.Pokemons
            .Concat(PokemonBox.All)
            .Where(p => p.Species != SpeciesDefinition.None)
            .ToList();

        foreach (var pokemon in allPokemon)
        {
            pokemon.Owner.Name = Name;
            pokemon.Owner.TID = TID;
            pokemon.Owner.SID = SID;
        }

        Commit();
    }

    public string? RivalName => _game.SaveFile switch
    {
        SAV1 gen1 => gen1.RivalName,
        SAV2 gen2 => gen2.RivalName,
        SAV3FRLG gen3 => gen3.RivalName,
        SAV4 gen4 => gen4.RivalName,
        SAV5B2W2 gen5 => gen5.RivalName,
        SAV7b gen7 => gen7.Misc.RivalName,
        SAV8BS gen8 => gen8.RivalName,
        _ => null
    };

    public void AddOrUpdate(UniqueId id, Pokemon pokemon, PokemonSource source)
    {
        IMutablePokemonCollection collection = source switch
        {
            PokemonSource.Box => PokemonBox,
            PokemonSource.Party => Party,
            _ => throw new InvalidOperationException($"{source} is not supported when updating pokemon"),
        };
        
        collection.AddOrUpdate(id, pokemon);
    }

    internal void Commit()
    {
        Party.Commit();
        PokemonBox.Commit();
    }
}