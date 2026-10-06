using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimCrouchState(double Factor, double FullHeight, double ViewHeight, int DefaultViewHeightRaw, bool Locked);

internal static class SimCrouchArchive
{
    private const int Stride = 36;
    internal static SimCrouchState? Capture(PlayerPawn player) =>
        player.CrouchFactor != 1 || player.UncrouchLocked || player.FullHeight != player.Height.ToDouble()
        || player.ViewHeight != PlayerPawn.StandingViewHeight || player.DefaultViewHeight != Fixed.FromDouble(PlayerPawn.StandingViewHeight)
            ? new(player.CrouchFactor, player.FullHeight, player.ViewHeight, player.DefaultViewHeight.Raw, player.UncrouchLocked) : null;

    private static bool Valid(SimCrouchState value) => double.IsFinite(value.Factor)
        && value.Factor >= PlayerPawn.MinimumCrouchFactor && value.Factor <= 1
        && double.IsFinite(value.FullHeight) && value.FullHeight >= 0 && double.IsFinite(value.ViewHeight);

    internal static void Validate(SimSaveState state)
    {
        if (state.Actors.Any(a => a.Crouch is { } value && !Valid(value)))
            throw new InvalidOperationException("Invalid saved crouch state.");
    }

    internal static byte[] Write(SimSaveState state, byte[] archive)
    {
        Validate(state);
        if (!state.Actors.Any(a => a.Crouch.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * Stride);
        var result = new byte[checked(archive.Length + size)];
        archive.CopyTo(result, 0);
        var trailer = result.AsSpan(archive.Length);
        BinaryPrimitives.WriteInt32LittleEndian(trailer, BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(trailer[4..], state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            var item = trailer.Slice(8 + i * Stride, Stride);
            var value = state.Actors[i].Crouch;
            BinaryPrimitives.WriteInt32LittleEndian(item, value.HasValue ? 1 : 0);
            BinaryPrimitives.WriteDoubleLittleEndian(item[4..], value?.Factor ?? 0);
            BinaryPrimitives.WriteDoubleLittleEndian(item[12..], value?.FullHeight ?? 0);
            BinaryPrimitives.WriteDoubleLittleEndian(item[20..], value?.ViewHeight ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[28..], value?.DefaultViewHeightRaw ?? 0);
            BinaryPrimitives.WriteInt32LittleEndian(item[32..], value?.Locked == true ? 1 : 0);
        }
        BinaryPrimitives.WriteInt32LittleEndian(trailer[^4..], size);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(4), 91);
        return result;
    }

    internal static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new(); error = null;
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
        if (size < 12 || size > bytes.Length - 16) { error = "save-crouch-size"; return false; }
        var start = bytes.Length - size;
        var trailer = bytes[start..];
        var prior = BinaryPrimitives.ReadInt32LittleEndian(trailer);
        var count = BinaryPrimitives.ReadInt32LittleEndian(trailer[4..]);
        if (prior < 15 || prior > 90 || count < 0 || (long)count * Stride + 12 != size)
        { error = "save-crouch-header"; return false; }
        var values = new SimCrouchState?[count];
        for (var i = 0; i < count; i++)
        {
            var item = trailer.Slice(8 + i * Stride, Stride);
            var present = BinaryPrimitives.ReadInt32LittleEndian(item);
            var locked = BinaryPrimitives.ReadInt32LittleEndian(item[32..]);
            var value = new SimCrouchState(BinaryPrimitives.ReadDoubleLittleEndian(item[4..]),
                BinaryPrimitives.ReadDoubleLittleEndian(item[12..]), BinaryPrimitives.ReadDoubleLittleEndian(item[20..]),
                BinaryPrimitives.ReadInt32LittleEndian(item[28..]), locked == 1);
            if (present is < 0 or > 1 || locked is < 0 or > 1 || present == 1 && !Valid(value))
            { error = "save-crouch-value"; return false; }
            values[i] = present == 1 ? value : null;
        }
        var legacy = bytes[..start].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
        if (!SimSavegame.TryRead(legacy, out state, out error)) return false;
        if (state.Actors.Count != count) { state = new(); error = "save-crouch-count"; return false; }
        for (var i = 0; i < count; i++) state.Actors[i].Crouch = values[i];
        return true;
    }
}
