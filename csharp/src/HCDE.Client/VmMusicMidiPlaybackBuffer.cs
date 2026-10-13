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

public sealed class VmMusicMidiPlaybackBuffer
{
    private readonly IVmMusicSmfSource source;
    private bool initial = true;
    private bool restarting;
    public VmMusicMidiChannelVolume Channels { get; } = new();
    public VmMusicMidiVolumeUpdates VolumeUpdates { get; }
    public uint Tempo { get; private set; }
    public bool SourceDone => source.CheckDone();
    public bool RestartPending => restarting;

    public VmMusicMidiPlaybackBuffer(IVmMusicSmfSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        this.source = source;
        Tempo = source.InitialTempo;
        VolumeUpdates = new(Channels);
    }

    public void RequestRestart() => restarting = true;

    public int Fill(Span<uint> buffer, uint maximumTime, bool paused, bool canHandleSysex)
    {
        var initialWords = initial ? (canHandleSysex ? 16 : 10) : 0;
        var restartWords = !paused && restarting ? VmMusicMidiControlBuffers.StopNotesWords + 3 : 0;
        var prefix = initialWords + VolumeUpdates.RequiredWords(paused) + restartWords;
        var required = prefix + (paused ? 3 : 6);
        if (buffer.Length < required) throw new ArgumentException("MIDI playback buffer is too small.", nameof(buffer));
        if (source.Division == 0 || Tempo == 0) throw new InvalidDataException("Invalid MIDI playback timing.");
        if (paused) _ = checked((uint)Math.Max(1UL, (ulong)(maximumTime / 3) * source.Division / Tempo));
        var written = 0;
        if (initial)
        {
            written = VmMusicMidiControlBuffers.WriteInitial(buffer, canHandleSysex, Channels);
            initial = false;
        }
        if (paused)
            return written + VmMusicMidiControlBuffers.WritePaused(buffer[written..], maximumTime, source.Division, Tempo, VolumeUpdates);
        written += VolumeUpdates.WritePending(buffer[written..], false);
        if (restarting)
        {
            source.Restart();
            Tempo = source.InitialTempo;
            if (Tempo == 0 || Tempo > 0xffffff) throw new InvalidDataException("Invalid MIDI restart tempo.");
            buffer[written++] = 0;
            buffer[written++] = 0;
            buffer[written++] = 0x01000000 | Tempo;
            written += VmMusicMidiControlBuffers.WriteStopNotes(buffer[written..]);
            restarting = false;
        }
        // Native reserves the final event slot; source buffers receive only their available region.
        var songBuffer = buffer.Slice(written, buffer.Length - written - 3);
        var count = source.MakeEvents(songBuffer, maximumTime);
        if (count < 0 || count > songBuffer.Length) throw new InvalidDataException("Invalid MIDI source buffer length.");
        Validate(songBuffer[..count]);
        for (var offset = 0; offset < count;)
        {
            var word = songBuffer[offset + 2];
            if ((word >> 24) == 0)
                songBuffer[offset + 2] = Channels.Apply(new(songBuffer[offset], word)).Event;
            else if ((word >> 24) == 1) Tempo = word & 0xffffff;
            offset += 3 + (word >= 0x80000000 ? (int)(((word & 0xffffff) + 3) / 4) : 0);
        }
        return written + count;
    }

    private static void Validate(ReadOnlySpan<uint> buffer)
    {
        for (var offset = 0; offset < buffer.Length;)
        {
            if (buffer.Length - offset < 3) throw new InvalidDataException("Truncated MIDI source event header.");
            var word = buffer[offset + 2];
            var words = 3 + (word >= 0x80000000 ? (int)(((word & 0xffffff) + 3) / 4) : 0);
            if (words > buffer.Length - offset) throw new InvalidDataException("Truncated MIDI source packet.");
            if ((word >> 24) == 1 && (word & 0xffffff) == 0) throw new InvalidDataException("MIDI tempo must be positive.");
            if ((word >> 24) == 0 && (word & 0xf0) == 0xb0 && ((word >> 8) & 255) == 7 && ((word >> 16) & 255) > 127)
                throw new InvalidDataException("MIDI volume must be seven-bit.");
            offset += words;
        }
    }
}
