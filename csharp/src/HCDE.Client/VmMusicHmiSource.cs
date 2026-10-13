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

public sealed class VmMusicHmiSource : IVmMusicSmfSource, IVmMusicMidiSysexPolicy
{
    private readonly VmMusicHmiDocument document;
    private IReadOnlyList<VmMusicSmfEvent> events;
    private int technology = 1;
    private int position;
    private uint tempo;
    private bool disposed;
    public ushort Division => document.Division;
    public uint InitialTempo { get; private set; }
    public int LoopLimit { get; set; }

    public VmMusicHmiSource(ReadOnlySpan<byte> data)
    {
        document = VmMusicHmiDocument.Parse(data);
        var track = VmMusicHmiSchedule.Decode(document, technology);
        events = track.Events;
        InitialTempo = tempo = track.InitialTempo;
    }

    public void CheckCapabilities(int technology)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        this.technology = technology;
    }

    public void Restart()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var track = VmMusicHmiSchedule.Decode(document, technology);
        events = track.Events;
        InitialTempo = tempo = track.InitialTempo;
        position = 0;
    }

    public void StartPlayback(bool looped, int loopLimit)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(loopLimit);
        LoopLimit = loopLimit;
        tempo = InitialTempo;
    }

    public bool CheckDone()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return position >= events.Count;
    }

    public bool SkipSysex { get; set; }

    public int MakeEvents(Span<uint> buffer, uint maximumTime)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return VmMusicSmfEventBuffer.Write(events, ref position, ref tempo, Division, buffer, maximumTime, SkipSysex);
    }

    public void Dispose() => disposed = true;
}
