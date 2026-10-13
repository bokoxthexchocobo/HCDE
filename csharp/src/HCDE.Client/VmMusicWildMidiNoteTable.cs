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

public sealed class VmMusicWildMidiNoteTable
{
    private readonly VmMusicWildMidiNoteReleaseState[,,] slots = new VmMusicWildMidiNoteReleaseState[2, 16, 128];
    public List<VmMusicWildMidiNoteReleaseState> Voices { get; } = [];

    public VmMusicWildMidiNoteTable()
    {
        for (var slot = 0; slot < 2; slot++)
            for (var channel = 0; channel < 16; channel++)
                for (var note = 0; note < 128; note++)
                    slots[slot, channel, note] = new() { NoteId = (ushort)((channel << 8) | note) };
    }

    public bool NoteOff(int channel, byte note, bool percussion)
    {
        Validate(channel, note);
        return VmMusicWildMidiNoteRelease.NoteOff(slots[0, channel, note], slots[1, channel, note], percussion);
    }

    public bool Aftertouch(VmMusicWildMidiChannelState state, int channel, byte note, byte pressure,
        int masterVolume, bool logarithmic)
    {
        Validate(channel, note);
        ValidatePressure(state, pressure);
        return VmMusicWildMidiAftertouch.Apply(slots[0, channel, note], slots[1, channel, note], pressure,
            voice => voice.MixVolume = VmMusicWildMidiNoteVolume.Compute(state, voice.Velocity, masterVolume, logarithmic));
    }

    public void ChannelPressure(VmMusicWildMidiChannelState state, int channel, byte pressure,
        int masterVolume, bool logarithmic)
    {
        Validate(channel, 0);
        ValidatePressure(state, pressure);
        VmMusicWildMidiAftertouch.ApplyChannelPressure(Voices, channel, pressure,
            voice => voice.MixVolume = VmMusicWildMidiNoteVolume.Compute(state, voice.Velocity, masterVolume, logarithmic));
    }

    private static void ValidatePressure(VmMusicWildMidiChannelState state, byte pressure)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (pressure > 127) throw new ArgumentOutOfRangeException(nameof(pressure));
    }

    public void PitchBend(VmMusicWildMidiChannelState state, int channel, int pitch, int sampleRate)
    {
        Validate(channel, 0);
        ArgumentNullException.ThrowIfNull(state);
        if (pitch is < 0 or > 16383) throw new ArgumentOutOfRangeException(nameof(pitch));
        if (sampleRate is < 11025 or > 65535) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        VmMusicWildMidiPitchBend.Apply(state, Voices, channel, pitch, sampleRate);
    }

    public bool Control(VmMusicWildMidiChannelState state, int channel, byte controller, byte value,
        int masterVolume, bool logarithmic)
    {
        Validate(channel, 0);
        ArgumentNullException.ThrowIfNull(state);
        if (controller > 127) throw new ArgumentOutOfRangeException(nameof(controller));
        if (value > 127) throw new ArgumentOutOfRangeException(nameof(value));
        return VmMusicWildMidiChannelControls.Apply(state, Voices, channel, controller, value,
            () => VmMusicWildMidiChannelVolume.Adjust(Voices, channel,
                (_, voice) => voice.MixVolume = VmMusicWildMidiNoteVolume.Compute(state, voice.Velocity, masterVolume, logarithmic)));
    }

    public void Clear()
    {
        Voices.Clear();
        foreach (var voice in slots)
        {
            voice.Active = false;
            voice.Replay = null;
            voice.Sample = null;
            voice.EnvelopeTargets = [];
            voice.EnvelopeRates = [];
            voice.Modes = voice.Hold = voice.Envelope = voice.EnvelopeLevel = voice.EnvelopeIncrement = 0;
            voice.IsOff = false;
            voice.SamplePosition = 0;
        }
    }

    public bool NoteOn(int channel, byte note, byte velocity, bool percussion,
        Func<int, byte, byte, VmMusicWildMidiInitializedVoice?> prepare)
    {
        Validate(channel, note);
        if (velocity > 127) throw new ArgumentOutOfRangeException(nameof(velocity));
        ArgumentNullException.ThrowIfNull(prepare);
        if (velocity == 0) return NoteOff(channel, note, percussion);
        var prepared = prepare(channel, note, velocity);
        if (prepared is null) return false;
        if (prepared.Value.Voice is null || prepared.Value.Voice.NoteId != (channel << 8 | note))
            throw new ArgumentException("Prepared voice identity must match the MIDI event.", nameof(prepare));
        return VmMusicWildMidiVoiceInitialization.Start(Voices, slots[0, channel, note],
            slots[1, channel, note], prepared.Value) is not null;
    }

    private static void Validate(int channel, byte note)
    {
        if (channel is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        if (note > 127) throw new ArgumentOutOfRangeException(nameof(note));
    }
}
