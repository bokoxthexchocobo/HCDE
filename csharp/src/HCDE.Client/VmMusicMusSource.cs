/*
** music_mus_midiout.cpp
** Code to let ZDoom play MUS music through the MIDI streaming API.
**
**---------------------------------------------------------------------------
** Copyright 1998-2008 Marisa Heit
** All rights reserved.
**
** Redistribution and use in source and binary forms, with or without
** modification, are permitted provided that the following conditions
** are met:
**
** 1. Redistributions of source code must retain the above copyright
**    notice, this list of conditions and the following disclaimer.
** 2. Redistributions in binary form must reproduce the above copyright
**    notice, this list of conditions and the following disclaimer in the
**    documentation and/or other materials provided with the distribution.
** 3. The name of the author may not be used to endorse or promote products
**    derived from this software without specific prior written permission.
**
** THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
** IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
** OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
** IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
** INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
** NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
** DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
** THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
** (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
** THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
**---------------------------------------------------------------------------
*/

using System.Buffers.Binary;

namespace HCDE.Client;

public sealed class VmMusicMusSource : IVmMusicSmfSource
{
    private static readonly byte[] Controllers = [0, 0, 1, 7, 10, 11, 91, 93, 64, 67, 120, 123, 126, 127, 121];
    private readonly byte[] score;
    private readonly ushort channels;
    private readonly ushort instrumentCount;
    private readonly byte[] instruments;
    private readonly byte[] velocities = new byte[16];
    private int position;
    private bool disposed;
    public ushort Division => 140;
    public uint InitialTempo => 1000000;
    public int LoopLimit { get; set; }

    public VmMusicMusSource(ReadOnlySpan<byte> data)
    {
        var start = data[..Math.Min(data.Length, 32)].IndexOf("MUS\u001a"u8);
        if (start < 0) throw new InvalidDataException("MUS signature not found in the first 32 bytes.");
        data = data[start..];
        if (data.Length < 16) throw new InvalidDataException("Truncated MUS header.");
        var length = BinaryPrimitives.ReadUInt16LittleEndian(data[4..]);
        var offset = BinaryPrimitives.ReadUInt16LittleEndian(data[6..]);
        channels = BinaryPrimitives.ReadUInt16LittleEndian(data[8..]);
        if (channels > 15 || offset < 16 || offset > data.Length)
            throw new InvalidDataException("Invalid MUS channel count or score offset.");
        score = data.Slice(offset, Math.Min(length, data.Length - offset)).ToArray();
        instrumentCount = BinaryPrimitives.ReadUInt16LittleEndian(data[12..]);
        instruments = data.Slice(16, offset - 16).ToArray();
        Array.Fill(velocities, (byte)127);
    }

    public void CheckCapabilities(int technology) => ObjectDisposedException.ThrowIf(disposed, this);
    public IReadOnlyList<ushort> PrecacheData()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var result = new List<ushort>(instrumentCount);
        var cursor = 0;
        for (var i = 0; i < instrumentCount; i++)
        {
            var instrument = ReadInstrument(ref cursor);
            var banks = ReadInstrument(ref cursor);
            var supported = instrument < 128 || instrument is >= 135 and <= 188;
            var value = instrument < 128 ? instrument : instrument - 100 + (1 << 14);
            if (banks == 0)
            {
                if (supported) result.Add((ushort)value);
            }
            else
            {
                for (var bank = 0; bank < banks; bank++)
                {
                    var number = ReadInstrument(ref cursor);
                    if (supported) result.Add((ushort)(value | (number << 7)));
                }
            }
        }
        return result.AsReadOnly();
    }

    private byte ReadInstrument(ref int cursor)
    {
        if (cursor >= instruments.Length) throw new InvalidDataException("Truncated MUS instrument bank table.");
        return instruments[cursor++];
    }

    public void Restart()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        position = 0;
        Array.Fill(velocities, (byte)127);
    }
    public void StartPlayback(bool looped, int loopLimit)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(loopLimit);
        LoopLimit = loopLimit;
    }
    public bool CheckDone()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return position >= score.Length;
    }

    public int MakeEvents(Span<uint> buffer, uint maximumTime)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (buffer.Length < 6) throw new ArgumentException("MUS event buffer requires room for an event and trailing delay.", nameof(buffer));
        var maximumTicks = (ulong)maximumTime * Division / InitialTempo;
        ulong total = 0;
        uint delay = 0;
        var written = 0;
        while (position < score.Length && written <= buffer.Length - 6 && total <= maximumTicks)
        {
            var descriptor = ReadByte();
            var kind = descriptor & 112;
            if (kind == 96) { position = score.Length; break; }
            var value = ReadByte();
            var channel = descriptor & 15;
            channel = channel == 15 ? 9 : channel >= 9 ? channel + 1 : channel;
            int status, first, second;
            switch (kind)
            {
                case 0:
                    status = 144 | channel; first = value; second = 0;
                    break;
                case 16:
                    status = 144 | channel; first = value & 127;
                    if ((value & 128) != 0) velocities[channel] = ReadByte();
                    second = velocities[channel];
                    break;
                case 32:
                    status = 224 | channel; first = (value & 1) << 6; second = value >> 1;
                    break;
                case 48:
                    status = 176 | channel; first = Controller(value); second = value == 12 ? channels : 0;
                    break;
                case 64:
                    second = ReadByte();
                    if (value == 0) { status = 192 | channel; first = second; second = 0; }
                    else
                    {
                        status = 176 | channel; first = Controller(value);
                        if (first == 7) second = Math.Min(second, 127);
                    }
                    break;
                default:
                    position = score.Length;
                    goto Finish;
            }
            buffer[written++] = delay;
            buffer[written++] = 0;
            buffer[written++] = (uint)(status | (first << 8) | (second << 16));
            delay = 0;
            if ((descriptor & 128) != 0)
            {
                byte part;
                do
                {
                    part = ReadByte();
                    if (delay > (uint.MaxValue >> 7)) throw new InvalidDataException("MUS delay exceeds 32 bits.");
                    delay = (delay << 7) | (uint)(part & 127);
                } while ((part & 128) != 0);
            }
            total += delay;
        }
    Finish:
        if (delay != 0)
        {
            buffer[written++] = delay;
            buffer[written++] = 0;
            buffer[written++] = 0x02000000;
        }
        return written;
    }

    private byte ReadByte()
    {
        if (position >= score.Length) throw new InvalidDataException("Truncated MUS event or delay.");
        return score[position++];
    }
    private static byte Controller(byte value)
    {
        if (value >= Controllers.Length) throw new InvalidDataException("Invalid MUS controller index.");
        return Controllers[value];
    }
    public void Dispose() => disposed = true;
}
