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

using System.Globalization;

namespace HCDE.Client;

public static class VmMusicFmBankArguments
{
    public static VmMusicAdlConfiguration ResolveAdl(VmMusicAdlConfiguration configuration, string? argument,
        Func<string, VmMusicBankFormat, string?>? resolve = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configuration.Bank);
        ArgumentNullException.ThrowIfNull(configuration.Bank.CustomBank);
        var bank = !string.IsNullOrEmpty(argument) ? argument :
            configuration.Bank.UseCustomBank ? configuration.Bank.CustomBank : null;
        if (string.IsNullOrEmpty(bank)) return configuration;
        if (bank[0] is >= '0' and <= '9')
        {
            var count = 1;
            while (count < bank.Length && bank[count] is >= '0' and <= '9') count++;
            // Native strtoll saturates at INT64_MAX, then the bank index is narrowed to int.
            var number = long.TryParse(bank.AsSpan(0, count), NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
                ? parsed : long.MaxValue;
            return configuration with { Bank = configuration.Bank with { BuiltinBank = unchecked((int)number), UseCustomBank = false } };
        }
        var path = resolve is null ? bank : resolve(bank, VmMusicBankFormat.Wopl);
        return configuration with { Bank = configuration.Bank with { CustomBank = path ?? "", UseCustomBank = path is not null } };
    }

    public static VmMusicOpnConfiguration ResolveOpn(VmMusicOpnConfiguration configuration, string? argument,
        Func<string, VmMusicBankFormat, string?>? resolve = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configuration.Bank);
        ArgumentNullException.ThrowIfNull(configuration.Bank.CustomBank);
        var bank = !string.IsNullOrEmpty(argument) ? argument :
            configuration.Bank.UseCustomBank ? configuration.Bank.CustomBank : null;
        if (string.IsNullOrEmpty(bank)) return configuration;
        var path = resolve is null ? bank : resolve(bank, VmMusicBankFormat.Wopn);
        return configuration with { Bank = configuration.Bank with { CustomBank = path ?? "", UseCustomBank = path is not null } };
    }
}
