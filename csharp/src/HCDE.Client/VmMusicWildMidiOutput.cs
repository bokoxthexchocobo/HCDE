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

using System.Globalization;

namespace HCDE.Client;

public interface IVmMusicWildMidiRenderer : IVmMusicWildMidiSettingsDevice
{
    void ShortEvent(int status, int parameter1, int parameter2);
    void LongEvent(ReadOnlySpan<byte> data);
    void ComputeOutput(Span<float> stereoSamples, int frames);
    int GetVoiceCount();
}

public static class VmMusicWildMidiOutput
{
    public static void HandleEvent(IVmMusicWildMidiRenderer renderer, int status, int parameter1, int parameter2)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.ShortEvent(status, parameter1, parameter2);
    }

    public static void HandleLongEvent(IVmMusicWildMidiRenderer renderer, ReadOnlySpan<byte> data)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        renderer.LongEvent(data);
    }

    public static void ComputeOutput(IVmMusicWildMidiRenderer renderer, Span<float> buffer, int frames)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentOutOfRangeException.ThrowIfNegative(frames);
        var samples = checked(frames * 2);
        if (buffer.Length < samples)
            throw new ArgumentException("WildMIDI output buffer must hold every stereo frame.", nameof(buffer));
        renderer.ComputeOutput(buffer[..samples], frames);
    }

    public static string GetStatistics(IVmMusicWildMidiRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        return renderer.GetVoiceCount().ToString(CultureInfo.InvariantCulture).PadLeft(3) + " voices";
    }
}
