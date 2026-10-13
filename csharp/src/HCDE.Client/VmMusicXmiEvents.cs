/*
** music_xmi_midiout.cpp
** Code to let ZDoom play XMIDI music through the MIDI streaming API.
**
**---------------------------------------------------------------------------
** Copyright 2010 Marisa Heit
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
**
*/

namespace HCDE.Client;

public static class VmMusicXmiEvents
{
    public static VmMusicSmfTrack Decode(ReadOnlySpan<byte> data, int loopLimit = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(loopLimit);
        return new(500000, ReadEvents(data.ToArray(), loopLimit, false).ToList().AsReadOnly());
    }

    internal static IEnumerable<VmMusicSmfEvent> ReadEvents(ReadOnlyMemory<byte> data, int loopLimit, bool looping)
    {
        var events = new Queue<VmMusicSmfEvent>();
        var loops = new (int Begin, int Count)[4];
        var depth = 0;
        var offs = new VmMusicNoteOffQueue();
        var position = 0;
        ulong time = 0, emittedTime = 0;
        void Emit(ulong due, uint word, ReadOnlyMemory<byte> payload = default)
        {
            events.Enqueue(new(checked((uint)(due - emittedTime)), word, payload));
            emittedTime = due;
        }
        void Flush(ulong through)
        {
            while (offs.TryPeek(out _, out var due) && due <= through)
            {
                var word = offs.Dequeue();
                Emit(due, word);
            }
        }
        var controlSteps = 0;
        while (position < data.Length)
        {
            while (events.TryDequeue(out var pending))
            {
                controlSteps = 0;
                yield return pending;
            }
            if (looping && ++controlSteps > 4096) throw new InvalidDataException("XMI loop makes no event progress.");
            uint delay = 0;
            while (position < data.Length && data.Span[position] < 128)
                delay = checked(delay + data.Span[position++]);
            time = checked(time + delay);
            Flush(time);
            var status = ReadByte(data.Span, ref position);
            if (status == 255)
            {
                var kind = ReadByte(data.Span, ref position);
                var length = Variable(data.Span, ref position);
                _ = Payload(data.Span, ref position, length);
                // XMI uses a fixed 120 Hz clock; tempo metadata is ignored.
                if (time != emittedTime) Emit(time, 0x02000000);
                if (kind == 47) break;
                continue;
            }
            if (status is 240 or 247)
            {
                var length = Variable(data.Span, ref position);
                var payload = Payload(data.Span, ref position, length).ToArray();
                if (length > 0xffffffu - (status == 240 ? 1u : 0u))
                    throw new InvalidDataException("XMI SysEx exceeds the packed event limit.");
                if (status == 247 && length == 0)
                    throw new InvalidDataException("Empty XMI escape packet is unsupported.");
                var bytes = new byte[payload.Length + (status == 240 ? 1 : 0)];
                if (status == 240) bytes[0] = 240;
                payload.AsSpan().CopyTo(bytes.AsSpan(status == 240 ? 1 : 0));
                Emit(time, 0x80000000 | (uint)bytes.Length, bytes);
                continue;
            }
            if (status >= 240)
            {
                var count = VmMusicSystemMessages.DataLength(status);
                // SMF carries system-common and real-time messages in escape packets.
                var bytes = new byte[count + 1];
                bytes[0] = status;
                for (var i = 1; i < bytes.Length; i++)
                {
                    bytes[i] = ReadByte(data.Span, ref position);
                    if (bytes[i] >= 128) throw new InvalidDataException("XMI system data must be seven-bit values.");
                }
                Emit(time, 0x80000000 | (uint)bytes.Length, bytes);
                continue;
            }
            var first = ReadByte(data.Span, ref position);
            var second = (status >> 4) is 12 or 13 ? (byte)0 : ReadByte(data.Span, ref position);
            if (first >= 128 || second >= 128)
                throw new InvalidDataException("XMI channel data must be seven-bit values.");
            if ((status >> 4) == 11)
            {
                if (first is 116 or 117)
                {
                    if (first == 116)
                    {
                        if (depth < loops.Length)
                            loops[depth] = (position, VmMusicEmidi.ClampLoopCount(second, loopLimit));
                        depth++;
                    }
                    else if (depth > 0)
                    {
                        var index = depth - 1;
                        if (index < loops.Length)
                        {
                            var loop = loops[index];
                            // Infinite loops repeat only during live looping playback.
                            if (second < 64 || (loop.Count == 0 && !looping)) loop.Count = 1;
                            if (loop.Count == 0 || --loop.Count > 0) position = loop.Begin;
                            else depth = index;
                            loops[index] = loop;
                        }
                        else depth = index;
                    }
                    if (time != emittedTime) Emit(time, 0x02000000);
                    continue;
                }
                if (first is 110 or 111 or 112 or 113 or 115 or 118 or 119 or 120)
                {
                    if (time != emittedTime) Emit(time, 0x02000000);
                    continue;
                }
                if (first == 114) first = 0;
            }
            Emit(time, (uint)(status | (first << 8) | (second << 16)));
            if ((status >> 4) == 9)
            {
                var duration = Variable(data.Span, ref position);
                offs.Enqueue((uint)(0x90 | (status & 15) | (first << 8)), checked(time + duration));
            }
        }
        Flush(ulong.MaxValue);
        while (events.TryDequeue(out var pending)) yield return pending;
    }

    private static byte ReadByte(ReadOnlySpan<byte> data, ref int position)
    {
        if (position >= data.Length) throw new InvalidDataException("Truncated XMI event.");
        return data[position++];
    }

    private static uint Variable(ReadOnlySpan<byte> data, ref int position)
    {
        uint value = 0;
        for (var i = 0; i < 4; i++)
        {
            var part = ReadByte(data, ref position);
            value = (value << 7) | (uint)(part & 127);
            if (part < 128) return value;
        }
        throw new InvalidDataException("XMI variable-length value exceeds four bytes.");
    }

    private static ReadOnlySpan<byte> Payload(ReadOnlySpan<byte> data, ref int position, uint length)
    {
        if (length > data.Length - position) throw new InvalidDataException("Truncated XMI event payload.");
        var payload = data.Slice(position, (int)length);
        position += (int)length;
        return payload;
    }
}
