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

public static class VmMusicWildMidiPatchLookup
{
    public static void SetDrumTrack(VmMusicWildMidiChannelState channel, int value,
        Func<ushort, IVmMusicWildMidiPatch?> findExact)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(findExact);
        channel.IsDrum = value > 0;
        channel.Patch = channel.IsDrum ? null : Find(0, findExact);
    }

    public static void ResetChannels(IReadOnlyList<VmMusicWildMidiChannelState> channels,
        Func<ushort, IVmMusicWildMidiPatch?> findExact, Action<int> adjustChannelVolumes)
    {
        ArgumentNullException.ThrowIfNull(channels);
        ArgumentNullException.ThrowIfNull(findExact);
        ArgumentNullException.ThrowIfNull(adjustChannelVolumes);
        if (channels.Count != 16 || channels.Any(channel => channel is null))
            throw new ArgumentException("WildMIDI reset requires sixteen channel states.", nameof(channels));
        for (var i = 0; i < 16; i++)
        {
            var channel = channels[i];
            channel.Bank = 0;
            channel.Patch = i == 9 ? null : Find(0, findExact);
            channel.Hold = 0;
            channel.Volume = 100;
            channel.Pressure = 127;
            channel.Expression = 127;
            channel.Balance = 64;
            channel.Pan = 64;
            channel.Pitch = 0;
            channel.PitchRange = 200;
            channel.RegisteredParameter = 0xffff;
            channel.IsDrum = false;
        }
        adjustChannelVolumes(16);
        channels[9].IsDrum = true;
    }

    public static void ChangeProgram(VmMusicWildMidiChannelState channel, int program,
        Func<ushort, IVmMusicWildMidiPatch?> findExact)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(findExact);
        if (channel.IsDrum) channel.Bank = unchecked((byte)program);
        else channel.Patch = Find(unchecked((ushort)((channel.Bank << 8) | program)), findExact);
    }

    public static ushort InstrumentId(int bank, int percussion, int instrument) =>
        unchecked((ushort)((bank << 8) | instrument | (percussion != 0 ? 0x80 : 0)));

    public static TPatch? Find<TPatch>(ushort patchId, Func<ushort, TPatch?> findExact) where TPatch : class
    {
        ArgumentNullException.ThrowIfNull(findExact);
        var patch = findExact(patchId);
        return patch ?? ((patchId & 0xff00) != 0 ? findExact((ushort)(patchId & 0xff)) : null);
    }

    public static TSample? SelectSample<TSample>(IReadOnlyList<TSample> samples, uint frequency,
        Func<TSample, uint> lowFrequency, Func<TSample, uint> highFrequency) where TSample : class
    {
        ArgumentNullException.ThrowIfNull(samples);
        ArgumentNullException.ThrowIfNull(lowFrequency);
        ArgumentNullException.ThrowIfNull(highFrequency);
        if (samples.Count == 0) return null;
        var selected = samples[0];
        if (frequency == 0) return selected;
        foreach (var sample in samples)
        {
            if (frequency <= lowFrequency(sample)) continue;
            if (frequency < highFrequency(sample)) return sample;
            selected = sample;
        }
        return selected;
    }
}
