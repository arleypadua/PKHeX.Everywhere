using PKHeX.CLI.Base;
using PKHeX.Core;
using PKHeX.Facade;
using PKHeX.Facade.Extensions;
using PKHeX.Facade.Repositories;
using Spectre.Console;

namespace PKHeX.CLI.Commands;

public static class EditPokemon
{
    public static Result Handle(Game game, Pokemon pokemon)
    {
        IEnumerable<PokemonAttribute> attributes =
        [
            new PokemonAttribute.Id(pokemon),
            new PokemonAttribute.Name(pokemon),
            new PokemonAttribute.CapturedWith(pokemon),
            new PokemonAttribute.Level(pokemon),
            new PokemonAttribute.IsShiny(pokemon),
            new PokemonAttribute.EV(pokemon),
            new PokemonAttribute.IV(pokemon),
            new PokemonAttribute.Stats(pokemon),
            new PokemonAttribute.Move(pokemon, PokemonMove.MoveIndex.Move1),
            new PokemonAttribute.Move(pokemon, PokemonMove.MoveIndex.Move2),
            new PokemonAttribute.Move(pokemon, PokemonMove.MoveIndex.Move3),
            new PokemonAttribute.Move(pokemon, PokemonMove.MoveIndex.Move4),
        ];

        RepeatUntilExit(() =>
        {
            var selection = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                .Title($"{Environment.NewLine}[yellow]Editing Pokemon: [yellow]{pokemon.NameDisplay()}[/][/]{Environment.NewLine}")
                .PageSize(10)
                .AddChoices(OptionOrBack.WithValues(
                    options: attributes,
                    display: (attribute) => attribute.Display))
                .WrapAround());
            
            return selection is OptionOrBack.Option<PokemonAttribute> attributeOption 
                ? attributeOption.Value.HandleSelection() 
                : Result.Exit;
        });
        
        return Result.Continue;
    }

    abstract class PokemonAttribute
    {
        protected abstract string Label { get; }
        protected abstract string Value { get; }
        
        public virtual string Display => $"[yellow]{Label}:[/] {Value}";

        public virtual Result HandleSelection() => Result.Continue;
        
        public class Id(Pokemon pokemon) : PokemonAttribute
        {
            protected override string Label => "TID/SID";
            protected override string Value => pokemon.Id.ToString();
        }
        public class Name(Pokemon pokemon) : PokemonAttribute
        {
            protected override string Label => "Name/Nickname";
            protected override string Value => pokemon.Nickname;
            
            public override Result HandleSelection()
            {
                var newName = AnsiConsole.Ask(Label, pokemon.Nickname);
                newName = newName == pokemon.NameDisplay()
                    ? pokemon.NameDisplay()
                    : newName;

                pokemon.ChangeNickname(newName);
                
                return Result.Continue;
            }
        }
        public class Level(Pokemon pokemon) : PokemonAttribute
        {
            protected override string Label => "Level";
            protected override string Value => pokemon.Level.ToString();
            
            public override Result HandleSelection()
            {
                var newLevelString = AnsiConsole.Ask(Label, pokemon.Level.ToString());
                var parsed = int.TryParse(newLevelString, out int level);
                if (!parsed) return Result.Continue;
                
                pokemon.ChangeLevel(level);
                
                return Result.Continue;
            }
        }
        public class IsShiny(Pokemon pokemon) : PokemonAttribute
        {
            protected override string Label => "Shiny";
            protected override string Value => YesNoPrompt.LabelFrom(pokemon.IsShiny);
            
            public override Result HandleSelection()
            {
                var result = YesNoPrompt.AskOrDefault("Is Shiny", pokemon.IsShiny);
                pokemon.SetShiny(result);
                
                return Result.Continue;
            }
        }

        public class EV(Pokemon pokemon) : PokemonAttribute
        {
            protected override string Label => "EV";
            protected override string Value => pokemon.EVs.ToString();
        }
        
        public class IV(Pokemon pokemon) : PokemonAttribute
        {
            protected override string Label => "IV";
            protected override string Value => pokemon.IVs.ToString();
        }
        
        public class Stats(Pokemon pokemon) : PokemonAttribute
        {
            protected override string Label => "ST";
            protected override string Value => pokemon.Status.ToString();
        }
        

        public class CapturedWith(Pokemon pokemon) : PokemonAttribute
        {
            protected override string Label => "Captured with";
            protected override string Value => pokemon.Ball.Name;
            public override Result HandleSelection()
            {
                var balls = BallApplicator
                    .GetLegalBalls(pokemon.Pkm)
                    .Select(b => pokemon.Game.ItemRepository.GetItem((ushort)b));

                var ball = AnsiConsole.Prompt(new SelectionPrompt<OptionOrBack>()
                    .Title(Display)
                    .PageSize(10)
                    .AddChoices(OptionOrBack.WithValues(
                        options: balls,
                        display: ball => ball.Name))
                    .WrapAround());

                if (ball is OptionOrBack.Option<ItemDefinition> selectedBall)
                {
                    pokemon.Ball = selectedBall.Value;
                }
                
                return Result.Continue;
            }
        }

        public class Move(Pokemon pokemon, PokemonMove.MoveIndex moveIndex) : PokemonAttribute
        {
            private readonly PokemonMove _move = pokemon.Moves[moveIndex];
            protected override string Label => $"Move #{(int)moveIndex + 1}";
            protected override string Value => $"{_move.Move.Name,-20}PP: {_move.PP,-10}";
        }
    }
}