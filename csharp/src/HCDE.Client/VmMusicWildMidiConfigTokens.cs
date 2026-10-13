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

public static class VmMusicWildMidiConfigTokens
{
    public static IReadOnlyList<string> Tokenize(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        var nul = line.IndexOf('\0');
        var chars = (nul >= 0 ? line[..nul] : line).ToCharArray();
        var starts = new List<int>();
        var inQuotes = false;
        var inToken = false;
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '#') break;
            if (chars[i] == '"') inQuotes = !inQuotes;
            else if (!inQuotes && chars[i] is ' ' or '\t')
            {
                if (inToken) { inToken = false; chars[i] = '\0'; }
            }
            else if (!inToken)
            {
                inToken = true;
                starts.Add(i);
            }
        }
        return starts.Select(start =>
        {
            var end = Array.IndexOf(chars, '\0', start);
            return new string(chars, start, (end < 0 ? chars.Length : end) - start);
        }).ToArray();
    }
}
