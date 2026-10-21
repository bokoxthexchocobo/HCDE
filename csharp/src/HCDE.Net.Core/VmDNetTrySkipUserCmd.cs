namespace HCDE.Net.Core;

/// <summary><c>Net_TrySkipUserCmdMessage</c> and record boundary helpers (<c>d_net.cpp</c> 9508+).</summary>
public static class VmDNetTrySkipUserCmd
{
    public const byte DemUserCmd = 1;
    public const byte DemEmptyUserCmd = 2;
    public const byte UcmdfButtons = 1;
    public const byte UcmdfPitch = 2;
    public const byte UcmdfYaw = 4;
    public const byte UcmdfForwardMove = 8;
    public const byte UcmdfSideMove = 16;
    public const byte UcmdfUpMove = 32;
    public const byte UcmdfRoll = 64;
    public const byte MoreButtons = 128;

    public static bool IsUserCmdTypeNative(int type) => type == DemUserCmd;

    public static bool IsEmptyUserCmdTypeNative(int type) => type == DemEmptyUserCmd;

    public static bool HasButtonsFlagNative(byte flags) => (flags & UcmdfButtons) != 0;

    public static bool HasMoreButtonsNative(byte input) => (input & MoreButtons) != 0;

    public static bool TrySkipAxisNative(int streamSize, int offset, out int newOffset) =>
        TryAdvance(streamSize, offset, 2, out newOffset);

    public static bool ShouldSkipPitchNative(byte flags) => (flags & UcmdfPitch) != 0;

    public static bool ShouldSkipYawNative(byte flags) => (flags & UcmdfYaw) != 0;

    public static bool ShouldSkipForwardMoveNative(byte flags) => (flags & UcmdfForwardMove) != 0;

    public static bool ShouldSkipSideMoveNative(byte flags) => (flags & UcmdfSideMove) != 0;

    public static bool ShouldSkipUpMoveNative(byte flags) => (flags & UcmdfUpMove) != 0;

    public static bool ShouldSkipRollNative(byte flags) => (flags & UcmdfRoll) != 0;

    public static bool AllowImplicitEmptyAtEndNative(bool allowImplicitEmptyAtEnd, bool sawEventCommand) =>
        allowImplicitEmptyAtEnd && sawEventCommand;

    public static bool CommandOffsetInRangeNative(int commandOffset, int commandTics) =>
        commandOffset < commandTics && commandOffset < 64;

    public static bool CommandOffsetAlreadySeenNative(ulong offsetsSeen, int commandOffset) =>
        (offsetsSeen & (1ul << commandOffset)) != 0;

    public static ulong MarkCommandOffsetSeenNative(ulong offsetsSeen, int commandOffset) =>
        offsetsSeen | (1ul << commandOffset);

    public static bool RecordBytesFitNative(ulong recordBytes, int dataSize, int cursor) =>
        recordBytes <= (ulong)(dataSize - cursor);

    public static bool CursorConsumedAllDataNative(int cursor, int dataSize) => cursor == dataSize;

    public static bool PlausibleNextCommandOffsetNative(
        int nextOffset,
        int commandTics,
        ulong commandOffsetsSeen,
        bool nextByteIsUserCommand) =>
        nextOffset < commandTics
        && nextOffset < 64
        && (commandOffsetsSeen & (1ul << nextOffset)) == 0
        && !nextByteIsUserCommand;

    public static bool NextByteIsUserCommandNative(int nextOffset) =>
        nextOffset == DemUserCmd || nextOffset == DemEmptyUserCmd;

    private static bool TryAdvance(int streamSize, int offset, int bytes, out int newOffset)
    {
        newOffset = offset;
        if (offset + bytes > streamSize)
            return false;
        newOffset = offset + bytes;
        return true;
    }
}
