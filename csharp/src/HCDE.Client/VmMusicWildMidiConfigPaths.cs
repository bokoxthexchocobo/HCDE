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

public sealed class VmMusicWildMidiConfigPaths
{
    private readonly bool windowsDrivePaths;
    public string? Directory { get; private set; }

    public VmMusicWildMidiConfigPaths(string configFile, bool windowsDrivePaths)
    {
        ArgumentNullException.ThrowIfNull(configFile);
        this.windowsDrivePaths = windowsDrivePaths;
        var separator = windowsDrivePaths ? configFile.LastIndexOfAny(['/', '\\']) : configFile.LastIndexOf('/');
        Directory = separator < 0 ? null : configFile[..(separator + 1)];
    }

    public bool Apply(string line)
    {
        var tokens = VmMusicWildMidiConfigTokens.Tokenize(line);
        if (tokens.Count == 0 || !string.Equals(tokens[0], "dir", StringComparison.OrdinalIgnoreCase)) return false;
        if (tokens.Count < 2 || tokens[1].Length == 0) throw new InvalidDataException("The dir directive requires a directory name.");
        var directory = tokens[1];
        Directory = directory[^1] is '/' or '\\' ? directory : directory + "/";
        return true;
    }

    public string ResolvePatch(string name)
    {
        var resolved = ResolveResource(name);
        return resolved.EndsWith(".pat", StringComparison.OrdinalIgnoreCase) ? resolved : resolved + ".pat";
    }

    public string ResolveResource(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        var absolute = name[0] is '/' or '\\' || (windowsDrivePaths && name.Length > 1 && name[1] == ':');
        return !absolute && Directory is not null ? Directory + name : name;
    }
}
