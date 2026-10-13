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

public readonly record struct VmMusicWildMidiSampleData(ReadOnlyMemory<short> Data, VmMusicWildMidiSampleBounds Bounds);

public static class VmMusicWildMidiLinearMixer
{
    public static void Mix(IList<VmMusicWildMidiNoteReleaseState> voices, Span<int> stereoOutput,
        bool enhancedResampling = false) => Mix(voices, stereoOutput,
            voice => voice.Sample ?? throw new InvalidOperationException("The voice has no bound sample."), enhancedResampling);

    public static VmMusicWildMidiMixVolume Interpolate(ReadOnlySpan<short> samples,
        VmMusicWildMidiNoteReleaseState voice)
    {
        ArgumentNullException.ThrowIfNull(voice);
        var index = (int)(voice.SamplePosition >> 10);
        if (index >= samples.Length - 1)
            throw new ArgumentException("Linear interpolation requires the current and following sample.", nameof(samples));
        var fraction = (int)(voice.SamplePosition & 1023);
        var interpolated = samples[index] + (samples[index + 1] - samples[index]) * fraction / 1024;
        var premix = unchecked(interpolated * (voice.EnvelopeLevel >> 12)) / 1024;
        return new(unchecked(premix * voice.MixVolume.Left) / 1024,
            unchecked(premix * voice.MixVolume.Right) / 1024);
    }

    public static void Mix(IList<VmMusicWildMidiNoteReleaseState> voices, Span<int> stereoOutput,
        Func<VmMusicWildMidiNoteReleaseState, VmMusicWildMidiSampleData> sampleForVoice,
        bool enhancedResampling = false)
    {
        ArgumentNullException.ThrowIfNull(voices);
        ArgumentNullException.ThrowIfNull(sampleForVoice);
        if (voices.IsReadOnly) throw new ArgumentException("The mixer voice list must be mutable.", nameof(voices));
        if ((stereoOutput.Length & 1) != 0)
            throw new ArgumentException("Stereo output must contain complete frames.", nameof(stereoOutput));
        for (var frame = 0; frame < stereoOutput.Length; frame += 2)
        {
            var left = 0;
            var right = 0;
            var index = 0;
            while (index < voices.Count)
            {
                var voice = voices[index];
                var sample = sampleForVoice(voice);
                var contribution = enhancedResampling
                    ? VmMusicWildMidiGaussianInterpolation.Interpolate(sample, voice)
                    : Interpolate(sample.Data.Span, voice);
                left = unchecked(left + contribution.Left);
                right = unchecked(right + contribution.Right);
                index = VmMusicWildMidiVoiceLifecycle.AdvanceSample(voices, index, sample.Bounds, enhancedResampling);
            }
            stereoOutput[frame] = left;
            stereoOutput[frame + 1] = right;
        }
    }
}
