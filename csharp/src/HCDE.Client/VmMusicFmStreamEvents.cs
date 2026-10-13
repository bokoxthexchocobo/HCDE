/*
** music_softsynth_mididevice.cpp
** Common base class for software synthesis MIDI devices.
**
**---------------------------------------------------------------------------
** Copyright 2008-2010 Marisa Heit
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

public readonly record struct VmMusicFmStreamEvent(uint DeltaTicks, int WordsConsumed, uint? Tempo);

public static class VmMusicFmStreamEvents
{
    public static VmMusicFmStreamEvent DispatchNext(IVmMusicFmMidiDevice device, ReadOnlySpan<uint> buffer)
    {
        ArgumentNullException.ThrowIfNull(device);
        var words = GetRecordWords(buffer);
        var packed = buffer[2];
        var type = packed >> 24;
        var parameter = packed & 0x00ffffff;
        uint? tempo = null;
        switch (type)
        {
            case 0:
                VmMusicFmMidiEvents.HandleEvent(device, (int)(packed & 0xff),
                    (int)((packed >> 8) & 0x7f), (int)((packed >> 16) & 0x7f));
                break;
            case 1:
                tempo = parameter;
                break;
            case 0x80:
                // Packed MIDI words use little-endian byte order regardless of host architecture.
                var payload = new byte[(int)parameter];
                for (var i = 0; i < payload.Length; i++)
                    payload[i] = (byte)(buffer[3 + i / 4] >> ((i % 4) * 8));
                VmMusicFmMidiEvents.HandleLongEvent(device, payload);
                break;
        }
        return new(buffer[0], words, tempo);
    }

    internal static int GetRecordWords(ReadOnlySpan<uint> buffer)
    {
        if (buffer.Length < 3) throw new ArgumentException("MIDI event header is incomplete.", nameof(buffer));
        var packed = buffer[2];
        var words = 3 + ((packed & 0x80000000) != 0 ? (int)(((packed & 0x00ffffff) + 3) / 4) : 0);
        if (buffer.Length < words) throw new ArgumentException("MIDI event payload is incomplete.", nameof(buffer));
        return words;
    }
}
