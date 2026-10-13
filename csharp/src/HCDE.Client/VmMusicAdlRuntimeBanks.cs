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

public sealed class VmMusicAdlRuntimeBanks
{
    private readonly VmMusicStartupData resources;
    private readonly IVmMusicAdlConfigurationDevice device;
    private readonly VmMusicFmRuntimeSettings settings;
    private readonly bool hasGenMidi;
    public VmMusicAdlBankConfiguration Configuration { get; private set; }
    public VmMusicAdlBankKind? LastLoadedBank { get; private set; }

    // Both bank and gain state describe the same already initialized renderer.
    public VmMusicAdlRuntimeBanks(VmMusicAdlBankConfiguration configuration, VmMusicStartupData resources,
        IVmMusicAdlConfigurationDevice device, VmMusicFmRuntimeSettings settings)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configuration.CustomBank);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(settings);
        Configuration = configuration;
        this.resources = resources;
        this.device = device;
        this.settings = settings;
        hasGenMidi = resources.CreateAdlGenMidiBank() is not null;
    }

    public bool ChangeInteger(string setting, int value)
    {
        ArgumentNullException.ThrowIfNull(setting);
        switch (setting)
        {
            case "libadl.usecustombank":
                var customChanged = Configuration.UseCustomBank != (value != 0);
                Configuration = Configuration with { UseCustomBank = value != 0 };
                if (customChanged) Reload(false);
                break;
            case "libadl.usegenmidi":
                var genChanged = Configuration.UseGenMidi != (value != 0);
                Configuration = Configuration with { UseGenMidi = value != 0 };
                if (genChanged && hasGenMidi) Reload(false);
                break;
            case "libadl.banknum":
                var bankChanged = Configuration.BuiltinBank != value;
                Configuration = Configuration with { BuiltinBank = value };
                if (bankChanged)
                {
                    device.SetBuiltinBank(value);
                    LastLoadedBank = VmMusicAdlBankKind.Builtin;
                    settings.RefreshOutputGain();
                }
                break;
            default: return false;
        }
        return true;
    }

    public bool ChangeString(string setting, string value)
    {
        ArgumentNullException.ThrowIfNull(setting);
        ArgumentNullException.ThrowIfNull(value);
        if (!string.Equals(setting, "libadl.custombank", StringComparison.Ordinal)) return false;
        Configuration = Configuration with { CustomBank = value };
        if (Configuration.UseCustomBank) Reload(true);
        return true;
    }

    private void Reload(bool refreshOnSuccess)
    {
        var bank = VmMusicAdlBankSelection.Initialize(Configuration, resources, device);
        LastLoadedBank = bank;
        // Native integer toggles refresh only on fallback; filename changes always refresh.
        if (refreshOnSuccess || bank == VmMusicAdlBankKind.Builtin) settings.RefreshOutputGain();
    }
}
