using System.Reflection;
using AwesomeAssertions;
using PKHeX.Core;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Facade;
using PKHeX.Facade.Events;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;

namespace PKHeX.Everywhere.Engine.Tests;

public class CommandTopicTests
{
    private static readonly Dictionary<string, Func<Game, string, IEnumerable<string>>> SampleArgs = new()
    {
        ["box.addEncounter"] = (game, _) => game.Supports(Capability.Encounters) ? [Args(0)] : [],
        // A hack Pokémon's file is in the hack's format, which PKHeX can't read back.
        ["box.addFromFile"] = (game, saveFile) => saveFile is SaveFilePath.Unbound or SaveFilePath.RadicalRed or SaveFilePath.Imperium
            ? []
            : [Args(Convert.ToBase64String(game.Trainer.Party.Pokemons[0].ToFile().Bytes))],
        ["box.get"] = (_, _) => ["[]"],
        ["box.showdown"] = (_, _) => ["[]"],
        ["catalog.names"] = (_, _) => [Args(new { speciesIds = new[] { 25 }, itemIds = new[] { 1 } })],
        ["encounters.search"] = (game, _) => [Args(game.GameVersionApproximation.Id, (int)Species.Abra)],
        ["encounters.versions"] = (_, _) => ["[]"],
        ["events.flag"] = (game, _) => Flags(game).Select(f => Args(f.Index)),
        ["events.get"] = (_, _) => ["[]"],
        ["events.giveTickets"] = (game, _) => game.Events?.Gen3 is null ? [] : [Args(true)],
        ["events.setFlag"] = (game, _) => Flags(game).Select(f => Args(f.Index, !f.Value)),
        ["events.setWork"] = (game, _) => game.Events?.Work.LastOrDefault() is { } work ? [Args(work.Index, work.Value == 7 ? 8 : 7)] : [],
        ["game.blankVersions"] = (_, _) => ["[]"],
        ["game.formats"] = (_, _) => ["[]"],
        ["game.enableFormat"] = (_, _) => [Args("unbound")],
        ["game.natures"] = (_, _) => ["[]"],
        ["game.balls"] = (_, _) => ["[]"],
        ["game.languages"] = (_, _) => ["[]"],
        ["game.heldItems"] = (_, _) => ["[]"],
        ["game.originGames"] = (_, _) => ["[]"],
        ["game.moves"] = (_, _) => ["[]"],
        ["game.export"] = (_, _) => ["[]"],
        ["game.file"] = (_, _) => ["[]"],
        ["game.get"] = (_, _) => ["[]"],
        ["game.version"] = (_, _) => ["[]"],
        ["game.load"] = (_, saveFile) =>
        [
            Args(Convert.ToBase64String(File.ReadAllBytes(saveFile == SaveFilePath.HgSs ? SaveFilePath.Emerald : SaveFilePath.HgSs)), "other.sav", null!),
        ],
        ["game.loadBlank"] = (game, _) => [Args((int)(game.SaveVersion.Version == GameVersion.SW ? GameVersion.SH : GameVersion.SW))],
        ["game.close"] = (_, _) => ["[]"],
        ["inventory.get"] = (_, _) => ["[]"],
        ["inventory.setItem"] = (game, _) => SetItems(game),
        ["party.get"] = (_, _) => ["[]"],
        ["party.showdown"] = (_, _) => ["[]"],
        ["pokemon.get"] = (game, _) => PokemonsAndDraft(game).Select(p => Args(p.At)),
        ["pokemon.showdown"] = (game, _) => PokemonsAndDraft(game).Select(p => Args(p.At)),
        ["pokemon.details"] = (game, _) => PokemonsAndDraft(game).Select(p => Args(p.At)),
        ["pokemon.options"] = (game, _) => PokemonsAndDraft(game).Select(p => Args(p.At)),
        ["pokemon.export"] = (game, _) => PokemonsAndDraft(game).Select(p => Args(p.At)),
        ["pokemon.setLevel"] = (game, _) => PokemonsAndDraft(game).Select(p => Args(p.At, p.Level == 50 ? 51 : 50)),
        ["pokemon.update"] = (game, _) => PokemonsAndDraft(game).Select(p => Args(p.At, new { nickname = "Sparky", level = p.Level == 50 ? 51 : 50, friendship = 1 })),
        ["pokemon.edit"] = (game, _) => Pokemons(game).Select(p => Args(p.At)),
        ["pokemon.commit"] = (_, _) => ["[]"],
        ["pokemon.clone"] = (game, _) => Pokemons(game).Select(p => Args(p.At)),
        ["pokemon.addToBox"] = (_, _) => ["[]"],
        ["species.list"] = (_, _) => ["[]"],
        ["trainer.get"] = (_, _) => ["[]"],
        ["trainer.setName"] = (_, _) => [Args("Ash")],
        ["trainer.setGender"] = (game, _) => [Args(game.Trainer.Gender == PKHeX.Facade.Gender.Female ? "male" : "female")],
        ["trainer.setMoney"] = (game, _) => game.Trainer.Money.IsSupported ? [Args(game.Trainer.Money.Amount == 1234 ? 4321 : 1234)] : [],
        ["trainer.setBattlePoints"] = (game, _) => game.BattlePoints.IsSupported(out var supported) ? [Args(supported.BattlePoints == 1 ? 2 : 1)] : [],
    };

    private static readonly (string Name, string[] Topics)[] Queries = Handlers<QueryAttribute>()
        .Select(m => (m.Attribute.Name, m.Attribute.Topics))
        .ToArray();

    public static TheoryData<string, string> Commands()
    {
        var data = new TheoryData<string, string>();
        foreach (var saveFile in new SupportedSaveFilesAttribute().GetData(null!).Select(d => (string)d[0]))
        foreach (var (command, _) in Handlers<CommandAttribute>())
            data.Add(saveFile, command.Name);
        return data;
    }

    [Fact]
    public void EveryCallHasSampleArguments() =>
        SampleArgs.Keys.Should().BeEquivalentTo(Queries.Select(q => q.Name).Concat(Handlers<CommandAttribute>().Select(c => c.Attribute.Name)));

    [Theory]
    [MemberData(nameof(Commands))]
    public void CommandsOnlyChangeDataUnderTheTopicsTheyReport(string saveFile, string command)
    {
        foreach (var args in SampleArgs[command](SaveFilePath.Load(saveFile), saveFile))
        {
            var session = new Session();
            session.Load(SaveFilePath.Load(saveFile), saveFile);
            OpenDraft(session);
            var queries = Queries
                .SelectMany(q => SampleArgs[q.Name](session.Game!, saveFile).Select(a => (Call: q.Name, Args: a, q.Topics)))
                .ToList();
            var before = queries.Select(q => Dispatch(session, q.Call, q.Args)).ToList();
            var reported = new List<string>();
            session.Changed += reported.AddRange;

            EngineResults.Value(Dispatch(session, command, args));

            for (var i = 0; i < queries.Count; i++)
            {
                var (call, queryArgs, topics) = queries[i];
                if (Dispatch(session, call, queryArgs) == before[i]) continue;

                Affects(reported, topics).Should().BeTrue(
                    $"{command}{args} changed {call}{queryArgs}, which reads [{string.Join(", ", topics)}], but reported only [{string.Join(", ", reported)}]");
            }
        }
    }

    // A draft is open before each command, so the draft calls have one to work on and the other commands show they leave it alone.
    private static void OpenDraft(Session session)
    {
        EngineResults.Value(Dispatch(session, "pokemon.edit", Args(PokemonHandle.Party(0))));
        EngineResults.Value(Dispatch(session, "pokemon.update", Args(PokemonHandle.Draft(), new { nickname = "Drafty" })));
    }

    private static IEnumerable<(PokemonHandle At, int Level)> PokemonsAndDraft(Game game) =>
        Pokemons(game).Append((PokemonHandle.Draft(), game.Trainer.Party.Pokemons[0].Level));

    private static IEnumerable<(PokemonHandle At, int Level)> Pokemons(Game game)
    {
        yield return (PokemonHandle.Party(0), game.Trainer.Party.Pokemons[0].Level);
        if (FirstBoxPokemon(game) is var (at, index)) yield return (at, game.Trainer.PokemonBox.All[index].Level);
    }

    private static IEnumerable<string> SetItems(Game game)
    {
        if (AddableItem(game) is var (at, maxCount)) yield return Args(at, maxCount);
        if (OwnedItem(game) is { } owned) yield return Args(owned, 0);
        if (game.Events?.Gen3?.Tickets.Missing.FirstOrDefault() is { } ticket) yield return Args(new ItemHandle("KeyItems", ticket.Id), 1);
    }

    private static IEnumerable<EventFlagEntry> Flags(Game game) =>
        game.Events is { } events ? [events.Flags.Last(), .. events.Gen3?.Islands.Take(1) ?? []] : [];

    private static bool Affects(IEnumerable<string> changed, IEnumerable<string> read) =>
        changed.Any(c => read.Any(r => Covers(c, r) || Covers(r, c)));

    private static bool Covers(string path, string other) =>
        path == Topics.All || other == path || other.StartsWith(path + "/");

    private static IEnumerable<(T Attribute, MethodInfo Method)> Handlers<T>() where T : Attribute =>
        typeof(Session).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Select(m => (Attribute: m.GetCustomAttribute<T>()!, Method: m))
            .Where(m => m.Attribute is not null);
}
