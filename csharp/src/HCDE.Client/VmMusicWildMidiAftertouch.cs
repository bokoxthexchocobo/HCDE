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

public static class VmMusicWildMidiAftertouch
{
    public static void ApplyChannelPressure(IEnumerable<VmMusicWildMidiNoteReleaseState> voices,
        int channel, int pressure, Action<VmMusicWildMidiNoteReleaseState> adjustVolume)
    {
        ArgumentNullException.ThrowIfNull(voices);
        ArgumentNullException.ThrowIfNull(adjustVolume);
        if (channel is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        foreach (var voice in voices)
            if ((voice.NoteId >> 8) == channel) AdjustVoiceAndReplay(voice, pressure, adjustVolume);
    }

    public static bool Apply(VmMusicWildMidiNoteReleaseState primary,
        VmMusicWildMidiNoteReleaseState secondary, int pressure,
        Action<VmMusicWildMidiNoteReleaseState> adjustVolume)
    {
        ArgumentNullException.ThrowIfNull(primary);
        ArgumentNullException.ThrowIfNull(secondary);
        ArgumentNullException.ThrowIfNull(adjustVolume);
        var note = primary.Active ? primary : secondary;
        if (!note.Active) return false;
        AdjustVoiceAndReplay(note, pressure, adjustVolume);
        return true;
    }

    private static void AdjustVoiceAndReplay(VmMusicWildMidiNoteReleaseState note, int pressure,
        Action<VmMusicWildMidiNoteReleaseState> adjustVolume)
    {
        note.Velocity = unchecked((byte)pressure);
        adjustVolume(note);
        if (note.Replay is { } replay)
        {
            replay.Velocity = unchecked((byte)pressure);
            adjustVolume(replay);
        }
    }
}
