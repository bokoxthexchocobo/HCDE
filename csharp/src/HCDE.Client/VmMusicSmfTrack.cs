/*
** music_midi_midiout.cpp
** Code to let ZDoom play SMF MIDI music through the MIDI streaming API.
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
**
** This file also supports the Apogee Sound System's EMIDI files. That
** basically means you can play the Duke3D songs without any editing and
** have them sound right.
*/

namespace HCDE.Client;

public sealed record VmMusicSmfTrack(uint InitialTempo, IReadOnlyList<VmMusicSmfEvent> Events, bool HasInitialTempo = false);

public static class VmMusicSmfTrackDecoder
{
    public static VmMusicSmfTrack Decode(ReadOnlySpan<byte> data)
    {
        var position = 0;
        byte running = 0;
        uint initialTempo = 500000;
        var hasInitialTempo = false;
        var initialMeta = true;
        var events = new List<VmMusicSmfEvent>();
        while (position < data.Length)
        {
            var delay = Variable(data, ref position);
            var status = Byte(data, ref position);
            if (status == 255)
            {
                var kind = Byte(data, ref position);
                var length = Variable(data, ref position);
                var payload = Payload(data, ref position, length);
                if (kind == 47)
                {
                    if (length != 0) throw new InvalidDataException("MIDI end-of-track payload must be empty.");
                    if (delay != 0) events.Add(new(delay, 0x02000000));
                    break;
                }
                if (kind == 81)
                {
                    if (length != 3) throw new InvalidDataException("MIDI tempo payload must contain three bytes.");
                    var tempo = (uint)((payload[0] << 16) | (payload[1] << 8) | payload[2]);
                    if (tempo == 0) throw new InvalidDataException("MIDI tempo must be positive.");
                    if (initialMeta && delay == 0) { initialTempo = tempo; hasInitialTempo = true; }
                    else events.Add(new(delay, 0x01000000 | tempo));
                }
                else if (delay != 0) events.Add(new(delay, 0x02000000));
                if (delay != 0) initialMeta = false;
                continue;
            }
            initialMeta = false;
            if (status is 240 or 247)
            {
                var length = Variable(data, ref position);
                var payload = Payload(data, ref position, length);
                if (status == 247 && length == 0) throw new InvalidDataException("Empty MIDI escape packet is unsupported.");
                if (length > 0xffffffu - (status == 240 ? 1u : 0u))
                    throw new InvalidDataException("MIDI packet exceeds the packed event length limit.");
                var bytes = new byte[payload.Length + (status == 240 ? 1 : 0)];
                if (status == 240) bytes[0] = 240;
                payload.CopyTo(bytes.AsSpan(status == 240 ? 1 : 0));
                events.Add(new(delay, 0x80000000 | (uint)bytes.Length, bytes));
                continue;
            }
            byte first;
            if (status < 128)
            {
                if (running == 0) throw new InvalidDataException("MIDI running status has no preceding channel message.");
                first = status;
                status = running;
            }
            else
            {
                if (status >= 240) throw new InvalidDataException("Unsupported MIDI system status.");
                running = status;
                first = Byte(data, ref position);
            }
            var second = (status >> 4) is 12 or 13 ? (byte)0 : Byte(data, ref position);
            if (first >= 128 || second >= 128) throw new InvalidDataException("MIDI channel data must be seven-bit values.");
            events.Add(new(delay, (uint)(status | (first << 8) | (second << 16))));
        }
        return new(initialTempo, events.AsReadOnly(), hasInitialTempo);
    }

    private static byte Byte(ReadOnlySpan<byte> data, ref int position)
    {
        if (position >= data.Length) throw new InvalidDataException("Truncated MIDI event.");
        return data[position++];
    }
    private static uint Variable(ReadOnlySpan<byte> data, ref int position)
    {
        uint value = 0;
        for (var i = 0; i < 4; i++)
        {
            var part = Byte(data, ref position);
            value = (value << 7) | (uint)(part & 127);
            if ((part & 128) == 0) return value;
        }
        throw new InvalidDataException("MIDI variable-length value exceeds four bytes.");
    }
    private static ReadOnlySpan<byte> Payload(ReadOnlySpan<byte> data, ref int position, uint length)
    {
        if (length > data.Length - position) throw new InvalidDataException("Truncated MIDI event payload.");
        var payload = data.Slice(position, (int)length);
        position += (int)length;
        return payload;
    }
}
