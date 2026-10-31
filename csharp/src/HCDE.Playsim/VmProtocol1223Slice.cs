using System.Globalization;
using System.Text;

namespace HCDE.Playsim;

/// <summary>
/// User-command packing, demo-command scan, and IFF chunk lengths from
/// <c>d_protocol.cpp</c> (<b>1–387</b>). Chunk <b>1223</b>.
/// Look and movement axes are resent while they are nonzero. Roll is resent
/// only when it differs from the basis. Button bits 29–31 are not stored.
/// An odd IFF payload is padded, and a chunk length of -1 advances zero bytes.
/// </summary>
public sealed class VmProtocol1223
{
    public const int BackupTics = 175;
    public const int UserCmd = 1;
    public const int EmptyUserCmd = 2;
    public const int MoreButtons = 0x80;

    public int Gametic;
    public int TicDup;
    public Action<int, VmProtoView1223>? SkipCommand;
    public Action<int, VmProtoView1223, int>? DoCommand;

    readonly VmUserCmd1223[] _commands = new VmUserCmd1223[8 * BackupTics];
    readonly byte[][] _payloads = new byte[8 * BackupTics][];
    byte[]? _chunkBuf;
    int _chunkAt;
    bool _chunkLive;

    public VmProtocol1223()
    {
        TicDup = 1;
        for (int i = 0; i < _payloads.Length; i++)
            _payloads[i] = Array.Empty<byte>();
    }

    public VmUserCmd1223 CommandAt(int player, int tic)
    {
        return _commands[Slot(player, tic)];
    }

    public void SetCommand(int player, int tic, VmUserCmd1223 cmd)
    {
        _commands[Slot(player, tic)] = cmd;
    }

    public byte[] PayloadAt(int player, int tic)
    {
        return _payloads[Slot(player, tic)];
    }

    public void SetPayload(int player, int tic, byte[] data)
    {
        var copy = new byte[data.Length];
        Buffer.BlockCopy(data, 0, copy, 0, data.Length);
        _payloads[Slot(player, tic)] = copy;
    }

    public VmProtoArc1223 Serialize(VmProtoArc1223 arc, string key, ref VmUserCmd1223 cmd)
    {
        if (arc.BeginObject(key))
        {
            arc.U32("buttons", ref cmd.Buttons);
            arc.I16("pitch", ref cmd.Pitch);
            arc.I16("yaw", ref cmd.Yaw);
            arc.I16("roll", ref cmd.Roll);
            arc.I16("forwardmove", ref cmd.Forward);
            arc.I16("sidemove", ref cmd.Side);
            arc.I16("upmove", ref cmd.Up);
            arc.EndObject();
        }

        return arc;
    }

    public void Unpack(ref VmUserCmd1223 cmd, VmUserCmd1223? basis, bool sameAddress, VmProtoView1223 stream)
    {
        if (basis.HasValue)
        {
            if (!sameAddress)
                cmd = basis.Value;
        }
        else
        {
            cmd = default;
        }

        byte flags = ReadInt8(stream);
        if (flags != 0)
        {
            if ((flags & 0x01) != 0)
            {
                byte inn = ReadInt8(stream);
                uint buttons = (cmd.Buttons & ~0x7Fu) | (uint)(inn & 0x7F);
                if ((inn & MoreButtons) != 0)
                {
                    inn = ReadInt8(stream);
                    buttons = (buttons & ~(0x7Fu << 7)) | ((uint)(inn & 0x7F) << 7);
                    if ((inn & MoreButtons) != 0)
                    {
                        inn = ReadInt8(stream);
                        buttons = (buttons & ~(0x7Fu << 14)) | ((uint)(inn & 0x7F) << 14);
                        if ((inn & MoreButtons) != 0)
                        {
                            inn = ReadInt8(stream);
                            buttons = (buttons & ~(0xFFu << 21)) | ((uint)inn << 21);
                        }
                    }
                }

                cmd.Buttons = buttons;
            }

            if ((flags & 0x02) != 0)
                cmd.Pitch = ReadInt16(stream);
            if ((flags & 0x04) != 0)
                cmd.Yaw = ReadInt16(stream);
            if ((flags & 0x08) != 0)
                cmd.Forward = ReadInt16(stream);
            if ((flags & 0x10) != 0)
                cmd.Side = ReadInt16(stream);
            if ((flags & 0x20) != 0)
                cmd.Up = ReadInt16(stream);
            if ((flags & 0x40) != 0)
                cmd.Roll = ReadInt16(stream);
        }
    }

    public void Pack(VmUserCmd1223 cmd, VmUserCmd1223? basis, VmProtoView1223 stream)
    {
        byte flags = 0;
        var flagsPosition = new VmProtoView1223
        {
            Buf = stream.Buf,
            Pos = stream.Pos,
            End = stream.Pos + 1,
        };
        VmUserCmd1223 blank = default;
        VmUserCmd1223 basisCmd = basis ?? blank;
        Advance(stream, 1);
        uint changed = cmd.Buttons ^ basisCmd.Buttons;
        if (changed != 0)
        {
            byte b0 = (byte)(cmd.Buttons & 0x7F);
            byte b1 = (byte)((cmd.Buttons >> 7) & 0x7F);
            byte b2 = (byte)((cmd.Buttons >> 14) & 0x7F);
            byte b3 = (byte)((cmd.Buttons >> 21) & 0xFF);
            flags |= 0x01;
            if ((changed & 0xFFFFFF80u) != 0)
            {
                b0 |= MoreButtons;
                if ((changed & 0xFFFFC000u) != 0)
                {
                    b1 |= MoreButtons;
                    if ((changed & 0xFFE00000u) != 0)
                        b2 |= MoreButtons;
                }
            }

            WriteInt8(b0, stream);
            if ((b0 & MoreButtons) != 0)
            {
                WriteInt8(b1, stream);
                if ((b1 & MoreButtons) != 0)
                {
                    WriteInt8(b2, stream);
                    if ((b2 & MoreButtons) != 0)
                        WriteInt8(b3, stream);
                }
            }
        }

        if (cmd.Pitch != basisCmd.Pitch || cmd.Pitch != 0)
        {
            flags |= 0x02;
            WriteInt16(cmd.Pitch, stream);
        }

        if (cmd.Yaw != basisCmd.Yaw || cmd.Yaw != 0)
        {
            flags |= 0x04;
            WriteInt16(cmd.Yaw, stream);
        }

        if (cmd.Forward != basisCmd.Forward || cmd.Forward != 0)
        {
            flags |= 0x08;
            WriteInt16(cmd.Forward, stream);
        }

        if (cmd.Side != basisCmd.Side || cmd.Side != 0)
        {
            flags |= 0x10;
            WriteInt16(cmd.Side, stream);
        }

        if (cmd.Up != basisCmd.Up || cmd.Up != 0)
        {
            flags |= 0x20;
            WriteInt16(cmd.Up, stream);
        }

        if (cmd.Roll != basisCmd.Roll)
        {
            flags |= 0x40;
            WriteInt16(cmd.Roll, stream);
        }

        WriteInt8(flags, flagsPosition);
    }

    public void WriteMessage(VmUserCmd1223 cmd, VmUserCmd1223? basis, VmProtoView1223 stream)
    {
        if (basis == null)
        {
            if (cmd.Buttons != 0 || cmd.Pitch != 0 || cmd.Yaw != 0 || cmd.Roll != 0
                || cmd.Forward != 0 || cmd.Side != 0 || cmd.Up != 0)
            {
                WriteInt8(UserCmd, stream);
                Pack(cmd, null, stream);
                return;
            }
        }
        else if (cmd.Buttons != basis.Value.Buttons
            || cmd.Yaw != basis.Value.Yaw || cmd.Pitch != basis.Value.Pitch || cmd.Roll != basis.Value.Roll
            || cmd.Forward != basis.Value.Forward || cmd.Side != basis.Value.Side || cmd.Up != basis.Value.Up
            || cmd.Yaw != 0 || cmd.Pitch != 0
            || cmd.Forward != 0 || cmd.Side != 0 || cmd.Up != 0)
        {
            WriteInt8(UserCmd, stream);
            Pack(cmd, basis, stream);
            return;
        }

        WriteInt8(EmptyUserCmd, stream);
    }

    public void SkipMessage(VmProtoView1223 stream)
    {
        while (stream.Size > 0)
        {
            byte type = ReadInt8(stream);
            if (type == UserCmd)
            {
                byte flags = ReadInt8(stream);
                if ((flags & 0x01) != 0)
                {
                    byte inn = ReadInt8(stream);
                    if ((inn & MoreButtons) != 0)
                    {
                        inn = ReadInt8(stream);
                        if ((inn & MoreButtons) != 0)
                        {
                            inn = ReadInt8(stream);
                            if ((inn & MoreButtons) != 0)
                                ReadInt8(stream);
                        }
                    }
                }

                if ((flags & 0x02) != 0)
                    Advance(stream, 2);
                if ((flags & 0x04) != 0)
                    Advance(stream, 2);
                if ((flags & 0x08) != 0)
                    Advance(stream, 2);
                if ((flags & 0x10) != 0)
                    Advance(stream, 2);
                if ((flags & 0x20) != 0)
                    Advance(stream, 2);
                if ((flags & 0x40) != 0)
                    Advance(stream, 2);
                break;
            }

            if (type == EmptyUserCmd)
                break;

            int before = stream.Size;
            SkipCommand?.Invoke(type, stream);
            if (stream.Size >= before)
                break;
        }
    }

    public void ReadMessage(VmProtoView1223 stream, int player, int tic)
    {
        int slot = Slot(player, tic);
        int start = stream.Pos;
        int type = EmptyUserCmd;
        bool found = false;
        while (stream.Size > 0)
        {
            type = ReadInt8(stream);
            if (type == UserCmd || type == EmptyUserCmd)
            {
                found = true;
                break;
            }

            int before = stream.Size;
            SkipCommand?.Invoke(type, stream);
            if (stream.Size >= before)
                break;
        }

        int payload = found ? stream.Pos - start - 1 : stream.Pos - start;
        var copy = new byte[payload];
        if (payload > 0)
            Buffer.BlockCopy(stream.Buf, start, copy, 0, payload);
        _payloads[slot] = copy;
        if (found && type == UserCmd)
        {
            VmUserCmd1223 cmd = _commands[slot];
            VmUserCmd1223? basis = null;
            if (tic > 0)
                basis = _commands[Slot(player, tic - 1)];
            Unpack(ref cmd, basis, false, stream);
            _commands[slot] = cmd;
        }
        else if (tic > 0)
        {
            _commands[slot] = _commands[Slot(player, tic - 1)];
        }
        else
        {
            _commands[slot] = default;
        }
    }

    public void Run(int player, int tic)
    {
        if (unchecked(Gametic % TicDup) != 0)
            return;
        byte[] data = _payloads[Slot(player, tic)];
        var stream = new VmProtoView1223 { Buf = data, Pos = 0, End = data.Length };
        if (stream.Size != 0)
        {
            while (stream.Size > 0)
            {
                int cmd = ReadInt8(stream);
                DoCommand?.Invoke(cmd, stream, player);
            }
        }
    }

    public void StartChunk(int id, VmProtoView1223 stream)
    {
        WriteInt32(id, stream);
        _chunkBuf = stream.Buf;
        _chunkAt = stream.Pos;
        _chunkLive = true;
        Advance(stream, 4);
    }

    public void FinishChunk(VmProtoView1223 stream)
    {
        if (!_chunkLive || _chunkBuf == null)
            return;
        int len = stream.Pos - _chunkAt - 4;
        var hole = new VmProtoView1223 { Buf = _chunkBuf, Pos = _chunkAt, End = _chunkAt + 4 };
        WriteInt32(len, hole);
        if ((len & 1) != 0)
            WriteInt8(0, stream);
        _chunkLive = false;
        _chunkBuf = null;
    }

    public void SkipChunk(VmProtoView1223 stream)
    {
        int len = ReadInt32(stream);
        Advance(stream, len + (len & 1));
    }

    public void Advance(VmProtoView1223 stream, int bytes)
    {
        if ((uint)bytes > (uint)stream.Size)
            throw new InvalidOperationException("AdvanceStream");
        stream.Pos += bytes;
    }

    public byte ReadInt8(VmProtoView1223 stream)
    {
        if (stream.Size < 1)
            throw new InvalidOperationException("Attempted to read past end of stream");
        byte v = stream.Buf[stream.Pos];
        Advance(stream, 1);
        return v;
    }

    public short ReadInt16(VmProtoView1223 stream)
    {
        if (stream.Size < 2)
            throw new InvalidOperationException("Attempted to read past end of stream");
        int v = (stream.Buf[stream.Pos] << 8) | stream.Buf[stream.Pos + 1];
        Advance(stream, 2);
        return (short)v;
    }

    public int ReadInt32(VmProtoView1223 stream)
    {
        if (stream.Size < 4)
            throw new InvalidOperationException("Attempted to read past end of stream");
        int v = (stream.Buf[stream.Pos] << 24)
            | (stream.Buf[stream.Pos + 1] << 16)
            | (stream.Buf[stream.Pos + 2] << 8)
            | stream.Buf[stream.Pos + 3];
        Advance(stream, 4);
        return v;
    }

    public void WriteInt8(byte v, VmProtoView1223 stream)
    {
        if (stream.Size < 1)
            throw new InvalidOperationException("Attempted to write past end of stream");
        stream.Buf[stream.Pos] = v;
        Advance(stream, 1);
    }

    public void WriteInt16(short v, VmProtoView1223 stream)
    {
        if (stream.Size < 2)
            throw new InvalidOperationException("Attempted to write past end of stream");
        stream.Buf[stream.Pos] = (byte)(v >> 8);
        stream.Buf[stream.Pos + 1] = (byte)(v & 255);
        Advance(stream, 2);
    }

    public void WriteInt32(int v, VmProtoView1223 stream)
    {
        if (stream.Size < 4)
            throw new InvalidOperationException("Attempted to write past end of stream");
        stream.Buf[stream.Pos] = (byte)(v >> 24);
        stream.Buf[stream.Pos + 1] = (byte)((v >> 16) & 255);
        stream.Buf[stream.Pos + 2] = (byte)((v >> 8) & 255);
        stream.Buf[stream.Pos + 3] = (byte)(v & 255);
        Advance(stream, 4);
    }

    static int Slot(int player, int tic)
    {
        int mod = tic % BackupTics;
        return player * BackupTics + mod;
    }
}

public struct VmUserCmd1223
{
    public uint Buttons;
    public short Pitch;
    public short Yaw;
    public short Roll;
    public short Forward;
    public short Side;
    public short Up;
}

public sealed class VmProtoView1223
{
    public byte[] Buf = Array.Empty<byte>();
    public int Pos;
    public int End;
    public int Size => End - Pos;
}

public sealed class VmProtoArc1223
{
    public bool Open = true;
    public readonly StringBuilder Log = new();

    public bool BeginObject(string name)
    {
        Log.Append("B ");
        Log.Append(name);
        Log.Append('\n');
        return Open;
    }

    public void EndObject()
    {
        Log.Append("E\n");
    }

    public void U32(string key, ref uint value)
    {
        Log.Append("U ");
        Log.Append(key);
        Log.Append(' ');
        Log.Append(value.ToString(CultureInfo.InvariantCulture));
        Log.Append('\n');
    }

    public void I16(string key, ref short value)
    {
        Log.Append("S ");
        Log.Append(key);
        Log.Append(' ');
        Log.Append(value.ToString(CultureInfo.InvariantCulture));
        Log.Append('\n');
    }
}
