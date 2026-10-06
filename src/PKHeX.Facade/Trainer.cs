using PKHeX.Core;
using PKHeX.Facade.Abstractions;
using PKHeX.Facade.Pokemons;

namespace PKHeX.Facade;

public class Trainer
{
    private readonly Game _game;
    
    public Trainer(Game game)
    {
        _game = game;

        // A ROM hack save reports Gen 9, which would show its IDs in the six-digit format of Gen 7 onwards.
        Id = _game.Format is { BaseGame.Generation: < 7 }
            ? new EntityId(_game.SaveFile.TID16, _game.SaveFile.SID16)
            : new EntityId(_game.SaveFile.DisplayTID, _game.SaveFile.DisplaySID);
        Money = new Money(_game);
        Inventories = new Inventories(_game);
        Party = new PokemonParty(_game);
        PokemonBox = new PokemonBox(_game, Party);
    }

    public EntityId Id { get; }
    public string Name
    {
        get => _game.SaveFile.OT;
        set
        {
            var max = _game.SaveFile.MaxStringLengthTrainer;
            value ??= string.Empty;
            _game.SaveFile.OT = value.Length > max ? value[..max] : value;
        }
    }
    public bool HasGender => _game.SaveFile is not (SAV1 or SAV2 { Version: not GameVersion.C } or SAV_STADIUM);

    public Gender Gender
    {
        get => Gender.FromByte(_game.SaveFile.Gender);
        set => _game.SaveFile.Gender = value.ToByte();
    }

    public Money Money { get; }
    public Inventories Inventories { get; private set; }
    public PokemonParty Party { get; private set; }
    public PokemonBox PokemonBox { get; private set; }

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

    public event Action<PokemonSource>? PokemonsChanged;

    public void AddOrUpdate(UniqueId id, Pokemon pokemon, PokemonSource source)
    {
        IMutablePokemonCollection collection = source switch
        {
            PokemonSource.Box => PokemonBox,
            PokemonSource.Party => Party,
            _ => throw new InvalidOperationException($"{source} is not supported when updating pokemon"),
        };
        
        collection.AddOrUpdate(id, pokemon);
        PokemonsChanged?.Invoke(source);
    }

    internal void Commit()
    {
        Party.Commit();
        PokemonBox.Commit();
    }
}