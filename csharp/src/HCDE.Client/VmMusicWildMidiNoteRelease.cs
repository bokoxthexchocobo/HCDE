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

public sealed class VmMusicWildMidiNoteReleaseState
{
    public bool Active { get; set; }
    public ushort NoteId { get; set; }
    public byte Velocity { get; set; }
    public VmMusicWildMidiMixVolume MixVolume { get; set; }
    public ulong SampleIncrement { get; set; }
    public uint SamplePosition { get; set; }
    public byte PatchNote { get; set; }
    public ulong IncrementDivisor { get; set; }
    public VmMusicWildMidiNoteReleaseState? Replay { get; set; }
    public int[] EnvelopeTargets { get; set; } = [];
    public int[] EnvelopeRates { get; set; } = [];
    public VmMusicWildMidiSampleData? Sample { get; set; }
    public int Modes { get; set; }
    public int Hold { get; set; }
    public int Envelope { get; set; }
    public int EnvelopeLevel { get; set; }
    public int EnvelopeIncrement { get; set; }
    public bool IsOff { get; set; }
}

public static class VmMusicWildMidiNoteRelease
{
    public const int Loop = 4, Sustain = 32, Envelope = 64, Clamped = 128;

    public static void HoldPedal(VmMusicWildMidiChannelState channelState,
        IEnumerable<VmMusicWildMidiNoteReleaseState> voices, int channel, int value)
    {
        ArgumentNullException.ThrowIfNull(channelState);
        ArgumentNullException.ThrowIfNull(voices);
        if (channel is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        channelState.Hold = value > 63 ? (byte)1 : (byte)0;
        if (channelState.Hold != 0) return;
        foreach (var voice in voices)
        {
            if ((voice.NoteId >> 8) != channel) continue;
            if ((voice.Hold & 2) != 0)
            {
                if ((voice.Modes & Envelope) != 0)
                {
                    var stage = (voice.Modes & Clamped) != 0 ? 5 : 4;
                    if (voice.Envelope < stage)
                    {
                        if (voice.EnvelopeTargets.Length <= stage || voice.EnvelopeRates.Length <= stage)
                            throw new ArgumentException("Envelope arrays must include the pedal-release stage.", nameof(voices));
                        voice.Envelope = stage;
                        voice.EnvelopeIncrement = voice.EnvelopeLevel > voice.EnvelopeTargets[stage]
                            ? unchecked(-voice.EnvelopeRates[stage]) : voice.EnvelopeRates[stage];
                    }
                }
                else
                {
                    voice.Modes &= ~Loop;
                    voice.EnvelopeIncrement = 0;
                }
            }
            voice.Hold = 0;
        }
    }

    public static void NotesOff(IEnumerable<VmMusicWildMidiNoteReleaseState> voices, int channel, bool percussion)
    {
        ArgumentNullException.ThrowIfNull(voices);
        if (channel is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        if (percussion) return;
        foreach (var voice in voices)
        {
            if ((voice.NoteId >> 8) != channel) continue;
            if (voice.Hold != 0) voice.Hold |= 2;
            else if ((voice.Modes & Envelope) != 0 && voice.Envelope < 5)
            {
                if (voice.EnvelopeTargets.Length <= 5 || voice.EnvelopeRates.Length <= 5)
                    throw new ArgumentException("Envelope arrays must include the notes-off stage.", nameof(voices));
                voice.EnvelopeIncrement = voice.EnvelopeLevel > voice.EnvelopeTargets[5]
                    ? unchecked(-voice.EnvelopeRates[5]) : voice.EnvelopeRates[5];
                voice.Envelope = 5;
            }
        }
    }

    public static void SoundOff(IEnumerable<VmMusicWildMidiNoteReleaseState> voices, int channel)
    {
        ArgumentNullException.ThrowIfNull(voices);
        if (channel is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        foreach (var voice in voices)
        {
            if ((voice.NoteId >> 8) != channel) continue;
            voice.Active = false;
            voice.Replay = null;
        }
    }

    public static bool NoteOff(VmMusicWildMidiNoteReleaseState primary,
        VmMusicWildMidiNoteReleaseState secondary, bool percussion)
    {
        ArgumentNullException.ThrowIfNull(primary);
        ArgumentNullException.ThrowIfNull(secondary);
        var note = primary.Active ? primary : secondary;
        if (!note.Active || (percussion && (note.Modes & Loop) == 0)) return false;
        if ((note.Modes & Envelope) != 0 && note.Envelope == 0) note.IsOff = true;
        else Release(note, note.EnvelopeTargets, note.EnvelopeRates);
        return true;
    }

    public static void Release(VmMusicWildMidiNoteReleaseState note, ReadOnlySpan<int> targets, ReadOnlySpan<int> rates)
    {
        ArgumentNullException.ThrowIfNull(note);
        var stage = (note.Modes & Sustain) != 0 ? 3 : (note.Modes & Clamped) != 0 ? 5 : 4;
        var changeStage = (note.Modes & Envelope) != 0 && note.Hold == 0 && note.Envelope < stage;
        if (changeStage && (targets.Length <= stage || rates.Length <= stage))
            throw new ArgumentException("Envelope arrays must include the release stage.");
        note.IsOff = false;
        if ((note.Modes & Envelope) == 0)
        {
            note.Modes &= ~Loop;
            note.EnvelopeIncrement = 0;
        }
        else if (note.Hold != 0) note.Hold |= 2;
        else if (changeStage)
        {
            note.Envelope = stage;
            note.EnvelopeIncrement = note.EnvelopeLevel > targets[stage] ? unchecked(-rates[stage]) : rates[stage];
        }
    }
}
