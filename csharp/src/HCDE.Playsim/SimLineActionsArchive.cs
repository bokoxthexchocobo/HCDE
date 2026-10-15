using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim;

public readonly record struct SimLineAction(int Special, int Arg3, int ActivationFlags)
{
    internal static SimLineAction Capture(LevelLine line) => new(line.Special, line.Arg3,
        (line.PlayerUse ? 1 : 0) | (line.PlayerUseBack ? 2 : 0) | (line.UseThrough ? 4 : 0)
        | (line.PlayerCross ? 8 : 0) | (line.Repeat ? 16 : 0));

    internal void Apply(LevelLine line)
    {
        line.Special = Special; line.Arg3 = Arg3;
        line.PlayerUse = (ActivationFlags & 1) != 0; line.PlayerUseBack = (ActivationFlags & 2) != 0;
        line.UseThrough = (ActivationFlags & 4) != 0; line.PlayerCross = (ActivationFlags & 8) != 0;
        line.Repeat = (ActivationFlags & 16) != 0;
    }
}

internal static class SimLineActionsArchive
{
    internal static void Validate(SimSaveState state)
    {
        if (state.LineActions is not { } lines) return;
        if (state.GeometryHealth is null || lines.Count != state.GeometryHealth.Lines.Count
            || lines.Any(line => line.ActivationFlags is < 0 or > 31))
            throw new InvalidOperationException("Invalid saved line action state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.LineActions is not { } lines) return archive;
        var size = checked(12 + lines.Count * 12);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], lines.Count);
        for (var i = 0; i < lines.Count; i++)
        {
            var item = trailer[(8 + i * 12)..];
            BinaryPrimitives.WriteInt32LittleEndian(item, lines[i].Special);
            BinaryPrimitives.WriteInt32LittleEndian(item[4..], lines[i].Arg3);
            BinaryPrimitives.WriteInt32LittleEndian(item[8..], lines[i].ActivationFlags);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 276);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-lineactions-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-lineactions-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer); var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 275 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-lineactions-header"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.GeometryHealth is null || state.GeometryHealth.Lines.Count != count)
        { state = new(); error = "save-lineactions-count"; return false; }
        var lines = new SimLineAction[count];
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(8 + i * 12)..];
            lines[i] = new(BinaryPrimitives.ReadInt32LittleEndian(item), BinaryPrimitives.ReadInt32LittleEndian(item[4..]),
                BinaryPrimitives.ReadInt32LittleEndian(item[8..]));
        }
        state.LineActions = lines;
        try { Validate(state); }
        catch (InvalidOperationException) { state = new(); error = "save-lineactions-state"; return false; }
        return true;
    }
}
