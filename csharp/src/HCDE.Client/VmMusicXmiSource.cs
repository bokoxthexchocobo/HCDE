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

public sealed class VmMusicXmiSource : IVmMusicSmfSource, IVmMusicMidiSysexPolicy
{
    private readonly VmMusicXmiDocument document;
    private IReadOnlyList<VmMusicSmfEvent> events;
    private int position;
    private uint tempo = 500000;
    private bool disposed;
    private bool looping;
    private IEnumerator<VmMusicSmfEvent>? liveEvents;
    private VmMusicSmfEvent? pendingEvent;
    private bool liveDone;
    private System.Runtime.ExceptionServices.ExceptionDispatchInfo? liveError;
    public ushort Division => document.Division;
    public uint InitialTempo => document.InitialTempo;
    public int LoopLimit { get; set; }
    public int SongCount => document.Songs.Count;
    public int CurrentSong { get; private set; }

    public VmMusicXmiSource(ReadOnlySpan<byte> data)
    {
        document = VmMusicXmiDocument.Parse(data);
        events = VmMusicXmiEvents.Decode(document.Songs[0].Events.Span, 1).Events;
    }

    public bool SetSubsong(int subsong)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if ((uint)subsong >= (uint)SongCount) return false;
        var decoded = VmMusicXmiEvents.Decode(document.Songs[subsong].Events.Span, LoopLimit);
        CurrentSong = subsong;
        events = decoded.Events;
        position = 0;
        tempo = InitialTempo;
        if (looping) Restart();
        return true;
    }

    public void CheckCapabilities(int technology) => ObjectDisposedException.ThrowIf(disposed, this);
    public void Restart()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (looping)
        {
            liveEvents?.Dispose();
            liveEvents = VmMusicXmiEvents.ReadEvents(document.Songs[CurrentSong].Events, LoopLimit, true).GetEnumerator();
            pendingEvent = null;
            liveDone = false;
            liveError = null;
            tempo = InitialTempo;
            return;
        }
        var decoded = VmMusicXmiEvents.Decode(document.Songs[CurrentSong].Events.Span, LoopLimit);
        events = decoded.Events;
        position = 0;
        tempo = InitialTempo;
    }

    public void StartPlayback(bool looped, int loopLimit)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(loopLimit);
        looping = looped;
        liveEvents?.Dispose();
        liveEvents = null;
        pendingEvent = null;
        LoopLimit = loopLimit;
        Restart();
    }

    public bool CheckDone()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!looping) return position >= events.Count;
        ReadPending();
        return liveDone;
    }

    public bool SkipSysex { get; set; }

    public int MakeEvents(Span<uint> buffer, uint maximumTime)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!looping) return VmMusicSmfEventBuffer.Write(events, ref position, ref tempo, Division, buffer, maximumTime, SkipSysex);
        if (buffer.Length < 3) throw new ArgumentException("MIDI buffer must hold an event.", nameof(buffer));
        var written = 0;
        ulong elapsed = 0;
        while (elapsed <= maximumTime)
        {
            ReadPending();
            if (liveDone) break;
            var item = pendingEvent ?? throw new InvalidOperationException("Missing XMI event.");
            var words = 3 + (SkipSysex && item.Event >= 0x80000000 ? 0 : (item.Data.Length + 3) / 4);
            if (words > buffer.Length) throw new InvalidDataException("MIDI event exceeds output buffer capacity.");
            if (words > buffer.Length - written) break;
            var index = 0;
            written += VmMusicSmfEventBuffer.Write([item], ref index, ref tempo, Division,
                buffer[written..], uint.MaxValue, SkipSysex);
            elapsed += (ulong)item.Delay * tempo / Division;
            pendingEvent = null;
        }
        return written;
    }

    private void ReadPending()
    {
        liveError?.Throw();
        if (pendingEvent is not null || liveDone) return;
        try
        {
            if (liveEvents!.MoveNext()) pendingEvent = liveEvents.Current;
            else liveDone = true;
        }
        catch (Exception error)
        {
            liveError = System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error);
            throw;
        }
    }

    public void Dispose()
    {
        liveEvents?.Dispose();
        disposed = true;
    }
}
