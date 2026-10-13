/*
 ** midisource.cpp
 ** Implements base class for the different MIDI formats
 **
 **---------------------------------------------------------------------------
 ** Copyright 2008-2016 Marisa Heit
 ** Copyright 2017-2018 Christoph Oelckers
 ** All rights reserved.
 **
 ** Redistribution and use in source and binary forms, with or without
 ** modification, are permitted provided that the following conditions
 ** are met:
 **
 ** 1. Redistributions of source code must retain the above copyright
 **    notice, this list of conditions and the following disclaimer.
 ** 2. Redistributions in binary form must reproduce the above copyright
 **    notice, this list of conditions and the following disclaimer in the
 **    documentation and/or other materials provided with the distribution.
 ** 3. The name of the author may not be used to endorse or promote products
 **    derived from this software without specific prior written permission.
 **
 ** THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 ** IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 ** OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 ** IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 ** INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 ** NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 ** DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 ** THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 ** (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 ** THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 **---------------------------------------------------------------------------
 **
 */

namespace HCDE.Client;

public sealed class VmMusicMidiChannelVolume
{
    private readonly byte[] channelVolumes = new byte[16];
    public ushort MasterVolume { get; set; } = ushort.MaxValue;

    public VmMusicMidiChannelVolume() => Initialize();

    public void Initialize() => Array.Fill(channelVolumes, (byte)100);

    public byte GetChannelVolume(int channel)
    {
        ValidateChannel(channel);
        return channelVolumes[channel];
    }

    public byte ControllerChange(int channel, byte volume, bool exporting = false)
    {
        ValidateChannel(channel);
        if (volume > 127) throw new ArgumentOutOfRangeException(nameof(volume));
        channelVolumes[channel] = volume;
        return exporting ? volume : Scale(volume, MasterVolume);
    }

    public VmMusicSmfEvent Apply(VmMusicSmfEvent item, bool exporting = false)
    {
        if ((item.Event >> 24) != 0 || (item.Event & 0xf0) != 0xb0 || ((item.Event >> 8) & 255) != 7)
            return item;
        var volume = (byte)(item.Event >> 16);
        var scaled = ControllerChange((int)(item.Event & 15), volume, exporting);
        return item with { Event = (item.Event & 0xffff) | ((uint)scaled << 16) };
    }

    public IReadOnlyList<VmMusicSmfEvent> CreateVolumeChanges()
        => CreateVolumeChanges(MasterVolume);

    public IReadOnlyList<VmMusicSmfEvent> CreateVolumeChanges(ushort masterVolume)
    {
        var events = new VmMusicSmfEvent[16];
        for (var channel = 0; channel < events.Length; channel++)
            events[channel] = new(0, (uint)(0xb0 | channel | (7 << 8) | (Scale(channelVolumes[channel], masterVolume) << 16)));
        return Array.AsReadOnly(events);
    }

    private static byte Scale(byte volume, ushort master) => (byte)(((volume + 1) * master) >> 16);

    private static void ValidateChannel(int channel)
    {
        if ((uint)channel >= 16) throw new ArgumentOutOfRangeException(nameof(channel));
    }
}
