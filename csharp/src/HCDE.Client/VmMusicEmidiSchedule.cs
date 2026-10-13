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

internal static class VmMusicEmidiSchedule
{
    private sealed class Track(VmMusicSmfTrack source, ushort division, int technology)
    {
        internal readonly IReadOnlyList<VmMusicSmfEvent> Events = source.Events;
        internal readonly VmMusicEmidi.Filter Filter = new(division, technology);
        internal int Position;
        internal uint Delay = source.Events.Count == 0 ? 0 : source.Events[0].Delay;
        internal ulong PlayedTime;
        internal int LoopPosition;
        internal uint LoopDelay;
        internal int LoopCount = -1;
        internal ulong LoopPlayedTime;
        internal bool Finished = source.Events.Count == 0;
        internal bool LoopFinished;

        internal void Save(int count)
        {
            LoopPosition = Position;
            LoopDelay = Delay;
            LoopCount = count == 0 ? 0 : count - 1;
            LoopFinished = Finished;
            LoopPlayedTime = PlayedTime;
        }

        internal void Restore(bool looping, bool validateDuration = true)
        {
            if (LoopCount < 0) return;
            if (LoopCount == 0 && !looping) { Finished = true; return; }
            if (validateDuration && looping && LoopCount == 0 && !LoopFinished && PlayedTime == LoopPlayedTime)
                throw new InvalidDataException("An infinite EMIDI loop must advance playback time.");
            LoopPlayedTime = PlayedTime;
            if (LoopCount > 0 && --LoopCount == 0) LoopCount = -1;
            Position = LoopPosition;
            Delay = LoopDelay;
            Finished = LoopFinished;
        }
    }

    internal static bool HasGlobalControls(IReadOnlyList<VmMusicSmfTrack> tracks) => tracks.Any(track =>
        track.Events.Any(item => (item.Event >> 24) == 0 && (item.Event & 0xf0) == 0xb0 &&
            ((item.Event >> 8) & 0x7f) is 118 or 119));

    internal static IEnumerable<VmMusicSmfEvent> ReadEvents(IReadOnlyList<VmMusicSmfTrack> sources,
        ushort division, int technology, int loopLimit, bool looping, bool sequential = false)
    {
        var tracks = sources.Select(source => new Track(source, division, technology)).ToArray();
        var previous = tracks.Length == 0 ? -1 : 0;
        var controlSteps = 0;
        while (true)
        {
            var selected = -1;
            if (sequential)
            {
                // Independent format-2 tracks retain their delays until selected.
                for (var i = Math.Max(previous, 0); i < tracks.Length; i++)
                    if (!tracks[i].Finished) { selected = i; break; }
            }
            else if (previous >= 0 && !tracks[previous].Finished && tracks[previous].Delay == 0) selected = previous;
            else
                for (var i = 0; i < tracks.Length; i++)
                    if (!tracks[i].Finished && (selected < 0 || tracks[i].Delay < tracks[selected].Delay)) selected = i;
            if (selected < 0) yield break;
            if (looping && ++controlSteps > 4096) throw new InvalidDataException("EMIDI loop makes no event progress.");
            var track = tracks[selected];
            var delay = track.Delay;
            foreach (var active in tracks)
            {
                if (active.Finished || (sequential && active != track)) continue;
                active.Delay -= delay;
                active.PlayedTime = checked(active.PlayedTime + delay);
            }
            var item = track.Events[track.Position++] with { Delay = delay };
            track.Finished = track.Position >= track.Events.Count;
            track.Delay = track.Finished ? 0 : track.Events[track.Position].Delay;
            var word = item.Event;
            var controller = (int)((word >> 8) & 127);
            var value = (int)((word >> 16) & 127);
            var loopControl = (word >> 24) == 0 && (word & 0xf0) == 0xb0 && controller is >= 116 and <= 119;
            if (loopControl)
            {
                switch (controller)
                {
                    case 116 or 118:
                        var count = VmMusicEmidi.ClampLoopCount(value == 0 ? 0 : value + 1, loopLimit);
                        if (count != 1)
                        {
                            if (controller == 116) track.Save(count);
                            else foreach (var state in tracks) state.Save(count);
                        }
                        break;
                    case 117 when value == 127:
                        track.Restore(looping);
                        break;
                    case 119 when value == 127:
                        foreach (var state in tracks) state.Restore(looping, state == track);
                        break;
                }
            }
            var filtered = loopControl ? null : track.Filter.Apply(item, track.PlayedTime);
            previous = selected;
            if (filtered is { } output)
            {
                controlSteps = 0;
                yield return output;
            }
            else if (delay != 0)
            {
                controlSteps = 0;
                yield return new(delay, 0x02000000);
            }
        }
    }
}
