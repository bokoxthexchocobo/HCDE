/*
** music_midistream.cpp
** Implements base class for MIDI and MUS streaming.
**
**---------------------------------------------------------------------------
** Copyright 2008 Marisa Heit
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

public static class VmMusicMidiControlBuffers
{
    public const int StopNotesWords = 16 * 6;

    public static int WritePaused(Span<uint> buffer, uint maximumTime, ushort division, uint tempo,
        VmMusicMidiVolumeUpdates volumeUpdates)
    {
        ArgumentNullException.ThrowIfNull(volumeUpdates);
        if (division == 0) throw new ArgumentOutOfRangeException(nameof(division));
        if (tempo == 0) throw new ArgumentOutOfRangeException(nameof(tempo));
        var ticks = checked((uint)Math.Max(1UL, (ulong)(maximumTime / 3) * division / tempo));
        Require(buffer, volumeUpdates.RequiredWords(true) + 3);
        var written = volumeUpdates.WritePending(buffer, true);
        buffer[written] = ticks;
        buffer[written + 1] = 0;
        buffer[written + 2] = 0x02000000;
        return written + 3;
    }

    public static int WriteStopNotes(Span<uint> buffer)
    {
        Require(buffer, StopNotesWords);
        for (var channel = 0; channel < 16; channel++)
        {
            var offset = channel * 6;
            buffer[offset] = buffer[offset + 1] = buffer[offset + 3] = buffer[offset + 4] = 0;
            buffer[offset + 2] = (uint)(0xb0 | channel | (123 << 8));
            buffer[offset + 5] = (uint)(0xb0 | channel | (121 << 8));
        }
        return StopNotesWords;
    }

    public static int WriteStop(Span<uint> buffer)
    {
        Require(buffer, StopNotesWords + 3);
        var written = WriteStopNotes(buffer);
        buffer[written] = 500;
        buffer[written + 1] = 0;
        buffer[written + 2] = 0x02000000;
        return written + 3;
    }

    public static int WriteInitial(Span<uint> buffer, bool canHandleSysex, VmMusicMidiChannelVolume channels)
    {
        ArgumentNullException.ThrowIfNull(channels);
        var count = canHandleSysex ? 16 : 10;
        Require(buffer, count);
        var position = WritePacket(buffer, [0xf0, 0x7e, 0x7f, 0x09, 0x01, 0xf7]);
        if (canHandleSysex)
            position += WritePacket(buffer[position..], [0xf0, 0x41, 0x7f, 0x42, 0x12, 0x40, 0x00, 0x7f, 0x00, 0x41, 0xf7]);
        position += WritePacket(buffer[position..], [0xf0, 0x7f, 0x7f, 0x04, 0x01, 0x7f, 0x7f, 0xf7]);
        channels.Initialize();
        return position;
    }

    private static int WritePacket(Span<uint> buffer, ReadOnlySpan<byte> payload)
    {
        buffer[0] = buffer[1] = 0;
        buffer[2] = 0x80000000 | (uint)payload.Length;
        var words = (payload.Length + 3) / 4;
        for (var i = 0; i < words; i++)
        {
            uint value = 0;
            for (var j = 0; j < 4 && i * 4 + j < payload.Length; j++)
                value |= (uint)payload[i * 4 + j] << (j * 8);
            buffer[3 + i] = value;
        }
        return 3 + words;
    }

    private static void Require(Span<uint> buffer, int words)
    {
        if (buffer.Length < words) throw new ArgumentException("MIDI control buffer is too small.", nameof(buffer));
    }
}
