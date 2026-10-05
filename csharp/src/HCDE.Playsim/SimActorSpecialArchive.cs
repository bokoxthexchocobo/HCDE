using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimActorSpecialArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.ActorSpecial.HasValue)) return archive;
        var activation = state.Actors.Any(actor => actor.ActorSpecial is { ActivationType: not 0 });
        var upgrades = state.Actors.Any(actor => actor.ActorSpecial is { Stamina: not 0 } or { BonusHealth: not 0 });
        var pickupHealth = state.Actors.Any(actor => actor.ActorSpecial is { MaxPickupHealth: not 0 });
        upgrades |= pickupHealth;
        activation |= upgrades;
        var recordSize = pickupHealth ? 40 : upgrades ? 36 : activation ? 28 : 24;
        var size = checked(12 + state.Actors.Count * recordSize);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var value = state.Actors[i].ActorSpecial ?? default;
            var fields = new[] { value.Special, value.Arg0, value.Arg1, value.Arg2, value.Arg3, value.Arg4 };
            for (var j = 0; j < fields.Length; j++)
                BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * recordSize + j * 4), fields[j]);
            if (activation) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * recordSize + 24), value.ActivationType);
            if (upgrades)
            {
                BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * recordSize + 28), value.Stamina);
                BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * recordSize + 32), value.BonusHealth);
            }
        }
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        if (pickupHealth)
            for (var i = 0; i < state.Actors.Count; i++)
                BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * recordSize + 36), state.Actors[i].ActorSpecial?.MaxPickupHealth ?? 0);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), pickupHealth ? (ushort)45 : upgrades ? (ushort)44 : activation ? (ushort)43 : (ushort)42);
        return bytes;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        var pickupHealth = version == 45;
        var upgrades = version >= 44;
        var activation = version >= 43;
        var recordSize = pickupHealth ? 40 : upgrades ? 36 : activation ? 28 : 24;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16 || (size - 12) % recordSize != 0)
        { error = "save-actor-special-size"; return false; }
        var start = bytes.Length - size;
        var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
        var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
        if (prior < 18 || prior > 41 || count < 0 || count != (size - 12) / recordSize)
        { error = "save-actor-special-header"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count)
        { state = new(); error = "save-actor-special-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var data = bytes[(start + 8 + i * recordSize)..];
            state.Actors[i].ActorSpecial = new SimActorSpecial(
                BinaryPrimitives.ReadInt32LittleEndian(data), BinaryPrimitives.ReadInt32LittleEndian(data[4..]),
                BinaryPrimitives.ReadInt32LittleEndian(data[8..]), BinaryPrimitives.ReadInt32LittleEndian(data[12..]),
                BinaryPrimitives.ReadInt32LittleEndian(data[16..]), BinaryPrimitives.ReadInt32LittleEndian(data[20..]),
                activation ? BinaryPrimitives.ReadInt32LittleEndian(data[24..]) : 0,
                upgrades ? BinaryPrimitives.ReadInt32LittleEndian(data[28..]) : 0,
                upgrades ? BinaryPrimitives.ReadInt32LittleEndian(data[32..]) : 0,
                pickupHealth ? BinaryPrimitives.ReadInt32LittleEndian(data[36..]) : 0);
        }
        return true;
    }
}
