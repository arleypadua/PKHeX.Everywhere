using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public sealed class EventEncounter : Encounter
{
    private readonly SaveFile _save;

    internal EventEncounter(IEncounterable data, PKM sample, Game game) : base(data, game)
    {
        _save = game.SaveFile;
        Trainer = new EventTrainer(
            FixesName(data) ? sample.OriginalTrainerName : null,
            FixesId(data) ? sample.TID16 : null,
            FixesLanguage(data) ? LanguageCodes.Of(sample.Language) : null);
    }

    public EventTrainer Trainer { get; }

    internal override PKM Generate() => SaveRules.Under(_save, () => Generate(Data, _save)) ?? base.Generate();

    // PKHeX sometimes makes an event Pokémon that its own legality check rejects, such as a PCNY Flygon whose ability doesn't match its PID, so an event takes the first legal one of a few tries.
    internal static PKM? Generate(IEncounterable gift, SaveFile save)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (save.GetCompatible([gift.ConvertToPKM(save)]).FirstOrDefault() is { } pkm && new LegalityAnalysis(pkm).Valid)
                return pkm;
        }
        return null;
    }

    // PCNY and PCJP gifts went out under a few trainer names, and the tour Mews under random IDs.
    private static bool FixesName(IEncounterable gift) => gift switch
    {
        EncounterGift1 gen1 => gen1.Trainer is not EncounterGift1.TrainerType.Recipient,
        EncounterGift2 gen2 => gen2.Trainer is not (EncounterGift2.TrainerType.Recipient or EncounterGift2.TrainerType.PokemonCenterNewYork),
        EncounterGift3NY or EncounterGift3JPN => false,
        IFixedTrainer fixedTrainer => fixedTrainer.IsFixedTrainer,
        _ => false,
    };

    private static bool FixesId(IEncounterable gift) => gift switch
    {
        EncounterGift1 gen1 => gen1.Trainer is EncounterGift1.TrainerType.Stadium or EncounterGift1.TrainerType.VirtualConsoleMew,
        EncounterGift2 gen2 => gen2.Trainer is EncounterGift2.TrainerType.GiftStadiumJPN or EncounterGift2.TrainerType.GiftStadiumENG or EncounterGift2.TrainerType.GiftStadiumINT,
        EncounterGift3 gen3 => gen3.TID16 != ushort.MaxValue,
        EncounterGift3JPN or EncounterGift3Colo => true,
        _ => false,
    };

    private static bool FixesLanguage(IEncounterable gift) => gift switch
    {
        EncounterGift1 gen1 => gen1.Language is EncounterGift1.LanguageRestriction.Japanese,
        EncounterGift2 gen2 => gen2.Language is EncounterGift2.LanguageRestriction.Japanese or EncounterGift2.LanguageRestriction.English,
        EncounterGift3 gen3 => gen3.Language != 0,
        EncounterGift3NY or EncounterGift3JPN => true,
        EncounterGift3Colo colo => colo.IsJapaneseBonusDisk,
        _ => false,
    };
}

public record EventTrainer(string? Name, int? Id, string? Language);
