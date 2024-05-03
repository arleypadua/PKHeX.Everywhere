using PKHeX.Core;

namespace PKHeX.Facade.Repositories;

public class MoveRepository
{
    private readonly Dictionary<ushort, MoveDefinition> _moves;

    public MoveRepository()
    {
        _moves = GameInfo.Strings.movelist
            .Select((moveName, id) => (id: Convert.ToUInt16(id), moveName))
            .ToDictionary(x => Convert.ToUInt16(x.id), x => new MoveDefinition(Convert.ToUInt16(x.id), x.moveName));
    }

    public MoveDefinition GetMove(ushort id) => _moves[id];
}

public record MoveDefinition(ushort Id, string Name)
{
    public static int None = 0;
};