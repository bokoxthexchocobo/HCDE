namespace HCDE.Playsim;

/// <summary>
/// Stale-room recovery and field codecs from d_net.cpp 2814-3069.
/// </summary>
public static class VmNet1450Slice
{
    public const int BandStart = 2814;
    public const int BandEnd = 3069;
    public const int Intro = 8;
    public const int Cutscene = 9;
    public const int LiveHeader = 15;
    public const int GameplayHeader = 12;

    public sealed class World
    {
        public int Room;
        public int GameTic;
        public int ClientTic;
        public int GameState;
        public bool HasRunner;
        public bool LocalAuthority;
        public int AuthoritySlot;
        public ulong LastForce;
        public int EndJobs;
        public int StalledTicks;
        public List<string> Logs = new();
    }

    public static bool RecoverStaleRoom(World w, int client, int room, ulong now)
    {
        byte expected = (byte)(w.Room + 1);
        bool ahead = (byte)room == expected && client == w.AuthoritySlot;
        bool stuck = (w.GameState == Cutscene || w.GameState == Intro) && w.HasRunner;
        if (ahead && stuck && !w.LocalAuthority)
        {
            if (w.LastForce == 0 || now - w.LastForce >= 300)
            {
                w.LastForce = now;
                w.Logs.Add(
                    "client room desync recovery: authority (client=" + client
                    + ") is one room ahead (room=" + (uint)room
                    + " local=" + (uint)w.Room
                    + ") while we're still on a cutscene (gamestate=" + w.GameState
                    + " gametic=" + w.GameTic
                    + " clienttic=" + w.ClientTic
                    + "). Forcing local EndScreenJob");
                w.EndJobs++;
                w.StalledTicks++;
            }
        }
        return false;
    }

    public static (bool Ok, int PayloadSize, int PayloadIndex) CompleteEnvelope(int payloadSize)
    {
        return (true, payloadSize - GameplayHeader, LiveHeader + GameplayHeader);
    }

    public static (bool Ok, int Cursor) AppendByte(byte[] output, int cursor, byte value, List<string> logs)
    {
        if (cursor >= output.Length)
        {
            logs.Add("HCDE append byte failed: cursor=" + cursor + " outputCapacity=" + output.Length + " reason=cursor_ge_capacity");
            return (false, cursor);
        }
        output[cursor] = value;
        return (true, cursor + 1);
    }

    public static (bool Ok, int Cursor) AppendBE16(byte[] output, int cursor, ushort value, List<string> logs)
    {
        if (cursor >= output.Length)
        {
            logs.Add("HCDE append BE16 failed: cursor=" + cursor + " outputCapacity=" + output.Length + " required=2 reason=cursor_ge_capacity");
            return (false, cursor);
        }
        if (output.Length - cursor < 2)
        {
            logs.Add("HCDE append BE16 failed: cursor=" + cursor + " outputCapacity=" + output.Length + " required=2 reason=insufficient_capacity");
            return (false, cursor);
        }
        output[cursor] = (byte)(value >> 8);
        output[cursor + 1] = (byte)value;
        return (true, cursor + 2);
    }

    public static (bool Ok, int Cursor) AppendBE32(byte[] output, int cursor, uint value, List<string> logs)
    {
        if (cursor >= output.Length)
        {
            logs.Add("HCDE append BE32 failed: cursor=" + cursor + " outputCapacity=" + output.Length + " required=4 reason=cursor_ge_capacity");
            return (false, cursor);
        }
        if (output.Length - cursor < 4)
        {
            logs.Add("HCDE append BE32 failed: cursor=" + cursor + " outputCapacity=" + output.Length + " required=4 reason=insufficient_capacity");
            return (false, cursor);
        }
        output[cursor] = (byte)(value >> 24);
        output[cursor + 1] = (byte)(value >> 16);
        output[cursor + 2] = (byte)(value >> 8);
        output[cursor + 3] = (byte)value;
        return (true, cursor + 4);
    }

    public static (bool Ok, int Cursor) AppendBE64(byte[] output, int cursor, ulong value, List<string> logs)
    {
        if (cursor >= output.Length)
        {
            logs.Add("HCDE append BE64 failed: cursor=" + cursor + " outputCapacity=" + output.Length + " required=8 reason=cursor_ge_capacity");
            return (false, cursor);
        }
        if (output.Length - cursor < 8)
        {
            logs.Add("HCDE append BE64 failed: cursor=" + cursor + " outputCapacity=" + output.Length + " required=8 reason=insufficient_capacity");
            return (false, cursor);
        }
        for (int i = 0; i < 8; i++)
            output[cursor + i] = (byte)(value >> (56 - 8 * i));
        return (true, cursor + 8);
    }

    public static (bool Ok, int Cursor) AppendDouble(byte[] output, int cursor, double value, List<string> logs)
    {
        if (!double.IsFinite(value))
        {
            logs.Add("HCDE append double failed: cursor=" + cursor + " reason=not_finite_value");
            return (false, cursor);
        }
        ulong bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
        return AppendBE64(output, cursor, bits, logs);
    }

    public static (bool Ok, int Cursor) AppendFloat(byte[] output, int cursor, double value, List<string> logs)
    {
        float compact = (float)value;
        if (!float.IsFinite(compact))
        {
            logs.Add("HCDE append float failed: cursor=" + cursor + " reason=not_finite_value");
            return (false, cursor);
        }
        uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(compact));
        return AppendBE32(output, cursor, bits, logs);
    }

    public static (bool Ok, int Cursor) AppendBytes(byte[] output, int cursor, byte[] data, List<string> logs)
    {
        if (cursor >= output.Length)
        {
            logs.Add("HCDE append bytes failed: cursor=" + cursor + " outputCapacity=" + output.Length + " reason=cursor_ge_capacity");
            return (false, cursor);
        }
        if (data.Length > output.Length - cursor)
        {
            logs.Add("HCDE append bytes failed: cursor=" + cursor + " outputCapacity=" + output.Length + " size=" + data.Length + " reason=insufficient_capacity");
            return (false, cursor);
        }
        if (data.Length != 0)
            Array.Copy(data, 0, output, cursor, data.Length);
        return (true, cursor + data.Length);
    }

    public static (bool Ok, byte Value, int Cursor) ReadByte(byte[] data, int cursor, List<string> logs)
    {
        if (cursor >= data.Length)
        {
            logs.Add("HCDE read byte failed: cursor=" + cursor + " dataSize=" + data.Length);
            return (false, 0, cursor);
        }
        return (true, data[cursor], cursor + 1);
    }

    public static (bool Ok, ushort Value, int Cursor) ReadBE16(byte[] data, int cursor, List<string> logs)
    {
        if (cursor >= data.Length)
        {
            logs.Add("HCDE read BE16 failed: cursor=" + cursor + " dataSize=" + data.Length + " required=2 reason=cursor_ge_size");
            return (false, 0, cursor);
        }
        if (data.Length - cursor < 2)
        {
            logs.Add("HCDE read BE16 failed: cursor=" + cursor + " dataSize=" + data.Length + " required=2 reason=insufficient_data");
            return (false, 0, cursor);
        }
        ushort value = (ushort)((data[cursor] << 8) | data[cursor + 1]);
        return (true, value, cursor + 2);
    }

    public static (bool Ok, uint Value, int Cursor) ReadBE32(byte[] data, int cursor, List<string> logs)
    {
        if (cursor >= data.Length)
        {
            logs.Add("HCDE read BE32 failed: cursor=" + cursor + " dataSize=" + data.Length + " required=4 reason=cursor_ge_size");
            return (false, 0, cursor);
        }
        if (data.Length - cursor < 4)
        {
            logs.Add("HCDE read BE32 failed: cursor=" + cursor + " dataSize=" + data.Length + " required=4 reason=insufficient_data");
            return (false, 0, cursor);
        }
        uint value = ((uint)data[cursor] << 24) | ((uint)data[cursor + 1] << 16) | ((uint)data[cursor + 2] << 8) | data[cursor + 3];
        return (true, value, cursor + 4);
    }

    public static (bool Ok, double Value, int Cursor) ReadDouble(byte[] data, int cursor, List<string> logs)
    {
        if (cursor >= data.Length)
        {
            logs.Add("HCDE read double failed: cursor=" + cursor + " dataSize=" + data.Length + " required=8 reason=cursor_ge_size");
            return (false, 0, cursor);
        }
        if (data.Length - cursor < 8)
        {
            logs.Add("HCDE read double failed: cursor=" + cursor + " dataSize=" + data.Length + " required=8 reason=insufficient_data");
            return (false, 0, cursor);
        }
        ulong bits = 0;
        for (int i = 0; i < 8; i++)
            bits = (bits << 8) | data[cursor + i];
        cursor += 8;
        double value = BitConverter.Int64BitsToDouble(unchecked((long)bits));
        if (!double.IsFinite(value))
        {
            logs.Add("HCDE read double failed: cursor=" + cursor + " reason=not_finite_value");
            return (false, value, cursor);
        }
        return (true, value, cursor);
    }

    public static (bool Ok, double Value, int Cursor) ReadFloat(byte[] data, int cursor, List<string> logs)
    {
        var bits = ReadBE32(data, cursor, logs);
        if (!bits.Ok)
            return (false, 0, bits.Cursor);
        float compact = BitConverter.Int32BitsToSingle(unchecked((int)bits.Value));
        if (!float.IsFinite(compact))
        {
            logs.Add("HCDE read float failed: cursor=" + bits.Cursor + " reason=not_finite_value");
            return (false, 0, bits.Cursor);
        }
        return (true, compact, bits.Cursor);
    }
}
