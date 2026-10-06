using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimFriendship(int FriendPlayer, int TidToHate, bool Friendly, bool NoHatePlayers);

internal static class SimFriendshipArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.Friendship.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 16);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer[(8 + i * 16)..]; var friendship = state.Actors[i].Friendship;
            BinaryPrimitives.WriteInt32LittleEndian(item, friendship.HasValue ? 1 : 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[4..], friendship?.FriendPlayer ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[8..], friendship?.TidToHate ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[12..], (friendship?.Friendly == true ? 1 : 0) | (friendship?.NoHatePlayers == true ? 2 : 0));
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 86); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-friendship-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 85 || count < 0 || (long)count * 16 + 12 != size)
        { error = "save-friendship-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 16)..]; var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            var flags = BinaryPrimitives.ReadInt32LittleEndian(item[12..]);
            if (present is not (0 or 1) || present == 0 &&
                (BinaryPrimitives.ReadInt32LittleEndian(item[4..]) != 0 || BinaryPrimitives.ReadInt32LittleEndian(item[8..]) != 0 || flags != 0)
                || (flags & ~3) != 0)
            { error = "save-friendship-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-friendship-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 16)..];
            var flags = BinaryPrimitives.ReadInt32LittleEndian(item[12..]);
            if (BinaryPrimitives.ReadInt32LittleEndian(item) != 0)
                state.Actors[i].Friendship = new(BinaryPrimitives.ReadInt32LittleEndian(item[4..]), BinaryPrimitives.ReadInt32LittleEndian(item[8..]), (flags & 1) != 0, (flags & 2) != 0);
        }
        return true;
    }
}
