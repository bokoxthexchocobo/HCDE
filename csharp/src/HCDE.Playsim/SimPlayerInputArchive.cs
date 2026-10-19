using System.Buffers.Binary;

namespace HCDE.Playsim;

internal readonly record struct SimPlayerInput(
    int OriginalButtons, int ModifiedButtons, int OriginalOldButtons, int ModifiedOldButtons,
    short OriginalPitch, short OriginalYaw, short OriginalForward, short OriginalSide, short OriginalUp,
    short ModifiedPitch, short ModifiedYaw, short ModifiedForward, short ModifiedSide, short ModifiedUp)
{
    public bool IsZero => OriginalButtons == 0 && ModifiedButtons == 0 && OriginalOldButtons == 0 && ModifiedOldButtons == 0
        && OriginalPitch == 0 && OriginalYaw == 0 && OriginalForward == 0 && OriginalSide == 0 && OriginalUp == 0
        && ModifiedPitch == 0 && ModifiedYaw == 0 && ModifiedForward == 0 && ModifiedSide == 0 && ModifiedUp == 0;
}

internal static class SimPlayerInputArchive
{
    private const int RecordSize = 40;

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.PlayerInput is { IsZero: false })) return archive;
        var size = checked(12 + state.Actors.Count * RecordSize);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            WriteRecord(trailer[(8 + i * RecordSize)..], state.Actors[i].PlayerInput);
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 365);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-playerinput-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-playerinput-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 364 || count < 0 || (long)count * RecordSize + 12 != size)
        { error = "save-playerinput-header"; return false; }
        for (var i = 0; i < count; i++)
            if (!Valid(trailer[(8 + i * RecordSize)..])) { error = "save-playerinput-value"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-playerinput-count"; return false; }
        for (var i = 0; i < count; i++)
            state.Actors[i].PlayerInput = ReadRecord(trailer[(8 + i * RecordSize)..]);
        return true;
    }

    private static void WriteRecord(Span<byte> record, SimPlayerInput? input)
    {
        if (input is not { IsZero: false } value)
        {
            BinaryPrimitives.WriteInt32LittleEndian(record, 0);
            return;
        }
        BinaryPrimitives.WriteInt32LittleEndian(record, 1);
        BinaryPrimitives.WriteInt32LittleEndian(record[4..], value.OriginalButtons);
        BinaryPrimitives.WriteInt32LittleEndian(record[8..], value.ModifiedButtons);
        BinaryPrimitives.WriteInt32LittleEndian(record[12..], value.OriginalOldButtons);
        BinaryPrimitives.WriteInt32LittleEndian(record[16..], value.ModifiedOldButtons);
        BinaryPrimitives.WriteInt16LittleEndian(record[20..], value.OriginalPitch);
        BinaryPrimitives.WriteInt16LittleEndian(record[22..], value.OriginalYaw);
        BinaryPrimitives.WriteInt16LittleEndian(record[24..], value.OriginalForward);
        BinaryPrimitives.WriteInt16LittleEndian(record[26..], value.OriginalSide);
        BinaryPrimitives.WriteInt16LittleEndian(record[28..], value.OriginalUp);
        BinaryPrimitives.WriteInt16LittleEndian(record[30..], value.ModifiedPitch);
        BinaryPrimitives.WriteInt16LittleEndian(record[32..], value.ModifiedYaw);
        BinaryPrimitives.WriteInt16LittleEndian(record[34..], value.ModifiedForward);
        BinaryPrimitives.WriteInt16LittleEndian(record[36..], value.ModifiedSide);
        BinaryPrimitives.WriteInt16LittleEndian(record[38..], value.ModifiedUp);
    }

    private static SimPlayerInput? ReadRecord(ReadOnlySpan<byte> record)
    {
        if (BinaryPrimitives.ReadInt32LittleEndian(record) == 0) return null;
        return new SimPlayerInput(
            BinaryPrimitives.ReadInt32LittleEndian(record[4..]),
            BinaryPrimitives.ReadInt32LittleEndian(record[8..]),
            BinaryPrimitives.ReadInt32LittleEndian(record[12..]),
            BinaryPrimitives.ReadInt32LittleEndian(record[16..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[20..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[22..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[24..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[26..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[28..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[30..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[32..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[34..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[36..]),
            BinaryPrimitives.ReadInt16LittleEndian(record[38..]));
    }

    private static bool Valid(ReadOnlySpan<byte> record)
    {
        var marker = BinaryPrimitives.ReadInt32LittleEndian(record);
        if (marker is not (0 or 1)) return false;
        if (marker == 1) return true;
        for (var i = 4; i < RecordSize; i++)
            if (record[i] != 0) return false;
        return true;
    }
}
