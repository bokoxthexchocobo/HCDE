using System.Buffers.Binary;

namespace HCDE.Playsim;

public sealed class SimActorPose
{
    public uint Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public uint Angle { get; init; }
    public int Health { get; init; }
}

public sealed class SimSaveState
{
    public int Tic { get; init; }
    public bool Exited { get; init; }
    public bool SecretExit { get; init; }
    public List<SimActorPose> Actors { get; } = new();
    public List<(short Floor, short Ceiling)> Sectors { get; } = new();
}

/// <summary>
/// Authority pose archive. Actors match by id. Sector floor and ceiling heights round-trip.
/// This is not <c>p_saveg.cpp</c>.
/// </summary>
public static class SimSavegame
{
    public static ReadOnlySpan<byte> Magic => "HCSV"u8;

    public static byte[] Write(AuthoritySimulation sim) => Write(sim.CaptureState());

    public static byte[] Write(SimSaveState state)
    {
        var size = 4 + 2 + 4 + 1 + 1 + 4 + state.Actors.Count * 20 + 4 + state.Sectors.Count * 4;
        var buffer = new byte[size];
        var span = buffer.AsSpan();
        Magic.CopyTo(span);
        var cursor = 4;
        BinaryPrimitives.WriteUInt16LittleEndian(span[cursor..], 1);
        cursor += 2;
        BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Tic);
        cursor += 4;
        span[cursor++] = state.Exited ? (byte)1 : (byte)0;
        span[cursor++] = state.SecretExit ? (byte)1 : (byte)0;
        BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Actors.Count);
        cursor += 4;
        foreach (var actor in state.Actors)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span[cursor..], actor.Id);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 4)..], actor.X);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 8)..], actor.Y);
            BinaryPrimitives.WriteUInt32LittleEndian(span[(cursor + 12)..], actor.Angle);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 16)..], actor.Health);
            cursor += 20;
        }

        BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Sectors.Count);
        cursor += 4;
        foreach (var sector in state.Sectors)
        {
            BinaryPrimitives.WriteInt16LittleEndian(span[cursor..], sector.Floor);
            BinaryPrimitives.WriteInt16LittleEndian(span[(cursor + 2)..], sector.Ceiling);
            cursor += 4;
        }

        return buffer;
    }

    public static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new SimSaveState();
        error = null;
        if (bytes.Length < 16 || !bytes.StartsWith(Magic))
        {
            error = "save-magic";
            return false;
        }

        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        if (version != 1)
        {
            error = "save-version";
            return false;
        }

        var tic = BinaryPrimitives.ReadInt32LittleEndian(bytes[6..]);
        var exited = bytes[10] != 0;
        var secret = bytes[11] != 0;
        var actorCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[12..]);
        if (actorCount < 0)
        {
            error = "save-actor-count";
            return false;
        }

        var cursor = 16;
        var actors = new List<SimActorPose>(actorCount);
        for (var i = 0; i < actorCount; i++)
        {
            if (cursor + 20 > bytes.Length)
            {
                error = "save-truncated-actor";
                return false;
            }

            actors.Add(new SimActorPose
            {
                Id = BinaryPrimitives.ReadUInt32LittleEndian(bytes[cursor..]),
                X = BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 4)..]),
                Y = BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 8)..]),
                Angle = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(cursor + 12)..]),
                Health = BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 16)..]),
            });
            cursor += 20;
        }

        if (cursor + 4 > bytes.Length)
        {
            error = "save-truncated-sectors";
            return false;
        }

        var sectorCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]);
        cursor += 4;
        if (sectorCount < 0 || cursor + sectorCount * 4 != bytes.Length)
        {
            error = "save-sector-size";
            return false;
        }

        state = new SimSaveState { Tic = tic, Exited = exited, SecretExit = secret };
        state.Actors.AddRange(actors);
        for (var i = 0; i < sectorCount; i++)
        {
            state.Sectors.Add((
                BinaryPrimitives.ReadInt16LittleEndian(bytes[cursor..]),
                BinaryPrimitives.ReadInt16LittleEndian(bytes[(cursor + 2)..])));
            cursor += 4;
        }

        return true;
    }

    public static void Apply(AuthoritySimulation sim, ReadOnlySpan<byte> bytes)
    {
        if (!TryRead(bytes, out var state, out var error))
            throw new InvalidOperationException(error);
        sim.RestoreState(state);
    }
}
