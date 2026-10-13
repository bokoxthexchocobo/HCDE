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

public static class VmMusicWildMidiSystemExclusive
{
    public static bool Handle(ReadOnlySpan<byte> data, IReadOnlyList<VmMusicWildMidiChannelState> channels,
        Func<ushort, IVmMusicWildMidiPatch?> findExact, Action<int> adjustChannelVolumes)
    {
        ArgumentNullException.ThrowIfNull(channels);
        ArgumentNullException.ThrowIfNull(findExact);
        ArgumentNullException.ThrowIfNull(adjustChannelVolumes);
        if (channels.Count != 16 || channels.Any(channel => channel is null))
            throw new ArgumentException("WildMIDI SysEx requires sixteen channel states.", nameof(channels));
        if (data.Length >= 11 && data[^1] == 0xf7 && data[0] == 0xf0 && data[1] == 0x41
            && data[2] == 0x10 && data[3] == 0x42 && data[4] == 0x12 && data[5] == 0x40)
        {
            var checksum = 0;
            foreach (var value in data[5..^2]) checksum = (checksum + value) & 127;
            if (data[^2] != 128 - checksum) return false;
            if ((data[6] & 0xf0) == 0x10 && data[7] == 0x15)
            {
                var channel = data[6] & 15;
                channel = channel == 0 ? 9 : channel <= 9 ? channel - 1 : channel;
                VmMusicWildMidiPatchLookup.SetDrumTrack(channels[channel], data[8], findExact);
                return true;
            }
            if (data[6] != 0 || data[7] != 0x7f || data[8] != 0) return false;
        }
        else if (!data.SequenceEqual(new byte[] { 0xf0, 0x7e, 0x7f, 9, 1, 0xf7 })
            && !data.SequenceEqual(new byte[] { 0xf0, 0x43, 0x10, 0x4c, 0, 0, 0x7e, 0, 0xf7 })) return false;
        VmMusicWildMidiPatchLookup.ResetChannels(channels, findExact, adjustChannelVolumes);
        return true;
    }
}
