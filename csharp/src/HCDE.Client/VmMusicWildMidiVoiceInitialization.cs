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

public readonly record struct VmMusicWildMidiInitializedVoice(VmMusicWildMidiNoteReleaseState Voice,
    VmMusicWildMidiSampleData Sample);

public static class VmMusicWildMidiVoiceInitialization
{
    public static VmMusicWildMidiNoteReleaseState? Start(IList<VmMusicWildMidiNoteReleaseState> voices,
        VmMusicWildMidiNoteReleaseState primary, VmMusicWildMidiNoteReleaseState secondary,
        VmMusicWildMidiInitializedVoice prepared)
    {
        ArgumentNullException.ThrowIfNull(prepared.Voice);
        if (prepared.Voice.EnvelopeRates.Length < 7 || prepared.Voice.EnvelopeTargets.Length < 7)
            throw new ArgumentException("Prepared voices require all seven envelope stages.", nameof(prepared));
        if (ReferenceEquals(prepared.Voice, primary) || ReferenceEquals(prepared.Voice, secondary))
            throw new ArgumentException("Prepared voice state must be separate from the reusable slots.", nameof(prepared));
        var destination = VmMusicWildMidiNoteSlots.Select(voices, primary, secondary);
        if (destination is null) return null;
        var source = prepared.Voice;
        destination.NoteId = source.NoteId;
        destination.Velocity = source.Velocity;
        destination.PatchNote = source.PatchNote;
        destination.IncrementDivisor = source.IncrementDivisor;
        destination.SampleIncrement = source.SampleIncrement;
        destination.SamplePosition = 0;
        destination.EnvelopeTargets = source.EnvelopeTargets;
        destination.EnvelopeRates = source.EnvelopeRates;
        destination.Envelope = 0;
        destination.EnvelopeLevel = 0;
        destination.EnvelopeIncrement = source.EnvelopeRates[0];
        destination.Modes = source.Modes;
        destination.Hold = source.Hold;
        destination.Replay = null;
        destination.IsOff = false;
        destination.MixVolume = source.MixVolume;
        destination.Sample = prepared.Sample;
        return destination;
    }

    public static VmMusicWildMidiInitializedVoice Create(VmMusicWildMidiGusPatchSample sample,
        VmMusicWildMidiChannelState channelState, int channel, byte note, byte velocity,
        byte patchNote, int sampleRate, int masterVolume, bool logarithmic)
    {
        ArgumentNullException.ThrowIfNull(sample);
        ArgumentNullException.ThrowIfNull(channelState);
        if (channel is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        if (note > 127) throw new ArgumentOutOfRangeException(nameof(note));
        if (velocity is 0 or > 127) throw new ArgumentOutOfRangeException(nameof(velocity));
        if (patchNote > 127) throw new ArgumentOutOfRangeException(nameof(patchNote));
        var mixerSample = sample.MixerSample;
        var envelope = sample.ConvertEnvelope(sampleRate);
        var noteId = (ushort)((channel << 8) | note);
        var increment = VmMusicWildMidiSampleIncrement.Compute(noteId, patchNote,
            channelState.PitchAdjust, sampleRate, sample.Header.IncrementDivisor);
        var voice = new VmMusicWildMidiNoteReleaseState
        {
            NoteId = noteId, Velocity = velocity, PatchNote = patchNote,
            IncrementDivisor = sample.Header.IncrementDivisor,
            SampleIncrement = increment, EnvelopeTargets = envelope.Targets, EnvelopeRates = envelope.Rates,
            EnvelopeIncrement = envelope.Rates[0], Modes = sample.Pcm.Modes, Hold = channelState.Hold,
            MixVolume = VmMusicWildMidiNoteVolume.Compute(channelState, velocity, masterVolume, logarithmic)
        };
        // List insertion or replay activation owns Active; freshly prepared voices remain inactive.
        return new(voice, mixerSample);
    }
}
