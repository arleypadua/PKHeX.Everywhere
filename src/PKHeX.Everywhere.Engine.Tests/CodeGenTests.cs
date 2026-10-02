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
        Client.Should().Contain("load(data: Binary, fileName?: string | null): Promise<void>")
            .And.Contain("addFromFile(bytes: Binary): Promise<AddedPokemon>")
            .And.Contain("isSupported(assembly: Binary): Promise<boolean>")
            .And.Contain("register(assembly: Binary, stored: PlugInState | null): Promise<InstalledPlugIn>")
            .And.Contain("load: async (data, fileName) => invoke('game.load', [await toBase64(data), fileName ?? fileNameOf(data)]),");
    }

    [Fact]
    public void ReturnedBytesAreUint8Arrays()
    {
        Types.Should().Contain("export interface ExportedSave {\n  bytes: Uint8Array<ArrayBuffer>\n")
            .And.Contain("export interface ExportedPokemon {\n  bytes: Uint8Array<ArrayBuffer>\n")
            .And.Contain("export interface LoadedSave {\n  bytes: Uint8Array<ArrayBuffer>\n");
        Client.Should().Contain("export: async () => withBytes(await invoke<ExportedSave>('game.export', []), ['bytes']),")
            .And.Contain("file: async () => withBytes(await invoke<LoadedSave | null>('game.file', []), ['bytes']),");
    }

    [Fact]
    public void BytesNestedInAnInputStayBase64()
    {
        Types.Should().Contain("  file?: Base64 | null\n").And.Contain("export type Base64 = string\n");
    }
}
