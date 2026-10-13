/*
** music_opnmidi_mididevice.cpp
** Provides access to libOPNMIDI as a generic MIDI device.
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

public sealed class VmMusicOpnRuntimeBanks
{
    private readonly VmMusicStartupData resources;
    private readonly IVmMusicOpnBankDevice device;
    public VmMusicOpnBankConfiguration Configuration { get; private set; }
    public VmMusicOpnBankKind? LastLoadedBank { get; private set; }

    // Tracks an already initialized renderer's bank configuration.
    public VmMusicOpnRuntimeBanks(VmMusicOpnBankConfiguration configuration,
        VmMusicStartupData resources, IVmMusicOpnBankDevice device)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configuration.CustomBank);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(device);
        Configuration = configuration;
        this.resources = resources;
        this.device = device;
    }

    public bool ChangeInteger(string setting, int value)
    {
        ArgumentNullException.ThrowIfNull(setting);
        if (!string.Equals(setting, "libopn.usecustombank", StringComparison.Ordinal)) return false;
        var enabled = value != 0;
        var changed = enabled != Configuration.UseCustomBank;
        Configuration = Configuration with { UseCustomBank = enabled };
        if (changed) Reload();
        return true;
    }

    public bool ChangeString(string setting, string value)
    {
        ArgumentNullException.ThrowIfNull(setting);
        ArgumentNullException.ThrowIfNull(value);
        if (!string.Equals(setting, "libopn.custombank", StringComparison.Ordinal)) return false;
        Configuration = Configuration with { CustomBank = value };
        if (Configuration.UseCustomBank) Reload();
        return true;
    }

    private void Reload() => LastLoadedBank = VmMusicOpnBankSelection.Initialize(Configuration, resources, device);
}
