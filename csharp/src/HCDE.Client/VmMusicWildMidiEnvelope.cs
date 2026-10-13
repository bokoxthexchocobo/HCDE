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

public static class VmMusicWildMidiEnvelope
{
    // True asks the mixer to remove the voice or replace it with its replay.
    public static bool Advance(VmMusicWildMidiNoteReleaseState note)
    {
        ArgumentNullException.ThrowIfNull(note);
        if (note.EnvelopeIncrement == 0) return false;
        if (note.Envelope is < 0 or > 6 || note.EnvelopeTargets.Length < 7 || note.EnvelopeRates.Length < 7)
            throw new ArgumentException("Envelope arrays must include all seven stages.", nameof(note));
        note.EnvelopeLevel = unchecked(note.EnvelopeLevel + note.EnvelopeIncrement);
        var target = note.EnvelopeTargets[note.Envelope];
        if (note.EnvelopeIncrement < 0 ? note.EnvelopeLevel > target : note.EnvelopeLevel < target)
            return false;
        note.EnvelopeLevel = target;
        switch (note.Envelope)
        {
            case 0 when (note.Modes & VmMusicWildMidiNoteRelease.Envelope) == 0:
            case 2 when (note.Modes & VmMusicWildMidiNoteRelease.Sustain) != 0:
                note.EnvelopeIncrement = 0;
                return false;
            case 2 when (note.Modes & VmMusicWildMidiNoteRelease.Clamped) != 0:
                note.Envelope = 5;
                SetIncrement(note);
                return false;
            case 5:
                if (note.EnvelopeLevel == 0) return true;
                note.Modes &= ~VmMusicWildMidiNoteRelease.Loop;
                note.EnvelopeIncrement = 0;
                return false;
            case 6:
                return true;
        }
        note.Envelope++;
        if (note.IsOff) VmMusicWildMidiNoteRelease.Release(note, note.EnvelopeTargets, note.EnvelopeRates);
        SetIncrement(note);
        return false;
    }

    private static void SetIncrement(VmMusicWildMidiNoteReleaseState note)
    {
        note.EnvelopeIncrement = note.EnvelopeLevel > note.EnvelopeTargets[note.Envelope]
            ? unchecked(-note.EnvelopeRates[note.Envelope]) : note.EnvelopeRates[note.Envelope];
    }
}
