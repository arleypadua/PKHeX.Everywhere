using AwesomeAssertions;
using PKHeX.Everywhere.Engine.CodeGen;
using PKHeX.Everywhere.Engine.Dtos;
using PKHeX.Everywhere.Engine.Tests.Handlers;
using PKHeX.Facade.Tests.Base;
using static PKHeX.Everywhere.Engine.Tests.EngineCalls;
using static PKHeX.Everywhere.Engine.Tests.EngineResults;

namespace PKHeX.Everywhere.Engine.Tests;

public class SecondAssemblyHandlerTests
{
    private static Session WithLeadHandlers(Session session)
    {
        LeadHandlers.AddTo(session);
        return session;
    }

    [Fact]
    public void DispatchesAQueryDeclaredInASecondAssembly()
    {
        var session = WithLeadHandlers(Loaded(SaveFilePath.HgSs));
        var lead = session.Game!.Trainer.Party.Pokemons[0];

        var value = Value(Dispatcher.Dispatch(session, "lead.get", "[]"))!;

        value.ToJsonString().Should().Be(
            $$"""{"at":{"source":"party","slot":0,"box":null},"species":"{{lead.Species.Name}}","level":{{lead.Level}}}""");
    }

    [Fact]
    public void ACallIsUnknownUntilItsAssemblyIsAdded() =>
        Error(Dispatcher.Dispatch(Loaded(SaveFilePath.HgSs), "lead.get", "[]")).Should().Be(ErrorCodes.UnknownCall);

    [Fact]
    public void ReturnsNoSaveWithoutALoadedSave() =>
        Error(Dispatcher.Dispatch(WithLeadHandlers(new Session()), "lead.get", "[]")).Should().Be(ErrorCodes.NoSave);

    [Theory]
    [InlineData("lead.get", "[1]")]
    [InlineData("lead.setLevel", """[{"source":"party","slot":0},"high"]""")]
    public void ReturnsBadArgumentsForMalformedArguments(string call, string args)
    {
        var session = WithLeadHandlers(Loaded(SaveFilePath.HgSs));

        Error(Dispatcher.Dispatch(session, call, args)).Should().Be(ErrorCodes.BadArguments);
    }

    [Fact]
    public void ReturnsTheErrorCodeAHandlerThrows()
    {
        var session = WithLeadHandlers(Loaded(SaveFilePath.HgSs));

        Error(Dispatcher.Dispatch(session, "lead.setLevel", Args(PokemonHandle.Party(0), 101))).Should().Be(ErrorCodes.OutOfRange);
    }

    [Fact]
    public void ACommandInvalidatesTheTopicOfItsHandle()
    {
        var session = WithLeadHandlers(Loaded(SaveFilePath.HgSs));
        var changed = new List<string>();
        session.Changed += changed.AddRange;

        Value(Dispatcher.Dispatch(session, "lead.setLevel", Args(PokemonHandle.Party(0), 42)));

        changed.Should().Equal(Topics.Party);
        Value(Dispatcher.Dispatch(session, "lead.get", "[]"))!["level"]!.GetValue<int>().Should().Be(42);
    }

    [Fact]
    public void TheSdkCoversCallsFromRegisteredAssemblies()
    {
        var contract = Contract.Read(typeof(Session).Assembly, [typeof(LeadHandlers).Assembly]);
        var files = TypeScript.Write(contract).ToDictionary(f => f.File, f => f.Content);

        contract.Calls.Select(c => c.Name).Should().Contain(["party.get", "lead.get", "lead.setLevel"]);
        files["engine/src/generated/types.ts"].Should().Contain("export interface Lead {");
        files["engine/src/generated/client.ts"].Should().Contain("setLevel(at: PokemonHandle, level: number): Promise<void>");
        files["engine/src/generated/topics.ts"].Should().Contain("'lead.get': ['party'],");
        files["react/src/generated/hooks.ts"].Should().Contain("export function useLead() {");
    }

    [Fact]
    public void TheSdkRejectsACallDeclaredInTwoAssemblies()
    {
        var engine = typeof(Session).Assembly;

        var read = () => Contract.Read(engine, [engine]);

        read.Should().Throw<InvalidOperationException>().WithMessage("*is declared more than once.");
    }
}
