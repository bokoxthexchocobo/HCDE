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

public static class VmMusicWildMidiChannelVolume
{
    public static void Adjust(IEnumerable<VmMusicWildMidiNoteReleaseState> voices, int channel,
        Action<int, VmMusicWildMidiNoteReleaseState> adjustNoteVolume)
    {
        ArgumentNullException.ThrowIfNull(voices);
        ArgumentNullException.ThrowIfNull(adjustNoteVolume);
        if (channel is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(channel));
        foreach (var voice in voices)
        {
            if (channel <= 15 && (voice.NoteId >> 8) != channel) continue;
            adjustNoteVolume(channel, voice);
            if (voice.Replay is { } replay) adjustNoteVolume(channel, replay);
        }
    }
}
