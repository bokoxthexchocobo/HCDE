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

internal sealed record VmMusicHmiEvent(VmMusicSmfEvent Event, uint? Duration = null);

public static class VmMusicHmiSchedule
{
    public static VmMusicSmfTrack Decode(VmMusicHmiDocument document, int technology = 1)
    {
        ArgumentNullException.ThrowIfNull(document);
        var streams = new List<VmMusicHmiEvent>[document.Tracks.Count];
        var enabled = new bool[streams.Length];
        var initialTempo = document.InitialTempo;
        var hasTempo = false;
        for (var i = 0; i < streams.Length; i++)
        {
            streams[i] = [];
            var track = document.Tracks[i];
            var decoded = VmMusicHmpEvents.Decode(track.Events.Span, !document.IsHmp, streams[i]);
            // Native initial metadata is processed even on device-disabled tracks.
            if (decoded.HasInitialTempo) { initialTempo = decoded.InitialTempo; hasTempo = true; }
            enabled[i] = VmMusicHmiDevices.IsEnabled(track.Designations, technology);
        }
        return Merge(streams, enabled, initialTempo, hasTempo);
    }

    internal static VmMusicSmfTrack DecodeTrack(ReadOnlySpan<byte> data)
    {
        var stream = new List<VmMusicHmiEvent>();
        var track = VmMusicHmpEvents.Decode(data, true, stream);
        return Merge([stream], [true], track.InitialTempo, track.HasInitialTempo);
    }

    private static VmMusicSmfTrack Merge(List<VmMusicHmiEvent>[] streams, bool[] enabled,
        uint initialTempo, bool hasTempo)
    {
        var positions = new int[streams.Length];
        var due = new ulong[streams.Length];
        for (var i = 0; i < streams.Length; i++)
            if (streams[i].Count != 0) due[i] = streams[i][0].Event.Delay;
        var offs = new VmMusicNoteOffQueue();
        var previous = streams.Length == 0 ? -2 : 0;
        ulong time = 0, emittedTime = 0;
        var output = new List<VmMusicSmfEvent>();
        while (true)
        {
            var selected = -2;
            if (previous >= 0 && enabled[previous] && positions[previous] < streams[previous].Count && due[previous] == time)
                selected = previous;
            else if (previous == -1 && offs.TryPeek(out _, out var nextOff) && nextOff == time)
                selected = -1;
            else
            {
                for (var i = 0; i < streams.Length; i++)
                    if (enabled[i] && positions[i] < streams[i].Count && (selected == -2 || due[i] < due[selected])) selected = i;
                if (offs.TryPeek(out _, out var off) && (selected == -2 || off <= due[selected])) selected = -1;
            }
            if (selected == -2) break;
            VmMusicSmfEvent item;
            if (selected == -1)
            {
                offs.TryDequeue(out var word, out var off);
                time = off;
                item = new(0, word);
            }
            else
            {
                time = due[selected];
                var real = streams[selected][positions[selected]++];
                item = real.Event;
                if (real.Duration is uint duration)
                    offs.Enqueue(item.Event & 0xffff, checked(time + duration));
                if (positions[selected] < streams[selected].Count)
                    due[selected] = checked(time + streams[selected][positions[selected]].Event.Delay);
            }
            var delay = checked((uint)(time - emittedTime));
            if (delay != 0 || item.Event != 0x02000000)
            {
                output.Add(item with { Delay = delay });
                emittedTime = time;
            }
            previous = selected;
        }
        return new(initialTempo, output.AsReadOnly(), hasTempo);
    }
}
