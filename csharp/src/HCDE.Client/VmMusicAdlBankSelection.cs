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

public sealed record VmMusicAdlBankConfiguration(bool UseCustomBank, bool UseGenMidi, string CustomBank, int BuiltinBank);
public enum VmMusicAdlBankKind { Builtin, GenMidi, CustomFile }

public interface IVmMusicAdlBankDevice
{
    // Bank load methods return zero on success, matching libADLMIDI.
    int OpenBankData(ReadOnlyMemory<byte> bank);
    int OpenBankFile(string filename);
    void SetBuiltinBank(int bank);
}

public static class VmMusicAdlBankSelection
{
    public static VmMusicAdlBankKind Initialize(VmMusicAdlBankConfiguration configuration,
        VmMusicStartupData resources, IVmMusicAdlBankDevice device)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(configuration.CustomBank);
        var genMidi = resources.CreateAdlGenMidiBank();
        if (configuration.UseCustomBank)
        {
            if (configuration.UseGenMidi && genMidi is not null && device.OpenBankData(genMidi) == 0)
                return VmMusicAdlBankKind.GenMidi;
            if (configuration.CustomBank.Length != 0 && device.OpenBankFile(configuration.CustomBank) == 0)
                return VmMusicAdlBankKind.CustomFile;
        }
        device.SetBuiltinBank(configuration.BuiltinBank);
        return VmMusicAdlBankKind.Builtin;
    }
}
