using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace PKHeX.Facade;

public sealed class MysteryGiftAlbum
{
    private readonly SaveFile _save;
    private readonly IMysteryGiftFlags _flags;

    private MysteryGiftAlbum(SaveFile save, IMysteryGiftFlags flags)
    {
        _save = save;
        _flags = flags;
    }

    internal static MysteryGiftAlbum? Of(Game game) => game.Supports(Capability.Events)
        ? game.SaveFile switch
        {
            SAV4 sav4 => new MysteryGiftAlbum(sav4, sav4.Mystery),
            SAV5 sav5 => new MysteryGiftAlbum(sav5, Initialized(sav5.Mystery)),
            _ => null,
        }
        : null;

    // PKHeX's blank saves leave the album as plain zeros, which decrypt to noise. An empty album is zeros once decrypted.
    private static MysteryBlock5 Initialized(MysteryBlock5 block)
    {
        if (block.Data.ContainsAnyExcept((byte)0)) return block;

        block.ClearReceivedFlags();
        for (var i = 0; i < block.GiftCountMax; i++) block.SetMysteryGift(i, new PGF());
        return block;
    }

    public int SlotCount => _save is SAV5 sav5 ? sav5.Mystery.GiftCountMax : Gen4Cards;

    /// <summary>Whether the game shows Mystery Gift in its menu, or null for Generation 5, which shows it once the player has the Pokédex.</summary>
    public bool? Unlocked => _save is SAV4 sav4 ? sav4.IsMysteryGiftUnlocked : null;

    /// <exception cref="NotSupportedException">The game has no unlock of its own.</exception>
    public void SetUnlocked(bool value)
    {
        if (_save is not SAV4 sav4) throw new NotSupportedException("Generation 5 games show Mystery Gift once the player has the Pokédex.");
        sav4.IsMysteryGiftUnlocked = value;
    }

    public IReadOnlyList<WonderCard?> Slots => Cards().Select(card => card.IsEmpty ? null : new WonderCard(card.Clone())).ToList();

    public IReadOnlyList<MysteryGiftEvent> Events => MysteryGiftDatabase.ReceivableBy(_save)
        .Select(entry => new MysteryGiftEvent(entry.Id, new WonderCard(entry.Gift), IsReceived(entry.Gift.CardID)))
        .ToList();

    /// <returns>The event with that id, or null when there is none this save's game can receive.</returns>
    public WonderCard? FindEvent(string id) => MysteryGiftDatabase.ReceivableBy(_save)
        .Where(entry => entry.Id == id)
        .Select(entry => new WonderCard(entry.Gift))
        .FirstOrDefault();

    /// <summary>Reads a <c>.pgt</c>, <c>.pcd</c>, <c>.wc4</c> or <c>.pgf</c> file. A <c>.pgt</c> holds only the gift, so it gets a blank card.</summary>
    /// <exception cref="WonderCardRefusedException">The bytes aren't a card, or the card is for another generation.</exception>
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
        if (gift.Generation != _save.Generation)
            throw new WonderCardRefusedException(WonderCardRefusal.WrongGeneration, $"The card is for Generation {gift.Generation} games, not Generation {_save.Generation}.");

        return new WonderCard(gift);
    }

    /// <summary>
    /// Puts the card in the album as the game does when it receives one. The gift waits for the player to collect it from the delivery person,
    /// and the card is marked received. A filled slot is replaced. A slot past the filled ones puts the card in the first empty slot, so the album never has a gap.
    /// </summary>
    /// <returns>The slot the card landed in.</returns>
    public int Insert(WonderCard card, int slot)
    {
        RequireSlot(slot);
        if (card.Generation != _save.Generation)
            throw new WonderCardRefusedException(WonderCardRefusal.WrongGeneration, $"The card is for Generation {card.Generation} games, not Generation {_save.Generation}.");
        if (card.Gift is PCD { CanConvertToPGT: false })
            throw new WonderCardRefusedException(WonderCardRefusal.NotForTheAlbum, "The Lock Capsule card doesn't go in the album.");

        var cards = Cards();
        var at = Math.Min(slot, FilledCount(cards));
        if (_save is SAV4 sav4) InsertGen4(sav4, (PCD)card.Gift.Clone(), at);
        else InsertGen5((SAV5)_save, (PGF)card.Gift.Clone(), at);

        if (card.CardId > 0 && card.CardId < _flags.MysteryGiftReceivedFlagMax) _flags.SetMysteryGiftReceivedFlag(card.CardId, true);
        return at;
    }

    /// <summary>Empties a slot. The cards after it move up, so the album never has a gap. A gift still waiting for the delivery person goes with its card.</summary>
    public void Remove(int slot)
    {
        RequireSlot(slot);
        var cards = Cards();
        if (cards[slot].IsEmpty) return;

        if (_save is SAV4 sav4) RemoveGen4(sav4, slot);
        else RemoveGen5((SAV5)_save, slot);
    }

    public bool IsReceived(int cardId)
    {
        RequireCardId(cardId);
        return _flags.GetMysteryGiftReceivedFlag(cardId);
    }

    public void SetReceived(int cardId, bool value)
    {
        RequireCardId(cardId);
        _flags.SetMysteryGiftReceivedFlag(cardId, value);
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

    private void RemoveGen5(SAV5 sav5, int slot)
    {
        var cards = Cards();
        PGF[] moved = [.. cards.Where((_, i) => i != slot).Cast<PGF>(), new PGF()];
        for (var i = slot; i < moved.Length; i++) sav5.Mystery.SetMysteryGift(i, moved[i]);
    }

    private DataMysteryGift[] Cards() => _save switch
    {
        SAV4 sav4 => Enumerable.Range(0, Gen4Cards).Select(DataMysteryGift (i) => sav4.Mystery.GetMysteryGiftPCD(i)).ToArray(),
        SAV5 sav5 => Enumerable.Range(0, sav5.Mystery.GiftCountMax).Select(DataMysteryGift (i) => sav5.Mystery.GetMysteryGift(i)).ToArray(),
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
        // A card lists the games that can receive it as bits by GameVersion. PKHeX has no setter, and a blank card would list none.
        WriteUInt16LittleEndian(card.Data[0x14C..], (1 << (int)GameVersion.D) | (1 << (int)GameVersion.P) | (1 << (int)GameVersion.Pt) | (1 << (int)GameVersion.HG) | (1 << (int)GameVersion.SS));
        return card;
    }

    private void RequireSlot(int slot)
    {
        if (slot < 0 || slot >= SlotCount) throw new ArgumentOutOfRangeException(nameof(slot), $"The album has no slot {slot}.");
    }

    private void RequireCardId(int cardId)
    {
        if (cardId < 0 || cardId >= _flags.MysteryGiftReceivedFlagMax) throw new ArgumentOutOfRangeException(nameof(cardId), $"There is no card {cardId}.");
    }
}
