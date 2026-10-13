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

public sealed class VmMusicWildMidiConfigBank
{
    public ushort PatchId { get; private set; }

    public bool Apply(string line)
    {
        var tokens = VmMusicWildMidiConfigTokens.Tokenize(line);
        if (tokens.Count == 0) return false;
        var drum = string.Equals(tokens[0], "drumset", StringComparison.OrdinalIgnoreCase);
        if (!drum && !string.Equals(tokens[0], "bank", StringComparison.OrdinalIgnoreCase)) return false;
        if (tokens.Count < 2) throw new InvalidDataException("The bank directive requires a decimal bank number.");
        var bank = ParseDecimalPrefix(tokens[1]);
        PatchId = (ushort)(((bank & 255) << 8) | (drum ? 128 : 0));
        return true;
    }

    public ushort SelectInstrument(string number)
    {
        PatchId = ResolveInstrument(number);
        return PatchId;
    }

    public ushort ResolveInstrument(string number) => (ushort)((PatchId & 0xff80) | (ParseDecimalPrefix(number) & 127));

    internal static int ParseDecimalPrefix(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0 || text[0] is < '0' or > '9')
            throw new InvalidDataException("The configuration number must begin with an ASCII decimal digit.");
        var value = 0;
        foreach (var character in text)
        {
            if (character is < '0' or > '9') break;
            var digit = character - '0';
            // Native atoi overflow is undefined; reject it before changing parser state.
            if (value > (int.MaxValue - digit) / 10)
                throw new InvalidDataException("The configuration number exceeds the native signed integer range.");
            value = value * 10 + digit;
        }
        return value;
    }
}
