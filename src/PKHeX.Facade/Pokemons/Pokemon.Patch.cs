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
        this.LegalityReport());

    public PokemonOptions Options() => new(SpeciesChoices(), AbilityChoices(), FormChoices());

    /// <summary>
    /// Applies the patch in a fixed order, species before form before ability, so each field is checked against the ones before it.
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
        return Enumerable.Range(0, personal.AbilityCount)
            .Select(personal.GetAbilityAtIndex)
            .Append(Pkm.Ability)
            .Where(id => id > 0)
            .Distinct()
            .Select(id => new Choice(id, AbilityRepository.Instance.Get(id).Name))
            .ToArray();
    }

    private Choice[] FormChoices() => Form.HasForm
        ? FormRepository.GetFor(Pkm).Select(form => new Choice(form.Id, form.Name)).ToArray()
        : [];
}
