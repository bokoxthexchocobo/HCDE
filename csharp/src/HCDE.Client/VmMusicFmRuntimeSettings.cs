/*
** music_timidity_mididevice.cpp
** Provides access to TiMidity as a generic MIDI device.
**
**---------------------------------------------------------------------------
** Copyright 2008 Marisa Heit
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

public sealed class VmMusicFmRuntimeSettings
{
    private readonly IVmMusicFmConfigurationDevice device;
    private readonly Func<int>? effectiveVolumeModel;
    private readonly string prefix;
    private float configuredGain;

    public VmMusicFmRuntimeSettings(IVmMusicAdlConfigurationDevice device, float gain)
        : this(device, gain, "libadl.", () => device.GetVolumeRangeModel()) { }

    public VmMusicFmRuntimeSettings(IVmMusicOpnConfigurationDevice device, float gain)
        : this(device, gain, "libopn.", null) { }

    private VmMusicFmRuntimeSettings(IVmMusicFmConfigurationDevice device, float gain, string prefix, Func<int>? model)
    {
        ArgumentNullException.ThrowIfNull(device);
        this.device = device;
        this.prefix = prefix;
        effectiveVolumeModel = model;
        configuredGain = gain;
        OutputGain = ComputeGain(gain);
    }

    public float OutputGain { get; private set; }
    public void RefreshOutputGain() => OutputGain = ComputeGain(configuredGain);
    private float ComputeGain(float gain) => effectiveVolumeModel is null ? 4 * gain :
        VmMusicFmInitialization.AdlOutputGain(effectiveVolumeModel(), gain);

    public bool ChangeInteger(string setting, int value)
    {
        ArgumentNullException.ThrowIfNull(setting);
        if (!setting.StartsWith(prefix, StringComparison.Ordinal)) return false;
        switch (setting[prefix.Length..])
        {
            case "volumemodel":
                device.SetVolumeRangeModel(value);
                if (effectiveVolumeModel is not null) OutputGain = ComputeGain(configuredGain);
                break;
            case "chanalloc": device.SetChannelAllocation(value); break;
            case "emulator": device.SwitchEmulator(value); break;
            case "numchips": device.SetNumChips(value); break;
            case "fullpan": device.SetSoftPan(value != 0); break;
            case "runatpcmrate": device.SetRunAtPcmRate(value != 0); break;
            case "autoarpeggio": device.SetAutoArpeggio(value != 0); break;
            default: return false;
        }
        return true;
    }

    public bool ChangeNumber(string setting, double value)
    {
        ArgumentNullException.ThrowIfNull(setting);
        if (!string.Equals(setting, prefix + "gain", StringComparison.Ordinal)) return false;
        var gain = (float)value;
        var output = effectiveVolumeModel is null ? (float)(4.0f * value) : ComputeGain(gain);
        configuredGain = gain;
        OutputGain = output;
        return true;
    }
}
