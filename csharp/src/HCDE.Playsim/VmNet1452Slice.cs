namespace HCDE.Playsim;

/// <summary>
/// User-command record skipping from d_net.cpp 9508-9676.
/// Event opcodes are delegated; that switch is already credited.
/// </summary>
public static class VmNet1452Slice
{
    public const int BandStart = 9508;
    public const int BandEnd = 9676;
    public const byte UserCmd = 1;
    public const byte EmptyUserCmd = 2;
    public const int MoreButtons = 0x80;
    public const int Buttons = 0x01;
    public const int Pitch = 0x02;
    public const int Yaw = 0x04;
    public const int Forward = 0x08;
    public const int Side = 0x10;
    public const int Up = 0x20;
    public const int Roll = 0x40;

    public sealed class Bytes
    {
        public byte[] Data = System.Array.Empty<byte>();
        public int At;
        public int End;
        public int Count => End - At;
        public byte this[int index] => Data[At + index];
        public void Advance(int count) => At += count;
    }

    public delegate bool SkipEvent(int command, Bytes stream);

    public static bool TrySkipUserCmd(Bytes stream, bool allowImplicitEmpty, SkipEvent skip)
    {
        bool sawEvent = false;
        while (stream.Count > 0)
        {
            byte type = stream[0];
            stream.Advance(1);
            if (type == UserCmd)
            {
                if (stream.Count < 1)
                    return false;
                byte flags = stream[0];
                stream.Advance(1);
                if ((flags & Buttons) != 0)
                {
                    if (stream.Count < 1)
                        return false;
                    byte button = stream[0];
                    stream.Advance(1);
                    if ((button & MoreButtons) != 0)
                    {
                        if (stream.Count < 1)
                            return false;
                        button = stream[0];
                        stream.Advance(1);
                        if ((button & MoreButtons) != 0)
                        {
                            if (stream.Count < 1)
                                return false;
                            button = stream[0];
                            stream.Advance(1);
                            if ((button & MoreButtons) != 0)
                            {
                                if (stream.Count < 1)
                                    return false;
                                stream.Advance(1);
                            }
                        }
                    }
                }
                if (((flags & Pitch) != 0 && !SkipAxis(stream))
                    || ((flags & Yaw) != 0 && !SkipAxis(stream))
                    || ((flags & Forward) != 0 && !SkipAxis(stream))
                    || ((flags & Side) != 0 && !SkipAxis(stream))
                    || ((flags & Up) != 0 && !SkipAxis(stream))
                    || ((flags & Roll) != 0 && !SkipAxis(stream)))
                    return false;
                return true;
            }
            if (type == EmptyUserCmd)
                return true;
            if (!skip(type, stream))
                return false;
            sawEvent = true;
        }
        return allowImplicitEmpty && sawEvent;
    }

    public static (bool Ok, int RecordBytes) TrySkipWithBoundary(
        byte[] data, int origin, int length, bool allowImplicitEmpty, byte commandTics,
        ulong offsetsSeen, byte remainingRecords, SkipEvent skip)
    {
        int cursor = 0;
        bool sawEvent = false;
        while (cursor < length)
        {
            int commandStart = cursor;
            byte type = data[origin + cursor];
            cursor++;
            if (type == UserCmd || type == EmptyUserCmd)
            {
                var view = new Bytes { Data = data, At = origin + commandStart, End = origin + length };
                if (!TrySkipUserCmd(view, false, skip))
                    return (false, 0);
                return (true, view.At - origin);
            }
            var events = new Bytes { Data = data, At = origin + cursor, End = origin + length };
            if (!skip(type, events))
                return (false, 0);
            cursor = events.At - origin;
            sawEvent = true;
            if (remainingRecords > 0 && cursor < length)
            {
                byte next = data[origin + cursor];
                bool nextIsCommand = next == UserCmd || next == EmptyUserCmd;
                bool plausible = next < commandTics && next < 64
                    && (offsetsSeen & (1UL << next)) == 0
                    && !nextIsCommand;
                if (plausible
                    && TrySkipRemaining(data, origin + cursor, length - cursor, commandTics, offsetsSeen, remainingRecords, skip))
                    return (true, cursor);
            }
        }
        if (allowImplicitEmpty && sawEvent)
            return (true, length);
        return (false, 0);
    }

    public static bool TrySkipRemaining(
        byte[] data, int origin, int length, byte commandTics, ulong offsetsSeen, byte remaining, SkipEvent skip)
    {
        int cursor = 0;
        ulong seen = offsetsSeen;
        for (int record = 0; record < remaining; record++)
        {
            if (cursor >= length)
                return false;
            byte offset = data[origin + cursor];
            cursor++;
            if (offset >= commandTics || offset >= 64)
                return false;
            ulong mask = 1UL << offset;
            if ((seen & mask) != 0)
                return false;
            seen |= mask;
            bool finalRecord = record + 1 == remaining;
            var skipped = TrySkipWithBoundary(
                data, origin + cursor, length - cursor, finalRecord, commandTics, seen,
                (byte)(remaining - record - 1), skip);
            if (!skipped.Ok || skipped.RecordBytes > length - cursor)
                return false;
            cursor += skipped.RecordBytes;
        }
        return cursor == length;
    }

    public static void SkipCommand(int command, Bytes stream, SkipEvent skip)
    {
        skip(command, stream);
    }

    static bool SkipAxis(Bytes stream)
    {
        if (stream.Count < 2)
            return false;
        stream.Advance(2);
        return true;
    }
}
