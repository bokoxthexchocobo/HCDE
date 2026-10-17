using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimRemappedDecorationArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        SimMapDecorationArchive.Validate(state);
        if (!state.Actors.Any(a => a.MapDecorationIdentity is { DefinitionDoomEdNum: not 0 })) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var identity = state.Actors[i].MapDecorationIdentity;
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * 4)..],
                identity is { DefinitionDoomEdNum: not 0 } item ? item.DoomEdNum : 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 356);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-remappeddecoration-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-remappeddecoration-size"; return false; }
        var trailer = bytes[^size..]; var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 220 || prior > 355 || count < 0 || (long)count * 4 + 12 != size)
        { error = "save-remappeddecoration-header"; return false; }
        for (var i = 0; i < count; i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]) is < 0 or > ushort.MaxValue)
            { error = "save-remappeddecoration-value"; return false; }
        var legacy = bytes[..^size].ToArray(); BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-remappeddecoration-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var editor = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]);
            if (editor == 0) continue;
            if (state.Actors[i].MapDecorationIdentity is not { } identity)
            { state = new(); error = "save-remappeddecoration-state"; return false; }
            state.Actors[i].MapDecorationIdentity = identity with { DefinitionDoomEdNum = identity.DoomEdNum, DoomEdNum = editor };
        }
        try { SimMapDecorationArchive.Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-remappeddecoration-state"; return false; }
        return true;
    }
}
