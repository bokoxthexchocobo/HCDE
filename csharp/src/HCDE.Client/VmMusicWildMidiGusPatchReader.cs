/*
 * gus_pat.c -- Midi Wavetable Processing library
 *
 * Copyright (C) WildMIDI Developers 2001-2015
 *
 * This file is part of WildMIDI.
 *
 * WildMIDI is free software: you can redistribute and/or modify the player
 * under the terms of the GNU General Public License and you can redistribute
 * and/or modify the library under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation, either version 3 of
 * the licenses, or(at your option) any later version.
 *
 * WildMIDI is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License and
 * the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License and the
 * GNU Lesser General Public License along with WildMIDI.  If not,  see
 * <http://www.gnu.org/licenses/>.
 */

namespace HCDE.Client;

public sealed record VmMusicWildMidiGusPatchSample(VmMusicWildMidiGusSampleHeader Header,
    VmMusicWildMidiDecodedPcm Pcm, ReadOnlyMemory<byte> EnvelopeRates, ReadOnlyMemory<byte> EnvelopeLevels)
{
    public bool CopyStageTwoToRelease { get; init; }
    public byte? EnvelopeSourceModes { get; init; }
    public IReadOnlyList<int?>? EnvelopeTargetOverrides { get; init; }
    public IReadOnlyList<float?>? EnvelopeTimeOverrides { get; init; }
    public VmMusicWildMidiSampleData MixerSample => VmMusicWildMidiPcmDecoder.ToMixerSample(Pcm);
    public VmMusicWildMidiGusEnvelope ConvertEnvelope(int sampleRate)
    {
        // Native decoding precedes keep=env; enabling it later retains decoded defaults.
        var sourceModes = (Header.Modes & 64) != 0 ? EnvelopeSourceModes ?? Header.Modes : Header.Modes;
        var envelope = VmMusicWildMidiGusEnvelopes.Convert(EnvelopeRates.Span, EnvelopeLevels.Span, sourceModes, sampleRate);
        if (CopyStageTwoToRelease)
        {
            for (var stage = 3; stage < 6; stage++)
            {
                envelope.Targets[stage] = envelope.Targets[2];
                envelope.Rates[stage] = envelope.Rates[2];
            }
        }
        if ((Header.Modes & 64) != 0 && EnvelopeTargetOverrides is not null)
        {
            if (EnvelopeTargetOverrides.Count != 6) throw new InvalidDataException("Envelope overrides require six stages.");
            for (var stage = 0; stage < 6; stage++)
                if (EnvelopeTargetOverrides[stage] is int target) envelope.Targets[stage] = target;
        }
        if ((Header.Modes & 64) != 0 && EnvelopeTimeOverrides is not null)
        {
            if (EnvelopeTimeOverrides.Count != 6) throw new InvalidDataException("Envelope overrides require six stages.");
            for (var stage = 0; stage < 6; stage++)
                if (EnvelopeTimeOverrides[stage] is float time)
                    envelope.Rates[stage] = (int)(4194303.0 / ((float)sampleRate * (time / 1000.0)));
        }
        return envelope;
    }
}

public static class VmMusicWildMidiGusPatchReader
{
    public static IReadOnlyList<VmMusicWildMidiGusPatchSample> Read(ReadOnlySpan<byte> patch)
    {
        if (patch.Length < 239) throw new ArgumentException("The GUS patch header is truncated.", nameof(patch));
        if (!patch[..22].SequenceEqual("GF1PATCH110\0ID#000002\0"u8)
            && !patch[..22].SequenceEqual("GF1PATCH100\0ID#000002\0"u8))
            throw new ArgumentException("The GUS patch signature is unsupported.", nameof(patch));
        if (patch[82] > 1 || patch[151] > 1)
            throw new ArgumentException("GUS patches with multiple instruments or layers are unsupported.", nameof(patch));
        var samples = new VmMusicWildMidiGusPatchSample[patch[198]];
        var offset = 239;
        for (var i = 0; i < samples.Length; i++)
        {
            if (patch.Length - offset < VmMusicWildMidiGusSampleHeader.Size)
                throw new ArgumentException("A GUS sample header is truncated.", nameof(patch));
            var headerBytes = patch.Slice(offset, VmMusicWildMidiGusSampleHeader.Size);
            var header = VmMusicWildMidiGusSampleHeader.Parse(headerBytes);
            offset += VmMusicWildMidiGusSampleHeader.Size;
            if (header.ByteLength > (uint)(patch.Length - offset))
                throw new ArgumentException("A GUS sample payload is truncated.", nameof(patch));
            var pcm = header.Decode(patch.Slice(offset, (int)header.ByteLength));
            samples[i] = new(header, pcm, headerBytes.Slice(37, 6).ToArray(), headerBytes.Slice(43, 6).ToArray());
            offset += (int)header.ByteLength;
        }
        return Array.AsReadOnly(samples);
    }
}
