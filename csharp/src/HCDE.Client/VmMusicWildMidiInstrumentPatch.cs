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

public interface IVmMusicWildMidiInstrumentPatch : IVmMusicWildMidiPatch
{
    VmMusicWildMidiInstrument Instrument { get; }
}

public sealed class VmMusicWildMidiInstrumentPatch : IVmMusicWildMidiInstrumentPatch
{
    public ushort Id { get; }
    public bool Loaded { get; set; }
    public int UseCount { get; set; }
    public VmMusicWildMidiInstrument Instrument { get; private set; }
    public bool HasSamples => Instrument.Samples.Count != 0;
    public byte Keep { get; private set; }
    public byte Remove { get; private set; }
    public int Amplification { get; private set; } = 1024;
    private int?[] envelopeTargets = new int?[6];
    private float?[] envelopeTimes = new float?[6];

    public VmMusicWildMidiInstrumentPatch(ushort id, byte note = 0)
    {
        if (note > 127) throw new ArgumentOutOfRangeException(nameof(note));
        Id = id;
        Instrument = new(note, Array.Empty<VmMusicWildMidiGusPatchSample>());
    }

    public void Load(ReadOnlySpan<byte> patchBytes, bool automaticAmplification = false, bool combineAmplification = false)
    {
        var samples = VmMusicWildMidiGusPatchReader.Read(patchBytes);
        var amplification = automaticAmplification
            ? VmMusicWildMidiAutoAmplification.Compute(samples, Amplification, combineAmplification) : Amplification;
        samples = Array.AsReadOnly(samples.Select(sample =>
        {
            var modes = sample.Pcm.Modes;
            if ((Id & 128) != 0)
            {
                if ((Keep & 4) == 0) modes &= unchecked((byte)~4);
                if ((Keep & 64) == 0) modes &= unchecked((byte)~64);
            }
            modes &= unchecked((byte)~Remove);
            if ((Keep & 64) != 0) modes |= 64;
            return sample with
            {
                Header = sample.Header with { Modes = modes },
                Pcm = sample.Pcm with { Modes = modes },
                EnvelopeSourceModes = sample.Header.Modes,
                CopyStageTwoToRelease = Id == 47 && (modes & 4) == 0,
                EnvelopeTargetOverrides = Array.AsReadOnly((int?[])envelopeTargets.Clone()),
                EnvelopeTimeOverrides = Array.AsReadOnly((float?[])envelopeTimes.Clone())
            };
        }).ToArray());
        Instrument = new(Instrument.Note, samples);
        Amplification = amplification;
        Loaded = true;
    }

    public void FreeSamples()
    {
        Instrument = new(Instrument.Note, Array.Empty<VmMusicWildMidiGusPatchSample>());
        Loaded = false;
    }

    internal void ConfigureNote(byte note, byte keep = 0, byte remove = 0, int amplification = 1024, int?[]? targets = null, float?[]? times = null)
    {
        if (note > 127) throw new ArgumentOutOfRangeException(nameof(note));
        if (amplification < 0) throw new ArgumentOutOfRangeException(nameof(amplification));
        Instrument = new(note, Instrument.Samples);
        Keep = keep;
        Remove = remove;
        Amplification = amplification;
        envelopeTargets = targets is null ? new int?[6] : (int?[])targets.Clone();
        envelopeTimes = times is null ? new float?[6] : (float?[])times.Clone();
    }
}
