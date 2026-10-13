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

public sealed class VmMusicWildMidiChannelState
{
    public bool IsDrum { get; set; }
    public IVmMusicWildMidiPatch? Patch { get; set; }
    public byte Bank { get; set; }
    public byte Volume { get; set; }
    public byte Expression { get; set; }
    public sbyte Balance { get; set; }
    public sbyte Pan { get; set; }
    public byte NonRegistered { get; set; }
    public ushort RegisteredParameter { get; set; }
    public short PitchRange { get; set; }
    public byte Pressure { get; set; }
    public short Pitch { get; set; }
    public long PitchAdjust { get; set; }
    public byte Hold { get; set; }
}

public static class VmMusicWildMidiChannelControls
{
    public static bool Apply(VmMusicWildMidiChannelState channelState,
        IEnumerable<VmMusicWildMidiNoteReleaseState> voices, int channel, int controller, int value,
        Action adjustChannelVolumes)
    {
        ArgumentNullException.ThrowIfNull(channelState);
        ArgumentNullException.ThrowIfNull(voices);
        ArgumentNullException.ThrowIfNull(adjustChannelVolumes);
        if (channel is < 0 or > 15) throw new ArgumentOutOfRangeException(nameof(channel));
        switch (controller)
        {
            case 64: VmMusicWildMidiNoteRelease.HoldPedal(channelState, voices, channel, value); return true;
            case 120: VmMusicWildMidiNoteRelease.SoundOff(voices, channel); return true;
            case 123: VmMusicWildMidiNoteRelease.NotesOff(voices, channel, channelState.IsDrum); return true;
            default: return Apply(channelState, controller, value, adjustChannelVolumes);
        }
    }

    public static bool Apply(VmMusicWildMidiChannelState channel, int controller, int value, Action adjustChannelVolumes)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(adjustChannelVolumes);
        switch (controller)
        {
            case 0: channel.Bank = unchecked((byte)value); return true;
            case 6:
                if (channel.NonRegistered == 0 && channel.RegisteredParameter == 0)
                    channel.PitchRange = unchecked((short)(value * 100 + channel.PitchRange % 100));
                return true;
            case 38:
                if (channel.NonRegistered == 0 && channel.RegisteredParameter == 0)
                    channel.PitchRange = unchecked((short)(channel.PitchRange / 100 * 100 + value));
                return true;
            case 96:
                if (channel.NonRegistered == 0 && channel.RegisteredParameter == 0 && channel.PitchRange < 0x3fff)
                    channel.PitchRange++;
                return true;
            case 97:
                if (channel.NonRegistered == 0 && channel.RegisteredParameter == 0 && channel.PitchRange > 0)
                    channel.PitchRange--;
                return true;
            case 98:
            case 100:
                channel.RegisteredParameter = unchecked((ushort)((channel.RegisteredParameter & 0x3f80) | value));
                channel.NonRegistered = controller == 98 ? (byte)1 : (byte)0;
                return true;
            case 99:
            case 101:
                channel.RegisteredParameter = unchecked((ushort)((channel.RegisteredParameter & 0x7f) | (value << 7)));
                channel.NonRegistered = controller == 99 ? (byte)1 : (byte)0;
                return true;
            case 7: channel.Volume = unchecked((byte)value); break;
            case 8: channel.Balance = unchecked((sbyte)value); break;
            case 10: channel.Pan = unchecked((sbyte)value); break;
            case 11: channel.Expression = unchecked((byte)value); break;
            case 121:
                channel.Expression = 127;
                channel.Pressure = 127;
                channel.RegisteredParameter = 0xffff;
                channel.PitchRange = 200;
                channel.Pitch = 0;
                channel.PitchAdjust = 0;
                channel.Hold = 0;
                break;
            default: return false;
        }
        adjustChannelVolumes();
        return true;
    }
}
