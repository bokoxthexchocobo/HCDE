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

public static class VmMusicEmidi
{
    public static int ClampLoopCount(int loopCount, int loopLimit)
    {
        if (loopLimit == 0) return loopCount;
        if (loopLimit == 1) return 1;
        return loopCount == 0 ? loopLimit : loopCount;
    }

    public static VmMusicSmfTrack Apply(VmMusicSmfTrack track, ushort division, int technology = 1, int loopLimit = 0,
        bool singleTrackGlobalLoops = false)
    {
        ArgumentNullException.ThrowIfNull(track);
        return new(track.InitialTempo, ReadEvents(track, division, technology, loopLimit, singleTrackGlobalLoops, false)
            .ToList().AsReadOnly(), track.HasInitialTempo);
    }

    internal static IEnumerable<VmMusicSmfEvent> ReadEvents(VmMusicSmfTrack track, ushort division,
        int technology, int loopLimit, bool singleTrackGlobalLoops, bool looping)
    {
        var filter = new Filter(division, technology);
        ulong ticks = 0;
        var loopStart = 0;
        var remaining = -1;
        var controlSteps = 0;
        for (var index = 0; index < track.Events.Count; index++)
        {
            var item = track.Events[index];
            if (looping && ++controlSteps > 4096) throw new InvalidDataException("EMIDI loop makes no event progress.");
            var jump = -1;
            var finish = false;
            ticks = checked(ticks + item.Delay);
            var word = item.Event;
            if ((word >> 24) != 0) { controlSteps = 0; yield return item; continue; }
            var first = (int)((word >> 8) & 127);
            var second = (int)((word >> 16) & 127);
            var loopControl = false;
            if ((word & 0xf0) == 0xb0)
            {
                if (singleTrackGlobalLoops && first is 118 or 119) first -= 2;
                if (first is 118 or 119)
                {
                    if (loopLimit != 1)
                        throw new InvalidDataException("Managed MIDI repeated global EMIDI loops are not implemented.");
                    loopControl = true;
                }
                switch (first)
                {
                    case 116:
                        var count = ClampLoopCount(second == 0 ? 0 : second + 1, loopLimit);
                        if (count != 1) { loopStart = index + 1; remaining = count == 0 ? 0 : count - 1; }
                        loopControl = true;
                        break;
                    case 117:
                        if (remaining >= 0 && second == 127)
                        {
                            if (remaining == 0 && !looping) finish = true;
                            else
                            {
                                if (remaining > 0 && --remaining == 0) remaining = -1;
                                jump = loopStart;
                            }
                        }
                        loopControl = true;
                        break;
                }
            }
            var filtered = loopControl ? null : filter.Apply(item, ticks);
            if (filtered is { } output)
            {
                controlSteps = 0;
                yield return output;
            }
            else if (item.Delay != 0) { controlSteps = 0; yield return new(item.Delay, 0x02000000); }
            if (finish) break;
            if (jump >= 0) index = jump - 1;
        }
    }

    internal sealed class Filter(ushort division, int technology)
    {
        private readonly int mask = technology == 4 ? 1 << 7 : 1;
        private bool designated;
        private int designation;
        private bool program;
        private bool volume;

        internal VmMusicSmfEvent? Apply(VmMusicSmfEvent item, ulong ticks)
        {
            if ((item.Event >> 24) != 0) return item;
            var status = (int)(item.Event & 255);
            var first = (int)((item.Event >> 8) & 127);
            var second = (int)((item.Event >> 16) & 127);
            var emit = (status & 0xf0) != 0xc0 || !program;
            if ((status & 0xf0) == 0xb0)
            {
                switch (first)
                {
                    case 7 or 39:
                        if (volume) emit = false;
                        break;
                    case 110 when ticks < division:
                        if (second == 127) { designation = 0xffff; designated = true; }
                        else if (second <= 9) { designation |= 1 << second; designated = true; }
                        emit = false;
                        break;
                    case 111 when ticks < division:
                        if (!designated) { designation = 0xffff; designated = true; }
                        if (second <= 9) designation &= ~(1 << second);
                        emit = false;
                        break;
                    case 112 when ticks < division || program:
                        program = true;
                        status = 0xc0 | (status & 15); first = second; second = 0;
                        break;
                    case 113 when ticks < division || volume:
                        volume = true;
                        first = 7;
                        break;
                }
            }
            return emit && (!designated || (designation & mask) != 0)
                ? item with { Event = (uint)(status | (first << 8) | (second << 16)) } : null;
        }
    }
}
