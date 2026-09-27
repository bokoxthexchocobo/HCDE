using System.Buffers.Binary;

namespace HCDE.Playsim;

public sealed class SimActorPose
{
    public uint Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public uint Angle { get; init; }
    public int Health { get; init; }
    public int Z { get; init; }
    public int VelocityX { get; init; }
    public int VelocityY { get; init; }
    public int VelocityZ { get; init; }
    public int State { get; init; }
    public int StateTics { get; init; } = -1;
    public bool OnGround { get; init; } = true;
    public bool HasPhysics { get; init; } = true;
    public int WeaponCooldown { get; init; }
    public int Pitch { get; init; }
    public bool UseHeld { get; init; }
}

public sealed class SimSaveState
{
    public int Tic { get; init; }
    public bool Exited { get; init; }
    public bool SecretExit { get; init; }
    public uint? CombatRandomState { get; init; }
    public List<SimActorPose> Actors { get; } = new();
    public List<(double Floor, double Ceiling)> Sectors { get; } = new();
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
        ValidateSectors(state);
        var size = checked(28 + state.Actors.Count * 60 + state.Sectors.Count * 8);
        var buffer = new byte[size];
        var span = buffer.AsSpan();
        Magic.CopyTo(span);
        var cursor = 4;
        BinaryPrimitives.WriteUInt16LittleEndian(span[cursor..], 5);
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
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 20)..], actor.Z);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 24)..], actor.VelocityX);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 28)..], actor.VelocityY);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 32)..], actor.VelocityZ);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 36)..], actor.State);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 40)..], actor.StateTics);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 44)..], (actor.OnGround ? 1 : 0) | (actor.HasPhysics ? 2 : 0));
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 48)..], actor.WeaponCooldown);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 52)..], actor.Pitch);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 56)..], actor.UseHeld ? 1 : 0);
            cursor += 60;
        }

        BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Sectors.Count);
        cursor += 4;
        foreach (var sector in state.Sectors)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], Fixed.FromDouble(sector.Floor).Raw);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 4)..], Fixed.FromDouble(sector.Ceiling).Raw);
            cursor += 8;
        }

        BinaryPrimitives.WriteUInt32LittleEndian(span[cursor..], state.CombatRandomState ?? 0);
        BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 4)..], state.CombatRandomState.HasValue ? 1 : 0);
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
        if (version is not (1 or 2 or 3 or 4 or 5))
        {
            error = "save-version";
            return false;
        }

        var tic = BinaryPrimitives.ReadInt32LittleEndian(bytes[6..]);
        var exited = bytes[10] != 0;
        var secret = bytes[11] != 0;
        var actorCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[12..]);
        var actorSize = version == 1 ? 20 : version == 2 ? 48 : version == 3 ? 52 : 60;
        if (actorCount < 0 || actorCount > (bytes.Length - 16) / actorSize)
        {
            error = "save-actor-count";
            return false;
        }

        var cursor = 16;
        var actors = new List<SimActorPose>(actorCount);
        for (var i = 0; i < actorCount; i++)
        {
            if (cursor + actorSize > bytes.Length)
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
                Z = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 20)..]) : 0,
                VelocityX = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 24)..]) : 0,
                VelocityY = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 28)..]) : 0,
                VelocityZ = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 32)..]) : 0,
                State = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 36)..]) : 0,
                StateTics = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 40)..]) : -1,
                OnGround = version == 1 || (BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 44)..]) & 1) != 0,
                HasPhysics = version >= 2 && (BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 44)..]) & 2) != 0,
                WeaponCooldown = version >= 3 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 48)..]) : 0,
                Pitch = version >= 4 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 52)..]) : 0,
                UseHeld = version >= 4 && BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 56)..]) != 0,
            });
            cursor += actorSize;
        }

        if (cursor + 4 > bytes.Length)
        {
            error = "save-truncated-sectors";
            return false;
        }

        var sectorCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]);
        cursor += 4;
        var remaining = bytes.Length - cursor - (version >= 3 ? 8 : 0);
        var sectorSize = version >= 5 ? 8 : 4;
        if (remaining < 0 || sectorCount < 0 || sectorCount != remaining / sectorSize || remaining % sectorSize != 0)
        {
            error = "save-sector-size";
            return false;
        }

        var footer = bytes.Length - 8;
        state = new SimSaveState { Tic = tic, Exited = exited, SecretExit = secret,
            CombatRandomState = version >= 3 && BinaryPrimitives.ReadInt32LittleEndian(bytes[(footer + 4)..]) != 0
                ? BinaryPrimitives.ReadUInt32LittleEndian(bytes[footer..]) : null };
        state.Actors.AddRange(actors);
        for (var i = 0; i < sectorCount; i++)
        {
            var floor = version >= 5 ? new Fixed(BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..])).ToDouble()
                : BinaryPrimitives.ReadInt16LittleEndian(bytes[cursor..]);
            var ceiling = version >= 5 ? new Fixed(BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 4)..])).ToDouble()
                : BinaryPrimitives.ReadInt16LittleEndian(bytes[(cursor + 2)..]);
            if (floor > ceiling) { error = "save-sector-inverted"; state = new(); return false; }
            state.Sectors.Add((floor, ceiling));
            cursor += sectorSize;
        }

        return true;
    }

    public static void Apply(AuthoritySimulation sim, ReadOnlySpan<byte> bytes)
    {
        if (!TryRead(bytes, out var state, out var error))
            throw new InvalidOperationException(error);
        sim.RestoreState(state);
    }

    internal static void ValidateSectors(SimSaveState state)
    {
        foreach (var (floor, ceiling) in state.Sectors)
            if (!double.IsFinite(floor) || !double.IsFinite(ceiling) || floor < -32768
                || ceiling > int.MaxValue / 65536.0 || floor > ceiling)
                throw new InvalidOperationException("Saved sector planes are invalid.");
    }
}
