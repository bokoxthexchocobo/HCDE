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

public static class VmMusicWildMidiVoiceLifecycle
{
    public static int AdvanceSample(IList<VmMusicWildMidiNoteReleaseState> voices, int index,
        VmMusicWildMidiSampleBounds bounds, bool enhancedResampling)
    {
        Validate(voices, index);
        if (VmMusicWildMidiSamplePosition.Advance(voices[index], bounds, enhancedResampling))
        {
            Complete(voices, index);
            return index;
        }
        return AdvanceEnvelope(voices, index);
    }

    // The returned index is the next voice to process in the same mixer frame.
    public static int AdvanceEnvelope(IList<VmMusicWildMidiNoteReleaseState> voices, int index)
    {
        Validate(voices, index);
        if (!VmMusicWildMidiEnvelope.Advance(voices[index])) return index + 1;
        Complete(voices, index);
        return index;
    }

    // Sample exhaustion and envelope completion share the native END_THIS_NOTE path.
    public static VmMusicWildMidiNoteReleaseState? Complete(
        IList<VmMusicWildMidiNoteReleaseState> voices, int index)
    {
        Validate(voices, index);
        var voice = voices[index];
        if (voice.Replay is { } replay)
        {
            voices[index] = replay;
            voice.Active = false;
            replay.Active = true;
            return replay;
        }
        voices.RemoveAt(index);
        voice.Active = false;
        return index < voices.Count ? voices[index] : null;
    }

    private static void Validate(IList<VmMusicWildMidiNoteReleaseState> voices, int index)
    {
        ArgumentNullException.ThrowIfNull(voices);
        if (index < 0 || index >= voices.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (voices.IsReadOnly) throw new ArgumentException("The mixer voice list must be mutable.", nameof(voices));
    }
}
