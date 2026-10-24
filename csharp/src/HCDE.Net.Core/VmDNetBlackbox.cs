namespace HCDE.Net.Core;

/// <summary>Net blackbox ring buffer policy (<c>d_net_blackbox.cpp</c>).</summary>
public static class VmDNetBlackbox
{
    public const uint BlackboxMagic = 0x58484248;
    public const uint BlackboxVersion = 1;
    public const int BlackboxHeaderSize = 40;
    public const ulong AutoSaveCooldownMs = 5000;
    public const byte BboxPacket = 1;
    public const byte BboxTicIndex = 2;

    public static int ClampRecordCvarNative(int value)
    {
        if (value < 0)
            return 0;
        if (value > 1)
            return 1;
        return value;
    }

    public static int ClampSizeMbCvarNative(int value)
    {
        if (value < 4)
            return 4;
        if (value > 256)
            return 256;
        return value;
    }

    public static ulong BlackboxBudgetBytesNative(int sizeMb) =>
        (ulong)sizeMb * 1024UL * 1024UL;

    public static bool ShouldEvictOldestNative(int ringCount) => ringCount > 0;

    public static ulong EntryByteCostNative(int payloadSize) =>
        (ulong)BlackboxHeaderSize + (ulong)payloadSize;

    public static bool ShouldAutoSaveNative(ulong now, ulong lastAutoSaveMs) =>
        now - lastAutoSaveMs >= AutoSaveCooldownMs;

    public static bool ShouldRecordWhenEnabledNative(int recordCvar) => recordCvar != 0;

    public static void MarkAutoSaveNative(ref ulong lastAutoSaveMs, ulong now) => lastAutoSaveMs = now;

    public static bool ValidateHeaderMagicNative(uint magic) => magic == BlackboxMagic;

    public static int NormalizeRingIndexNative(int start, int capacity, int offset) =>
        (start + offset) % capacity;

    public static bool ShouldSkipRecordPacketNative(int recordCvar, int payloadSize, bool payloadNull) =>
        recordCvar == 0 || payloadSize == 0 || payloadNull;

    public static void FillPacketHeaderNative(
        ref BlackboxEntryHeaderState header,
        byte direction,
        int peerSlot,
        byte lane,
        byte room,
        uint seq,
        uint ack,
        int gametic,
        int clientTic,
        ulong timestampMs,
        int payloadSize)
    {
        header.Type = BboxPacket;
        header.Direction = direction;
        header.Lane = lane;
        header.Room = room;
        header.Seq = seq;
        header.Ack = ack;
        header.PeerSlot = peerSlot;
        header.Gametic = gametic;
        header.ClientTic = clientTic;
        header.TimestampMs = timestampMs;
        header.PayloadSize = payloadSize;
    }

    public static bool ShouldLazyInitNative(bool initialized) => !initialized;

    public static bool ShouldSkipAutoSaveWhenDisabledNative(int recordCvar) => recordCvar == 0;

    public static bool ShouldGrowRingVectorNative(int ringSize, int capacity) => ringSize < capacity;

    public static int ComputeRingInsertIndexNative(int ringStart, int ringCount, int capacity) =>
        (ringStart + ringCount) % capacity;

    public static void IncrementRingCountNative(ref int ringCount) => ringCount++;

    public static void AddBytesUsedNative(ref ulong bytesUsed, ulong entryBytes) => bytesUsed += entryBytes;

    public static void SubtractBytesUsedNative(ref ulong bytesUsed, ulong entryBytes) =>
        bytesUsed -= entryBytes;

    public static void ResetRingStateNative(ref int ringStart, ref int ringCount, ref ulong bytesUsed)
    {
        ringStart = 0;
        ringCount = 0;
        bytesUsed = 0;
    }

    public static void FillTicIndexHeaderNative(ref BlackboxEntryHeaderState header, int gametic, int clientTic, ulong timestampMs)
    {
        header.Type = BboxTicIndex;
        header.Gametic = gametic;
        header.ClientTic = clientTic;
        header.TimestampMs = timestampMs;
        header.PayloadSize = 0;
    }

    public static bool ShouldSkipSaveWhenDisabledNative(int recordCvar) => recordCvar == 0;

    public static string BuildAutoSaveLabelNative(string trigger) => $"autosave_{trigger}";

    public static bool ShouldEvictForBudgetNative(ulong bytesUsed, ulong entryBytes, ulong budget, int ringCount) =>
        bytesUsed + entryBytes > budget && ringCount > 0;

    public static bool ShouldEvictForCapacityNative(int ringCount, int capacity) =>
        ringCount >= capacity && ringCount > 0;

    public static void AdvanceRingStartNative(ref int ringStart, int capacity) =>
        ringStart = (ringStart + 1) % capacity;

    public static void DecrementRingCountNative(ref int ringCount) => ringCount--;

    public static bool HasPayloadNative(int payloadSize) => payloadSize > 0;

    public static uint PackHeaderMagicNative() => BlackboxMagic;

    public static byte PackHeaderVersionNative() => (byte)BlackboxVersion;

    public static int DefaultRingCapacityNative() => 4096;

    public static bool RingHasEntriesNative(int ringCount) => ringCount > 0;

    public static bool RingIsEmptyNative(int ringCount) => ringCount == 0;

    public static void BlackboxEvictOldestNative(ref BlackboxRingState ring)
    {
        if (ring.RingCount == 0)
            return;
        var index = ring.RingStart % ring.Capacity;
        var payloadSize = ring.PayloadSizes[index];
        ring.BytesUsed -= (ulong)BlackboxHeaderSize + (ulong)payloadSize;
        ring.PayloadSizes[index] = 0;
        ring.RingStart = (ring.RingStart + 1) % ring.Capacity;
        ring.RingCount--;
    }

    public static void BlackboxStoreEntryNative(ref BlackboxRingState ring, int payloadSize)
    {
        var entryBytes = EntryByteCostNative(payloadSize);
        var budget = ring.BudgetBytes;
        while (ShouldEvictForBudgetNative(ring.BytesUsed, entryBytes, budget, ring.RingCount))
            BlackboxEvictOldestNative(ref ring);
        while (ShouldEvictForCapacityNative(ring.RingCount, ring.Capacity))
            BlackboxEvictOldestNative(ref ring);

        if (ring.StorageCount < ring.Capacity)
        {
            ring.StorageCount++;
            ring.PayloadSizes[ring.StorageCount - 1] = payloadSize;
        }
        else
        {
            var index = ComputeRingInsertIndexNative(ring.RingStart, ring.RingCount, ring.Capacity);
            ring.PayloadSizes[index] = payloadSize;
        }

        IncrementRingCountNative(ref ring.RingCount);
        AddBytesUsedNative(ref ring.BytesUsed, entryBytes);
    }

    public static void NetBlackboxInitStateNative(ref BlackboxRingState ring)
    {
        ring.RingStart = 0;
        ring.RingCount = 0;
        ring.BytesUsed = 0;
        ring.StorageCount = 0;
        ring.LastAutoSaveMs = 0;
        ring.Initialized = true;
    }

    public static void NetBlackboxShutdownStateNative(ref BlackboxRingState ring)
    {
        ring.RingStart = 0;
        ring.RingCount = 0;
        ring.BytesUsed = 0;
        ring.StorageCount = 0;
        ring.Initialized = false;
    }

    public static bool NetBlackboxRecordPacketNative(
        int recordCvar,
        int payloadSize,
        bool payloadNull,
        ref bool initialized,
        ref BlackboxRingState ring,
        Action ensureInit,
        Action<BlackboxEntryHeaderState, int> storePacket)
    {
        if (ShouldSkipRecordPacketNative(recordCvar, payloadSize, payloadNull))
            return false;
        if (ShouldLazyInitNative(initialized))
        {
            ensureInit();
            initialized = true;
        }
        var header = new BlackboxEntryHeaderState();
        storePacket(header, payloadSize);
        BlackboxStoreEntryNative(ref ring, payloadSize);
        return true;
    }

    public static bool NetBlackboxRecordTicIndexNative(
        int recordCvar,
        ref bool initialized,
        ref BlackboxRingState ring,
        Action ensureInit,
        Action<BlackboxEntryHeaderState> storeTic)
    {
        if (recordCvar == 0)
            return false;
        if (ShouldLazyInitNative(initialized))
        {
            ensureInit();
            initialized = true;
        }
        var header = new BlackboxEntryHeaderState();
        storeTic(header);
        BlackboxStoreEntryNative(ref ring, 0);
        return true;
    }

    public static bool NetBlackboxAutoSaveNative(
        int recordCvar,
        ulong nowMs,
        ref ulong lastAutoSaveMs,
        Func<string, bool> trySave,
        string trigger)
    {
        if (recordCvar == 0)
            return false;
        if (lastAutoSaveMs != 0 && nowMs - lastAutoSaveMs < AutoSaveCooldownMs)
            return false;
        lastAutoSaveMs = nowMs;
        return trySave(trigger);
    }

    public static void NetBlackboxClearNative(ref BlackboxRingState ring) =>
        ResetRingStateNative(ref ring.RingStart, ref ring.RingCount, ref ring.BytesUsed);

    public static bool NetBlackboxSaveIterateNative(
        int ringStart,
        int ringCount,
        int ringSize,
        Func<int, bool> writeEntryAtIndex)
    {
        if (ringSize <= 0)
            return true;
        for (var i = 0; i < ringCount; i++)
        {
            var index = (ringStart + i) % ringSize;
            if (!writeEntryAtIndex(index))
                return false;
        }
        return true;
    }

    public static void RunBlackboxSaveCcmdNative(int argc, Func<string> defaultLabel, Func<string, bool> trySave, Action<string> printOk, Action printFail)
    {
        var label = argc > 1 ? defaultLabel() : "manual";
        if (trySave(label))
            printOk(label);
        else
            printFail();
    }

    public static void RunBlackboxClearCcmdNative(Action clear, Action<string> printCleared)
    {
        clear();
        printCleared("HCDE black box cleared.");
    }

    public static void RunBlackboxRecordCcmdNative(int argc, Func<int, int> setRecordCvar, Func<int, string> formatStatus, Action<string> printStatus, Func<int, string?> getArg)
    {
        if (argc > 1)
        {
            var arg = getArg(1);
            if (arg is not null && int.TryParse(arg, out var value))
                setRecordCvar(value != 0 ? 1 : 0);
        }
        printStatus(formatStatus(setRecordCvar(-1)));
    }
}

public struct BlackboxRingState
{
    public int RingStart;
    public int RingCount;
    public int Capacity;
    public int StorageCount;
    public ulong BytesUsed;
    public ulong BudgetBytes;
    public ulong LastAutoSaveMs;
    public bool Initialized;
    public int[] PayloadSizes;
}

public struct BlackboxEntryHeaderState
{
    public byte Type;
    public byte Direction;
    public byte Lane;
    public byte Room;
    public uint Seq;
    public uint Ack;
    public int PeerSlot;
    public int Gametic;
    public int ClientTic;
    public ulong TimestampMs;
    public int PayloadSize;
}
