using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimRipArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        var pushing = state.Actors.Any(a => a.Pushable || a.CannotPush || a.PushFactor != 0.25);
        var levels = pushing || state.Actors.Any(a => a.NoBossRip || a.RipperLevel != 0 || a.RipLevelMin != 0 || a.RipLevelMax != 0);
        if (!levels && !state.Actors.Any(a => a.Rip || a.DontRip)) return archive;
        var stride = pushing ? 24 : levels ? 16 : 4;
        var size = checked(12 + state.Actors.Count * stride);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * stride)..], (state.Actors[i].Rip ? 1 : 0) | (state.Actors[i].DontRip ? 2 : 0) | (state.Actors[i].NoBossRip ? 4 : 0) | (state.Actors[i].Pushable ? 8 : 0) | (state.Actors[i].CannotPush ? 16 : 0));
            if (levels)
            {
                BinaryPrimitives.WriteInt32LittleEndian(trailer[(12 + i * stride)..], state.Actors[i].RipperLevel);
                BinaryPrimitives.WriteInt32LittleEndian(trailer[(16 + i * stride)..], state.Actors[i].RipLevelMin);
                BinaryPrimitives.WriteInt32LittleEndian(trailer[(20 + i * stride)..], state.Actors[i].RipLevelMax);
            }
        }
        if (pushing)
            for (var i = 0; i < state.Actors.Count; i++)
            {
                if (!double.IsFinite(state.Actors[i].PushFactor)) throw new ArgumentException("Invalid push factor.", nameof(state));
                BinaryPrimitives.WriteDoubleLittleEndian(trailer[(24 + i * stride)..], state.Actors[i].PushFactor);
            }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), pushing ? (ushort)60 : levels ? (ushort)59 : (ushort)58);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var pushing = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]) == 60;
        var levels = pushing || BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]) == 59;
        var stride = pushing ? 24 : levels ? 16 : 4;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-rip-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 57 || count < 0 || (long)count * stride + 12 != size)
        { error = "save-rip-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var flags = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * stride)..]);
            if (flags < 0 || flags > (pushing ? 31 : levels ? 7 : 3))
            { error = "save-rip-flags"; return false; }
        }
        if (pushing)
            for (var i = 0; i < count; i++)
                if (!double.IsFinite(BinaryPrimitives.ReadDoubleLittleEndian(trailer[(24 + i * stride)..])))
                { error = "save-rip-push-factor"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-rip-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var flags = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * stride)..]);
            state.Actors[i].Rip = (flags & 1) != 0;
            state.Actors[i].DontRip = (flags & 2) != 0;
            state.Actors[i].NoBossRip = (flags & 4) != 0;
            state.Actors[i].Pushable = (flags & 8) != 0;
            state.Actors[i].CannotPush = (flags & 16) != 0;
            if (pushing) state.Actors[i].PushFactor = BinaryPrimitives.ReadDoubleLittleEndian(trailer[(24 + i * stride)..]);
            if (levels)
            {
                state.Actors[i].RipperLevel = BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * stride)..]);
                state.Actors[i].RipLevelMin = BinaryPrimitives.ReadInt32LittleEndian(trailer[(16 + i * stride)..]);
                state.Actors[i].RipLevelMax = BinaryPrimitives.ReadInt32LittleEndian(trailer[(20 + i * stride)..]);
            }
        }
        return true;
    }
}
