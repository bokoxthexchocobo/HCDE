namespace HCDE.Net.Core;

/// <summary>Legacy master heartbeat packet helpers (<c>sv_master.cpp</c> 674–835).</summary>
public static class VmSvMasterHeartbeat
{
    public const uint ServerHeartbeatMarker = 0xFFFFFFFF;

    public static void WriteLongNative(Span<byte> packet, ref int offset, uint value)
    {
        packet[offset++] = (byte)(value & 0xff);
        packet[offset++] = (byte)((value >> 8) & 0xff);
        packet[offset++] = (byte)((value >> 16) & 0xff);
        packet[offset++] = (byte)((value >> 24) & 0xff);
    }

    public static void WriteShortNative(Span<byte> packet, ref int offset, ushort value)
    {
        packet[offset++] = (byte)(value & 0xff);
        packet[offset++] = (byte)((value >> 8) & 0xff);
    }

    public static bool ShouldSkipHeartbeatNative(int masterSocket) => masterSocket < 0;

    public static int BuildLegacyHeartbeatPacketNative(Span<byte> packet, ushort gamePort)
    {
        var offset = 0;
        WriteLongNative(packet, ref offset, ServerHeartbeatMarker);
        WriteShortNative(packet, ref offset, gamePort);
        return offset;
    }

    public static bool MasterListContainsNative(string listEntry, string candidate)
    {
        if (listEntry.Length != candidate.Length)
            return false;
        for (var i = 0; i < listEntry.Length; i++)
        {
            if (char.ToLowerInvariant(listEntry[i]) != char.ToLowerInvariant(candidate[i]))
                return false;
        }
        return true;
    }

    public static bool MasterMatchesPrefixNative(string masterIp, string needle)
    {
        if (needle.Length > masterIp.Length)
            return false;
        for (var i = 0; i < needle.Length; i++)
        {
            if (char.ToLowerInvariant(masterIp[i]) != char.ToLowerInvariant(needle[i]))
                return false;
        }
        return true;
    }

    public static bool IsMissingStartupMasterValueNative(
        int argIndex,
        int argc,
        Func<int, string?>? getArg)
    {
        if (argIndex + 1 >= argc)
            return true;
        var next = getArg?.Invoke(argIndex + 1);
        if (string.IsNullOrEmpty(next))
            return true;
        return next[0] == '-' || next[0] == '+';
    }

    public static bool ResolveStartupAdvertisingNative(
        ref bool resolved,
        ref bool svUseMasters,
        bool noAdvertiseFlag,
        bool advertiseFlag)
    {
        if (resolved)
            return svUseMasters;
        resolved = true;
        if (noAdvertiseFlag)
        {
            svUseMasters = false;
            return false;
        }
        if (advertiseFlag)
            svUseMasters = true;
        return svUseMasters;
    }

    public static bool SendHeartbeatNative(
        int masterSocket,
        byte[] packet,
        int packetSize,
        Func<byte[], int, bool> sendTo,
        Action<string>? debugMark,
        Action<string>? printError)
    {
        if (masterSocket < 0)
            return false;
        if (!sendTo(packet, packetSize))
        {
            debugMark?.Invoke("heartbeat failed");
            printError?.Invoke("Failed to contact master server: send failed");
            return false;
        }
        debugMark?.Invoke("heartbeat sent");
        return true;
    }

    public static void UpdateMasterServersInternalNative(
        IReadOnlyList<MasterServerHeartbeatEntry> masters,
        bool forceRegister,
        Func<MasterServerHeartbeatEntry, VmSvMasterNms1Wait.Nms1HeartbeatResult> tryHeartbeat,
        Action<MasterServerHeartbeatEntry, bool> tryRegister,
        Action<MasterServerHeartbeatEntry> sendHeartbeat)
    {
        foreach (var master in masters)
        {
            if (master.Nms1Registered)
            {
                if (tryHeartbeat(master) == VmSvMasterNms1Wait.Nms1HeartbeatResult.NeedsRegister)
                    tryRegister(master, true);
            }
            else
                tryRegister(master, forceRegister);
            sendHeartbeat(master);
        }
    }

    public static void UnregisterMasterServersInternalNative(
        IReadOnlyList<MasterServerHeartbeatEntry> masters,
        Action<MasterServerHeartbeatEntry> tryUnregister)
    {
        foreach (var master in masters)
            tryUnregister(master);
    }

    public static void AddStartupMastersNative(
        Func<int, int> checkMasterParm,
        Func<int, string?> getArg,
        int argc,
        Action<string> addMaster,
        Action<string> printMissingValue)
    {
        var start = 1;
        for (;;)
        {
            var arg = checkMasterParm(start);
            if (arg == 0)
                break;
            if (IsMissingStartupMasterValueNative(arg, argc, getArg))
            {
                printMissingValue("Missing value for -master; expected host[:port]");
                start = arg + 1;
                continue;
            }
            var value = getArg(arg + 1);
            if (!string.IsNullOrEmpty(value))
                addMaster(value);
            start = arg + 2;
        }
    }
}

public readonly record struct MasterServerHeartbeatEntry(bool Nms1Registered);
