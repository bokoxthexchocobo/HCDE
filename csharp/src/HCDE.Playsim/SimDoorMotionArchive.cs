using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimDoorMotion(int SectorIndex, double ClosedFloor, double ClosedCeiling,
    double TargetCeiling, double Speed, int Delay, int Wait, int Flags);

internal static class SimDoorMotionArchive
{
    internal static SimDoorMotion Capture(SectorMotion m) => new(m.SectorIndex, m.ClosedFloor, m.ClosedCeiling,
        m.TargetCeiling, m.Speed, m.Delay, m.Wait, (m.CloseAfterOpen ? 1 : 0) | (m.CloseOnly ? 2 : 0)
        | (m.OpenAfterClose ? 4 : 0) | (m.Closing ? 8 : 0) | (m.Paused ? 16 : 0) | (m.Completed ? 32 : 0));

    internal static SectorMotion Restore(SimDoorMotion m) => new() {
        Kind = MotionKind.Door, SectorIndex = m.SectorIndex, ClosedFloor = m.ClosedFloor,
        ClosedCeiling = m.ClosedCeiling, TargetCeiling = m.TargetCeiling, Speed = m.Speed,
        Delay = m.Delay, Wait = m.Wait, CloseAfterOpen = (m.Flags & 1) != 0, CloseOnly = (m.Flags & 2) != 0,
        OpenAfterClose = (m.Flags & 4) != 0, Closing = (m.Flags & 8) != 0,
        Paused = (m.Flags & 16) != 0, Completed = (m.Flags & 32) != 0
    };

    internal static void Validate(SimSaveState state)
    {
        if (state.DoorMotions is not { Count: > 0 } motions) return;
        if (state.GeometryHealth is null) throw new InvalidOperationException("Door motions require geometry state.");
        var sectors = new HashSet<int>();
        foreach (var m in motions)
            if ((uint)m.SectorIndex >= (uint)state.Sectors.Count || !sectors.Add(m.SectorIndex)
                || !double.IsFinite(m.ClosedFloor) || !double.IsFinite(m.ClosedCeiling) || !double.IsFinite(m.TargetCeiling)
                || !double.IsFinite(m.Speed) || m.Speed <= 0 || m.Delay < 0 || m.Wait < 0 || (m.Flags & ~63) != 0)
                throw new InvalidOperationException("Invalid saved door motion.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.DoorMotions is not { Count: > 0 } motions) return archive;
        var size = checked(12 + motions.Count * 48);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], motions.Count);
        for (var i = 0; i < motions.Count; i++)
        {
            var record = trailer.Slice(8 + i * 48, 48); var m = motions[i];
            BinaryPrimitives.WriteInt32LittleEndian(record, m.SectorIndex);
            BinaryPrimitives.WriteDoubleLittleEndian(record[4..], m.ClosedFloor);
            BinaryPrimitives.WriteDoubleLittleEndian(record[12..], m.ClosedCeiling);
            BinaryPrimitives.WriteDoubleLittleEndian(record[20..], m.TargetCeiling);
            BinaryPrimitives.WriteDoubleLittleEndian(record[28..], m.Speed);
            BinaryPrimitives.WriteInt32LittleEndian(record[36..], m.Delay);
            BinaryPrimitives.WriteInt32LittleEndian(record[40..], m.Wait);
            BinaryPrimitives.WriteInt32LittleEndian(record[44..], m.Flags);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 257); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-door-motion-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-door-motion-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 15 or > 256 || count < 0 || (long)count * 48 + 12 != size)
        { error = "save-door-motion-header"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (count > state.Sectors.Count) { state = new(); error = "save-door-motion-count"; return false; }
        var motions = new SimDoorMotion[count];
        for (var i = 0; i < count; i++)
        {
            var record = trailer.Slice(8 + i * 48, 48);
            motions[i] = new(BinaryPrimitives.ReadInt32LittleEndian(record), BinaryPrimitives.ReadDoubleLittleEndian(record[4..]),
                BinaryPrimitives.ReadDoubleLittleEndian(record[12..]), BinaryPrimitives.ReadDoubleLittleEndian(record[20..]),
                BinaryPrimitives.ReadDoubleLittleEndian(record[28..]), BinaryPrimitives.ReadInt32LittleEndian(record[36..]),
                BinaryPrimitives.ReadInt32LittleEndian(record[40..]), BinaryPrimitives.ReadInt32LittleEndian(record[44..]));
        }
        state.DoorMotions = motions;
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-door-motion-state"; return false; }
        return true;
    }
}
