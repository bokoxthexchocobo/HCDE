namespace HCDE.Playsim;

/// <summary>
/// Actor-delta codecs, view-error decay, and invasion snapshot header from d_net.cpp 3071-3349.
/// </summary>
public static class VmNet1451Slice
{
    public const int BandStart = 3071;
    public const int BandEnd = 3349;
    public const int MaxPlayers = 64;
    public const int LocalTics = 525;
    public const int BackupTics = 175;
    public const int UserCmdBytes = 16;
    public const int AuthorityLane = 2;
    public const int ActorLane = 4;

    public struct Cmd
    {
        public uint Buttons;
        public short Pitch, Yaw, Roll, Forward, Side, Up;
    }

    public sealed class World
    {
        public int Console = -1;
        public int TicDup = 1;
        public int ClientTic;
        public bool Smooth;
        public double DecayFactor = 1;
        public bool Active;
        public double X, Y, Z, Yaw, Pitch;
        public bool HasMo;
        public double VelLengthSq;
        public bool Invasion;
        public ulong Caps;
        public bool Boss;
        public bool Fallback;
        public int State, StateTics, Wave, MaxWaves, Budget, Spawned, Cleared, Monsters;
        public int Spots, ActiveSpots, PlanBudget, Tag, FallbackSource;
        public int Bandwidth;
        public bool EventsOk = true;
        public bool DeltasOk = true;
        public int Packets;
        public int Bytes;
        public Cmd[] Local = new Cmd[LocalTics];
        public Cmd[,] Remote = new Cmd[MaxPlayers, BackupTics];
        public List<string> Logs = new();
        public List<string> Calls = new();
    }

    static readonly int[,] Budgets =
    {
        { 96, 4096, 384, 4096, 900, 512, 512 },
        { 96, 4096, 640, 4096, 1800, 768, 512 },
        { 96, 4096, 896, 4096, 3000, 1024, 512 },
    };

    public static int QuantizePos(double value)
    {
        if (!double.IsFinite(value))
            return 0;
        long q = (long)Math.Round(value * 16d, MidpointRounding.AwayFromZero);
        return (int)Math.Clamp(q, int.MinValue, int.MaxValue);
    }

    public static short QuantizeVel(double value)
    {
        if (!double.IsFinite(value))
            return 0;
        long q = (long)Math.Round(value * 32d, MidpointRounding.AwayFromZero);
        return (short)Math.Clamp(q, short.MinValue, short.MaxValue);
    }

    public static double DequantizePos(int value) => value / 16d;

    public static double DequantizeVel(short value) => value / 32d;

    public static ushort CompactAngle(uint bam) => (ushort)(bam >> 16);

    public static uint ExpandAngle(ushort compact) => (uint)compact << 16;

    public static (bool Ok, int Cursor) AppendQuantizedPos(byte[] output, int cursor, double value, List<string> logs)
        => Be32(output, cursor, unchecked((uint)QuantizePos(value)), logs);

    public static (bool Ok, int Cursor) AppendQuantizedVel(byte[] output, int cursor, double value, List<string> logs)
        => Be16(output, cursor, unchecked((ushort)QuantizeVel(value)), logs);

    public static (bool Ok, double Value, int Cursor) ReadQuantizedPos(byte[] data, int cursor, List<string> logs)
    {
        var raw = ReadBe32(data, cursor, logs);
        if (!raw.Ok)
            return (false, 0, raw.Cursor);
        return (true, DequantizePos(unchecked((int)raw.Value)), raw.Cursor);
    }

    public static (bool Ok, double Value, int Cursor) ReadQuantizedVel(byte[] data, int cursor, List<string> logs)
    {
        var raw = ReadBe16(data, cursor, logs);
        if (!raw.Ok)
            return (false, 0, raw.Cursor);
        return (true, DequantizeVel(unchecked((short)raw.Value)), raw.Cursor);
    }

    public static (bool Ok, int OutCursor, int InCursor) AppendField(byte[] output, int outCursor, byte[] data, int inCursor, int size, List<string> logs)
    {
        if (inCursor >= data.Length)
        {
            logs.Add("HCDE append field failed: inputCursor=" + inCursor + " dataSize=" + data.Length + " reason=input_ge_size");
            return (false, outCursor, inCursor);
        }
        if (size > data.Length - inCursor)
        {
            logs.Add("HCDE append field failed: inputCursor=" + inCursor + " dataSize=" + data.Length + " size=" + size + " reason=insufficient_data");
            return (false, outCursor, inCursor);
        }
        var copied = Bytes(output, outCursor, data, inCursor, size, logs);
        if (!copied.Ok)
        {
            logs.Add("HCDE append field failed: outputCursor=" + outCursor + " outputCapacity=" + output.Length + " size=" + size + " reason=append_failed");
            return (false, outCursor, inCursor);
        }
        return (true, copied.Cursor, inCursor + size);
    }

    public static Cmd? BuildBasis(World w, int player, uint sequence)
    {
        if (sequence == 0)
            return null;
        uint last = sequence - 1;
        if (player == w.Console)
        {
            int dup = w.TicDup > 0 ? w.TicDup : 0;
            int index = (int)((last * (uint)dup) % LocalTics);
            return w.Local[index];
        }
        if (player >= 0 && player < MaxPlayers)
            return w.Remote[player, (int)(last % BackupTics)];
        return null;
    }

    public static Cmd? ReceiveBasis(World w, int player, uint sequence)
    {
        if (sequence == 0 || player < 0 || player >= MaxPlayers)
            return null;
        return w.Remote[player, (int)((sequence - 1) % BackupTics)];
    }

    public static (bool Ok, int Cursor) AppendUserCmd(byte[] output, int cursor, Cmd command, List<string> logs)
    {
        var step = Be32(output, cursor, command.Buttons, logs);
        if (!step.Ok) return step;
        step = Be16(output, step.Cursor, unchecked((ushort)command.Pitch), logs);
        if (!step.Ok) return step;
        step = Be16(output, step.Cursor, unchecked((ushort)command.Yaw), logs);
        if (!step.Ok) return step;
        step = Be16(output, step.Cursor, unchecked((ushort)command.Roll), logs);
        if (!step.Ok) return step;
        step = Be16(output, step.Cursor, unchecked((ushort)command.Forward), logs);
        if (!step.Ok) return step;
        step = Be16(output, step.Cursor, unchecked((ushort)command.Side), logs);
        if (!step.Ok) return step;
        return Be16(output, step.Cursor, unchecked((ushort)command.Up), logs);
    }

    public static (bool Ok, Cmd Command, int Cursor) ReadUserCmdPrefix(byte[] data, int cursor, List<string> logs)
    {
        if (cursor >= data.Length)
        {
            logs.Add("HCDE read usercmd failed: cursor=" + cursor + " dataSize=" + data.Length + " required=16 reason=cursor_ge_size");
            return (false, default, cursor);
        }
        if (data.Length - cursor < UserCmdBytes)
        {
            logs.Add("HCDE read usercmd failed: cursor=" + cursor + " dataSize=" + data.Length + " required=16 reason=insufficient_data");
            return (false, default, cursor);
        }
        var command = new Cmd
        {
            Buttons = ReadBe32(data, cursor, logs).Value,
            Pitch = unchecked((short)ReadBe16(data, cursor + 4, logs).Value),
            Yaw = unchecked((short)ReadBe16(data, cursor + 6, logs).Value),
            Roll = unchecked((short)ReadBe16(data, cursor + 8, logs).Value),
        };
        return (true, command, cursor + 10);
    }

    public static void DecayView(World w)
    {
        if (!w.Smooth)
            return;
        if (w.Active && w.Console >= 0 && w.Console < MaxPlayers && w.HasMo)
        {
            int dup = Math.Max(w.TicDup, 1);
            int start = Math.Max(w.ClientTic - 4 * dup, 0);
            bool moved = false;
            for (int tic = start; tic < w.ClientTic; tic++)
            {
                var cmd = w.Local[tic % LocalTics];
                if (cmd.Forward != 0 || cmd.Side != 0 || cmd.Up != 0)
                {
                    moved = true;
                    break;
                }
            }
            if (!moved)
            {
                w.X = 0;
                w.Y = 0;
                if (w.VelLengthSq < 0.01)
                {
                    w.Z = 0;
                    w.Yaw = 0;
                    w.Pitch = 0;
                    w.Active = false;
                }
            }
        }
        w.X *= w.DecayFactor;
        w.Y *= w.DecayFactor;
        w.Z *= w.DecayFactor;
        w.Yaw *= w.DecayFactor;
        w.Pitch *= w.DecayFactor;
        if (w.X * w.X + w.Y * w.Y + w.Z * w.Z < 0.01 && Math.Abs(w.Yaw) < 0.01 && Math.Abs(w.Pitch) < 0.01)
        {
            w.X = w.Y = w.Z = w.Yaw = w.Pitch = 0;
            w.Active = false;
        }
    }

    public static bool ViewOffset(World w, out double x, out double y, out double z, out double yaw, out double pitch)
    {
        x = y = z = yaw = pitch = 0;
        if (!w.Smooth || !w.Active)
            return false;
        x = w.X;
        y = w.Y;
        z = w.Z;
        yaw = w.Yaw;
        pitch = w.Pitch;
        return true;
    }

    public static (bool Ok, int Cursor) AppendInvasion(World w, int client, byte[] output, int cursor)
    {
        if (client < 0 || client >= MaxPlayers)
            return (false, cursor);
        if (!w.Invasion || (w.Caps & (1UL << 4)) == 0)
            return (true, cursor);
        int start = cursor;
        bool deltas = (w.Caps & (1UL << 17)) != 0 && (w.Caps & (1UL << 16)) != 0;
        bool events = (w.Caps & (1UL << 19)) != 0;
        byte flags = w.Boss ? (byte)1 : (byte)0;
        byte spawn = w.Fallback ? (byte)1 : (byte)0;
        var logs = w.Logs;
        var step = Bytes(output, cursor, new byte[] { (byte)'H', (byte)'C', (byte)'I', (byte)'V' }, 0, 4, logs);
        if (!step.Ok) return (false, cursor);
        step = Byte(output, step.Cursor, 2, logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Byte(output, step.Cursor, flags, logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Byte(output, step.Cursor, (byte)Math.Clamp(w.State, 0, 7), logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Byte(output, step.Cursor, 0, logs);
        if (!step.Ok) return (false, step.Cursor);
        foreach (int number in new[] { w.StateTics, w.Wave, w.MaxWaves, w.Budget, w.Spawned, w.Cleared, w.Monsters })
        {
            step = Be32(output, step.Cursor, (uint)Math.Max(number, 0), logs);
            if (!step.Ok) return (false, step.Cursor);
        }
        step = Be16(output, step.Cursor, (ushort)Math.Clamp(w.Spots, 0, 65535), logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Be16(output, step.Cursor, (ushort)Math.Clamp(w.ActiveSpots, 0, 65535), logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Be32(output, step.Cursor, (uint)Math.Max(w.PlanBudget, 0), logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Be32(output, step.Cursor, (uint)Math.Max(w.Tag, 0), logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Byte(output, step.Cursor, spawn, logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Byte(output, step.Cursor, (byte)Math.Clamp(w.FallbackSource, 0, 4), logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Byte(output, step.Cursor, 0, logs);
        if (!step.Ok) return (false, step.Cursor);
        step = Byte(output, step.Cursor, 0, logs);
        if (!step.Ok) return (false, step.Cursor);
        w.Calls.Add("tx " + AuthorityLane + " " + client + " " + (step.Cursor - start));
        int end = Math.Min(output.Length, step.Cursor + 1200);
        if (events)
        {
            int budget = BudgetEnd(w, AuthorityLane, step.Cursor, end);
            w.Calls.Add("events " + budget);
            if (!w.EventsOk)
                return (false, step.Cursor);
        }
        if (deltas)
        {
            int budget = BudgetEnd(w, ActorLane, step.Cursor, end);
            w.Calls.Add("deltas " + budget);
            if (!w.DeltasOk)
                return (false, step.Cursor);
        }
        w.Packets++;
        w.Bytes += step.Cursor - start;
        return (true, step.Cursor);
    }


    static int BudgetEnd(World w, int lane, int cursor, int capacity)
    {
        int budget = (w.Caps & (1UL << 18)) == 0 || lane >= 7 ? 0 : Budgets[w.Bandwidth, lane];
        if (budget == 0 || cursor >= capacity)
            return capacity;
        int available = Math.Min(capacity - cursor, budget);
        int end = cursor + available;
        if (end < capacity)
            w.Calls.Add("clamp " + lane);
        return end;
    }

    static (bool Ok, int Cursor) Byte(byte[] output, int cursor, byte value, List<string> logs)
    {
        if (cursor >= output.Length)
            return (false, cursor);
        output[cursor] = value;
        return (true, cursor + 1);
    }

    static (bool Ok, int Cursor) Be16(byte[] output, int cursor, ushort value, List<string> logs)
    {
        if (output.Length - cursor < 2)
            return (false, cursor);
        output[cursor] = (byte)(value >> 8);
        output[cursor + 1] = (byte)value;
        return (true, cursor + 2);
    }

    static (bool Ok, int Cursor) Be32(byte[] output, int cursor, uint value, List<string> logs)
    {
        if (output.Length - cursor < 4)
            return (false, cursor);
        output[cursor] = (byte)(value >> 24);
        output[cursor + 1] = (byte)(value >> 16);
        output[cursor + 2] = (byte)(value >> 8);
        output[cursor + 3] = (byte)value;
        return (true, cursor + 4);
    }

    static (bool Ok, ushort Value, int Cursor) ReadBe16(byte[] data, int cursor, List<string> logs)
    {
        if (data.Length - cursor < 2)
            return (false, 0, cursor);
        return (true, (ushort)((data[cursor] << 8) | data[cursor + 1]), cursor + 2);
    }

    static (bool Ok, uint Value, int Cursor) ReadBe32(byte[] data, int cursor, List<string> logs)
    {
        if (data.Length - cursor < 4)
            return (false, 0, cursor);
        uint value = ((uint)data[cursor] << 24) | ((uint)data[cursor + 1] << 16) | ((uint)data[cursor + 2] << 8) | data[cursor + 3];
        return (true, value, cursor + 4);
    }

    static (bool Ok, int Cursor) Bytes(byte[] output, int cursor, byte[] data, int offset, int size, List<string> logs)
    {
        if (cursor >= output.Length || size > output.Length - cursor)
            return (false, cursor);
        if (size != 0)
            Array.Copy(data, offset, output, cursor, size);
        return (true, cursor + size);
    }
}
