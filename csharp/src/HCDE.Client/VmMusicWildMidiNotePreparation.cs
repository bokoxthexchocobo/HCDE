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

public sealed record VmMusicWildMidiInstrument(byte Note, IReadOnlyList<VmMusicWildMidiGusPatchSample> Samples);

public static class VmMusicWildMidiNotePreparation
{
    public static VmMusicWildMidiInitializedVoice? Prepare(VmMusicWildMidiChannelState state,
        Func<ushort, IVmMusicWildMidiInstrumentPatch?> findExact, int channel, byte note, byte velocity,
        int sampleRate, int masterVolume, bool logarithmic)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(findExact);
        var melodic = state.Patch switch
        {
            IVmMusicWildMidiInstrumentPatch patch => patch.Instrument,
            null => null,
            _ when state.IsDrum => null,
            _ => throw new ArgumentException("The selected channel patch must expose decoded instrument samples.", nameof(state))
        };
        return Prepare(state, melodic, id => findExact(id)?.Instrument,
            channel, note, velocity, sampleRate, masterVolume, logarithmic);
    }

    public static VmMusicWildMidiInitializedVoice? Prepare(VmMusicWildMidiChannelState state,
        VmMusicWildMidiInstrument? melodicInstrument, Func<ushort, VmMusicWildMidiInstrument?> findExact,
        int channel, byte note, byte velocity, int sampleRate, int masterVolume, bool logarithmic)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(findExact);
        if (channel is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        if (note > 127) throw new ArgumentOutOfRangeException(nameof(note));
        if (velocity is 0 or > 127) throw new ArgumentOutOfRangeException(nameof(velocity));
        var instrument = state.IsDrum
            ? VmMusicWildMidiPatchLookup.Find((ushort)((state.Bank << 8) | note | 128), findExact)
            : melodicInstrument;
        if (instrument is null) return null;
        // Melodic sample selection ignores patch pitch; percussion uses its override.
        var selectionNote = state.IsDrum && instrument.Note != 0 ? instrument.Note : note;
        var frequency = VmMusicWildMidiSampleIncrement.NoteFrequency(selectionNote) / 100;
        var sample = VmMusicWildMidiPatchLookup.SelectSample(instrument.Samples, frequency,
            sample => sample.Header.FrequencyLow, sample => sample.Header.FrequencyHigh);
        return sample is null ? null : VmMusicWildMidiVoiceInitialization.Create(sample, state,
            channel, note, velocity, instrument.Note, sampleRate, masterVolume, logarithmic);
    }
}
