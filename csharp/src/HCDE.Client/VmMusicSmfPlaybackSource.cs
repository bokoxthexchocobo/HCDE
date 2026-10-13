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

public sealed class VmMusicSmfPlaybackSource : IVmMusicSmfSource, IVmMusicMidiSysexPolicy
{
    private IReadOnlyList<VmMusicSmfEvent> events = [];
    private readonly VmMusicSmfTrack[] tracks;
    private readonly ushort format;
    private int position;
    private uint tempo;
    private bool disposed;
    private int technology = 1;
    private bool looping;
    private VmMusicMidiEventCursor? liveCursor;
    public ushort Division { get; }
    public uint InitialTempo { get; private set; }
    public int LoopLimit { get; set; }

    public VmMusicSmfPlaybackSource(ReadOnlySpan<byte> data)
    {
        var document = VmMusicSmfDocument.Parse(data);
        if ((document.Division & 0x8000) != 0) throw new InvalidDataException("Managed MIDI SMPTE timing is not implemented.");
        Division = document.Division;
        format = document.Format;
        tracks = document.Tracks.Select(data => VmMusicSmfTrackDecoder.Decode(data.Span)).ToArray();
        Compile(1);
    }

    private void Compile(int loopLimit)
    {
        if (format is 1 or 2 && VmMusicEmidiSchedule.HasGlobalControls(tracks))
        {
            InitialTempo = tempo = VmMusicSmfMerge.InitialTempo(tracks).Tempo;
            events = VmMusicEmidiSchedule.ReadEvents(tracks, Division, technology, loopLimit, false, format == 2)
                .ToList().AsReadOnly();
            return;
        }
        var transformed = tracks.Select(track => VmMusicEmidi.Apply(track, Division, technology, loopLimit: loopLimit,
            singleTrackGlobalLoops: tracks.Length == 1)).ToArray();
        var track = format switch
        {
            0 => transformed[0],
            1 => VmMusicSmfMerge.Merge(transformed),
            _ => VmMusicSmfMerge.Sequence(transformed),
        };
        InitialTempo = tempo = track.InitialTempo;
        events = track.Events;
    }

    public void CheckCapabilities(int technology)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        this.technology = technology;
    }
    public void Restart()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (looping)
        {
            liveCursor?.Dispose();
            InitialTempo = tempo = VmMusicSmfMerge.InitialTempo(tracks).Tempo;
            if (format is 1 or 2 && VmMusicEmidiSchedule.HasGlobalControls(tracks))
            {
                liveCursor = new(VmMusicEmidiSchedule.ReadEvents(tracks, Division, technology, LoopLimit, true, format == 2));
                return;
            }
            var streams = tracks.Select(track => VmMusicEmidi.ReadEvents(track, Division, technology, LoopLimit,
                tracks.Length == 1, true)).ToArray();
            liveCursor = new(format == 1 ? VmMusicSmfMerge.ReadMergedEvents(streams) : streams.SelectMany(stream => stream));
            return;
        }
        Compile(LoopLimit);
        position = 0;
        tempo = InitialTempo;
    }
    public void StartPlayback(bool looped, int loopLimit)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(loopLimit);
        looping = looped;
        liveCursor?.Dispose();
        liveCursor = null;
        LoopLimit = loopLimit;
        tempo = InitialTempo;
        Restart();
    }
    public bool CheckDone()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return looping ? liveCursor!.CheckDone() : position >= events.Count;
    }
    public bool SkipSysex { get; set; }

    public int MakeEvents(Span<uint> buffer, uint maximumTime)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (looping) return liveCursor!.Write(Division, ref tempo, buffer, maximumTime, SkipSysex);
        return VmMusicSmfEventBuffer.Write(events, ref position, ref tempo, Division, buffer, maximumTime, SkipSysex);
    }
    public void Dispose() { liveCursor?.Dispose(); disposed = true; }
}

internal static class VmMusicSmfEventBuffer
{
    internal static int Write(IReadOnlyList<VmMusicSmfEvent> events, ref int position, ref uint tempo,
        ushort division, Span<uint> buffer, uint maximumTime, bool skipSysex = false)
    {
        if (buffer.Length < 3) throw new ArgumentException("MIDI buffer must hold an event.", nameof(buffer));
        ulong elapsed = 0;
        var written = 0;
        while (position < events.Count && elapsed <= maximumTime)
        {
            var item = events[position];
            if (skipSysex && item.Event >= 0x80000000)
                item = new(item.Delay, 0x02000000);
            var payloadWords = (item.Data.Length + 3) / 4;
            var words = 3 + payloadWords;
            if (words > buffer.Length) throw new InvalidDataException("MIDI event exceeds output buffer capacity.");
            if (words > buffer.Length - written) break;
            buffer[written++] = item.Delay;
            buffer[written++] = 0;
            buffer[written++] = item.Event;
            for (var i = 0; i < payloadWords; i++)
            {
                uint value = 0;
                for (var j = 0; j < 4 && i * 4 + j < item.Data.Length; j++)
                    value |= (uint)item.Data.Span[i * 4 + j] << (j * 8);
                buffer[written++] = value;
            }
            elapsed += (ulong)item.Delay * tempo / division;
            if ((item.Event >> 24) == 1) tempo = item.Event & 0xffffff;
            position++;
        }
        return written;
    }
}
