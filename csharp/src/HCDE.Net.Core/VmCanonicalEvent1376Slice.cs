namespace HCDE.Net.Core;

/// <summary>
/// <c>HCDEAppendCanonicalCVarChange</c>, <c>HCDEAppendCanonicalRunArgs</c>, and
/// <c>HCDEAppendCanonicalEventPayload</c> from <c>d_net.cpp</c> (3685–3901).
/// </summary>
public static class VmCanonicalEvent1376Slice
{
    public const byte CvarBool = 0, CvarInt = 1, CvarFloat = 2, CvarString = 3;
    public const byte DemMusicChange = 4, DemPrint = 5, DemCenterPrint = 6, DemStop = 7;
    public const byte DemUinfChanged = 8, DemSinfChanged = 9, DemGenericCheat = 10, DemGiveCheat = 11;
    public const byte DemSay = 12, DemTaunt = 13, DemChangeMap = 15, DemSuicide = 16;
    public const byte DemAddBot = 17, DemKillBots = 18, DemInvUseAll = 19, DemInvUse = 20;
    public const byte DemPause = 21, DemSaveGame = 22, DemSummon = 27, DemFov = 28;
    public const byte DemMyFov = 29, DemChangeMap2 = 30, DemRunScript = 33, DemSinfChangedXor = 34;
    public const byte DemInvDrop = 35, DemWarpCheat = 36, DemCenterView = 37, DemSummonFriend = 38;
    public const byte DemSpray = 39, DemCrouch = 40, DemRunScript2 = 41, DemCheckAutosave = 42;
    public const byte DemDoAutosave = 43, DemMorphEx = 44, DemSummonFoe = 45, DemTakeCheat = 48;
    public const byte DemAddController = 49, DemDelController = 50, DemKillClassCheat = 51, DemSummon2 = 53;
    public const byte DemSummonFriend2 = 54, DemSummonFoe2 = 55, DemAddSlotDefault = 56, DemAddSlot = 57;
    public const byte DemSetSlot = 58, DemSummonMbf = 59, DemConvReply = 60, DemConvClose = 61;
    public const byte DemConvNull = 62, DemRunSpecial = 63, DemSetPitchLimit = 64, DemRunNamedScript = 66;
    public const byte DemRevertCamera = 67, DemSetSlotPnum = 68, DemRemove = 69, DemFinishGame = 70;
    public const byte DemNetEvent = 71, DemMdk = 72, DemSetInv = 73, DemEndScreenJob = 74;
    public const byte DemZscCmd = 75, DemChangeSkill = 76, DemKick = 77, DemReadied = 78;
    public const byte DemWeapSelect = 79, DemUseFlechette = 80;

    public static bool AppendCVarChange(
        byte eventType, ReadOnlySpan<byte> data, ref int input, Span<byte> output, ref int outputCursor)
    {
        if (!ReadByte(data, ref input, out var descriptor))
            return false;

        var type = (byte)(descriptor >> 6);
        var nameBytes = descriptor & 0x3f;
        if (type > CvarString || nameBytes == 0 || input > data.Length || nameBytes > data.Length - input)
            return false;

        if (!AppendByte(output, ref outputCursor, type)
            || !AppendCanonicalString(output, ref outputCursor, data.Slice(input, nameBytes)))
        {
            return false;
        }

        input += nameBytes;
        if (eventType == DemSinfChangedXor)
            return AppendFieldBytes(data, ref input, output, ref outputCursor, 1);

        return type switch
        {
            CvarBool => AppendFieldBytes(data, ref input, output, ref outputCursor, 1),
            CvarInt or CvarFloat => AppendFieldBytes(data, ref input, output, ref outputCursor, 4),
            CvarString => AppendCanonicalNullString(data, ref input, output, ref outputCursor),
            _ => false,
        };
    }

    public static bool AppendRunArgs(
        ReadOnlySpan<byte> data, ref int input, Span<byte> output, ref int outputCursor, bool named)
    {
        byte argCount;
        if (named)
        {
            if (!AppendCanonicalNullString(data, ref input, output, ref outputCursor)
                || !ReadByte(data, ref input, out argCount)
                || !AppendByte(output, ref outputCursor, argCount))
            {
                return false;
            }

            argCount &= 127;
        }
        else if (!AppendFieldBytes(data, ref input, output, ref outputCursor, 2)
            || !ReadByte(data, ref input, out argCount)
            || !AppendByte(output, ref outputCursor, argCount))
        {
            return false;
        }

        return AppendFieldBytes(data, ref input, output, ref outputCursor, argCount * 4);
    }

    public static bool AppendEventPayload(
        byte eventType, ReadOnlySpan<byte> data, ref int input, Span<byte> output, ref int outputCursor)
    {
        switch (eventType)
        {
            case DemSuicide:
            case DemKillBots:
            case DemInvUseAll:
            case DemPause:
            case DemCenterView:
            case DemCrouch:
            case DemCheckAutosave:
            case DemDoAutosave:
            case DemConvClose:
            case DemConvNull:
            case DemRevertCamera:
            case DemFinishGame:
            case DemEndScreenJob:
            case DemReadied:
            case DemUseFlechette:
                return true;
            case DemSay:
            case DemTaunt:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 1)
                    && AppendCanonicalNullString(data, ref input, output, ref outputCursor);
            case DemAddBot:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 1)
                    && AppendCanonicalNullString(data, ref input, output, ref outputCursor)
                    && AppendFieldBytes(data, ref input, output, ref outputCursor, 4);
            case DemGiveCheat:
            case DemTakeCheat:
                return AppendCanonicalNullString(data, ref input, output, ref outputCursor)
                    && AppendFieldBytes(data, ref input, output, ref outputCursor, 4);
            case DemSetInv:
                return AppendCanonicalNullString(data, ref input, output, ref outputCursor)
                    && AppendFieldBytes(data, ref input, output, ref outputCursor, 5);
            case DemNetEvent:
                return AppendCanonicalNullString(data, ref input, output, ref outputCursor)
                    && AppendFieldBytes(data, ref input, output, ref outputCursor, 14);
            case DemZscCmd:
                return AppendCanonicalNullString(data, ref input, output, ref outputCursor)
                    && ReadBe16(data, ref input, out var commandBytes)
                    && AppendBe16(output, ref outputCursor, commandBytes)
                    && AppendFieldBytes(data, ref input, output, ref outputCursor, commandBytes);
            case DemSummon2:
            case DemSummonFriend2:
            case DemSummonFoe2:
                return AppendCanonicalNullString(data, ref input, output, ref outputCursor)
                    && AppendFieldBytes(data, ref input, output, ref outputCursor, 25);
            case DemChangeMap2:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 1)
                    && AppendCanonicalNullString(data, ref input, output, ref outputCursor);
            case DemMusicChange:
            case DemPrint:
            case DemCenterPrint:
            case DemUinfChanged:
            case DemChangeMap:
            case DemSummon:
            case DemSummonFriend:
            case DemSummonFoe:
            case DemSummonMbf:
            case DemRemove:
            case DemSpray:
            case DemMorphEx:
            case DemKillClassCheat:
            case DemMdk:
                return AppendCanonicalNullString(data, ref input, output, ref outputCursor);
            case DemWarpCheat:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 6);
            case DemInvUse:
            case DemFov:
            case DemMyFov:
            case DemChangeSkill:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 4);
            case DemInvDrop:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 8);
            case DemGenericCheat:
            case DemAddController:
            case DemDelController:
            case DemKick:
            case DemWeapSelect:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 1);
            case DemSaveGame:
                return AppendCanonicalNullString(data, ref input, output, ref outputCursor)
                    && AppendCanonicalNullString(data, ref input, output, ref outputCursor);
            case DemSinfChanged:
            case DemSinfChangedXor:
                return AppendCVarChange(eventType, data, ref input, output, ref outputCursor);
            case DemRunScript:
            case DemRunScript2:
            case DemRunSpecial:
                return AppendRunArgs(data, ref input, output, ref outputCursor, false);
            case DemRunNamedScript:
                return AppendRunArgs(data, ref input, output, ref outputCursor, true);
            case DemConvReply:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 3);
            case DemSetSlot:
            case DemSetSlotPnum:
                if (eventType == DemSetSlotPnum
                    && !AppendFieldBytes(data, ref input, output, ref outputCursor, 1))
                {
                    return false;
                }

                if (!ReadByte(data, ref input, out var slot)
                    || !ReadByte(data, ref input, out var count)
                    || !AppendByte(output, ref outputCursor, slot)
                    || !AppendByte(output, ref outputCursor, count))
                {
                    return false;
                }

                for (var i = 0; i < count; i++)
                {
                    if (!AppendWeaponIndex(data, ref input, output, ref outputCursor))
                        return false;
                }

                return true;
            case DemAddSlot:
            case DemAddSlotDefault:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 1)
                    && AppendWeaponIndex(data, ref input, output, ref outputCursor);
            case DemSetPitchLimit:
                return AppendFieldBytes(data, ref input, output, ref outputCursor, 2);
            default:
                return false;
        }
    }

    static bool AppendCanonicalString(Span<byte> output, ref int outputCursor, ReadOnlySpan<byte> stringBytes)
    {
        if (stringBytes.Length > ushort.MaxValue)
            return false;
        return AppendBe16(output, ref outputCursor, (ushort)stringBytes.Length)
            && AppendBytes(output, ref outputCursor, stringBytes);
    }

    static bool AppendCanonicalNullString(
        ReadOnlySpan<byte> data, ref int input, Span<byte> output, ref int outputCursor)
    {
        if (input >= data.Length)
            return false;

        var end = input;
        while (end < data.Length && data[end] != 0)
            end++;
        if (end >= data.Length)
            return false;
        if (!AppendCanonicalString(output, ref outputCursor, data.Slice(input, end - input)))
            return false;
        input = end + 1;
        return true;
    }

    static bool AppendWeaponIndex(ReadOnlySpan<byte> data, ref int input, Span<byte> output, ref int outputCursor)
    {
        if (!ReadByte(data, ref input, out var first))
            return false;

        var index = (ushort)(first & 0x7f);
        if ((first & 0x80) != 0)
        {
            if (!ReadByte(data, ref input, out var high))
                return false;
            index |= (ushort)(high << 7);
        }

        return AppendBe16(output, ref outputCursor, index);
    }

    static bool AppendFieldBytes(
        ReadOnlySpan<byte> data, ref int input, Span<byte> output, ref int outputCursor, int size)
    {
        if (input >= data.Length)
            return false;
        if (size > data.Length - input)
            return false;
        if (!AppendBytes(output, ref outputCursor, data.Slice(input, size)))
            return false;
        input += size;
        return true;
    }

    static bool AppendByte(Span<byte> output, ref int outputCursor, byte value)
    {
        if (outputCursor >= output.Length)
            return false;
        output[outputCursor++] = value;
        return true;
    }

    static bool AppendBe16(Span<byte> output, ref int outputCursor, ushort value)
    {
        if (outputCursor >= output.Length || output.Length - outputCursor < 2)
            return false;
        output[outputCursor++] = (byte)(value >> 8);
        output[outputCursor++] = (byte)value;
        return true;
    }

    static bool AppendBytes(Span<byte> output, ref int outputCursor, ReadOnlySpan<byte> bytes)
    {
        if (outputCursor >= output.Length || bytes.Length > output.Length - outputCursor)
            return false;
        if (bytes.Length != 0)
            bytes.CopyTo(output[outputCursor..]);
        outputCursor += bytes.Length;
        return true;
    }

    static bool ReadByte(ReadOnlySpan<byte> data, ref int input, out byte value)
    {
        value = 0;
        if (input >= data.Length)
            return false;
        value = data[input++];
        return true;
    }

    static bool ReadBe16(ReadOnlySpan<byte> data, ref int input, out ushort value)
    {
        value = 0;
        if (input >= data.Length || data.Length - input < 2)
            return false;
        value = (ushort)((data[input] << 8) | data[input + 1]);
        input += 2;
        return true;
    }
}
