using PKHeX.Core;
using PKHeX.Everywhere.RomHacks.Cfru;

namespace PKHeX.Everywhere.RomHacks.Expansion.Imperium;

public sealed class ImperiumPokemon : ExpansionPokemon
{
    internal static readonly CfruSpeciesMap Map = new(ImperiumSpeciesTable.NationalByIndex);
    internal static readonly CfruItemMap Items = new(ImperiumItemTable.ModernByIndex);

    // PKHeX has no Park Ball.
    internal static readonly Ball[] Balls =
    [
        Core.Ball.Strange, Core.Ball.Poke, Core.Ball.Great, Core.Ball.Ultra, Core.Ball.Master, Core.Ball.Premier,
        Core.Ball.Heal, Core.Ball.Net, Core.Ball.Nest, Core.Ball.Dive, Core.Ball.Dusk, Core.Ball.Timer, Core.Ball.Quick,
        Core.Ball.Repeat, Core.Ball.Luxury, Core.Ball.Level, Core.Ball.Lure, Core.Ball.Moon, Core.Ball.Friend,
        Core.Ball.Love, Core.Ball.Fast, Core.Ball.Heavy, Core.Ball.Dream, Core.Ball.Safari, Core.Ball.Sport,
        Core.Ball.None, Core.Ball.Beast, Core.Ball.Cherish,
    ];

    public ImperiumPokemon() { }
    public ImperiumPokemon(Memory<byte> data) : base(data) { }

    protected override CfruSpeciesMap SpeciesMap => Map;
    protected override CfruItemMap ItemMap => Items;
    private protected override MoveMap MoveMap => ImperiumMoveTable.Map;
    protected override int ShinyOdds => 36;
    protected override Ball[] BallOrder => Balls;

    public override ImperiumPokemon Clone() => new(Data.ToArray());
}
