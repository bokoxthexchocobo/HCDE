/*
 wildmidi_lib.c

 Midi Wavetable Processing library

 Copyright (C) Chris Ison  2001-2014
 Copyright (C) Bret Curtis 2013-2014

 This file is part of WildMIDI.

 WildMIDI is free software: you can redistribute and/or modify the player
 under the terms of the GNU General Public License and you can redistribute
 and/or modify the library under the terms of the GNU Lesser General Public
 License as published by the Free Software Foundation, either version 3 of
 the licenses, or(at your option) any later version.

 WildMIDI is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License and
 the GNU Lesser General Public License for more details.

 You should have received a copy of the GNU General Public License and the
 GNU Lesser General Public License along with WildMIDI.  If not,  see
 <http://www.gnu.org/licenses/>.
 */

namespace HCDE.Client;

public static class VmMusicWildMidiMixerOutput
{
    // Applied after integer mixing and reverb, once per produced sample.
    public static void Convert(ReadOnlySpan<int> stereoSamples, Span<float> destination)
    {
        if ((stereoSamples.Length & 1) != 0)
            throw new ArgumentException("WildMIDI mixer output must contain complete stereo frames.", nameof(stereoSamples));
        if (destination.Length < stereoSamples.Length)
            throw new ArgumentException("Output buffer must hold every mixed sample.", nameof(destination));
        const float gain = 1.3f / 32768f;
        for (var i = 0; i < stereoSamples.Length; i++)
            destination[i] = (float)stereoSamples[i] * gain;
    }
}
