/*
** music_wildmidi_mididevice.cpp
** Provides access to WildMidi as a generic MIDI device.
**
**---------------------------------------------------------------------------
** Copyright 2015 Marisa Heit
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

public interface IVmMusicWildMidiSettingsDevice
{
    void LoadInstrument(int bank, int drumSet, int instrument);
    void SetOption(int mask, int value);
}

public static class VmMusicWildMidiSettings
{
    public const int EnhancedResampling = 2;
    public const int Reverb = 4;

    public static void Initialize(IVmMusicWildMidiSettingsDevice device, bool enhancedResampling, bool reverb)
    {
        ArgumentNullException.ThrowIfNull(device);
        device.SetOption(EnhancedResampling | Reverb,
            (enhancedResampling ? EnhancedResampling : 0) | (reverb ? Reverb : 0));
    }

    public static void PrecacheInstruments(IVmMusicWildMidiSettingsDevice device, ReadOnlySpan<ushort> instruments)
    {
        ArgumentNullException.ThrowIfNull(device);
        foreach (var instrument in instruments)
            device.LoadInstrument((instrument >> 7) & 127, instrument >> 14, instrument & 127);
    }

    public static bool ChangeInteger(IVmMusicWildMidiSettingsDevice device, string option, int value)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(option);
        var mask = option.Equals("wildmidi.reverb", StringComparison.OrdinalIgnoreCase) ? Reverb
            : option.Equals("wildmidi.resampling", StringComparison.OrdinalIgnoreCase) ? EnhancedResampling : 0;
        if (mask == 0) return false;
        device.SetOption(mask, unchecked(mask * value));
        return true;
    }
}
