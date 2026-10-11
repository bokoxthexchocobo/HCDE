using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimInfiniteAmmoPowerArchive
{
    private static bool ValidRegenerationStrength(double value) =>
        double.IsFinite(value) && value >= int.MinValue && value <= int.MaxValue;

    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(actor => !double.IsFinite(actor.PowerHighJumpStrength) || !double.IsFinite(actor.PowerDrainStrength) || !ValidRegenerationStrength(actor.PowerRegenerationStrength)))
            throw new InvalidOperationException("Invalid saved powerup strength.");
        if (state.Actors.Any(actor => actor.PowerInfiniteAmmoTics < 0 || actor.PowerDrainTics < 0 || actor.PowerRegenerationTics < 0 || actor.PowerDoubleFiringSpeedTics < 0 || actor.PowerHighJumpTics < 0 || actor.PowerProsperityTics < 0 || actor.PowerSpreadTics < 0))
            throw new InvalidOperationException("Invalid saved finite power duration.");
        if (state.GeometryHealth is null && state.Actors.Any(actor => actor.PowerInfiniteAmmoTics != 0 || actor.PowerDrainTics != 0 || actor.PowerRegenerationTics != 0 || actor.PowerDoubleFiringSpeedTics != 0 || actor.PowerHighJumpTics != 0 || actor.PowerHighJumpPendingRemoval || actor.PowerHighJumpStrength != 2 || actor.PowerDrainStrength != 0.5 || actor.PowerRegenerationStrength != 5 || actor.PowerProsperityTics != 0 || actor.PowerSpreadTics != 0))
            throw new InvalidOperationException("Finite powers require a full simulation save.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        var drain = state.Actors.Any(actor => actor.PowerDrainTics != 0);
        if (!drain && !state.Actors.Any(actor => actor.PowerInfiniteAmmoTics != 0 || actor.PowerRegenerationTics != 0 || actor.PowerDoubleFiringSpeedTics != 0 || actor.PowerHighJumpTics != 0 || actor.PowerHighJumpPendingRemoval || actor.PowerHighJumpStrength != 2 || actor.PowerDrainStrength != 0.5 || actor.PowerRegenerationStrength != 5 || actor.PowerProsperityTics != 0 || actor.PowerSpreadTics != 0)) return archive;
        var regeneration = state.Actors.Any(actor => actor.PowerRegenerationTics != 0);
        var firingSpeed = state.Actors.Any(actor => actor.PowerDoubleFiringSpeedTics != 0);
        var highJump = state.Actors.Any(actor => actor.PowerHighJumpTics != 0);
        var transientJump = state.Actors.Any(actor => actor.PowerHighJumpPendingRemoval);
        var customStrength = state.Actors.Any(actor => actor.PowerHighJumpStrength != 2);
        var customDrain = state.Actors.Any(actor => actor.PowerDrainStrength != 0.5);
        var customRegeneration = state.Actors.Any(actor => actor.PowerRegenerationStrength != 5);
        var prosperity = state.Actors.Any(actor => actor.PowerProsperityTics != 0);
        var spread = state.Actors.Any(actor => actor.PowerSpreadTics != 0);
        var stride = spread ? 56 : prosperity ? 52 : customRegeneration ? 48 : customDrain ? 40 : customStrength ? 32 : transientJump ? 24 : highJump ? 20 : firingSpeed ? 16 : regeneration ? 12 : drain ? 8 : 4;
        var size = checked(12 + state.Actors.Count * stride);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * stride)..], state.Actors[i].PowerInfiniteAmmoTics);
            if (stride >= 8) BinaryPrimitives.WriteInt32LittleEndian(trailer[(12 + i * stride)..], state.Actors[i].PowerDrainTics);
            if (stride >= 12) BinaryPrimitives.WriteInt32LittleEndian(trailer[(16 + i * stride)..], state.Actors[i].PowerRegenerationTics);
            if (stride >= 16) BinaryPrimitives.WriteInt32LittleEndian(trailer[(20 + i * stride)..], state.Actors[i].PowerDoubleFiringSpeedTics);
            if (stride >= 20) BinaryPrimitives.WriteInt32LittleEndian(trailer[(24 + i * stride)..], state.Actors[i].PowerHighJumpTics);
            if (stride >= 24) BinaryPrimitives.WriteInt32LittleEndian(trailer[(28 + i * stride)..], state.Actors[i].PowerHighJumpPendingRemoval ? 1 : 0);
            if (stride >= 32) BinaryPrimitives.WriteDoubleLittleEndian(trailer[(32 + i * stride)..], state.Actors[i].PowerHighJumpStrength);
            if (stride >= 40) BinaryPrimitives.WriteDoubleLittleEndian(trailer[(40 + i * stride)..], state.Actors[i].PowerDrainStrength);
            if (stride >= 48) BinaryPrimitives.WriteDoubleLittleEndian(trailer[(48 + i * stride)..], state.Actors[i].PowerRegenerationStrength);
            if (stride >= 52) BinaryPrimitives.WriteInt32LittleEndian(trailer[(56 + i * stride)..], state.Actors[i].PowerProsperityTics);
            if (spread) BinaryPrimitives.WriteInt32LittleEndian(trailer[(60 + i * stride)..], state.Actors[i].PowerSpreadTics);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), spread ? (ushort)181 : prosperity ? (ushort)180 : customRegeneration ? (ushort)179 : customDrain ? (ushort)178 : customStrength ? (ushort)177 : transientJump ? (ushort)176 : highJump ? (ushort)175 : firingSpeed ? (ushort)174 : regeneration ? (ushort)173 : drain ? (ushort)172 : (ushort)171);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = "save-infinite-ammo-power-invalid";
        if (bytes.Length < 28) return false;
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var stride = version == 181 ? 56 : version == 180 ? 52 : version == 179 ? 48 : version == 178 ? 40 : version == 177 ? 32 : version == 176 ? 24 : version == 175 ? 20 : version == 174 ? 16 : version == 173 ? 12 : version == 172 ? 8 : 4;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) return false;
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior is < 15 or > 170 || count < 0 || (long)count * stride + 12 != size) return false;
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * stride)..]) < 0
                || stride >= 8 && BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * stride)..]) < 0
                || stride >= 12 && BinaryPrimitives.ReadInt32LittleEndian(trailer[(16 + i * stride)..]) < 0
                || stride >= 16 && BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * stride)..]) < 0
                || stride >= 20 && BinaryPrimitives.ReadInt32LittleEndian(trailer[(24 + i * stride)..]) < 0
                || stride >= 24 && BinaryPrimitives.ReadInt32LittleEndian(trailer[(28 + i * stride)..]) is < 0 or > 1
                || stride >= 32 && !double.IsFinite(BinaryPrimitives.ReadDoubleLittleEndian(trailer[(32 + i * stride)..]))
                || stride >= 40 && !double.IsFinite(BinaryPrimitives.ReadDoubleLittleEndian(trailer[(40 + i * stride)..]))
                || stride >= 48 && !ValidRegenerationStrength(BinaryPrimitives.ReadDoubleLittleEndian(trailer[(48 + i * stride)..]))
                || stride >= 52 && BinaryPrimitives.ReadInt32LittleEndian(trailer[(56 + i * stride)..]) < 0
                || stride == 56 && BinaryPrimitives.ReadInt32LittleEndian(trailer[(60 + i * stride)..]) < 0) return false;
        var inner = bytes[..start].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(inner.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(inner, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-infinite-ammo-power-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            state.Actors[i].PowerInfiniteAmmoTics = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * stride)..]);
            state.Actors[i].PowerDrainTics = stride >= 8 ? BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * stride)..]) : 0;
            state.Actors[i].PowerRegenerationTics = stride >= 12 ? BinaryPrimitives.ReadInt32LittleEndian(trailer[(16 + i * stride)..]) : 0;
            state.Actors[i].PowerDoubleFiringSpeedTics = stride >= 16 ? BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * stride)..]) : 0;
            state.Actors[i].PowerHighJumpTics = stride >= 20 ? BinaryPrimitives.ReadInt32LittleEndian(trailer[(24 + i * stride)..]) : 0;
            state.Actors[i].PowerHighJumpPendingRemoval = stride >= 24 && BinaryPrimitives.ReadInt32LittleEndian(trailer[(28 + i * stride)..]) != 0;
            state.Actors[i].PowerHighJumpStrength = stride >= 32 ? BinaryPrimitives.ReadDoubleLittleEndian(trailer[(32 + i * stride)..]) : 2;
            state.Actors[i].PowerDrainStrength = stride >= 40 ? BinaryPrimitives.ReadDoubleLittleEndian(trailer[(40 + i * stride)..]) : 0.5;
            state.Actors[i].PowerRegenerationStrength = stride >= 48 ? BinaryPrimitives.ReadDoubleLittleEndian(trailer[(48 + i * stride)..]) : 5;
            state.Actors[i].PowerProsperityTics = stride >= 52 ? BinaryPrimitives.ReadInt32LittleEndian(trailer[(56 + i * stride)..]) : 0;
            state.Actors[i].PowerSpreadTics = stride == 56 ? BinaryPrimitives.ReadInt32LittleEndian(trailer[(60 + i * stride)..]) : 0;
        }
        error = null; return true;
    }
}
