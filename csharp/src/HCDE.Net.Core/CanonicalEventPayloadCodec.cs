using System.Buffers.Binary;

namespace HCDE.Net.Core;

public static class CanonicalEventPayloadCodec
{
    public static bool TryBuildFromLegacy(byte eventType, ReadOnlySpan<byte> legacy, ref int legacyCursor, Span<byte> output, out int payloadLength)
    {
        payloadLength = 0;
        var cursor = 0;
        var ok = (DemoCommand)eventType switch
        {
            DemoCommand.Suicide or DemoCommand.Pause or DemoCommand.CenterView or DemoCommand.Crouch
                or DemoCommand.ConvClose or DemoCommand.ConvNull or DemoCommand.RevertCamera
                or DemoCommand.EndScreenJob or DemoCommand.Readied or DemoCommand.UseFlechette => true,

            DemoCommand.GenericCheat or DemoCommand.WeapSelect => TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, 1),
            DemoCommand.InvUse or DemoCommand.MyFov => TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, 4),
            DemoCommand.InvDrop => TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, 8),
            DemoCommand.SetPitchLimit => TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, 2),
            DemoCommand.ConvReply => TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, 3),

            DemoCommand.Say or DemoCommand.Taunt => TryCanonicalizeSay(legacy, ref legacyCursor, output, ref cursor),
            DemoCommand.Print or DemoCommand.CenterPrint or DemoCommand.UinfChanged or DemoCommand.ChangeMap
                or DemoCommand.Spray or DemoCommand.MusicChange => TryCanonicalizeNullStringOnly(legacy, ref legacyCursor, output, ref cursor),

            DemoCommand.GiveCheat => TryCanonicalizeStringPlusFixed(legacy, ref legacyCursor, output, ref cursor, 4),
            DemoCommand.SinfChanged => CanonicalCVarChangeCodec.TryBuildFromLegacy(legacy, ref legacyCursor, output, ref cursor, xorVariant: false),
            DemoCommand.RunScript or DemoCommand.RunScript2 or DemoCommand.RunSpecial
                => CanonicalRunArgsCodec.TryBuildFromLegacy(legacy, ref legacyCursor, output, ref cursor, named: false),
            DemoCommand.RunNamedScript
                => CanonicalRunArgsCodec.TryBuildFromLegacy(legacy, ref legacyCursor, output, ref cursor, named: true),
            DemoCommand.NetEvent => TryCanonicalizeStringPlusFixed(legacy, ref legacyCursor, output, ref cursor, 14),
            DemoCommand.ZscCmd => TryCanonicalizeZscCmd(legacy, ref legacyCursor, output, ref cursor),

            _ => false,
        };

        if (!ok)
            return false;

        payloadLength = cursor;
        return true;
    }

    public static bool PayloadsEqual(byte eventType, ReadOnlySpan<byte> canonical, ReadOnlySpan<byte> legacy)
    {
        Span<byte> rebuilt = stackalloc byte[256];
        var legacyCursor = 0;
        if (!TryBuildFromLegacy(eventType, legacy, ref legacyCursor, rebuilt, out var length))
            return false;

        return length == canonical.Length && canonical.SequenceEqual(rebuilt[..length]);
    }

    private static bool TryCopyFixedLegacy(ReadOnlySpan<byte> legacy, ref int legacyCursor, Span<byte> output, ref int cursor, int size)
    {
        if (legacy.Length - legacyCursor < size || output.Length - cursor < size)
            return false;

        legacy.Slice(legacyCursor, size).CopyTo(output[cursor..]);
        legacyCursor += size;
        cursor += size;
        return true;
    }

    private static bool TryCanonicalizeSay(ReadOnlySpan<byte> legacy, ref int legacyCursor, Span<byte> output, ref int cursor)
    {
        if (legacy.Length - legacyCursor < 1)
            return false;

        output[cursor++] = legacy[legacyCursor++];
        if (!CanonicalStringCodec.TryReadLegacyNullTerminated(legacy, ref legacyCursor, out var stringBytes))
            return false;

        return CanonicalStringCodec.Write(output, ref cursor, stringBytes) > 0;
    }

    private static bool TryCanonicalizeNullStringOnly(ReadOnlySpan<byte> legacy, ref int legacyCursor, Span<byte> output, ref int cursor)
    {
        if (!CanonicalStringCodec.TryReadLegacyNullTerminated(legacy, ref legacyCursor, out var stringBytes))
            return false;

        return CanonicalStringCodec.Write(output, ref cursor, stringBytes) > 0;
    }

    private static bool TryCanonicalizeStringPlusFixed(
        ReadOnlySpan<byte> legacy,
        ref int legacyCursor,
        Span<byte> output,
        ref int cursor,
        int fixedBytes)
    {
        if (!CanonicalStringCodec.TryReadLegacyNullTerminated(legacy, ref legacyCursor, out var stringBytes)
            || CanonicalStringCodec.Write(output, ref cursor, stringBytes) == 0)
            return false;

        return TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, fixedBytes);
    }

    private static bool TryCanonicalizeZscCmd(ReadOnlySpan<byte> legacy, ref int legacyCursor, Span<byte> output, ref int cursor)
    {
        if (!CanonicalStringCodec.TryReadLegacyNullTerminated(legacy, ref legacyCursor, out var nameBytes)
            || CanonicalStringCodec.Write(output, ref cursor, nameBytes) == 0
            || legacy.Length - legacyCursor < 2)
            return false;

        var commandBytes = System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(legacy[legacyCursor..]);
        legacyCursor += 2;
        if (output.Length - cursor < 2 + commandBytes || legacy.Length - legacyCursor < commandBytes)
            return false;

        System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(output[cursor..], commandBytes);
        cursor += 2;
        legacy.Slice(legacyCursor, commandBytes).CopyTo(output[cursor..]);
        legacyCursor += commandBytes;
        cursor += commandBytes;
        return true;
    }
}
