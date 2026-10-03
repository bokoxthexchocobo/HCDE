using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimPickupProperties(int Amount, bool IgnoreSkill, bool Depleted);

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
    public uint? Roll { get; internal set; }
    public SimPickupProperties? Pickup { get; internal set; }
    public int? ContactFlags { get; internal set; }
    public int? FloatFlags { get; internal set; }
    public int? DeathFlags { get; internal set; }
    public SimPainDeath? PainDeath { get; internal set; }
    public int? ProjectileFlags { get; internal set; }
    public SimProjectileLifetime? ProjectileLifetime { get; internal set; }
    public SimProjectilePointers? ProjectilePointers { get; internal set; }
    public int? CeilingFlags { get; internal set; }
    public int? FloorHuggerFlags { get; internal set; }
    public int? BlastedFlags { get; internal set; }
    public int? BlastEligibilityFlags { get; internal set; }
    public int? ThruActorsFlags { get; internal set; }
    public int? MissileThruSpeciesFlags { get; internal set; }
}

public sealed class SimSaveState
{
    public int Tic { get; init; }
    public bool Exited { get; init; }
    public bool SecretExit { get; init; }
    public uint? CombatRandomState { get; init; }
    public List<SimActorPose> Actors { get; } = new();
    public List<(double Floor, double Ceiling)> Sectors { get; } = new();
    public List<SimWallTransform>? Walls { get; init; }
    public List<SimPlaneTransform>? Planes { get; init; }
    public List<SimTextureScroll>? TextureScrolls { get; init; }
    public bool IncludesCarryScrolls { get; init; }
    public bool IncludesCeilingControls { get; init; }
    public bool IncludesFloorControls { get; init; }
    public bool IncludesWallControls { get; init; }
    public bool IncludesWallParts { get; init; }
    public SimGeometryHealth? GeometryHealth { get; internal set; }
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
        ValidateWalls(state);
        ValidatePlanes(state);
        ValidateTextureScrolls(state);
        ValidatePickups(state);
        ValidateContactFlags(state);
        ValidateFloatFlags(state);
        ValidateDeathFlags(state);
        SimProjectileFlagArchive.Validate(state);
        SimProjectileLifetimeArchive.Validate(state);
        SimProjectilePointerArchive.Validate(state);
        SimCeilingHuggerArchive.Validate(state);
        SimFloorHuggerArchive.Validate(state);
        SimBlastedArchive.Validate(state);
        SimBlastEligibilityArchive.Validate(state);
        SimThruActorsArchive.Validate(state);
        SimMissileThruSpeciesArchive.Validate(state);
        SimPainDeathArchive.Validate(state);
        if (state.Actors.Any(actor => actor.Pickup.HasValue) &&
            (state.GeometryHealth is null || state.Actors.Any(actor => !actor.Roll.HasValue)))
            throw new InvalidOperationException("Saved pickup properties require a complete current archive.");
        if (state.Actors.Any(actor => actor.Roll.HasValue) && (state.GeometryHealth is null
            || state.Actors.Any(actor => !actor.Roll.HasValue)))
            throw new InvalidOperationException("Saved actor rolls require a complete current archive.");
        if (state.GeometryHealth is { } geometry && (!state.IncludesWallParts || geometry.Groups.Keys.Any(id => id <= 0)))
            throw new InvalidOperationException("Saved geometry health is invalid.");
        var hasScrolls = state.TextureScrolls is not null;
        var scrollSize = state.IncludesCarryScrolls ? SimTextureScroll.CarrySize : SimTextureScroll.Size;
        var hasPlanes = state.Planes is not null || hasScrolls;
        var hasWalls = state.Walls is { Count: > 0 };
        var size = checked((hasPlanes ? 36 : hasWalls ? 32 : 28) + state.Actors.Count * 60 + state.Sectors.Count * 8
            + (state.Walls?.Count ?? 0) * SimWallTransform.Size
            + (state.Planes?.Count ?? 0) * SimPlaneTransform.Size
            + (hasScrolls ? 4 + state.TextureScrolls!.Count * scrollSize : 0));
        var buffer = new byte[size];
        var span = buffer.AsSpan();
        Magic.CopyTo(span);
        var cursor = 4;
        BinaryPrimitives.WriteUInt16LittleEndian(span[cursor..], hasScrolls ? (state.IncludesWallParts ? (ushort)14 : state.IncludesWallControls ? (ushort)13 : state.IncludesFloorControls ? (ushort)12 : state.IncludesCeilingControls ? (ushort)11 : state.IncludesCarryScrolls ? (ushort)10 : (ushort)9) : hasPlanes ? (ushort)8 : hasWalls ? (ushort)7 : (ushort)6);
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
        if (!hasWalls && !hasPlanes) return buffer;
        cursor += 8;
        BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Walls?.Count ?? -1);
        cursor += 4;
        if (state.Walls is { } walls)
            foreach (var wall in walls)
            {
                wall.Write(span[cursor..]);
                cursor += SimWallTransform.Size;
            }
        if (hasPlanes)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Planes?.Count ?? -1);
            cursor += 4;
            if (state.Planes is { } planes) foreach (var plane in planes)
            {
                plane.Write(span[cursor..]);
                cursor += SimPlaneTransform.Size;
            }
        }
        if (state.TextureScrolls is { } scrolls)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], scrolls.Count);
            cursor += 4;
            foreach (var scroll in scrolls)
            {
                scroll.Write(span[cursor..], state.IncludesCarryScrolls);
                cursor += scrollSize;
            }
        }
        if (state.GeometryHealth is { } health)
        {
            var trailer = health.Write();
            var archive = new byte[checked(buffer.Length + trailer.Length)];
            buffer.CopyTo(archive, 0);
            trailer.CopyTo(archive, buffer.Length);
            BinaryPrimitives.WriteUInt16LittleEndian(archive.AsSpan(4), 15);
            archive = SimProjectileFlagArchive.Write(state, SimPainDeathArchive.Write(state, WriteDeathFlags(state, WriteFloatFlags(state, WriteContactFlags(state, WritePickups(state, WriteRolls(state, archive)))))));
            archive = SimFloorHuggerArchive.Write(state, SimCeilingHuggerArchive.Write(state, SimProjectilePointerArchive.Write(state, SimProjectileLifetimeArchive.Write(state, archive))));
            return SimMissileThruSpeciesArchive.Write(state, SimThruActorsArchive.Write(state, SimBlastEligibilityArchive.Write(state, SimBlastedArchive.Write(state, archive))));
        }
        return buffer;
    }

    private static byte[] WriteRolls(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.Roll.HasValue)) return archive;
        var size = checked(8 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var offset = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 4 + i * 4), state.Actors[i].Roll!.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 16);
        return bytes;
    }

    internal static void ValidatePickups(SimSaveState state)
    {
        if (state.Actors.Any(actor => actor.Pickup is { Amount: < 0 }))
            throw new InvalidOperationException("Saved pickup amount cannot be negative.");
    }

    internal static void ValidateContactFlags(SimSaveState state)
    {
        if (!state.Actors.Any(actor => actor.ContactFlags.HasValue)) return;
        if (state.GeometryHealth is null || state.Actors.Any(actor => !actor.Roll.HasValue
            || !actor.ContactFlags.HasValue || actor.ContactFlags.Value is < 0 or > 3))
            throw new InvalidOperationException("Saved contact flags require a complete current archive.");
    }

    private static byte[] WriteContactFlags(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.ContactFlags.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 4), state.Actors[i].ContactFlags!.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 18);
        return bytes;
    }

    internal static void ValidateFloatFlags(SimSaveState state)
    {
        if (!state.Actors.Any(actor => actor.FloatFlags.HasValue)) return;
        if (state.Actors.All(actor => actor.FloatFlags == 0)) return;
        if (state.GeometryHealth is null || state.Actors.Any(actor => !actor.ContactFlags.HasValue) || state.Actors.Any(actor => !actor.Roll.HasValue
            || !actor.FloatFlags.HasValue || actor.FloatFlags.Value is < 0 or > 3))
            throw new InvalidOperationException("Saved float flags require a complete current archive.");
    }

    private static byte[] WriteFloatFlags(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.FloatFlags is > 0)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 4), state.Actors[i].FloatFlags!.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 19);
        return bytes;
    }

    internal static void ValidateDeathFlags(SimSaveState state)
    {
        if (!state.Actors.Any(actor => actor.DeathFlags.HasValue)) return;
        if (state.Actors.All(actor => actor.DeathFlags == 0)) return;
        if (state.GeometryHealth is null || state.Actors.Any(actor => !actor.ContactFlags.HasValue) || state.Actors.Any(actor => !actor.Roll.HasValue
            || !actor.DeathFlags.HasValue || actor.DeathFlags.Value is < 0 or > 1))
            throw new InvalidOperationException("Saved death flags require a complete current archive.");
    }

    private static byte[] WriteDeathFlags(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.DeathFlags is > 0)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 4), state.Actors[i].DeathFlags!.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 20);
        return bytes;
    }

    private static byte[] WritePickups(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.Pickup.HasValue)) return archive;
        var size = checked(8 + state.Actors.Count * 8);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            if (state.Actors[i].Pickup is not { } pickup) continue;
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4 + i * 8), pickup.Amount);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 8),
                4 | (pickup.IgnoreSkill ? 1 : 0) | (pickup.Depleted ? 2 : 0));
        }
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 17);
        return bytes;
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
        if (version == 30) return SimMissileThruSpeciesArchive.TryRead(bytes, out state, out error);
        if (version == 29) return SimThruActorsArchive.TryRead(bytes, out state, out error);
        if (version == 28) return SimBlastEligibilityArchive.TryRead(bytes, out state, out error);
        if (version == 27) return SimBlastedArchive.TryRead(bytes, out state, out error);
        if (version == 26) return SimFloorHuggerArchive.TryRead(bytes, out state, out error);
        if (version == 25) return SimCeilingHuggerArchive.TryRead(bytes, out state, out error);
        if (version == 24) return SimProjectilePointerArchive.TryRead(bytes, out state, out error);
        if (version == 23) return SimProjectileLifetimeArchive.TryRead(bytes, out state, out error);
        if (version == 22) return SimProjectileFlagArchive.TryRead(bytes, out state, out error);
        if (version == 21) return SimPainDeathArchive.TryRead(bytes, out state, out error);
        if (version == 20)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 12 || size > bytes.Length - 16 || (size - 12) % 4 != 0)
            { error = "save-death-size"; return false; }
            var start = bytes.Length - size;
            var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
            if (prior is not (18 or 19) || count < 0 || count != (size - 12) / 4)
            { error = "save-death-header"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-death-count"; return false; }
            for (var i = 0; i < count; i++)
            {
                var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 4)..]);
                if (flags is < 0 or > 1)
                { state = new(); error = "save-death-flags"; return false; }
                state.Actors[i].DeathFlags = flags;
            }
            return true;
        }
        if (version == 19)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 12 || size > bytes.Length - 16 || (size - 12) % 4 != 0)
            { error = "save-float-size"; return false; }
            var start = bytes.Length - size;
            var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
            if (prior != 18 || count < 0 || count != (size - 12) / 4)
            { error = "save-float-header"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-float-count"; return false; }
            for (var i = 0; i < count; i++)
            {
                var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 4)..]);
                if (flags is < 0 or > 3)
                { state = new(); error = "save-float-flags"; return false; }
                state.Actors[i].FloatFlags = flags;
            }
            return true;
        }
        if (version == 18)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 12 || size > bytes.Length - 16 || (size - 12) % 4 != 0)
            { error = "save-contact-size"; return false; }
            var start = bytes.Length - size;
            var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
            if (prior is not (16 or 17) || count < 0 || count != (size - 12) / 4)
            { error = "save-contact-header"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-contact-count"; return false; }
            for (var i = 0; i < count; i++)
            {
                var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 4)..]);
                if (flags is < 0 or > 3)
                { state = new(); error = "save-contact-flags"; return false; }
                state.Actors[i].ContactFlags = flags;
                state.Actors[i].FloatFlags = 0;
                state.Actors[i].ProjectileFlags = 0;
                state.Actors[i].CeilingFlags = 0;
                state.Actors[i].FloorHuggerFlags = 0;
                state.Actors[i].BlastedFlags = 0;
                state.Actors[i].DeathFlags = 0;
            }
            return true;
        }
        if (version == 17)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 8 || size > bytes.Length - 16 || (size - 8) % 8 != 0)
            { error = "save-pickup-size"; return false; }
            var start = bytes.Length - size;
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            if (count < 0 || count != (size - 8) / 8)
            { error = "save-pickup-count"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), 16);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-pickup-count"; return false; }
            for (var i = 0; i < count; i++)
            {
                var amount = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4 + i * 8)..]);
                var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 8)..]);
                if (amount < 0 || (flags & ~7) != 0 || (flags & 4) == 0 && (flags != 0 || amount != 0))
                { state = new(); error = "save-pickup-properties"; return false; }
                if ((flags & 4) != 0)
                    state.Actors[i].Pickup = new(amount, (flags & 1) != 0, (flags & 2) != 0);
            }
            return true;
        }
        if (version == 16)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 8 || size > bytes.Length - 16 || (size - 8) % 4 != 0)
            { error = "save-actor-roll-size"; return false; }
            var start = bytes.Length - size;
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            if (count < 0 || count != (size - 8) / 4)
            { error = "save-actor-roll-count"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), 15);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-actor-roll-count"; return false; }
            for (var i = 0; i < count; i++)
                state.Actors[i].Roll = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(start + 4 + i * 4)..]);
            return true;
        }
        if (version == 15)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 16 || size > bytes.Length - 16 || !SimGeometryHealth.TryRead(bytes[^size..], out var health))
            { error = "save-geometry-health"; return false; }
            var legacy = bytes[..^size].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), 14);
            if (!TryRead(legacy, out state, out error)) return false;
            state.GeometryHealth = health;
            return true;
        }
        if (version is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 10 or 11 or 12 or 13 or 14))
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
        if (remaining < 0 || sectorCount < 0 || sectorCount > remaining / sectorSize
            || version < 7 && (sectorCount != remaining / sectorSize || remaining % sectorSize != 0))
        {
            error = "save-sector-size";
            return false;
        }

        var footer = version >= 7 ? cursor + sectorCount * sectorSize : bytes.Length - 8;
        List<SimWallTransform>? walls = null;
        List<SimPlaneTransform>? planes = null;
        List<SimTextureScroll>? scrolls = null;
        if (version >= 7)
        {
            if (bytes.Length - footer < 12) { error = "save-truncated-walls"; return false; }
            var wallCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[(footer + 8)..]);
            var wallBytes = bytes.Length - footer - 12;
            if (version >= 8)
            {
                if (wallCount < -1 || wallCount > wallBytes / SimWallTransform.Size)
                { error = "save-wall-size"; return false; }
                var planeStart = footer + 12 + Math.Max(0, wallCount) * SimWallTransform.Size;
                if (bytes.Length - planeStart < 4) { error = "save-truncated-planes"; return false; }
                var planeCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[planeStart..]);
                var planeBytes = bytes.Length - planeStart - 4;
                if (version >= 9)
                {
                    if (planeCount < -1 || planeCount > planeBytes / SimPlaneTransform.Size)
                    { error = "save-plane-size"; return false; }
                    var scrollStart = planeStart + 4 + Math.Max(0, planeCount) * SimPlaneTransform.Size;
                    if (bytes.Length - scrollStart < 4) { error = "save-truncated-scrolls"; return false; }
                    var scrollCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[scrollStart..]);
                    var scrollBytes = bytes.Length - scrollStart - 4;
                    var scrollSize = version >= 10 ? SimTextureScroll.CarrySize : SimTextureScroll.Size;
                    if (scrollCount < 0 || scrollCount != scrollBytes / scrollSize || scrollBytes % scrollSize != 0)
                    { error = "save-scroll-size"; return false; }
                    scrolls = new List<SimTextureScroll>(scrollCount);
                    for (var i = 0; i < scrollCount; i++)
                    {
                        var scroll = SimTextureScroll.Read(bytes[(scrollStart + 4 + i * scrollSize)..], version >= 10);
                        if (!scroll.IsValid || version < 10 && scroll.Kind > 8 || version < 11 && scroll.Kind > 12 || version < 12 && scroll.Kind > 14 || version < 13 && scroll.Kind > 16 || version < 14 && scroll.Kind > 18) { error = "save-scroll-invalid"; return false; }
                        scrolls.Add(scroll);
                    }
                    planeBytes = Math.Max(0, planeCount) * SimPlaneTransform.Size;
                }
                if (planeCount < (version >= 9 ? -1 : 0) || planeCount >= 0
                    && (planeCount != planeBytes / SimPlaneTransform.Size || planeBytes % SimPlaneTransform.Size != 0))
                { error = "save-plane-size"; return false; }
                planes = planeCount < 0 ? null : new List<SimPlaneTransform>(planeCount);
                for (var i = 0; i < planeCount; i++)
                {
                    var plane = SimPlaneTransform.Read(bytes[(planeStart + 4 + i * SimPlaneTransform.Size)..]);
                    if (!plane.IsValid) { error = "save-plane-nonfinite"; return false; }
                    planes!.Add(plane);
                }
                wallBytes = Math.Max(0, wallCount) * SimWallTransform.Size;
            }
            if (wallCount < -1 || wallCount == -1 && wallBytes != 0
                || wallCount >= 0 && (wallCount != wallBytes / SimWallTransform.Size || wallBytes % SimWallTransform.Size != 0))
            { error = "save-wall-size"; return false; }
            if (wallCount >= 0)
            {
                walls = new List<SimWallTransform>(wallCount);
                for (var i = 0; i < wallCount; i++)
                {
                    var wall = SimWallTransform.Read(bytes[(footer + 12 + i * SimWallTransform.Size)..]);
                    if (!wall.IsValid) { error = "save-wall-nonfinite"; return false; }
                    walls.Add(wall);
                }
            }
        }
        state = new SimSaveState { Tic = tic, Exited = exited, SecretExit = secret, Walls = walls, Planes = planes, TextureScrolls = scrolls, IncludesCarryScrolls = version >= 10, IncludesCeilingControls = version >= 11, IncludesFloorControls = version >= 12, IncludesWallControls = version >= 13, IncludesWallParts = version >= 14,
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

    internal static void ValidateTextureScrolls(SimSaveState state)
    {
        if (state.IncludesWallParts && !state.IncludesWallControls
            || state.IncludesWallControls && !state.IncludesFloorControls
            || state.IncludesFloorControls && !state.IncludesCeilingControls
            || state.IncludesCeilingControls && !state.IncludesCarryScrolls
            || state.IncludesCarryScrolls && state.TextureScrolls is null
            || state.TextureScrolls is { } scrolls && scrolls.Any(scroll => scroll is null || !scroll.IsValid
                || !state.IncludesCarryScrolls && scroll.Kind > 8 || !state.IncludesCeilingControls && scroll.Kind > 12
                || !state.IncludesFloorControls && scroll.Kind > 14 || !state.IncludesWallControls && scroll.Kind > 16 || !state.IncludesWallParts && scroll.Kind > 18))
            throw new InvalidOperationException("Saved texture scrollers are invalid.");
    }

    internal static void ValidatePlanes(SimSaveState state)
    {
        if (state.Planes is { } planes && planes.Any(plane => plane is null || !plane.IsValid))
            throw new InvalidOperationException("Saved plane transforms are invalid.");
    }

    internal static void ValidateWalls(SimSaveState state)
    {
        if (state.Walls is { } walls && walls.Any(wall => wall is null || !wall.IsValid))
            throw new InvalidOperationException("Saved wall transforms are invalid.");
    }

    internal static void ValidateSectors(SimSaveState state)
    {
        foreach (var (floor, ceiling) in state.Sectors)
            if (!double.IsFinite(floor) || !double.IsFinite(ceiling) || floor < -32768
                || ceiling > int.MaxValue / 65536.0 || floor > ceiling)
                throw new InvalidOperationException("Saved sector planes are invalid.");
    }
}
