using AwesomeAssertions;
using PKHeX.Everywhere.Engine.CodeGen;
using PKHeX.Everywhere.Engine.PlugIns;

namespace PKHeX.Everywhere.Engine.Tests;

public class CodeGenTests
{
    private static readonly Contract Contract = Contract.Read(typeof(Session).Assembly, [typeof(PlugInHandlers).Assembly]);
    private static readonly Dictionary<string, string> Files = TypeScript.Write(Contract).ToDictionary(f => f.File, f => f.Content);

    private static string Client => Files["engine/src/generated/client.ts"];
    private static string Types => Files["engine/src/generated/types.ts"];
    private static string Hooks => Files["react/src/generated/hooks.ts"];

    [Theory]
    [InlineData("game.get", false, false)]
    [InlineData("party.get", true, false)]
    [InlineData("pokemon.get", true, false)]
    [InlineData("pokemon.commit", true, true)]
    [InlineData("pokemon.addToBox", true, true)]
    public void ReadsRequirementsFromInjectedParametersAndTheRequiresMarker(string call, bool save, bool draft) =>
        Contract.Calls.Single(c => c.Name == call).Should().Match<Call>(c => c.RequiresSave == save && c.RequiresDraft == draft);

    [Fact]
    public void ClientMethodsListTheirRequirements()
    {
        Client.Should().Contain("""
                /** Requires a loaded save; throws `no-save` otherwise. */
                get(): Promise<PokemonSummary[]>
            """.ReplaceLineEndings("\n"));
        Client.Should().Contain("""
                /** Requires a loaded save and an open draft; throws `no-save` or `no-draft` otherwise. */
                commit(): Promise<PokemonId>
            """.ReplaceLineEndings("\n"));
        Client.Should().NotContain("*/\n    blankVersions()");
    }

    [Fact]
    public void ClientMethodsCarryTheHandlerDocs()
    {
        Client.Should().Contain("""
                /** Lists the save formats PKHeX doesn't know, such as ROM hacks, that `game.load()` can load a save with. */
                formats(): Promise<FormatEntry[]>
            """.ReplaceLineEndings("\n"));
        Client.Should().MatchRegex(@"\* @param formatId The id of a save format from `game.formats\(\)` to load the save with, skipping detection\. [^\n]+\n\s+\*/\n\s+load\(");
    }

    [Fact]
    public void HookCommandsCarryTheHandlerDocs() =>
        Hooks.Should().MatchRegex(@"\* @param formatId [^\n]+\n\s+\*/\n\s+load: \(");

    [Fact]
    public void HooksListTheirRequirements()
    {
        Hooks.Should().Contain("""
            /** Requires a loaded save; throws `no-save` otherwise. Wrap in `<RequireGame>`. */
            export function useParty() {
            """.ReplaceLineEndings("\n"));
        Hooks.Should().Contain("""
                  /** Requires a loaded save; throws `no-save` otherwise. Wrap in `<RequireGame>`. */
                  export: () => engine.game.export(),
            """.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void BinaryParametersTakeAnyBinaryInput()
    {
        Client.Should().Contain("load(data: Binary, fileName?: string | null, formatId?: string | null): Promise<void>")
            .And.Contain("addFromFile(bytes: Binary, options?: AddFromFileOptions | null): Promise<AddedPokemon>")
            .And.Contain("isSupported(assembly: Binary): Promise<boolean>")
            .And.Contain("register(assembly: Binary, stored: PlugInState | null): Promise<InstalledPlugIn>")
            .And.Contain("load: async (data, fileName, formatId) => invoke('game.load', [await toBase64(data), fileName ?? fileNameOf(data), formatId]),");
    }

    [Fact]
    public void ReturnedBytesAreUint8Arrays()
    {
        foreach (var type in new[] { "ExportedSave", "ExportedPokemon", "LoadedSave" })
            Types.Should().MatchRegex($@"export interface {type} {{\n(  /\*\*.*\*/\n)?  bytes: Uint8Array<ArrayBuffer>\n");
        Client.Should().Contain("export: async () => withBytes(await invoke<ExportedSave>('game.export', []), ['bytes']),")
            .And.Contain("file: async () => withBytes(await invoke<LoadedSave | null>('game.file', []), ['bytes']),");
    }

    [Fact]
    public void BytesInARecordAReturnedRecordHoldsAreUint8Arrays() =>
        Client.Should().Contain("commit: async (offer) => withBytes(await invoke<TransferResult>('transfer.commit', [offer]), ['save.bytes', 'partner.bytes']),");

    [Fact]
    public void BytesNestedInAnInputStayBase64()
    {
        Types.Should().Contain("  file?: Base64 | null\n").And.Contain("export type Base64 = string\n");
    }

    [Fact]
    public void XmlDocsBecomeTsDoc()
    {
        Types.Should().Contain("/** A page an enabled plug-in declares, as listed by `plugins.pages`. */\nexport interface DeclaredPage {")
            .And.Contain("  /** The plug-in's id, its assembly name. */\n  plugInId: string\n");
    }
}
