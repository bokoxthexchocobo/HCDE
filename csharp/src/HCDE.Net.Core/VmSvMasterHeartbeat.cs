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
}
