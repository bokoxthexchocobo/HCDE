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

public static class VmMusicWildMidiShortEvents
{
    public static bool Dispatch(VmMusicWildMidiNoteTable notes,
        IReadOnlyList<VmMusicWildMidiChannelState> channels,
        Func<ushort, IVmMusicWildMidiInstrumentPatch?> findExact,
        int status, int parameter1, int parameter2, int sampleRate, int masterVolume, bool logarithmic)
    {
        ArgumentNullException.ThrowIfNull(notes);
        ArgumentNullException.ThrowIfNull(channels);
        ArgumentNullException.ThrowIfNull(findExact);
        if (channels.Count != 16) throw new ArgumentException("MIDI dispatch requires sixteen channels.", nameof(channels));
        var command = status & 0xf0;
        if (command is < 0x80 or > 0xe0) return false;
        if (parameter1 is < 0 or > 127) throw new ArgumentOutOfRangeException(nameof(parameter1));
        if (command is not (0xc0 or 0xd0) && parameter2 is < 0 or > 127)
            throw new ArgumentOutOfRangeException(nameof(parameter2));
        var channel = status & 15;
        var state = channels[channel];
        switch (command)
        {
            case 0x80: return notes.NoteOff(channel, (byte)parameter1, state.IsDrum);
            case 0x90: return notes.NoteOn(channel, (byte)parameter1, (byte)parameter2, state.IsDrum,
                (ch, note, velocity) => VmMusicWildMidiNotePreparation.Prepare(state, findExact,
                    ch, note, velocity, sampleRate, masterVolume, logarithmic));
            case 0xa0: return notes.Aftertouch(state, channel, (byte)parameter1, (byte)parameter2, masterVolume, logarithmic);
            case 0xb0: return notes.Control(state, channel, (byte)parameter1, (byte)parameter2, masterVolume, logarithmic);
            case 0xc0:
                VmMusicWildMidiPatchLookup.ChangeProgram(state, parameter1, id => findExact(id));
                return true;
            case 0xd0:
                notes.ChannelPressure(state, channel, (byte)parameter1, masterVolume, logarithmic);
                return true;
            case 0xe0:
                notes.PitchBend(state, channel, parameter1 | parameter2 << 7, sampleRate);
                return true;
            default: return false;
        }
    }
}
