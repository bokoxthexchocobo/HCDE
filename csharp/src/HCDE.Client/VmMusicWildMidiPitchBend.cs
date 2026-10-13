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

public static class VmMusicWildMidiPitchBend
{
    public static void Apply(VmMusicWildMidiChannelState channelState,
        IEnumerable<VmMusicWildMidiNoteReleaseState> voices, int channel, int pitchData, int sampleRate)
    {
        ArgumentNullException.ThrowIfNull(channelState);
        Apply(channelState, voices, channel, pitchData, voice => VmMusicWildMidiSampleIncrement.Compute(
            voice.NoteId, voice.PatchNote, channelState.PitchAdjust, sampleRate, voice.IncrementDivisor));
    }

    public static void Apply(VmMusicWildMidiChannelState channelState,
        IEnumerable<VmMusicWildMidiNoteReleaseState> voices, int channel, int pitchData,
        Func<VmMusicWildMidiNoteReleaseState, ulong> computeIncrement)
    {
        ArgumentNullException.ThrowIfNull(channelState);
        ArgumentNullException.ThrowIfNull(voices);
        ArgumentNullException.ThrowIfNull(computeIncrement);
        if (channel is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        channelState.Pitch = unchecked((short)(pitchData - 0x2000));
        channelState.PitchAdjust = channelState.PitchRange * channelState.Pitch
            / (channelState.Pitch < 0 ? 8192 : 8191);
        foreach (var voice in voices)
            if ((voice.NoteId >> 8) == channel) voice.SampleIncrement = computeIncrement(voice);
    }
}
