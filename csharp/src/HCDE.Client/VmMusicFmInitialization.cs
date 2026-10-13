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

public sealed record VmMusicFmSettings
{
    public int Chips { get; init; } = 6;
    public int Emulator { get; init; }
    public int VolumeModel { get; init; }
    public int ChannelAllocation { get; init; } = -1;
    public bool RunAtPcmRate { get; init; }
    public bool FullPan { get; init; } = true;
    public bool AutoArpeggio { get; init; }
    public float Gain { get; init; } = 1;
}

public sealed record VmMusicAdlConfiguration
{
    public VmMusicAdlBankConfiguration Bank { get; init; } = new(false, false, "", 14);
    public VmMusicFmSettings Settings { get; init; } = new();
}

public sealed record VmMusicOpnConfiguration
{
    public VmMusicOpnBankConfiguration Bank { get; init; } = new(false, "");
    public VmMusicFmSettings Settings { get; init; } = new() { Chips = 8 };
}

public interface IVmMusicFmConfigurationDevice
{
    // Adapters surface rejected settings as exceptions.
    void SwitchEmulator(int emulator);
    void SetRunAtPcmRate(bool enabled);
    void SetNumChips(int chips);
    void SetVolumeRangeModel(int model);
    void SetChannelAllocation(int mode);
    void SetSoftPan(bool enabled);
    void SetAutoArpeggio(bool enabled);
}

public interface IVmMusicAdlConfigurationDevice : IVmMusicFmConfigurationDevice, IVmMusicAdlBankDevice
{
    int GetVolumeRangeModel();
}

public interface IVmMusicOpnConfigurationDevice : IVmMusicFmConfigurationDevice, IVmMusicOpnBankDevice { }

public static class VmMusicFmInitialization
{
    public static (VmMusicAdlBankKind Bank, float OutputGain) InitializeAdl(VmMusicAdlConfiguration configuration,
        VmMusicStartupData resources, IVmMusicAdlConfigurationDevice device)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration.Settings, resources, device);
        ArgumentNullException.ThrowIfNull(configuration.Bank);
        ArgumentNullException.ThrowIfNull(configuration.Bank.CustomBank);
        device.SwitchEmulator(configuration.Settings.Emulator);
        device.SetRunAtPcmRate(configuration.Settings.RunAtPcmRate);
        var bank = VmMusicAdlBankSelection.Initialize(configuration.Bank, resources, device);
        ApplySettings(configuration.Settings, device);
        return (bank, AdlOutputGain(device.GetVolumeRangeModel(), configuration.Settings.Gain));
    }

    public static (VmMusicOpnBankKind Bank, float OutputGain) InitializeOpn(VmMusicOpnConfiguration configuration,
        VmMusicStartupData resources, IVmMusicOpnConfigurationDevice device)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Validate(configuration.Settings, resources, device);
        ArgumentNullException.ThrowIfNull(configuration.Bank);
        var bank = VmMusicOpnBankSelection.Initialize(configuration.Bank, resources, device);
        device.SwitchEmulator(configuration.Settings.Emulator);
        device.SetRunAtPcmRate(configuration.Settings.RunAtPcmRate);
        ApplySettings(configuration.Settings, device);
        return (bank, 4 * configuration.Settings.Gain);
    }

    public static float AdlOutputGain(int effectiveVolumeModel, float gain) =>
        (effectiveVolumeModel switch { 1 or 5 or 9 => 2f, 10 or 11 => 2.5f, 2 => 3.8f, _ => 3.5f }) * gain;

    private static void Validate(VmMusicFmSettings settings, VmMusicStartupData resources, IVmMusicFmConfigurationDevice device)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(device);
    }

    private static void ApplySettings(VmMusicFmSettings settings, IVmMusicFmConfigurationDevice device)
    {
        device.SetNumChips(settings.Chips);
        device.SetVolumeRangeModel(settings.VolumeModel);
        device.SetChannelAllocation(settings.ChannelAllocation);
        device.SetSoftPan(settings.FullPan);
        device.SetAutoArpeggio(settings.AutoArpeggio);
    }
}
