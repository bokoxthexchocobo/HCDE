using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using HCDE.MapLoader;

namespace HCDE.Client;

public sealed record VmIwadInfoEntry(VmIwadDefinition Definition,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Properties);
public sealed record VmIwadInfoCatalog(IReadOnlyList<VmIwadInfoEntry> Entries,
    IReadOnlyList<string> Names, IReadOnlyList<string> Order);

/// <summary>Reads the native IWADINFO catalog, retaining startup metadata for later consumers.</summary>
public static class VmIwadInfoParser
{
    public static VmIwadInfoEntry ReadEmbeddedArchive(VmScriptArchive archive, Action<string>? warning = null)
    {
        // Native CheckIWADInfo uses reserved folders/default blocks, with no active game filters.
        var text = VmArchiveScriptLoader.ReadGlobalText(archive, "IWADINFO", gameTypeFilters: Array.Empty<string>())
            ?? throw new InvalidDataException("Unable to find global IWADINFO in archive");
        return ParseSingle(text, warning);
    }

    public static VmIwadInfoEntry ReadEmbeddedWad(ReadOnlySpan<byte> wad, Action<string>? warning = null)
    {
        if (!WadArchiveReader.TryReadDirectory(wad, out var entries, out var error))
            throw new InvalidDataException(error);
        var global = VmWadScriptVisibility.GetGlobalEntries(entries);
        for (var index = entries.Length - 1; index >= 0; index--)
        {
            if (!global[index] || !entries[index].Name.Equals("IWADINFO", StringComparison.OrdinalIgnoreCase)) continue;
            if (!WadArchiveReader.TryReadLumpData(wad, entries[index], out var data, out error))
                throw new InvalidDataException(error);
            return ParseSingle(new UTF8Encoding(false, true).GetString(data), warning);
        }
        throw new InvalidDataException("Unable to find global IWADINFO in WAD");
    }

    public static VmIwadInfoEntry ParseSingle(string text, Action<string>? warning = null)
    {
        var catalog = Parse(text, singleRecord: true, warning);
        return catalog.Entries.Count == 1 ? catalog.Entries[0]
            : throw new InvalidDataException("IWADINFO contains no IWAD record");
    }

    public static VmIwadInfoCatalog Parse(string text, bool singleRecord = false, Action<string>? warning = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        var scanner = new Scanner(text);
        var entries = new List<VmIwadInfoEntry>();
        var names = new List<string>();
        var order = new List<string>();
        while (!scanner.End)
        {
            var block = scanner.Value().ToLowerInvariant();
            if (singleRecord && block == "iwad" && entries.Count > 0)
            {
                const string message = "IWADINFO: Multiple IWAD records ignored";
                if (warning is null) Console.Error.WriteLine(message); else warning(message);
                break;
            }
            if (singleRecord && block != "iwad") throw scanner.Error("Unknown keyword " + block);
            scanner.Require("{");
            if (block is "names" or "order")
            {
                var list = block == "names" ? names : order;
                if (block == "names") list.Add("");
                while (!scanner.Take("}")) list.Add(scanner.Value());
                continue;
            }
            if (block != "iwad") throw scanner.Error("Unknown block " + block);
            var properties = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            while (!scanner.Take("}"))
            {
                var key = scanner.Value().ToLowerInvariant();
                if (key is "nokeyboardcheats" or "skipbexstringsiflanguage")
                {
                    properties[key] = ["true"];
                    continue;
                }
                var listProperty = key is "compatibility" or "mustcontain" or "deletelumps" or "load";
                if (!listProperty && key is not ("name" or "autoname" or "iwadname" or "supportwad" or "config"
                    or "game" or "mapinfo" or "bannercolors" or "ignoretitlepatches" or "required"
                    or "startuptype" or "startupsong" or "loadlights" or "loadbrightmaps" or "loadwidescreen"
                    or "discordappid" or "steamappid")) throw scanner.Error("Unknown keyword " + key);
                scanner.Require("=");
                var values = new List<string> { scanner.Value() };
                if (listProperty)
                    while (scanner.Take(",")) values.Add(scanner.Value());
                else if (key == "bannercolors") { scanner.Require(","); values.Add(scanner.Value()); }
                else if (key == "iwadname" && scanner.Take(",")) values.Add(scanner.Value());
                if (key is "ignoretitlepatches" or "loadlights" or "loadbrightmaps" or "loadwidescreen")
                    values[0] = ParseNumber(values[0], scanner);
                if (key == "iwadname" && values.Count == 2) values[1] = ParseNumber(values[1], scanner);
                if (key == "compatibility")
                    foreach (var value in values)
                        if (value.ToLowerInvariant() is not ("poly1" or "poly2" or "shareware" or "teaser2"
                            or "extended" or "shorttex" or "stairs" or "nosectionmerge"))
                            throw scanner.Error("Unknown compatibility " + value);
                if (listProperty && properties.TryGetValue(key, out var prior)) prior.AddRange(values);
                else properties[key] = values;
            }
            string Scalar(string key, string fallback = "") => properties.TryGetValue(key, out var values) ? values[0] : fallback;
            var game = Scalar("game", "Doom").ToLowerInvariant() switch
            {
                "doom" => VmStartupGameType.Doom, "heretic" => VmStartupGameType.Heretic,
                "hexen" => VmStartupGameType.Hexen, "strife" => VmStartupGameType.Strife,
                "chex" => VmStartupGameType.Chex, _ => throw scanner.Error("Unknown game"),
            };
            if (string.IsNullOrWhiteSpace(Scalar("name"))) throw scanner.Error("IWAD name is required");
            if (Scalar("mapinfo").Length == 0) properties["mapinfo"] = ["mapinfo/mindefaults.txt"];
            var definition = new VmIwadDefinition(Scalar("name"), game, Scalar("autoname"),
                properties.TryGetValue("mustcontain", out var required) ? required : Array.Empty<string>());
            var snapshot = properties.ToDictionary(pair => pair.Key,
                pair => (IReadOnlyList<string>)pair.Value.AsReadOnly(), StringComparer.OrdinalIgnoreCase);
            entries.Add(new(definition, new ReadOnlyDictionary<string, IReadOnlyList<string>>(snapshot)));
        }
        return new(entries.AsReadOnly(), names.AsReadOnly(), order.AsReadOnly());
    }

    private static string ParseNumber(string value, Scanner scanner)
    {
        if (!VmIwadInfoNumber.TryParse(value, out var number))
            throw scanner.Error("Invalid integer " + value);
        return number.ToString(CultureInfo.InvariantCulture);
    }

    private sealed class Scanner(string text)
    {
        private int _offset;
        private (string Text, bool Punctuation)? _pending;
        public bool End { get { Skip(); return _pending is null && _offset == text.Length; } }
        public InvalidDataException Error(string message) => new($"IWADINFO at offset {_offset}: {message}");
        public bool Take(string token)
        {
            if (End) return false;
            _pending ??= Read();
            if (_pending.Value != (token, true)) return false;
            _pending = null;
            return true;
        }
        public void Require(string token) { if (!Take(token)) throw Error("Expected " + token); }
        public string Value()
        {
            var token = _pending ?? Read();
            _pending = null;
            if (token.Punctuation) throw Error("Expected value");
            return token.Text;
        }
        private (string Text, bool Punctuation) Read()
        {
            Skip();
            if (_offset == text.Length) throw Error("Unexpected end of file");
            var ch = text[_offset++];
            if (ch is '{' or '}' or '=' or ',') return (ch.ToString(), true);
            if (ch == '"')
            {
                var value = new StringBuilder();
                while (_offset < text.Length)
                {
                    ch = text[_offset++];
                    if (ch == '"') return (value.ToString(), false);
                    if (ch == '\\' && _offset < text.Length && text[_offset] == '"') ch = text[_offset++];
                    else if (ch == '\r' && _offset < text.Length && text[_offset] == '\n') ch = text[_offset++];
                    value.Append(ch);
                }
                throw Error("Unterminated string");
            }
            var start = _offset - 1;
            while (_offset < text.Length && !char.IsWhiteSpace(text[_offset])
                && text[_offset] is not ('{' or '}' or '=' or ',' or '"')
                && !(text[_offset] == '/' && _offset + 1 < text.Length && text[_offset + 1] is '/' or '*')) _offset++;
            return (text[start.._offset], false);
        }
        private void Skip()
        {
            if (_pending is not null) return;
            while (_offset < text.Length)
            {
                if (char.IsWhiteSpace(text[_offset]) || _offset == 0 && text[_offset] == '\uFEFF') { _offset++; continue; }
                if (_offset + 1 >= text.Length || text[_offset] != '/') return;
                if (text[_offset + 1] == '/')
                {
                    _offset += 2;
                    while (_offset < text.Length && text[_offset] != '\n') _offset++;
                }
                else if (text[_offset + 1] == '*')
                {
                    var end = text.IndexOf("*/", _offset + 2, StringComparison.Ordinal);
                    if (end < 0) throw Error("Unterminated comment");
                    _offset = end + 2;
                }
                else return;
            }
        }
    }
}
