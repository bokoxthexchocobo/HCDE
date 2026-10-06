using System.Buffers.Binary;

namespace HCDE.Playsim;

public sealed record SimLightEffect(int Sector, int Kind, int Start, int End, int Duration, int DarkTime, long Tics);
public sealed record SimLightAnimation(uint StrobeRandom, uint FlickerRandom, uint FlashRandom, uint FireRandom,
    IReadOnlyList<SimLightEffect> Effects);

internal static class SimLightAnimationArchive
{
    private static bool Valid(SimLightEffect e, int sectors) => (uint)e.Sector < (uint)sectors
        && (uint)e.Kind <= 7 && e.Start is >= short.MinValue and <= short.MaxValue
        && e.End is >= short.MinValue and <= short.MaxValue
        && (e.Kind is not (0 or 1) || e.Duration > 0);

    internal static void Validate(SimSaveState state)
    {
        if (state.LightAnimation is { } animation && (state.Lights is null || animation.Effects is null
            || animation.Effects.Any(e => e is null || !Valid(e, state.Sectors.Count))))
            throw new InvalidOperationException("Invalid saved lighting animation.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (state.LightAnimation is not { } animation) return archive;
        var size = checked(28 + animation.Effects.Count * 32);
        var result = new byte[checked(archive.Length + size)]; archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], animation.Effects.Count);
        var random = new[] { animation.StrobeRandom, animation.FlickerRandom, animation.FlashRandom, animation.FireRandom };
        for (var i = 0; i < 4; i++) BinaryPrimitives.WriteUInt32LittleEndian(trailer[(8 + i * 4)..], random[i]);
        for (var i = 0; i < animation.Effects.Count; i++)
        {
            var e = animation.Effects[i]; var item = trailer[(24 + i * 32)..];
            var fields = new[] { e.Sector, e.Kind, e.Start, e.End, e.Duration, e.DarkTime };
            for (var j = 0; j < 6; j++) BinaryPrimitives.WriteInt32LittleEndian(item[(j * 4)..], fields[j]);
            BinaryPrimitives.WriteInt64LittleEndian(item[24..], e.Tics);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 81); return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 28 || size > bytes.Length - 16) { error = "save-lightanimation-size"; return false; }
        var start = bytes.Length - size; var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer); var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 80 || count < 0 || (long)count * 32 + 28 != size)
        { error = "save-lightanimation-header"; return false; }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Lights is null) { state = new(); error = "save-lightanimation-lights"; return false; }
        var effects = new List<SimLightEffect>(count);
        for (var i = 0; i < count; i++)
        {
            var item = trailer[(24 + i * 32)..];
            var e = new SimLightEffect(BinaryPrimitives.ReadInt32LittleEndian(item), BinaryPrimitives.ReadInt32LittleEndian(item[4..]),
                BinaryPrimitives.ReadInt32LittleEndian(item[8..]), BinaryPrimitives.ReadInt32LittleEndian(item[12..]),
                BinaryPrimitives.ReadInt32LittleEndian(item[16..]), BinaryPrimitives.ReadInt32LittleEndian(item[20..]),
                BinaryPrimitives.ReadInt64LittleEndian(item[24..]));
            if (!Valid(e, state.Sectors.Count)) { state = new(); error = "save-lightanimation-value"; return false; }
            effects.Add(e);
        }
        state.LightAnimation = new(BinaryPrimitives.ReadUInt32LittleEndian(trailer[8..]), BinaryPrimitives.ReadUInt32LittleEndian(trailer[12..]),
            BinaryPrimitives.ReadUInt32LittleEndian(trailer[16..]), BinaryPrimitives.ReadUInt32LittleEndian(trailer[20..]), effects);
        return true;
    }
}
