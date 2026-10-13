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

public static class VmMusicWildMidiAutoAmplification
{
    public static int Compute(IReadOnlyList<VmMusicWildMidiGusPatchSample> samples, int amplification, bool combine)
    {
        ArgumentNullException.ThrowIfNull(samples);
        if (amplification < 0) throw new ArgumentOutOfRangeException(nameof(amplification));
        var maximum = 0;
        var minimum = 0;
        foreach (var sample in samples)
        {
            foreach (var value in sample.Pcm.Data.AsSpan(0, checked((int)sample.Pcm.DataLength)))
            {
                maximum = Math.Max(maximum, value);
                minimum = Math.Min(minimum, value);
            }
        }
        if (maximum == 0 && minimum == 0)
            throw new InvalidDataException("Automatic amplification is undefined for silent patches.");
        var factor = maximum >= -minimum ? (32767 << 10) / maximum : (32768 << 10) / -minimum;
        if (!combine) return factor;
        var product = (long)amplification * factor;
        if (product > int.MaxValue) throw new InvalidDataException("Automatic amplification exceeds the native signed multiplication range.");
        return (int)product >> 10;
    }
}
