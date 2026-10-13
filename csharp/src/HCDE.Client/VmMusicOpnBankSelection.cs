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

public sealed record VmMusicOpnBankConfiguration(bool UseCustomBank, string CustomBank);
public enum VmMusicOpnBankKind { CustomFile, ResourceDefault, EmbeddedDefault }

public interface IVmMusicOpnBankDevice
{
    // Bank load methods return zero on success, matching libOPNMIDI.
    int OpenBankData(ReadOnlyMemory<byte> bank);
    int OpenBankFile(string filename);
}

public static class VmMusicOpnBankSelection
{
    private static readonly Lazy<byte[]> EmbeddedBank = new(() =>
    {
        using var stream = typeof(VmMusicOpnBankSelection).Assembly.GetManifestResourceStream("HCDE.Client.Music.xg.wopn")
            ?? throw new InvalidOperationException("Embedded OPN default bank is missing.");
        using var data = new MemoryStream();
        stream.CopyTo(data);
        // The native generated xg_default array includes one trailing zero byte.
        data.WriteByte(0);
        return data.ToArray();
    });

    public static VmMusicOpnBankKind Initialize(VmMusicOpnBankConfiguration configuration,
        VmMusicStartupData resources, IVmMusicOpnBankDevice device)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(configuration.CustomBank);
        if (configuration.UseCustomBank && configuration.CustomBank.Length != 0 && device.OpenBankFile(configuration.CustomBank) == 0)
            return VmMusicOpnBankKind.CustomFile;
        var resourceBank = resources.CreateOpnDefaultBank();
        var kind = resourceBank is { Length: > 0 } ? VmMusicOpnBankKind.ResourceDefault : VmMusicOpnBankKind.EmbeddedDefault;
        var bank = resourceBank is { Length: > 0 } ? resourceBank : EmbeddedBank.Value.ToArray();
        var error = device.OpenBankData(bank);
        if (error != 0) throw new InvalidOperationException($"Loading OPN default bank failed: {error}.");
        return kind;
    }
}
