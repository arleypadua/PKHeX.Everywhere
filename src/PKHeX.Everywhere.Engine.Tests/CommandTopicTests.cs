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
        ["box.addEncounter"] = (_, _) => [Args(0)],
        ["box.addFromFile"] = (game, _) => [Args(Convert.ToBase64String(game.Trainer.Party.Pokemons[0].ToFile().Bytes))],
        ["box.get"] = (_, _) => ["[]"],
        ["box.showdown"] = (_, _) => ["[]"],
        ["encounters.search"] = (game, _) => [Args(game.GameVersionApproximation.Id, (int)Species.Abra)],
        ["encounters.versions"] = (_, _) => ["[]"],
        ["events.flag"] = (game, _) => Flags(game).Select(f => Args(f.Index)),
        ["events.get"] = (_, _) => ["[]"],
        ["events.giveTickets"] = (game, _) => game.Events?.Gen3 is null ? [] : [Args(true)],
        ["events.setFlag"] = (game, _) => Flags(game).Select(f => Args(f.Index, !f.Value)),
        ["events.setWork"] = (game, _) => game.Events?.Work.LastOrDefault() is { } work ? [Args(work.Index, work.Value == 7 ? 8 : 7)] : [],
        ["game.blankVersions"] = (_, _) => ["[]"],
        ["game.export"] = (_, _) => ["[]"],
        ["game.get"] = (_, _) => ["[]"],
        ["game.load"] = (_, saveFile) =>
        [
            Args(Convert.ToBase64String(File.ReadAllBytes(saveFile == SaveFilePath.HgSs ? SaveFilePath.Emerald : SaveFilePath.HgSs)), "other.sav"),
        ],
        ["game.loadBlank"] = (game, _) => [Args((int)(game.SaveVersion.Version == GameVersion.SW ? GameVersion.SH : GameVersion.SW))],
        ["game.close"] = (_, _) => ["[]"],
        ["inventory.get"] = (_, _) => ["[]"],
        ["inventory.setItem"] = (game, _) => SetItems(game),
        ["party.get"] = (_, _) => ["[]"],
        ["party.showdown"] = (_, _) => ["[]"],
        ["pokemon.get"] = (game, _) => Pokemons(game).Select(p => Args(p.At)),
        ["pokemon.showdown"] = (game, _) => Pokemons(game).Select(p => Args(p.At)),
        ["pokemon.setLevel"] = (game, _) => Pokemons(game).Select(p => Args(p.At, p.Level == 50 ? 51 : 50)),
        ["species.list"] = (_, _) => ["[]"],
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
        foreach (var args in SampleArgs[command](Game.LoadFrom(saveFile), saveFile))
        {
            var session = new Session();
            session.Load(Game.LoadFrom(saveFile), saveFile);
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
