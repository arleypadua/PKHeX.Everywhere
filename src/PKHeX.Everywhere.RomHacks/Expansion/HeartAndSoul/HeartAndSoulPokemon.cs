using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;
using PKHeX.Everywhere.RomHacks.Expansion.Imperium;

namespace PKHeX.Everywhere.RomHacks.Expansion.HeartAndSoul;

public sealed class HeartAndSoulPokemon : ExpansionPokemon
{
    internal static readonly CfruSpeciesMap Map = new(HeartAndSoulSpeciesTable.NationalByIndex);
    internal static readonly CfruItemMap Items = new(HeartAndSoulItemTable.ModernByIndex);

    // Imperium's balls and the GS Ball, which PKHeX doesn't have.
    internal static readonly Ball[] Balls = [.. ImperiumPokemon.Balls, Core.Ball.None];

    public HeartAndSoulPokemon() { }
    public HeartAndSoulPokemon(Memory<byte> data) : base(data) { }

    protected override CfruSpeciesMap SpeciesMap => Map;
    protected override CfruItemMap ItemMap => Items;
    private protected override MoveMap MoveMap => HeartAndSoulMoveTable.Map;
    protected override int ShinyOdds => 8;
    protected override Ball[] BallOrder => Balls;

    public override HeartAndSoulPokemon Clone() => new(Data.ToArray());
}
