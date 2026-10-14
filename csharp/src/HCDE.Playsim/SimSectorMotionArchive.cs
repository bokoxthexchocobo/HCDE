using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimSectorMotion(
    int SectorIndex,
    double ClosedFloor,
    double TargetFloor,
    double ClosedCeiling,
    double TargetCeiling,
    MotionKind Kind,
    bool CloseAfterOpen,
    bool CloseOnly,
    bool OpenAfterClose,
    int Wait,
    bool Closing,
    double Speed,
    int Delay,
    int CrushDamage,
    bool CrushWithoutDamage,
    bool FloorCrushStopEligible,
    int Tag,
    bool Paused,
    bool Loop,
    double ReturnSpeed,
    bool SlowOnCrush,
    bool StopOnCrush,
    bool Slowed,
    int StairGroup,
    int StairSeedIndex,
    bool Completed,
    bool StairStopped,
    int ResetCountdown,
    bool WaitingForReset,
    int StepPeriod,
    int StepCountdown,
    int StairPause,
    int CeilingDirection,
    int FloorDirection);

internal static class SimSectorMotionArchive
{
    private const int RecordSize = 272;
    internal static SimSectorMotion Capture(SectorMotion m) => new(m.SectorIndex, m.ClosedFloor, m.TargetFloor, m.ClosedCeiling, m.TargetCeiling, m.Kind, m.CloseAfterOpen, m.CloseOnly, m.OpenAfterClose, m.Wait, m.Closing, m.Speed, m.Delay, m.CrushDamage, m.CrushWithoutDamage, m.FloorCrushStopEligible, m.Tag, m.Paused, m.Loop, m.ReturnSpeed, m.SlowOnCrush, m.StopOnCrush, m.Slowed, m.StairGroup, m.StairSeedIndex, m.Completed, m.StairStopped, m.ResetCountdown, m.WaitingForReset, m.StepPeriod, m.StepCountdown, m.StairPause, m.CeilingDirection, m.FloorDirection);
    internal static SectorMotion Restore(SimSectorMotion m) => new() {
        SectorIndex = m.SectorIndex,
        ClosedFloor = m.ClosedFloor,
        TargetFloor = m.TargetFloor,
        ClosedCeiling = m.ClosedCeiling,
        TargetCeiling = m.TargetCeiling,
        Kind = m.Kind,
        CloseAfterOpen = m.CloseAfterOpen,
        CloseOnly = m.CloseOnly,
        OpenAfterClose = m.OpenAfterClose,
        Wait = m.Wait,
        Closing = m.Closing,
        Speed = m.Speed,
        Delay = m.Delay,
        CrushDamage = m.CrushDamage,
        CrushWithoutDamage = m.CrushWithoutDamage,
        FloorCrushStopEligible = m.FloorCrushStopEligible,
        Tag = m.Tag,
        Paused = m.Paused,
        Loop = m.Loop,
        ReturnSpeed = m.ReturnSpeed,
        SlowOnCrush = m.SlowOnCrush,
        StopOnCrush = m.StopOnCrush,
        Slowed = m.Slowed,
        StairGroup = m.StairGroup,
        StairSeedIndex = m.StairSeedIndex,
        Completed = m.Completed,
        StairStopped = m.StairStopped,
        ResetCountdown = m.ResetCountdown,
        WaitingForReset = m.WaitingForReset,
        StepPeriod = m.StepPeriod,
        StepCountdown = m.StepCountdown,
        StairPause = m.StairPause,
        CeilingDirection = m.CeilingDirection,
        FloorDirection = m.FloorDirection
    };

    internal static void Validate(SimSaveState state)
    {
        if (state.SectorMotions is not { Count: > 0 } motions) return;
        if (state.GeometryHealth is null) throw new InvalidOperationException("Sector motions require geometry state.");
        var planes = new HashSet<(int Sector, bool Ceiling)>();
        foreach (var m in motions)
        {
            if ((uint)m.SectorIndex >= (uint)state.Sectors.Count || !Enum.IsDefined(m.Kind)
                || !double.IsFinite(m.ClosedFloor) || !double.IsFinite(m.TargetFloor) || !double.IsFinite(m.ClosedCeiling) || !double.IsFinite(m.TargetCeiling) || !double.IsFinite(m.Speed) || !double.IsFinite(m.ReturnSpeed) || m.Speed < 0 && m.Kind != MotionKind.Stair || m.ReturnSpeed < 0
                || m.Wait < 0 || m.Delay < 0 || m.ResetCountdown < 0 || m.StepPeriod < 0
                || m.StepCountdown < 0 || m.StairPause < 0 || m.CeilingDirection is < -1 or > 1
                || m.FloorDirection is < -1 or > 1)
                throw new InvalidOperationException("Invalid saved sector motion.");
            if (!m.Completed && !planes.Add((m.SectorIndex, m.Kind is MotionKind.Door or MotionKind.Ceiling)))
                throw new InvalidOperationException("Duplicate saved moving plane.");
        }
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.SectorMotions is not { Count: > 0 } motions || !motions.Any(m => m.Kind != MotionKind.Door)) return archive;
        var size = checked(12 + motions.Count * RecordSize);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], motions.Count);
        for (var i = 0; i < motions.Count; i++)
        {
            var record = trailer.Slice(8 + i * RecordSize, RecordSize); var m = motions[i];
            BinaryPrimitives.WriteInt32LittleEndian(record[0..], m.SectorIndex);
            BinaryPrimitives.WriteDoubleLittleEndian(record[8..], m.ClosedFloor);
            BinaryPrimitives.WriteDoubleLittleEndian(record[16..], m.TargetFloor);
            BinaryPrimitives.WriteDoubleLittleEndian(record[24..], m.ClosedCeiling);
            BinaryPrimitives.WriteDoubleLittleEndian(record[32..], m.TargetCeiling);
            BinaryPrimitives.WriteInt32LittleEndian(record[40..], (int)m.Kind);
            BinaryPrimitives.WriteInt32LittleEndian(record[48..], m.CloseAfterOpen ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[56..], m.CloseOnly ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[64..], m.OpenAfterClose ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[72..], m.Wait);
            BinaryPrimitives.WriteInt32LittleEndian(record[80..], m.Closing ? 1 : 0);
            BinaryPrimitives.WriteDoubleLittleEndian(record[88..], m.Speed);
            BinaryPrimitives.WriteInt32LittleEndian(record[96..], m.Delay);
            BinaryPrimitives.WriteInt32LittleEndian(record[104..], m.CrushDamage);
            BinaryPrimitives.WriteInt32LittleEndian(record[112..], m.CrushWithoutDamage ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[120..], m.FloorCrushStopEligible ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[128..], m.Tag);
            BinaryPrimitives.WriteInt32LittleEndian(record[136..], m.Paused ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[144..], m.Loop ? 1 : 0);
            BinaryPrimitives.WriteDoubleLittleEndian(record[152..], m.ReturnSpeed);
            BinaryPrimitives.WriteInt32LittleEndian(record[160..], m.SlowOnCrush ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[168..], m.StopOnCrush ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[176..], m.Slowed ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[184..], m.StairGroup);
            BinaryPrimitives.WriteInt32LittleEndian(record[192..], m.StairSeedIndex);
            BinaryPrimitives.WriteInt32LittleEndian(record[200..], m.Completed ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[208..], m.StairStopped ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[216..], m.ResetCountdown);
            BinaryPrimitives.WriteInt32LittleEndian(record[224..], m.WaitingForReset ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(record[232..], m.StepPeriod);
            BinaryPrimitives.WriteInt32LittleEndian(record[240..], m.StepCountdown);
            BinaryPrimitives.WriteInt32LittleEndian(record[248..], m.StairPause);
            BinaryPrimitives.WriteInt32LittleEndian(record[256..], m.CeilingDirection);
            BinaryPrimitives.WriteInt32LittleEndian(record[264..], m.FloorDirection);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 258); return result;
    }

    private static bool ReadBoolean(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadInt32LittleEndian(bytes) switch {
        0 => false, 1 => true, _ => throw new InvalidDataException("Invalid saved motion boolean.")
    };

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-sector-motion-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-sector-motion-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 15 or > 257 || count < 0 || (long)count * RecordSize + 12 != size)
        { error = "save-sector-motion-header"; return false; }
        var legacy = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        var motions = new SimSectorMotion[count];
        try
        {
            for (var i = 0; i < count; i++)
            {
                var record = trailer.Slice(8 + i * RecordSize, RecordSize);
                motions[i] = new(BinaryPrimitives.ReadInt32LittleEndian(record[0..]),
                    BinaryPrimitives.ReadDoubleLittleEndian(record[8..]),
                    BinaryPrimitives.ReadDoubleLittleEndian(record[16..]),
                    BinaryPrimitives.ReadDoubleLittleEndian(record[24..]),
                    BinaryPrimitives.ReadDoubleLittleEndian(record[32..]),
                    (MotionKind)BinaryPrimitives.ReadInt32LittleEndian(record[40..]),
                    ReadBoolean(record[48..]),
                    ReadBoolean(record[56..]),
                    ReadBoolean(record[64..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[72..]),
                    ReadBoolean(record[80..]),
                    BinaryPrimitives.ReadDoubleLittleEndian(record[88..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[96..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[104..]),
                    ReadBoolean(record[112..]),
                    ReadBoolean(record[120..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[128..]),
                    ReadBoolean(record[136..]),
                    ReadBoolean(record[144..]),
                    BinaryPrimitives.ReadDoubleLittleEndian(record[152..]),
                    ReadBoolean(record[160..]),
                    ReadBoolean(record[168..]),
                    ReadBoolean(record[176..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[184..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[192..]),
                    ReadBoolean(record[200..]),
                    ReadBoolean(record[208..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[216..]),
                    ReadBoolean(record[224..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[232..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[240..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[248..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[256..]),
                    BinaryPrimitives.ReadInt32LittleEndian(record[264..]));
            }
        }
        catch (InvalidDataException) { state = new(); error = "save-sector-motion-boolean"; return false; }
        state.SectorMotions = motions;
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-sector-motion-state"; return false; }
        return true;
    }
}