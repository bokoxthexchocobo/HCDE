/*
** music_timidity_mididevice.cpp
** Provides access to TiMidity as a generic MIDI device.
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

public interface IVmMusicFmMidiDevice
{
    void NoteOn(int channel, int note, int velocity);
    void NoteOff(int channel, int note);
    void NoteAfterTouch(int channel, int note, int pressure);
    void ControllerChange(int channel, int controller, int value);
    void PatchChange(int channel, int program);
    void ChannelAfterTouch(int channel, int pressure);
    void PitchBend(int channel, int mostSignificant, int leastSignificant);
    void SystemExclusive(ReadOnlySpan<byte> data);
}

public static class VmMusicFmMidiEvents
{
    public static void HandleEvent(IVmMusicFmMidiDevice device, int status, int parameter1, int parameter2)
    {
        ArgumentNullException.ThrowIfNull(device);
        var channel = status & 0x0f;
        switch (status & 0xf0)
        {
            case 0x90: device.NoteOn(channel, parameter1, parameter2); break;
            case 0x80: device.NoteOff(channel, parameter1); break;
            case 0xa0: device.NoteAfterTouch(channel, parameter1, parameter2); break;
            case 0xb0: device.ControllerChange(channel, parameter1, parameter2); break;
            case 0xc0: device.PatchChange(channel, parameter1); break;
            case 0xd0: device.ChannelAfterTouch(channel, parameter1); break;
            case 0xe0: device.PitchBend(channel, parameter2, parameter1); break;
        }
    }

    public static void HandleLongEvent(IVmMusicFmMidiDevice device, ReadOnlySpan<byte> data)
    {
        ArgumentNullException.ThrowIfNull(device);
        device.SystemExclusive(data);
    }
}
