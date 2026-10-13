/*
** music_hmi_midiout.cpp
** Code to let ZDoom play HMI MIDI music through the MIDI streaming API.
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

public static class VmMusicHmpEvents
{
    public static VmMusicSmfTrack Decode(ReadOnlySpan<byte> data)
        => Decode(data, false);

    internal static VmMusicSmfTrack Decode(ReadOnlySpan<byte> data, bool hmi,
        List<VmMusicHmiEvent>? realEvents = null)
    {
        var position = 0;
        byte running = 0;
        var initialMeta = true;
        var hasInitialTempo = false;
        uint initialTempo = hmi ? 4000000u : 1000000u;
        ulong time = 0, emittedTime = 0;
        var offs = new VmMusicNoteOffQueue();
        var events = new List<VmMusicSmfEvent>();
        void Emit(uint word, ReadOnlyMemory<byte> payload = default)
        {
            events.Add(new(checked((uint)(time - emittedTime)), word, payload));
            realEvents?.Add(new(events[^1]));
            emittedTime = time;
        }
        void Flush(ulong through)
        {
            if (realEvents != null) return;
            while (offs.TryPeek(out _, out var due) && due <= through)
            {
                events.Add(new(checked((uint)(due - emittedTime)), offs.Dequeue()));
                emittedTime = due;
            }
        }
        while (position < data.Length)
        {
            var delay = Variable(data, ref position, hmi);
            time = checked(time + delay);
            Flush(time);
            if (delay != 0) initialMeta = false;
            var status = ReadByte(data, ref position);
            if (status == 255)
            {
                var kind = ReadByte(data, ref position);
                var payload = Payload(data, ref position, Variable(data, ref position, hmi));
                if (kind == 47)
                {
                    if (!payload.IsEmpty) throw new InvalidDataException("HMP end-of-track payload must be empty.");
                    if (time != emittedTime || realEvents != null) Emit(0x02000000);
                    break;
                }
                if (kind == 81)
                {
                    if (payload.Length != 3) throw new InvalidDataException("HMP tempo payload must contain three bytes.");
                    var tempo = (uint)((payload[0] << 16) | (payload[1] << 8) | payload[2]);
                    if (tempo == 0) throw new InvalidDataException("HMP tempo must be positive.");
                    if (hmi && initialMeta) { initialTempo = tempo; hasInitialTempo = true; }
                    else Emit(0x01000000 | tempo);
                }
                else if (time != emittedTime || (realEvents != null && !initialMeta)) Emit(0x02000000);
                continue;
            }
            initialMeta = false;
            if (status == 254)
            {
                var kind = ReadByte(data, ref position);
                if (kind is 0x13 or 0x15) _ = Payload(data, ref position, 6);
                else if (kind is 0x12 or 0x14) _ = Payload(data, ref position, 2);
                else if (kind == 0x10)
                {
                    _ = Payload(data, ref position, 2);
                    var length = ReadByte(data, ref position);
                    _ = Payload(data, ref position, (uint)length + 4);
                }
                else throw new InvalidDataException("Unknown HMI/HMP special event.");
                if (time != emittedTime || realEvents != null) Emit(0x02000000);
                continue;
            }
            if (status is 240 or 247)
            {
                var payload = Payload(data, ref position, Variable(data, ref position, hmi));
                if (payload.Length > 0xffffff - (status == 240 ? 1 : 0))
                    throw new InvalidDataException("HMP SysEx exceeds the packed event limit.");
                if (status == 247 && payload.IsEmpty) throw new InvalidDataException("Empty HMP escape packet is unsupported.");
                var bytes = new byte[payload.Length + (status == 240 ? 1 : 0)];
                if (status == 240) bytes[0] = 240;
                payload.CopyTo(bytes.AsSpan(status == 240 ? 1 : 0));
                Emit(0x80000000 | (uint)bytes.Length, bytes);
                continue;
            }
            if (status >= 240)
            {
                var bytes = new byte[VmMusicSystemMessages.DataLength(status) + 1];
                bytes[0] = status;
                for (var i = 1; i < bytes.Length; i++)
                {
                    bytes[i] = ReadByte(data, ref position);
                    if (bytes[i] >= 128) throw new InvalidDataException("HMI/HMP system data must be seven-bit values.");
                }
                Emit(0x80000000 | (uint)bytes.Length, bytes);
                continue;
            }
            byte first;
            if (status < 128)
            {
                if (running == 0) throw new InvalidDataException("HMP running status has no channel message.");
                first = status;
                status = running;
            }
            else
            {
                running = status;
                first = ReadByte(data, ref position);
            }
            var second = (status >> 4) is 12 or 13 ? (byte)0 : ReadByte(data, ref position);
            if (first >= 128 || second >= 128) throw new InvalidDataException("HMP channel data must be seven-bit values.");
            Emit((uint)(status | (first << 8) | (second << 16)));
            if (hmi && (status >> 4) == 9)
            {
                var duration = Variable(data, ref position, true);
                if (realEvents != null) realEvents[^1] = realEvents[^1] with { Duration = duration };
                else offs.Enqueue((uint)(0x90 | (status & 15) | (first << 8)),
                    checked(time + duration));
            }
        }
        Flush(ulong.MaxValue);
        return new(initialTempo, events.AsReadOnly(), hasInitialTempo);
    }

    private static byte ReadByte(ReadOnlySpan<byte> data, ref int position)
    {
        if (position >= data.Length) throw new InvalidDataException("Truncated HMP event.");
        return data[position++];
    }

    private static uint Variable(ReadOnlySpan<byte> data, ref int position, bool hmi)
    {
        uint value = 0;
        if (hmi)
        {
            for (var i = 0; i < 4; i++)
            {
                var part = ReadByte(data, ref position);
                value = (value << 7) | (uint)(part & 127);
                if (part < 128) return value;
            }
            throw new InvalidDataException("HMI variable-length value exceeds four bytes.");
        }
        for (var i = 0; i < 5; i++)
        {
            var part = ReadByte(data, ref position);
            if (i == 4 && (part & 127) > 15) throw new InvalidDataException("HMP variable-length value exceeds 32 bits.");
            value |= (uint)(part & 127) << (i * 7);
            if (part >= 128) return value;
        }
        throw new InvalidDataException("Unterminated HMP variable-length value.");
    }

    private static ReadOnlySpan<byte> Payload(ReadOnlySpan<byte> data, ref int position, uint length)
    {
        if (length > data.Length - position) throw new InvalidDataException("Truncated HMP payload.");
        var payload = data.Slice(position, (int)length);
        position += (int)length;
        return payload;
    }
}
