using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade;

public sealed class MysteryGiftAlbum
{
    private readonly SaveFile _save;

    private MysteryGiftAlbum(SaveFile save) => _save = save;

    internal static MysteryGiftAlbum? Of(Game game) =>
        game.Supports(Capability.Events) && game.SaveFile is SAV4 or SAV5 ? new MysteryGiftAlbum(game.SaveFile) : null;

    private IMysteryGiftFlags Flags => _save is SAV4 sav4 ? sav4.Mystery : Gen5Block((SAV5)_save);

    // PKHeX's blank saves leave the album as plain zeros, which decrypt to noise. An empty album is zeros once decrypted.
    private static MysteryBlock5 Gen5Block(SAV5 sav5)
    {
        var block = sav5.Mystery;
        if (block.Data.ContainsAnyExcept((byte)0)) return block;

        block.ClearReceivedFlags();
        for (var i = 0; i < block.GiftCountMax; i++) block.SetMysteryGift(i, new PGF());
        return block;
    }

    private int SlotCount => _save is SAV5 sav5 ? sav5.Mystery.GiftCountMax : Gen4Cards;

    // Generation 5 has no unlock: the menu shows Mystery Gift once the player has the Pokédex.
    public bool? Unlocked => _save is SAV4 sav4 ? sav4.IsMysteryGiftUnlocked : null;

    public void SetUnlocked(bool value)
    {
        if (_save is not SAV4 sav4) throw new NotSupportedException("Generation 5 games show Mystery Gift once the player has the Pokédex.");
        sav4.IsMysteryGiftUnlocked = value;
    }

    public IReadOnlyList<WonderCard?> Slots => Cards().Select(card => card.IsEmpty ? null : new WonderCard(card.Clone())).ToList();

    public IReadOnlyList<MysteryGiftEvent> Events => MysteryGiftDatabase.ReceivableBy(_save)
        .Select(entry => new MysteryGiftEvent(entry.Id, new WonderCard(entry.Gift), IsReceived(entry.Gift.CardID)))
        .ToList();

    public WonderCard? FindEvent(string id) => MysteryGiftDatabase.ReceivableBy(_save)
        .Where(entry => entry.Id == id)
        .Select(entry => new WonderCard(entry.Gift))
        .FirstOrDefault();

    public WonderCard ReadFile(byte[] bytes)
    {
        var gift = MysteryGift.GetMysteryGift(bytes.ToArray()) switch
        {
            PGF { Data.Length: > PGF.Size } => new WC5Full(bytes.ToArray()).Gift,
            var other => other,
        };

        if (gift is PGT pgt) gift = CardFor(pgt);
        var isCard = gift switch
        {
            null => false,
            PCD pcd => pcd.GiftType != GiftType4.None,
            PGF pgf => pgf.IsEntity || pgf.IsItem || pgf.IsPower,
            _ => true,
        };
        if (!isCard || gift is null)
            throw new WonderCardRefusedException(WonderCardRefusal.NotACard, "The file isn't a Wonder Card.");
        RequireGeneration(gift.Generation);
        return new WonderCard(gift);
    }

    // Returns the slot the card lands in: the given one when it holds a card, else the first empty one, so the album never has a gap.
    public int Insert(WonderCard card, int slot)
    {
        RequireSlot(slot);
        RequireGeneration(card.Generation);
        if (card.Gift is PCD { CanConvertToPGT: false })
            throw new WonderCardRefusedException(WonderCardRefusal.NotForTheAlbum, "The Lock Capsule card doesn't go in the album.");

        var at = Math.Min(slot, FilledCount(Cards()));
        if (_save is SAV4 sav4) InsertGen4(sav4, (PCD)card.Gift.Clone(), at);
        else InsertGen5((SAV5)_save, (PGF)card.Gift.Clone(), at);

        if (card.CardId > 0 && card.CardId < Flags.MysteryGiftReceivedFlagMax) Flags.SetMysteryGiftReceivedFlag(card.CardId, true);
        return at;
    }

    public void Remove(int slot)
    {
        RequireSlot(slot);
        var cards = Cards();
        if (cards[slot].IsEmpty) return;

        if (_save is SAV4 sav4) RemoveGen4(sav4, slot);
        else RemoveGen5((SAV5)_save, cards, slot);
    }

    public bool IsReceived(int cardId)
    {
        RequireCardId(cardId);
        return Flags.GetMysteryGiftReceivedFlag(cardId);
    }

    public void SetReceived(int cardId, bool value)
    {
        RequireCardId(cardId);
        Flags.SetMysteryGiftReceivedFlag(cardId, value);
    }

    private const int Gen4Cards = 3;
    private const int Gen4Queue = 8;

    private void InsertGen4(SAV4 sav4, PCD card, int at)
    {
        var block = sav4.Mystery;
        var (queue, cards) = Gen4Contents(block);
        Dequeue(queue, cards[at]);

        card.Gift.VerifyGiftEncryption();
        cards[at] = card;
        var free = Array.FindIndex(queue, gift => gift.IsEmpty);
        if (free < 0)
            throw new WonderCardRefusedException(WonderCardRefusal.DeliveryQueueFull, "Eight gifts are already waiting for the delivery person.");
        queue[free] = card.Gift.Clone();

        WriteGen4(sav4, queue, cards);
    }

    private void RemoveGen4(SAV4 sav4, int slot)
    {
        var (queue, cards) = Gen4Contents(sav4.Mystery);
        Dequeue(queue, cards[slot]);
        WriteGen4(sav4, queue, [.. cards.Where((_, i) => i != slot), new PCD()]);
    }

    private static (PGT[] Queue, PCD[] Cards) Gen4Contents(MysteryBlock4 block) => (
        Enumerable.Range(0, Gen4Queue).Select(block.GetMysteryGiftPGT).ToArray(),
        Enumerable.Range(0, Gen4Cards).Select(block.GetMysteryGiftPCD).ToArray());

    private static void Dequeue(PGT[] queue, PCD card)
    {
        if (card.IsEmpty) return;
        var waiting = Array.FindIndex(queue, card.GiftEquals);
        if (waiting >= 0) queue[waiting] = new PGT();
    }

    private static void WriteGen4(SAV4 sav4, PGT[] queue, PCD[] cards)
    {
        var block = sav4.Mystery;
        MysteryBlock4.UpdateSlotPGT(sav4 is SAV4HGSS, queue, cards);
        for (var i = 0; i < queue.Length; i++) block.SetMysteryGift(i, queue[i]);
        for (var i = 0; i < cards.Length; i++) block.SetMysteryGift(i, cards[i]);

        // PKHeX's IsDeliveryManActive also unlocks Mystery Gift, which is the app's call. The flag sits right past the received flags.
        block.SetMysteryGiftReceivedFlag(block.MysteryGiftReceivedFlagMax, queue.Any(gift => !gift.IsEmpty));
    }

    private static void InsertGen5(SAV5 sav5, PGF card, int at)
    {
        card.GiftUsed = false;
        sav5.Mystery.SetMysteryGift(at, card);
    }

    private static void RemoveGen5(SAV5 sav5, DataMysteryGift[] cards, int slot)
    {
        PGF[] moved = [.. cards.Where((_, i) => i != slot).Cast<PGF>(), new PGF()];
        for (var i = slot; i < moved.Length; i++) sav5.Mystery.SetMysteryGift(i, moved[i]);
    }

    private DataMysteryGift[] Cards() => _save switch
    {
        SAV4 sav4 => Enumerable.Range(0, Gen4Cards).Select(DataMysteryGift (i) => sav4.Mystery.GetMysteryGiftPCD(i)).ToArray(),
        SAV5 sav5 when Gen5Block(sav5) is var block => Enumerable.Range(0, block.GiftCountMax).Select(DataMysteryGift (i) => block.GetMysteryGift(i)).ToArray(),
        _ => [],
    };

    private static int FilledCount(DataMysteryGift[] cards)
    {
        var empty = Array.FindIndex(cards, card => card.IsEmpty);
        return empty < 0 ? cards.Length : empty;
    }

    private static PCD CardFor(PGT gift)
    {
        var card = new PCD();
        gift.Data.CopyTo(card.Data);
        // PKHeX has no setter for the games a card lists, and a blank card would list none.
        WriteUInt16LittleEndian(card.Data[0x14C..], MysteryGiftDatabase.Gen4Compatibility(MysteryGiftDatabase.Gen4Versions));
        return card;
    }

    private void RequireSlot(int slot)
    {
        if (slot < 0 || slot >= SlotCount) throw new ArgumentOutOfRangeException(nameof(slot), $"The album has no slot {slot}.");
    }

    private void RequireGeneration(int generation)
    {
        if (generation != _save.Generation)
            throw new WonderCardRefusedException(WonderCardRefusal.WrongGeneration, $"The card is for Generation {generation} games, not Generation {_save.Generation}.");
    }

    private void RequireCardId(int cardId)
    {
        if (cardId < 0 || cardId >= Flags.MysteryGiftReceivedFlagMax) throw new ArgumentOutOfRangeException(nameof(cardId), $"There is no card {cardId}.");
    }
}
