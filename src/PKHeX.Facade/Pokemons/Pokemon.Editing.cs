using PKHeX.Core;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Repositories;

namespace PKHeX.Facade.Pokemons;

public partial class Pokemon
{
    public PokemonDetails Details() => new(
        Species.Id,
        Pkm.Form,
        Gender,
        (int)Pkm.Nature,
        Math.Max(Pkm.Ability, 0),
        Pkm.HeldItem,
        Pkm.Ball,
        Pkm.CurrentFriendship,
        Pkm.Language,
        IsShiny,
        SupportsAlpha ? IsAlpha : null,
        Pkm.IsEgg,
        Nickname,
        Level,
        PID,
        Types.HasSecondary ? [Types.Type1, Types.Type2] : [Types.Type1],
        Flags.IsInfected,
        Flags.IsCured,
        Owner.TID,
        Owner.SID,
        Owner.Name,
        Owner.Gender,
        Owner.HandlingTrainerName,
        Owner.HandlingTrainerGender,
        Owner.CurrentHandler,
        (int)Pkm.Version,
        Pkm.MetLocation,
        Pkm.MetLevel,
        Pkm.MetDate,
        Pkm.FatefulEncounter,
        this.LegalityReport());

    public PokemonOptions Options() => new(SpeciesChoices(), AbilityChoices(), FormChoices(), MetLocationChoices());

    /// <summary>
    /// Applies the patch in a fixed order, species before form before ability and origin game before met location, so each field is checked against the ones before it.
    /// </summary>
    /// <exception cref="InvalidPatchException">A field holds a value the save can't store. Nothing is applied.</exception>
    public void Update(PokemonPatch patch)
    {
        Clone().Apply(patch);
        Apply(patch);
    }

    private void Apply(PokemonPatch patch)
    {
        if (patch.Species is { } species) ApplySpecies(species);
        if (patch.Form is { } form) ApplyForm(form);
        if (patch.Gender is { } gender) ApplyGender(gender);
        if (patch.Nature is { } nature) ApplyNature(nature);
        if (patch.Ability is { } ability) ApplyAbility(ability);
        if (patch.HeldItem is { } heldItem) ApplyHeldItem(heldItem);
        if (patch.Ball is { } ball) ApplyBall(ball);
        if (patch.Language is { } language) ApplyLanguage(language);
        if (patch.IsEgg is { } isEgg) ApplyIsEgg(isEgg);
        if (patch.Friendship is { } friendship) ApplyFriendship(friendship);
        if (patch.Nickname is { } nickname) ApplyNickname(nickname.Trim());
        if (patch.Level is { } level) ApplyLevel(level);
        if (patch.IsShiny is { } isShiny) ApplyIsShiny(isShiny);
        if (patch.IsAlpha is { } isAlpha) ApplyIsAlpha(isAlpha);
        if (patch.TrainerId is not null || patch.SecretId is not null) ApplyTrainerIds(patch.TrainerId ?? Pkm.DisplayTID, patch.SecretId ?? Pkm.DisplaySID);
        if (patch.OriginalTrainerName is { } originalTrainerName) ApplyOriginalTrainerName(originalTrainerName);
        if (patch.OriginalTrainerGender is { } originalTrainerGender)
            ApplyTrainerGender(originalTrainerGender, nameof(PokemonPatch.OriginalTrainerGender), "original trainer", value => Pkm.OriginalTrainerGender = value, () => Pkm.OriginalTrainerGender);
        if (patch.HandlingTrainerName is { } handlingTrainerName) ApplyHandlingTrainerName(handlingTrainerName);
        if (patch.HandlingTrainerGender is { } handlingTrainerGender)
            ApplyTrainerGender(handlingTrainerGender, nameof(PokemonPatch.HandlingTrainerGender), "handling trainer", value => Pkm.HandlingTrainerGender = value, () => Pkm.HandlingTrainerGender);
        if (patch.CurrentHandler is { } currentHandler) ApplyCurrentHandler(currentHandler);
        if (patch.Version is { } version) ApplyVersion(version);
        if (patch.MetLocation is { } metLocation) ApplyMetLocation(metLocation);
        if (patch.MetLevel is { } metLevel) ApplyMetLevel(metLevel);
        if (patch.MetDate is { } metDate) ApplyMetDate(metDate);
        if (patch.FatefulEncounter is { } fatefulEncounter) ApplyFatefulEncounter(fatefulEncounter);
    }

    private void ApplySpecies(int id)
    {
        var species = Game.SpeciesRepository.AllGameSpecies.FirstOrDefault(s => s.Id == id && SpeciesDefinition.IsSome(s));
        Require(species is not null, nameof(PokemonPatch.Species), $"Species {id} isn't in this game.");
        Species = species!;
    }

    private void ApplyForm(int id)
    {
        var forms = Form.HasForm ? FormRepository.GetFor(Pkm).Count() : 1;
        Require(id >= 0 && id < forms, nameof(PokemonPatch.Form), $"Form {id} doesn't exist for {Species.Name}.");
        Pkm.Form = (byte)id;
        Require(Pkm.Form == id, nameof(PokemonPatch.Form), $"Form {id} can't be stored for {Species.Name} in this game.");
    }

    private void ApplyGender(Gender gender)
    {
        Require(gender.Id is >= 0 and <= 2, nameof(PokemonPatch.Gender), $"Gender {gender.Name} isn't a Pokémon gender.");
        var value = gender.ToByte();
        // Up to Gen 5 the gender comes from the PID or IVs, so PKHeX would search forever for a gender the species can't have.
        var derived = Pkm.Format <= 5 && !Pkm.PersonalInfo.IsDualGender && value != Pkm.GetSaneGender();
        Require(!derived, nameof(PokemonPatch.Gender), $"Gender {gender.Name} isn't possible for {Species.Name} in this game.");
        Pkm.SetGender(value);
        Require(Pkm.Gender == value, nameof(PokemonPatch.Gender), $"Gender {gender.Name} can't be stored in this game.");
    }

    private void ApplyNature(int id)
    {
        Require(Game.Options.Natures.Any(n => n.Id == id), nameof(PokemonPatch.Nature), $"Nature {id} isn't in this game.");
        Require(Natures.ChangeAll((Nature)id), nameof(PokemonPatch.Nature),
            $"Nature can't change in Pokémon {Version.Name}, as it is based on the PID.");
    }

    private void ApplyAbility(int id)
    {
        Require(id > 0 && id <= Pkm.MaxAbilityID, nameof(PokemonPatch.Ability), $"Ability {id} isn't in this game.");
        var index = Pkm.PersonalInfo.GetIndexOfAbility(id);
        if (index >= 0) Pkm.RefreshAbility(index);
        else Pkm.Ability = id;
        Require(Pkm.Ability == id, nameof(PokemonPatch.Ability), $"Ability {AbilityRepository.Instance.Get(id).Name} can't be stored for {Species.Name} in this game.");
    }

    private void ApplyHeldItem(int id)
    {
        Require(Game.Options.HeldItems.Any(i => i.Id == id), nameof(PokemonPatch.HeldItem), $"Held item {id} isn't in this game.");
        Pkm.HeldItem = id;
        Require(Pkm.HeldItem == id, nameof(PokemonPatch.HeldItem), $"Held item {id} can't be stored in this game.");
    }

    private void ApplyBall(int id)
    {
        Require(Game.Options.Balls.Any(b => b.Id == id), nameof(PokemonPatch.Ball), $"Ball {id} isn't in this game.");
        Pkm.Ball = (byte)id;
        Require(Pkm.Ball == id, nameof(PokemonPatch.Ball), $"Ball {id} can't be stored in this game.");
    }

    private void ApplyLanguage(int id)
    {
        Require(Game.Options.Languages.Any(l => l.Id == id), nameof(PokemonPatch.Language), $"Language {id} isn't in this game.");
        Pkm.Language = id;
        Require(Pkm.Language == id, nameof(PokemonPatch.Language), $"Language {id} can't be stored in this game.");
    }

    private void ApplyIsEgg(bool isEgg)
    {
        Pkm.IsEgg = isEgg;
        Require(Pkm.IsEgg == isEgg, nameof(PokemonPatch.IsEgg), "Egg status can't change in this game.");
    }

    private void ApplyFriendship(int friendship)
    {
        Require(friendship is >= 0 and <= 255, nameof(PokemonPatch.Friendship), $"Friendship must be between 0 and 255, got {friendship}.");
        Pkm.CurrentFriendship = (byte)friendship;
        Require(Pkm.CurrentFriendship == friendship, nameof(PokemonPatch.Friendship), "Friendship isn't stored in this game.");
    }

    private void ApplyNickname(string nickname)
    {
        Require(nickname.Length <= Pkm.MaxStringLengthNickname, nameof(PokemonPatch.Nickname),
            $"Nickname can have at most {Pkm.MaxStringLengthNickname} characters, got {nickname.Length}.");
        ChangeNickname(nickname);
        Require(nickname.Length == 0 || Pkm.Nickname == nickname, nameof(PokemonPatch.Nickname),
            $"Nickname \"{nickname}\" has characters this game can't store.");
    }

    private void ApplyLevel(int level)
    {
        Require(level is >= 1 and <= 100, nameof(PokemonPatch.Level), $"Level must be between 1 and 100, got {level}.");
        ChangeLevel(level);
    }

    private void ApplyIsShiny(bool isShiny)
    {
        SetShiny(isShiny);
        Require(IsShiny == isShiny, nameof(PokemonPatch.IsShiny), "Shininess can't change for this Pokémon in this game.");
    }

    private void ApplyIsAlpha(bool isAlpha)
    {
        Require(SupportsAlpha, nameof(PokemonPatch.IsAlpha), "Alpha Pokémon aren't in this game.");
        IsAlpha = isAlpha;
    }

    private void ApplyTrainerIds(uint trainerId, uint secretId)
    {
        Pkm.SetDisplayID(trainerId, secretId);
        Require(Pkm.DisplayTID == trainerId, nameof(PokemonPatch.TrainerId), $"Trainer ID {trainerId} doesn't fit this game.");
        Require(Pkm.DisplaySID == secretId, nameof(PokemonPatch.SecretId), $"Secret ID {secretId} doesn't fit this game.");
    }

    private void ApplyOriginalTrainerName(string name)
    {
        Require(name.Length <= Pkm.MaxStringLengthTrainer, nameof(PokemonPatch.OriginalTrainerName),
            $"Original trainer name can have at most {Pkm.MaxStringLengthTrainer} characters, got {name.Length}.");
        Pkm.OriginalTrainerName = name;
        Require(Pkm.OriginalTrainerName == name, nameof(PokemonPatch.OriginalTrainerName),
            $"Original trainer name \"{name}\" has characters this game can't store.");
    }

    private void ApplyHandlingTrainerName(string name)
    {
        Pkm.HandlingTrainerName = name;
        Require(Pkm.HandlingTrainerName == name, nameof(PokemonPatch.HandlingTrainerName),
            $"This game can't store the handling trainer name \"{name}\".");
    }

    private void ApplyTrainerGender(Gender gender, string field, string trainer, Action<byte> set, Func<byte> get)
    {
        Require(gender.Equals(Gender.Male) || gender.Equals(Gender.Female), field, $"Trainer gender must be male or female, got {gender.Name}.");
        set(gender.ToByte());
        Require(get() == gender.ToByte(), field, $"This game can't store a {gender.Name.ToLowerInvariant()} {trainer}.");
    }

    private void ApplyCurrentHandler(Owner.Handler handler)
    {
        Pkm.CurrentHandler = (byte)handler;
        Require(Pkm.CurrentHandler == (byte)handler, nameof(PokemonPatch.CurrentHandler), "This game can't store a handling trainer.");
    }

    private void ApplyVersion(int id)
    {
        Require(Game.Options.OriginGames.Any(g => g.Id == id), nameof(PokemonPatch.Version), $"Origin game {id} isn't in this game.");
        var group = GameUtil.GetMetLocationVersionGroup(MetLocationVersion());
        Pkm.Version = (GameVersion)id;
        Require(Pkm.Version == (GameVersion)id, nameof(PokemonPatch.Version), $"Origin game {id} can't be stored in this game.");
        if (GameUtil.GetMetLocationVersionGroup(MetLocationVersion()) != group)
            Pkm.MetLocation = EncounterSuggestion.TryGetSuggestedTransferLocation(Pkm);
    }

    private void ApplyMetLocation(int id)
    {
        Require(MetLocationChoices().Any(l => l.Id == id), nameof(PokemonPatch.MetLocation), $"Met location {id} isn't in {Version.Name}.");
        Pkm.MetLocation = (ushort)id;
        Require(Pkm.MetLocation == id, nameof(PokemonPatch.MetLocation), $"Met location {id} can't be stored in this game.");
    }

    private void ApplyMetLevel(int level)
    {
        Require(level is >= 0 and <= 100, nameof(PokemonPatch.MetLevel), $"Met level must be between 0 and 100, got {level}.");
        Pkm.MetLevel = (byte)level;
        Require(Pkm.MetLevel == level, nameof(PokemonPatch.MetLevel), "Met level isn't stored in this game.");
    }

    private void ApplyMetDate(DateOnly date)
    {
        Require(date.Year is >= 2000 and <= 2099, nameof(PokemonPatch.MetDate), $"Met date must be between 2000 and 2099, got {date:yyyy-MM-dd}.");
        Pkm.MetDate = date;
        Require(Pkm.MetDate == date, nameof(PokemonPatch.MetDate), "Met date isn't stored in this game.");
    }

    private void ApplyFatefulEncounter(bool fatefulEncounter)
    {
        Pkm.FatefulEncounter = fatefulEncounter;
        Require(Pkm.FatefulEncounter == fatefulEncounter, nameof(PokemonPatch.FatefulEncounter), "Fateful encounters aren't stored in this game.");
    }

    private static void Require(bool condition, string field, string message)
    {
        if (!condition) throw new InvalidPatchException(field, message);
    }

    private Choice[] SpeciesChoices() => Game.SpeciesRepository.GetEvolutionsFrom(Species)
        .Where(SpeciesDefinition.IsSome)
        .Append(Species)
        .DistinctBy(species => species.Id)
        .Select(species => new Choice(species.Id, species.Name))
        .ToArray();

    private Choice[] AbilityChoices()
    {
        if (Pkm.MaxAbilityID <= 0) return [];

        var personal = Pkm.PersonalInfo;
        // Gen 3 stores only which of the species' abilities is active; later games store any ability.
        IEnumerable<int> others = Pkm.Format >= 4 ? Enumerable.Range(1, Pkm.MaxAbilityID).OrderBy(id => AbilityRepository.Instance.Get(id).Name) : [];
        return Enumerable.Range(0, personal.AbilityCount)
            .Select(personal.GetAbilityAtIndex)
            .Append(Pkm.Ability)
            .Concat(others)
            .Where(id => id > 0)
            .Distinct()
            .Select(id => new Choice(id, AbilityRepository.Instance.Get(id).Name))
            .ToArray();
    }

    private Choice[] FormChoices() => Form.HasForm
        ? FormRepository.GetFor(Pkm).Select(form => new Choice(form.Id, form.Name)).ToArray()
        : [];

    private Choice[] MetLocationChoices() => Pkm.Format <= 1
        ? []
        : GameInfo.GetLocationList(MetLocationVersion(), Pkm.Context)
            .DistinctBy(location => location.Value)
            .Select(location => new Choice(location.Value, location.Text))
            .ToArray();

    // Mirrors PKHeX's editor: an origin game without its own location list borrows the save's, then the format's.
    private GameVersion MetLocationVersion()
    {
        if (GameUtil.GetMetLocationVersionGroup(Pkm.Version) is not GameVersion.Invalid) return Pkm.Version;

        var version = Game.SaveFile.Version;
        return GameUtil.GetMetLocationVersionGroup(version) is GameVersion.Invalid || version is GameVersion.Any
            ? Pkm.Context.GetSingleGameVersion()
            : version;
    }
}
