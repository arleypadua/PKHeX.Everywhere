using PKHeX.CLI.Base;
using PKHeX.CLI.Extensions;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using Spectre.Console;

namespace PKHeX.CLI.Commands.EditPokemonCommand;

abstract class EditPokemonAttribute(Pokemon pokemon)
{
    protected Pokemon Pokemon { get; } = pokemon;
    protected abstract string Label { get; }
    protected abstract string Value { get; }

    public virtual string Display => $"[yellow]{Label}:[/] {Value}";
    public virtual bool Hidden => false;

    public virtual Result HandleSelection() => Result.Continue;

    internal abstract class SimpleAttribute(Pokemon pokemon, Func<string> label, Func<string> value)
        : EditPokemonAttribute(pokemon)
    {
        protected SimpleAttribute(Pokemon pokemon, string label, string value)
            : this(pokemon, () => label, () => value)
        {
        }

        protected SimpleAttribute(Pokemon pokemon, string label, Func<string> value)
            : this(pokemon, () => label, value)
        {
        }

        protected override string Label => label();
        protected override string Value => value();
    }

    internal class ReadOnlyAttribute(Pokemon pokemon, string label, Func<string> value)
        : SimpleAttribute(pokemon, label, value)
    {
        public ReadOnlyAttribute(Pokemon pokemon, string label, string value)
            : this(pokemon, label, () => value)
        {
        }
    }

    public class Name(Pokemon pokemon) : SimpleAttribute(pokemon, "Name/Nickname", () => pokemon.Nickname)
    {
        public override Result HandleSelection()
        {
            var newName = AnsiConsole.Ask(Label, Pokemon.Nickname);
            newName = newName == Pokemon.NameDisplay()
                ? Pokemon.NameDisplay()
                : newName;

            Pokemon.ChangeNickname(newName);

            return Result.Continue;
        }
    }

    public class Level(Pokemon pokemon) : SimpleAttribute(pokemon, "Level", () => pokemon.Level.ToString())
    {
        public override Result HandleSelection()
        {
            var newLevelString = AnsiConsole.Ask(Label, Pokemon.Level.ToString());
            var parsed = int.TryParse(newLevelString, out int level);
            if (!parsed) return Result.Continue;

            Pokemon.ChangeLevel(level);

            return Result.Continue;
        }
    }

    public class Nature(Pokemon pokemon) : EditPokemonAttribute(pokemon)
    {
        protected override string Label => string.Empty;
        protected override string Value => string.Empty;

        public override string Display =>
            $"[yellow]Nature:[/] {Pokemon.Natures.Nature,-15} [yellow]Stat Nature:[/] {Pokemon.Natures.StatNature}";
    }

    public class Flags(Pokemon pokemon) : EditPokemonAttribute(pokemon)
    {
        protected override string Label => string.Empty;
        protected override string Value => string.Empty;

        public override string Display => $"[yellow]Is Egg:[/] {Pokemon.Flags.IsEgg.ToDisplayEmoji(),-3} " +
                                          $"[yellow]Infected:[/] {Pokemon.Flags.IsInfected.ToDisplayEmoji(),-3} " +
                                          $"[yellow]Cured:[/] {Pokemon.Flags.IsCured.ToDisplayEmoji()}";
    }

    public class IsShiny(Pokemon pokemon)
        : SimpleAttribute(pokemon, "IsShiny", () => YesNoPrompt.LabelFrom(pokemon.IsShiny))
    {
        public override Result HandleSelection()
        {
            var result = YesNoPrompt.AskOrDefault("Is Shiny", Pokemon.IsShiny);
            Pokemon.SetShiny(result);

            return Result.Continue;
        }
    }

    public abstract class PokemonStatsBase(Pokemon pokemon, string label, Pokemon.Stats stats)
        : SimpleAttribute(pokemon, label, string.Empty)
    {
        public override string Display => $"[yellow]{Label}:[/]{Environment.NewLine}   " +
                                          $"HP {stats.Health,-3} " +
                                          $"Atk {stats.Attack,-3} " +
                                          $"Def {stats.Defense,-3} " +
                                          $"SpA {stats.SpecialAttack,-3} " +
                                          $"SpD {stats.SpecialDefense,-3} " +
                                          $"Spe {stats.Speed,-3} " +
                                          $"Total {stats.Total,-3} ";
    }

    public class EV(Pokemon pokemon) : PokemonStatsBase(pokemon, "EV", pokemon.EVs);

    public class IV(Pokemon pokemon) : PokemonStatsBase(pokemon, "IV", pokemon.IVs);

    public class BaseStats(Pokemon pokemon) : PokemonStatsBase(pokemon, "Base", pokemon.BaseStats);
}