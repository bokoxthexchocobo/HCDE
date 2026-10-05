using System.Buffers.Binary;

namespace HCDE.Playsim;

internal static class SimDamagePowerArchive
{
    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        if (state.Actors.Any(a => a.PowerDamageTics < 0 || a.PowerProtectionTics < 0 || a.PowerBuddhaTics < 0))
            throw new ArgumentException("Negative damage power timer.", nameof(state));
        var buddha = state.Actors.Any(a => a.PowerBuddhaTics != 0);
        if (!buddha && !state.Actors.Any(a => a.PowerDamageTics != 0 || a.PowerProtectionTics != 0)) return archive;
        var stride = buddha ? 12 : 8;
        var size = checked(12 + state.Actors.Count * stride);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(8 + i * stride)..], state.Actors[i].PowerDamageTics);
            BinaryPrimitives.WriteInt32LittleEndian(trailer[(12 + i * stride)..], state.Actors[i].PowerProtectionTics);
            if (buddha) BinaryPrimitives.WriteInt32LittleEndian(trailer[(16 + i * stride)..], state.Actors[i].PowerBuddhaTics);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), buddha ? (ushort)52 : (ushort)51);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var stride = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]) == 52 ? 12 : 8;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-damage-power-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || (prior > 50 && prior != 53 && prior != 54) || count < 0 || (long)count * stride + 12 != size)
        { error = "save-damage-power-header"; return false; }
        for (var i = 0; i < count * (stride / 4); i++)
            if (BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * 4)..]) < 0)
            { error = "save-damage-power-timer"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-damage-power-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            state.Actors[i].PowerDamageTics = BinaryPrimitives.ReadInt32LittleEndian(trailer[(8 + i * stride)..]);
            state.Actors[i].PowerProtectionTics = BinaryPrimitives.ReadInt32LittleEndian(trailer[(12 + i * stride)..]);
            if (stride == 12) state.Actors[i].PowerBuddhaTics = BinaryPrimitives.ReadInt32LittleEndian(trailer[(16 + i * stride)..]);
        }
        return true;
    }
}
