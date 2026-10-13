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

// All bounds use the native ten-bit fractional sample-position units.
public readonly record struct VmMusicWildMidiSampleBounds(uint DataLength, uint LoopStart, uint LoopEnd, uint LoopSize);

public static class VmMusicWildMidiSamplePosition
{
    // True signals sample exhaustion; the caller completes the voice before advancing its envelope.
    public static bool Advance(VmMusicWildMidiNoteReleaseState voice,
        VmMusicWildMidiSampleBounds bounds, bool enhancedResampling)
    {
        ArgumentNullException.ThrowIfNull(voice);
        var position = unchecked(voice.SamplePosition + (uint)voice.SampleIncrement);
        var looping = (voice.Modes & VmMusicWildMidiNoteRelease.Loop) != 0;
        if (looping && position > bounds.LoopEnd)
        {
            if (bounds.LoopSize == 0)
                throw new ArgumentException("A wrapping sample loop must have a nonzero size.", nameof(bounds));
            position = unchecked(bounds.LoopStart + unchecked(position - bounds.LoopStart) % bounds.LoopSize);
        }
        voice.SamplePosition = position;
        // The Gaussian mixer checks exhaustion only after crossing loop_end, even without looping.
        return !looping && (!enhancedResampling || position > bounds.LoopEnd) && position >= bounds.DataLength;
    }
}
