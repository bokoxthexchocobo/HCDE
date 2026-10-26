namespace HCDE.Client;

/// <summary><c>Net_TrySkipCommand</c> from <c>d_net.cpp</c> (9261–9506).</summary>
public static class VmDNetTrySkipCommand
{
    public const int DemSay = 12;
    public const int DemTaunt = 13;
    public const int DemMusicchange = 4;
    public const int DemPrint = 5;
    public const int DemCenterprint = 6;
    public const int DemUinfchanged = 8;
    public const int DemGenericcheat = 10;
    public const int DemGivecheat = 11;
    public const int DemTakecheat = 47;
    public const int DemDropplayer = 14;
    public const int DemChangemap = 15;
    public const int DemSuicide = 16;
    public const int DemAddbot = 17;
    public const int DemKillbots = 18;
    public const int DemInvuseall = 19;
    public const int DemInvuse = 20;
    public const int DemPause = 21;
    public const int DemSavegame = 22;
    public const int DemSummon = 26;
    public const int DemFov = 27;
    public const int DemMyfov = 28;
    public const int DemChangemap2 = 29;
    public const int DemRunscript = 32;
    public const int DemSinfchangedxor = 33;
    public const int DemInvdrop = 34;
    public const int DemWarpcheat = 35;
    public const int DemCenterview = 36;
    public const int DemSummonfriend = 37;
    public const int DemSpray = 38;
    public const int DemCrouch = 39;
    public const int DemRunscript2 = 40;
    public const int DemCheckautosave = 41;
    public const int DemDoautosave = 42;
    public const int DemMorphex = 43;
    public const int DemSummonfoe = 44;
    public const int DemAddcontroller = 48;
    public const int DemDelcontroller = 49;
    public const int DemKillclasscheat = 50;
    public const int DemSummon2 = 52;
    public const int DemSummonfriend2 = 53;
    public const int DemSummonfoe2 = 54;
    public const int DemAddslotdefault = 55;
    public const int DemAddslot = 56;
    public const int DemSetslot = 57;
    public const int DemSummonmbf = 58;
    public const int DemConvreply = 59;
    public const int DemConvclose = 60;
    public const int DemConvnull = 61;
    public const int DemRunspecial = 62;
    public const int DemSetpitchlimit = 63;
    public const int DemRunnamedscript = 65;
    public const int DemRevertcamera = 66;
    public const int DemSetslotpnum = 67;
    public const int DemRemove = 68;
    public const int DemFinishgame = 69;
    public const int DemNetevent = 70;
    public const int DemMdk = 71;
    public const int DemSetinv = 72;
    public const int DemEndscreenjob = 73;
    public const int DemZscCmd = 74;
    public const int DemChangeskill = 75;
    public const int DemKick = 76;
    public const int DemReadied = 77;
    public const int DemWeapselect = 78;
    public const int DemUseflechette = 79;
    public const int DemSinfchanged = 9;

    public const int CvarBool = 0;
    public const int CvarInt = 1;
    public const int CvarFloat = 2;
    public const int CvarString = 3;

    public static bool TryReadCStringBytes(int streamSize, int offset, ReadOnlySpan<byte> data, out int bytesWithNull)
    {
        bytesWithNull = 0;
        if (offset > streamSize)
            return false;
        var available = streamSize - offset;
        for (var i = 0; i < available; i++)
        {
            if (data[offset + i] == 0)
            {
                bytesWithNull = i + 1;
                return true;
            }
        }

        return false;
    }

    public static bool SkipCStringPlus(int stringBytes, int extra, out int skip) =>
        TryComputeSkip(stringBytes, extra, out skip);

    public static bool TryComputeSkip(int stringBytes, int extra, out int skip)
    {
        skip = stringBytes + extra;
        return true;
    }

    public static bool TrySkipSay(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 1, out skip);

    public static bool TrySkipSayFromStream(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (!TryReadCStringBytes(streamSize, 1, stream, out var stringBytes))
            return false;
        skip = 1 + stringBytes;
        return true;
    }

    public static bool TrySkipAddBot(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 5, out skip);

    public static bool TrySkipAddBotFromStream(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (!TryReadCStringBytes(streamSize, 1, stream, out var stringBytes))
            return false;
        return TrySkipAddBot(stringBytes, out skip);
    }

    public static bool TrySkipGiveCheat(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 4, out skip);

    public static bool TrySkipCStringAtZero(int streamSize, ReadOnlySpan<byte> stream, int extra, out int skip)
    {
        skip = 0;
        if (!TryReadCStringBytes(streamSize, 0, stream, out var stringBytes))
            return false;
        return SkipCStringPlus(stringBytes, extra, out skip);
    }

    public static bool TrySkipSetInv(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 5, out skip);

    public static bool TrySkipNetEvent(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 14, out skip);

    public static bool TrySkipZscCmd(int streamSize, int stringBytes, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = stringBytes;
        if (skip > streamSize || streamSize - skip < 2)
            return false;
        var len = (stream[skip] << 8) | stream[skip + 1];
        skip += 2 + len;
        return true;
    }

    public static bool TrySkipZscCmdFromStream(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (!TryReadCStringBytes(streamSize, 0, stream, out var stringBytes))
            return false;
        return TrySkipZscCmd(streamSize, stringBytes, stream, out skip);
    }

    public static bool TrySkipSummon2(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 25, out skip);

    public static bool TrySkipChangeMap2(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 1, out skip);

    public static bool TrySkipChangeMap2FromStream(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (!TryReadCStringBytes(streamSize, 1, stream, out var stringBytes))
            return false;
        return TrySkipChangeMap2(stringBytes, out skip);
    }

    public static bool TrySkipCStringOnly(int stringBytes, out int skip)
    {
        skip = stringBytes;
        return true;
    }

    public static bool TrySkipCStringOnlyFromStream(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (!TryReadCStringBytes(streamSize, 0, stream, out var stringBytes))
            return false;
        return TrySkipCStringOnly(stringBytes, out skip);
    }

    public static bool TrySkipFixed(int bytes, out int skip)
    {
        skip = bytes;
        return true;
    }

    public static bool TrySkipSaveGame(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (!TryReadCStringBytes(streamSize, 0, stream, out var first))
            return false;
        skip = first;
        if (!TryReadCStringBytes(streamSize, skip, stream, out var second))
            return false;
        skip += second;
        return true;
    }

    public static bool TrySkipRunScript(int argCount, out int skip)
    {
        skip = 3 + argCount * 4;
        return true;
    }

    public static bool TrySkipRunScriptFromStream(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (streamSize < 3)
            return false;
        return TrySkipRunScript(stream[2], out skip);
    }

    public static bool TrySkipRunNamedScript(int streamSize, int stringBytes, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = stringBytes + 1;
        if (skip > streamSize)
            return false;
        skip += (stream[skip - 1] & 127) * 4;
        return true;
    }

    public static bool TrySkipRunNamedScriptFromStream(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (!TryReadCStringBytes(streamSize, 0, stream, out var stringBytes))
            return false;
        return TrySkipRunNamedScript(streamSize, stringBytes, stream, out skip);
    }

    public static bool TrySkipSinFChanged(int streamSize, ReadOnlySpan<byte> stream, bool xorVariant, out int skip)
    {
        skip = 0;
        if (streamSize < 1)
            return false;
        var t = stream[0];
        skip = 1 + (t & 63);
        if (!xorVariant)
        {
            var type = t >> 6;
            skip += type switch
            {
                CvarBool => 1,
                CvarInt or CvarFloat => 4,
                CvarString => 0,
                _ => 0,
            };
            if (type == CvarString)
            {
                if (!TryReadCStringBytes(streamSize, skip, stream, out var sb))
                    return false;
                skip += sb;
            }
        }
        else
        {
            skip += 1;
        }

        return true;
    }

    public static bool TrySkipSetSlot(int streamSize, ReadOnlySpan<byte> stream, bool pnum, out int skip)
    {
        skip = 2 + (pnum ? 1 : 0);
        if (skip == 0 || skip > streamSize)
            return false;
        var numWeapons = stream[skip - 1];
        for (var i = 0; i < numWeapons; i++)
        {
            if (skip >= streamSize)
                return false;
            skip += 1 + (stream[skip] >> 7);
        }

        return true;
    }

    public static bool TrySkipAddSlot(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (streamSize < 2)
            return false;
        skip = 2 + (stream[1] >> 7);
        return true;
    }

    public static bool IsZeroPayloadCommand(int cmd) =>
        cmd is DemSuicide or DemKillbots or DemInvuseall or DemPause or DemCenterview or DemCrouch
            or DemCheckautosave or DemDoautosave or DemConvclose or DemConvnull or DemRevertcamera
            or DemFinishgame or DemEndscreenjob or DemReadied or DemUseflechette;

    public static bool TryComputeCommandSkip(int cmd, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        var streamSize = stream.Length;
        switch (cmd)
        {
            case DemSay:
            case DemTaunt:
                return TrySkipSayFromStream(streamSize, stream, out skip);
            case DemAddbot:
                return TrySkipAddBotFromStream(streamSize, stream, out skip);
            case DemGivecheat:
            case DemTakecheat:
                return TrySkipCStringAtZero(streamSize, stream, 4, out skip);
            case DemSetinv:
                return TrySkipCStringAtZero(streamSize, stream, 5, out skip);
            case DemNetevent:
                return TrySkipCStringAtZero(streamSize, stream, 14, out skip);
            case DemZscCmd:
                return TrySkipZscCmdFromStream(streamSize, stream, out skip);
            case DemSummon2:
            case DemSummonfriend2:
            case DemSummonfoe2:
                return TrySkipCStringAtZero(streamSize, stream, 25, out skip);
            case DemChangemap2:
                return TrySkipChangeMap2FromStream(streamSize, stream, out skip);
            case DemMusicchange:
            case DemPrint:
            case DemCenterprint:
            case DemUinfchanged:
            case DemChangemap:
            case DemSummon:
            case DemSummonfriend:
            case DemSummonfoe:
            case DemSummonmbf:
            case DemRemove:
            case DemSpray:
            case DemMorphex:
            case DemKillclasscheat:
            case DemMdk:
                return TrySkipCStringOnlyFromStream(streamSize, stream, out skip);
            case DemWarpcheat:
                return TrySkipFixed(6, out skip);
            case DemInvuse:
            case DemFov:
            case DemMyfov:
            case DemChangeskill:
                return TrySkipFixed(4, out skip);
            case DemInvdrop:
                return TrySkipFixed(8, out skip);
            case DemGenericcheat:
            case DemDropplayer:
            case DemAddcontroller:
            case DemDelcontroller:
            case DemKick:
            case DemWeapselect:
                return TrySkipFixed(1, out skip);
            case DemSavegame:
                return TrySkipSaveGame(streamSize, stream, out skip);
            case DemSinfchangedxor:
                return TrySkipSinFChanged(streamSize, stream, true, out skip);
            case DemSinfchanged:
                return TrySkipSinFChanged(streamSize, stream, false, out skip);
            case DemRunscript:
            case DemRunscript2:
                return TrySkipRunScriptFromStream(streamSize, stream, out skip);
            case DemRunnamedscript:
                return TrySkipRunNamedScriptFromStream(streamSize, stream, out skip);
            case DemRunspecial:
                return TrySkipRunScriptFromStream(streamSize, stream, out skip);
            case DemConvreply:
                return TrySkipFixed(3, out skip);
            case DemSetslot:
                return TrySkipSetSlot(streamSize, stream, false, out skip);
            case DemSetslotpnum:
                return TrySkipSetSlot(streamSize, stream, true, out skip);
            case DemAddslot:
            case DemAddslotdefault:
                return TrySkipAddSlot(streamSize, stream, out skip);
            case DemSetpitchlimit:
                return TrySkipFixed(2, out skip);
            default:
                if (IsZeroPayloadCommand(cmd))
                {
                    skip = 0;
                    return true;
                }

                return false;
        }
    }

    public static bool TrySkipCommand(int cmd, ReadOnlySpan<byte> stream, out int skip)
    {
        if (!TryComputeCommandSkip(cmd, stream, out skip))
            return false;
        return CanAdvance(skip, stream.Length);
    }

    public static bool CanAdvance(int skip, int streamSize) => skip <= streamSize;

    public static int AdvanceCursor(int cursor, int skip) => cursor + skip;

    public static bool UnknownCommand(int cmd)
    {
        if (IsZeroPayloadCommand(cmd))
            return false;
        return cmd switch
        {
            DemSay or DemTaunt or DemAddbot or DemGivecheat or DemTakecheat or DemSetinv or DemNetevent
                or DemZscCmd or DemSummon2 or DemSummonfriend2 or DemSummonfoe2 or DemChangemap2
                or DemMusicchange or DemPrint or DemCenterprint or DemUinfchanged or DemChangemap
                or DemSummon or DemSummonfriend or DemSummonfoe or DemSummonmbf or DemRemove or DemSpray
                or DemMorphex or DemKillclasscheat or DemMdk or DemWarpcheat or DemInvuse or DemFov
                or DemMyfov or DemChangeskill or DemInvdrop or DemGenericcheat or DemDropplayer
                or DemAddcontroller or DemDelcontroller or DemKick or DemWeapselect or DemSavegame
                or DemSinfchangedxor or DemSinfchanged or DemRunscript or DemRunscript2 or DemRunnamedscript
                or DemRunspecial or DemConvreply or DemSetslot or DemSetslotpnum or DemAddslot
                or DemAddslotdefault or DemSetpitchlimit => false,
            _ => true,
        };
    }
}
