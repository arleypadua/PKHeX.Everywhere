using PKHeX.Core;
using PKHeX.Facade.Pokemons;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade;

public record Team(int Number, string? Name, string? Cup, int Slots, IReadOnlyList<Pokemon> Members);

public class Teams
{
    public const int SlotsPerTeam = 6;

    private readonly Game _game;
    private readonly SAV_STADIUM _save;
    private readonly StadiumTeamLayout _layout;

    private Teams(Game game, SAV_STADIUM save, StadiumTeamLayout layout)
    {
        _game = game;
        _save = save;
        _layout = layout;
    }

    internal static Teams? Of(Game game) => game.SaveFile is SAV_STADIUM save ? new Teams(game, save, StadiumTeamLayout.Of(save)) : null;

    public int Count => _layout.Count;

    public IReadOnlyList<Team> All => Enumerable.Range(0, Count).Select(Get).ToList();

    public Team Get(int number)
    {
        RequireTeam(number);
        var block = Block(number);
        var name = _save.GetString(block.Slice(_layout.NameAt, _layout.NameLength));
        return new Team(
            number,
            string.IsNullOrWhiteSpace(name) ? null : name,
            _layout.CupOf(number),
            SlotsPerTeam,
            Members(number).Select(pkm => new Pokemon(pkm, _game)).ToList());
    }

    // Returns the slot the copy lands in: the given one when it holds a member, else the one after the last member.
    public int Place(int number, int slot, Pokemon pokemon)
    {
        RequireSlot(number, slot);
        var members = Members(number);
        var at = Math.Min(slot, members.Count);
        if (at == members.Count) members.Add(pokemon.Pkm.Clone());
        else members[at] = pokemon.Pkm.Clone();

        Write(number, members);
        return at;
    }

    public void Replace(int number, int slot, Pokemon pokemon)
    {
        var members = Members(number);
        if (slot < 0 || slot >= members.Count) throw new ArgumentOutOfRangeException(nameof(slot), $"Team {number} has no Pokémon in slot {slot}.");

        members[slot] = pokemon.Pkm.Clone();
        Write(number, members);
    }

    public void Clear(int number, int slot)
    {
        RequireSlot(number, slot);
        var members = Members(number);
        if (slot >= members.Count) return;

        members.RemoveAt(slot);
        Write(number, members);
    }

    private List<PKM> Members(int number)
    {
        RequireTeam(number);
        var block = Block(number);
        var members = new List<PKM>();
        for (var slot = 0; slot < SlotsPerTeam; slot++)
            if (SlotOf(block, slot) is var data && data[0] != 0)
                members.Add(_save.GetStoredSlot(data));
        return members;
    }

    private void Write(int number, List<PKM> members)
    {
        var block = Block(number);
        for (var slot = 0; slot < SlotsPerTeam; slot++)
        {
            var data = SlotOf(block, slot);
            if (slot < members.Count) WriteSlot(members[slot], data);
            else data.Clear();
        }

        if (_layout.CountAt is { } countAt) block[countAt] = (byte)members.Count;
        WriteUInt32LittleEndian(block[^6..], _layout.Magic);
        WriteUInt16BigEndian(block[^2..], Checksums.CheckSum16(block[..^2]));
    }

    // PKHeX writes a Pocket Monsters Stadium slot as a one-Pokémon list, which overflows the slot, so it gets the layout PKHeX reads.
    private void WriteSlot(PKM pkm, Span<byte> data)
    {
        if (_save is not SAV1StadiumJ || pkm is not PK1 pk1)
        {
            _save.SetSlotFormatStored(pkm, data, EntityImportSettings.None);
            return;
        }

        const int storedSize = 0x21;
        const int nameLength = 6;
        pk1.Stat_LevelBox = pk1.CurrentLevel;
        pk1.Data[..storedSize].CopyTo(data);
        pk1.NicknameTrash[..nameLength].CopyTo(data[storedSize..]);
        pk1.OriginalTrainerTrash[..nameLength].CopyTo(data[(storedSize + nameLength)..]);
    }

    private Span<byte> Block(int number) => _save.Data.Slice(_layout.Offset(_save, number), _layout.Size);

    private Span<byte> SlotOf(Span<byte> block, int slot) => block.Slice(_layout.HeaderSize + slot * _save.SIZE_STORED, _save.SIZE_STORED);

    private void RequireTeam(int number)
    {
        if (number < 0 || number >= Count) throw new ArgumentOutOfRangeException(nameof(number), $"There is no team {number}.");
    }

    private void RequireSlot(int number, int slot)
    {
        RequireTeam(number);
        if (slot is < 0 or >= SlotsPerTeam) throw new ArgumentOutOfRangeException(nameof(slot), $"A team has no slot {slot}.");
    }
}
