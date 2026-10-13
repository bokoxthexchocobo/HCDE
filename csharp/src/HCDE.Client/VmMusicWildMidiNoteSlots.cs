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

public static class VmMusicWildMidiNoteSlots
{
    // Null suppresses the repeated note; otherwise the caller initializes the returned slot.
    public static VmMusicWildMidiNoteReleaseState? Select(IList<VmMusicWildMidiNoteReleaseState> voices,
        VmMusicWildMidiNoteReleaseState primary, VmMusicWildMidiNoteReleaseState secondary)
    {
        ArgumentNullException.ThrowIfNull(voices);
        ArgumentNullException.ThrowIfNull(primary);
        ArgumentNullException.ThrowIfNull(secondary);
        if (ReferenceEquals(primary, secondary)) throw new ArgumentException("Note slots must be distinct.");
        if (primary.Active || secondary.Active)
        {
            // Native code consults the primary slot even when only the secondary is active.
            if ((primary.Modes & VmMusicWildMidiNoteRelease.Envelope) != 0
                && primary.Envelope < 3 && (primary.Hold & 2) == 0) return null;
            var current = primary.Active ? primary : secondary;
            var destination = primary.Active ? secondary : primary;
            if (current.EnvelopeRates.Length < 7)
                throw new ArgumentException("Replay scheduling requires the terminal envelope rate.");
            current.Replay = destination;
            current.Envelope = 6;
            current.EnvelopeIncrement = unchecked(-current.EnvelopeRates[6]);
            return destination;
        }
        if (voices.IsReadOnly) throw new ArgumentException("The mixer voice list must be mutable.", nameof(voices));
        voices.Add(primary);
        primary.Active = true;
        return primary;
    }
}
