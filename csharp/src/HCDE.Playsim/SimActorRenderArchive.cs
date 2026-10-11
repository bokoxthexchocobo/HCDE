using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimActorRenderProperties(ActorRenderStyle Style, double Alpha);

internal static class SimActorRenderArchive
{
    internal static uint Pack(ActorRenderStyle style) => (uint)style.Operation |
        (uint)style.Source << 8 | (uint)style.Destination << 16 | (uint)style.Flags << 24;

    private static ActorRenderStyle Unpack(uint value) => new((ActorBlendOperation)(byte)value,
        (ActorAlphaFactor)(byte)(value >> 8), (ActorAlphaFactor)(byte)(value >> 16), (byte)(value >> 24));

    private static bool Valid(ActorRenderStyle style) => (byte)style.Operation <= 8 &&
        (byte)style.Source <= 9 && (byte)style.Destination <= 9;

    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.RenderProperties.HasValue) && state.GeometryHealth is null)
            throw new InvalidOperationException("Render archives require geometry state.");
        if (state.Actors.Any(a => a.RenderProperties is { } render &&
            (!Valid(render.Style) || !double.IsFinite(render.Alpha))))
            throw new InvalidOperationException("Invalid saved render properties.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(a => a.RenderProperties.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 12);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var render = state.Actors[i].RenderProperties;
            var entry = trailer[(8 + i * 12)..];
            BinaryPrimitives.WriteUInt32LittleEndian(entry, render.HasValue ? Pack(render.Value.Style) : uint.MaxValue);
            BinaryPrimitives.WriteDoubleLittleEndian(entry[4..], render?.Alpha ?? 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 187);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        if (bytes.Length < 28) { error = "save-render-size"; return false; }
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-render-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 186 || count < 0 || (long)count * 12 + 12 != size)
        { error = "save-render-header"; return false; }
        for (var i = 0; i < count; i++)
        {
            var entry = trailer[(8 + i * 12)..];
            var packed = BinaryPrimitives.ReadUInt32LittleEndian(entry);
            var alpha = BinaryPrimitives.ReadDoubleLittleEndian(entry[4..]);
            if (!double.IsFinite(alpha) || (packed == uint.MaxValue ? alpha != 0 : !Valid(Unpack(packed))))
            { error = "save-render-value"; return false; }
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-render-count"; return false; }
        for (var i = 0; i < count; i++)
        {
            var entry = trailer[(8 + i * 12)..];
            var packed = BinaryPrimitives.ReadUInt32LittleEndian(entry);
            state.Actors[i].RenderProperties = packed == uint.MaxValue ? null :
                new SimActorRenderProperties(Unpack(packed), BinaryPrimitives.ReadDoubleLittleEndian(entry[4..]));
        }
        return true;
    }
}
