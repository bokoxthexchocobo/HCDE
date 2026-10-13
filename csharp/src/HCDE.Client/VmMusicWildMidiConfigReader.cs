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

public sealed class VmMusicWildMidiConfigReader
{
    private readonly VmMusicWildMidiInstrumentCatalog catalog;
    private readonly VmMusicWildMidiConfigBank bank = new();
    private readonly VmMusicWildMidiConfigPaths paths;
    private readonly Func<string, string?>? readConfig;
    private readonly bool windowsDrivePaths;
    private readonly int includeDepth;
    internal Action<string>? LoadInclude { get; init; }

    public VmMusicWildMidiConfigReader(VmMusicWildMidiInstrumentCatalog catalog, string configFile, bool windowsDrivePaths,
        Func<string, string?>? readConfig = null)
        : this(catalog, configFile, windowsDrivePaths, readConfig, 0) { }

    private VmMusicWildMidiConfigReader(VmMusicWildMidiInstrumentCatalog catalog, string configFile, bool windowsDrivePaths,
        Func<string, string?>? readConfig, int includeDepth)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        this.catalog = catalog;
        paths = new(configFile, windowsDrivePaths);
        this.readConfig = readConfig;
        this.windowsDrivePaths = windowsDrivePaths;
        this.includeDepth = includeDepth;
    }

    public void Read(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var nul = text.IndexOf('\0');
        if (nul >= 0) text = text[..nul];
        foreach (var line in text.Split(['\r', '\n'])) ApplyLine(line);
    }

    public void ApplyLine(string line)
    {
        var tokens = VmMusicWildMidiConfigTokens.Tokenize(line);
        if (tokens.Count == 0) return;
        if (bank.Apply(line) || paths.Apply(line) || catalog.Reverb.Apply(line)) return;
        if (string.Equals(tokens[0], "guspat_editor_author_cant_read_so_fix_release_time_for_me", StringComparison.OrdinalIgnoreCase))
        {
            catalog.EnableReleaseFix();
            return;
        }
        if (string.Equals(tokens[0], "auto_amp", StringComparison.OrdinalIgnoreCase)
            || string.Equals(tokens[0], "auto_amp_with_amp", StringComparison.OrdinalIgnoreCase))
        {
            catalog.EnableAutomaticAmplification(string.Equals(tokens[0], "auto_amp_with_amp", StringComparison.OrdinalIgnoreCase));
            return;
        }
        if (string.Equals(tokens[0], "source", StringComparison.OrdinalIgnoreCase))
        {
            if (tokens.Count < 2) throw new InvalidDataException("The source directive requires a configuration resource name.");
            if (readConfig is null && LoadInclude is null) throw new NotSupportedException("Source directives require a configuration resource reader.");
            if (includeDepth >= 64) throw new InvalidDataException("WildMIDI configuration includes exceed the managed depth limit of 64.");
            var file = paths.ResolveResource(tokens[1]);
            if (LoadInclude is not null)
            {
                LoadInclude(file);
                return;
            }
            var text = readConfig!(file) ?? throw new FileNotFoundException("The included configuration resource was not found.", file);
            new VmMusicWildMidiConfigReader(catalog, file, windowsDrivePaths, readConfig, includeDepth + 1).Read(text);
            return;
        }
        if (tokens[0][0] is < '0' or > '9')
            throw new NotSupportedException($"WildMIDI configuration directive '{tokens[0]}' has not been converted.");
        if (tokens.Count < 2) throw new InvalidDataException("The patch declaration requires a resource name.");
        byte note = 0;
        byte keep = 0, remove = 0;
        var amplification = 1024;
        var levels = new int?[6];
        var times = new float?[6];
        for (var index = 2; index < tokens.Count; index++)
        {
            var option = tokens[index];
            if (option.StartsWith("env_time", StringComparison.OrdinalIgnoreCase))
            {
                if (option.Length < 11 || option[8] is < '0' or > '5' || option[9] != '=' || option[10] is < '0' or > '9')
                    throw new InvalidDataException("Invalid envelope time option.");
                var time = ParseFloatPrefix(option[10..]);
                if (!float.IsFinite(time) || time < 1.47f || time > 45000f)
                    throw new InvalidDataException("Envelope times must be between 1.47 and 45000 milliseconds.");
                times[option[8] - '0'] = time;
                continue;
            }
            if (option.StartsWith("env_level", StringComparison.OrdinalIgnoreCase))
            {
                if (option.Length < 12 || option[9] is < '0' or > '5' || option[10] != '=' || option[11] is < '0' or > '9')
                    throw new InvalidDataException("Invalid envelope level option.");
                var level = ParseFloatPrefix(option[11..]);
                if (!float.IsFinite(level) || level > 1)
                    throw new InvalidDataException("Envelope levels must be between zero and one.");
                levels[option[9] - '0'] = 16448 * (int)(255.0 * level);
                continue;
            }
            if (option.StartsWith("amp=", StringComparison.OrdinalIgnoreCase))
            {
                var percent = VmMusicWildMidiConfigBank.ParseDecimalPrefix(option[4..]);
                if (percent > (int.MaxValue >> 10))
                    throw new InvalidDataException("The patch amplification exceeds the native signed shift range.");
                amplification = (percent << 10) / 100;
                continue;
            }
            if (string.Equals(option, "keep=loop", StringComparison.OrdinalIgnoreCase)) { keep |= 4; continue; }
            if (string.Equals(option, "keep=env", StringComparison.OrdinalIgnoreCase)) { keep |= 64; continue; }
            if (string.Equals(option, "remove=sustain", StringComparison.OrdinalIgnoreCase)) { remove |= 32; continue; }
            if (string.Equals(option, "remove=clamped", StringComparison.OrdinalIgnoreCase)) { remove |= 128; continue; }
            if (!option.StartsWith("note=", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"WildMIDI patch option '{option}' has not been converted.");
            var parsed = VmMusicWildMidiConfigBank.ParseDecimalPrefix(option[5..]);
            var wrapped = unchecked((byte)parsed);
            if (wrapped > 127) throw new InvalidDataException("The patch note override exceeds the supported MIDI note range after native byte conversion.");
            note = wrapped;
        }
        // Commit selection only after registration succeeds, so a rejected declaration is retryable.
        var id = bank.ResolveInstrument(tokens[0]);
        catalog.RegisterConfigured(id, tokens[1], paths, note, keep, remove, amplification, levels, times);
        bank.SelectInstrument(tokens[0]);
    }

    internal static float ParseFloatPrefix(string text)
    {
        var prefix = System.Text.RegularExpressions.Regex.Match(text, @"^[0-9]+(?:\.[0-9]*)?(?:[eE][+-]?[0-9]+)?").Value;
        return (float)double.Parse(prefix, System.Globalization.CultureInfo.InvariantCulture);
    }
}
