/*
 ** midisource.cpp
 ** Implements base class for the different MIDI formats
 **
 **---------------------------------------------------------------------------
 ** Copyright 2008-2016 Marisa Heit
 ** Copyright 2017-2018 Christoph Oelckers
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

public static class VmMusicMidiPrecache
{
    public static IReadOnlyList<ushort> Collect(IVmMusicSmfSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source is VmMusicMusSource mus) return mus.PrecacheData();
        source.LoopLimit = 1;
        try
        {
            source.Restart();
            return Scan(VmMusicSmfSource.ReadEvents(source));
        }
        finally
        {
            source.Restart();
        }
    }

    public static IReadOnlyList<ushort> Scan(IEnumerable<VmMusicSmfEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        var instruments = new bool[256];
        var banks = new bool[256];
        banks[0] = banks[128] = true;
        var multiple = false;
        foreach (var item in events)
        {
            var word = item.Event;
            if ((word >> 24) != 0) continue;
            var command = word & 0x70;
            var channel = word & 15;
            var first = (int)((word >> 8) & 127);
            var second = (int)((word >> 16) & 127);
            if (channel != 9 && command == 0x40) instruments[first] = true;
            else if (channel == 9 && command == 0x40 && first != 0)
            {
                multiple = true;
                banks[first | 128] = true;
            }
            else if (channel == 9 && command == 0x10 && second != 0) instruments[first | 128] = true;
            else if (command == 0x30 && first == 0 && second != 0)
            {
                multiple = true;
                banks[second | (channel == 9 ? 128 : 0)] = true;
            }
        }
        var packed = new List<ushort>();
        for (var instrument = 0; instrument < 256; instrument++)
        {
            if (!instruments[instrument]) continue;
            var number = (instrument & 127) | ((instrument & 128) << 7);
            if (!multiple) packed.Add((ushort)number);
            else
                for (var bank = 0; bank < 128; bank++)
                    if (banks[bank + (instrument & 128)]) packed.Add((ushort)(number | (bank << 7)));
        }
        return packed.AsReadOnly();
    }
}
