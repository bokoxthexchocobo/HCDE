using System.Globalization;

namespace HCDE.Gamedata;

public sealed class MapInfoMap
{
    public string MapName { get; set; } = "";
    public string LevelName { get; set; } = "";
    public bool LookupLevelName { get; set; }
    public int LevelNum { get; set; }
    public int Id24LevelNum { get; set; }
    public string NextMap { get; set; } = "";
    public string SecretNextMap { get; set; } = "";
    public int Cluster { get; set; }
    public string Sky1 { get; set; } = "";
    public double SkySpeed1 { get; set; }
    public string Sky2 { get; set; } = "";
    public double SkySpeed2 { get; set; }
    public bool HexenHack { get; set; }
}

public sealed class MapInfoCluster
{
    public int Cluster { get; set; }
    public string Name { get; set; } = "";
    public bool LookupName { get; set; }
    public string EnterText { get; set; } = "";
    public bool LookupEnterText { get; set; }
    public string ExitText { get; set; } = "";
    public bool LookupExitText { get; set; }
    public string Flat { get; set; } = "";
    public bool FinaleIsPic { get; set; }
    public bool Hub { get; set; }
    public string Music { get; set; } = "";
}

public sealed class MapInfoSet
{
    public IReadOnlyList<MapInfoMap> Maps { get; init; } = Array.Empty<MapInfoMap>();
    public IReadOnlyList<MapInfoCluster> Clusters { get; init; } = Array.Empty<MapInfoCluster>();

    public MapInfoMap? FindMap(string mapName) =>
        Maps.FirstOrDefault(map => map.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase));

    public MapInfoCluster? FindCluster(int cluster) =>
        Clusters.FirstOrDefault(entry => entry.Cluster == cluster);
}

/// <summary>
/// ZMAPINFO / MAPINFO subset used to boot a map: lump name, level name, next, secret next, sky, and cluster.
/// Mirrors <c>ParseMapHeader</c>, <c>ParseNextMap</c>, <c>sky1</c>, and <c>ParseCluster</c> in <c>g_mapinfo.cpp</c>.
/// </summary>
public static class MapInfoParser
{
    public const int TicRate = 35;

    public static bool TryParse(string text, out MapInfoSet set, out string? error)
    {
        var parser = new Parser(text);
        return parser.TryParse(out set, out error);
    }

    public static int DefaultLevelNum(string mapName, out int id24LevelNum)
    {
        id24LevelNum = 0;
        if (mapName.StartsWith("MAP", StringComparison.OrdinalIgnoreCase) && mapName.Length <= 5
            && int.TryParse(mapName[3..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var mapNum)
            && mapNum is >= 1 and <= 99)
        {
            id24LevelNum = mapNum;
            return mapNum;
        }

        if (mapName.Length is >= 4 and <= 5
            && (mapName[0] == 'E' || mapName[0] == 'e')
            && char.IsDigit(mapName[1])
            && (mapName[2] == 'M' || mapName[2] == 'm')
            && char.IsDigit(mapName[3])
            && int.TryParse(mapName[3..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var episodeMap))
        {
            id24LevelNum = episodeMap;
            if (mapName.Length == 4)
            {
                var episode = mapName[1] - '1';
                return episode * 10 + episodeMap;
            }
        }

        return 0;
    }

    private sealed class Parser
    {
        private readonly List<string> _tokens = new();
        private int _index;

        public Parser(string text) => Tokenize(text ?? string.Empty);

        public bool TryParse(out MapInfoSet set, out string? error)
        {
            var maps = new Dictionary<string, MapInfoMap>(StringComparer.OrdinalIgnoreCase);
            var clusters = new Dictionary<int, MapInfoCluster>();
            error = null;

            while (!Eof)
            {
                var token = Take();
                if (token.Equals("map", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseMap(maps, clusters, out error))
                    {
                        set = new MapInfoSet();
                        return false;
                    }
                }
                else if (token.Equals("cluster", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseCluster(clusters, out error))
                    {
                        set = new MapInfoSet();
                        return false;
                    }
                }
                else if (Peek("{"))
                {
                    if (!SkipBrace(out error))
                    {
                        set = new MapInfoSet();
                        return false;
                    }
                }
            }

            set = new MapInfoSet
            {
                Maps = maps.Values.ToArray(),
                Clusters = clusters.Values.OrderBy(cluster => cluster.Cluster).ToArray(),
            };
            return true;
        }

        private bool TryParseMap(
            Dictionary<string, MapInfoMap> maps,
            Dictionary<int, MapInfoCluster> clusters,
            out string? error)
        {
            error = null;
            if (Eof)
            {
                error = "map is missing a lump name.";
                return false;
            }

            var id = Take();
            var hexenHack = false;
            string mapName;
            if (int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var mapNumber))
            {
                mapName = $"MAP{mapNumber:00}";
                hexenHack = true;
            }
            else
            {
                mapName = id.ToUpperInvariant();
            }

            if (Eof)
            {
                error = $"map {mapName} is missing a level name.";
                return false;
            }

            var (levelName, lookup) = ReadText();
            var map = new MapInfoMap
            {
                MapName = mapName,
                LevelName = levelName,
                LookupLevelName = lookup,
                HexenHack = hexenHack,
            };
            map.LevelNum = DefaultLevelNum(map.MapName, out var id24);
            map.Id24LevelNum = id24;

            if (Peek("{"))
            {
                Take();
                while (!Eof && !Peek("}"))
                {
                    if (!ReadMapProperty(map, clusters, out error))
                        return false;
                }

                if (!Take("}"))
                {
                    error = $"Unterminated map {map.MapName}.";
                    return false;
                }
            }
            else
            {
                while (!Eof && !Peek("map") && !Peek("cluster"))
                {
                    if (!ReadMapProperty(map, clusters, out error))
                        return false;
                }
            }

            maps[map.MapName] = map;
            return true;
        }

        private bool ReadMapProperty(
            MapInfoMap map,
            Dictionary<int, MapInfoCluster> clusters,
            out string? error)
        {
            error = null;
            var key = Take();
            if (key == "=")
            {
                error = "Expected a property name.";
                return false;
            }

            Optional("=");
            if (key.Equals("next", StringComparison.OrdinalIgnoreCase))
            {
                map.NextMap = ReadNextMap(map.HexenHack);
                return true;
            }

            if (key.Equals("secretnext", StringComparison.OrdinalIgnoreCase)
                || key.Equals("secret", StringComparison.OrdinalIgnoreCase))
            {
                map.SecretNextMap = ReadNextMap(map.HexenHack);
                return true;
            }

            if (key.Equals("sky1", StringComparison.OrdinalIgnoreCase))
            {
                map.Sky1 = ReadLumpName();
                map.SkySpeed1 = ReadOptionalSkySpeed(map.HexenHack);
                return true;
            }

            if (key.Equals("sky2", StringComparison.OrdinalIgnoreCase))
            {
                map.Sky2 = ReadLumpName();
                map.SkySpeed2 = ReadOptionalSkySpeed(map.HexenHack);
                return true;
            }

            if (key.Equals("cluster", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryTakeInt(out var cluster))
                {
                    error = $"map {map.MapName} cluster is not a number.";
                    return false;
                }

                map.Cluster = cluster;
                EnsureCluster(clusters, cluster, map.HexenHack);
                return true;
            }

            if (key.Equals("levelnum", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryTakeInt(out var levelNum))
                {
                    error = $"map {map.MapName} levelnum is not a number.";
                    return false;
                }

                map.LevelNum = levelNum;
                return true;
            }

            SkipValue();
            return true;
        }

        private bool TryParseCluster(Dictionary<int, MapInfoCluster> clusters, out string? error)
        {
            error = null;
            if (!TryTakeInt(out var number))
            {
                error = "cluster is missing a number.";
                return false;
            }

            var cluster = EnsureCluster(clusters, number, hubFromHexen: false);
            if (!Peek("{"))
                return true;

            cluster.Hub = false;
            Take();
            while (!Eof && !Peek("}"))
            {
                var key = Take();
                Optional("=");
                if (key.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    var (name, lookup) = ReadText();
                    cluster.Name = name;
                    cluster.LookupName = lookup;
                }
                else if (key.Equals("entertext", StringComparison.OrdinalIgnoreCase))
                {
                    var (text, lookup) = ReadText();
                    cluster.EnterText = text;
                    cluster.LookupEnterText = lookup;
                }
                else if (key.Equals("exittext", StringComparison.OrdinalIgnoreCase))
                {
                    var (text, lookup) = ReadText();
                    cluster.ExitText = text;
                    cluster.LookupExitText = lookup;
                }
                else if (key.Equals("flat", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.Flat = ReadLumpName();
                    cluster.FinaleIsPic = false;
                }
                else if (key.Equals("pic", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.Flat = ReadLumpName();
                    cluster.FinaleIsPic = true;
                }
                else if (key.Equals("music", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.Music = ReadLumpName();
                }
                else if (key.Equals("hub", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.Hub = true;
                }
                else
                {
                    SkipValue();
                }
            }

            if (!Take("}"))
            {
                error = $"Unterminated cluster {number}.";
                return false;
            }

            return true;
        }

        private static MapInfoCluster EnsureCluster(Dictionary<int, MapInfoCluster> clusters, int number, bool hubFromHexen)
        {
            if (!clusters.TryGetValue(number, out var cluster))
            {
                cluster = new MapInfoCluster { Cluster = number, Hub = hubFromHexen };
                clusters.Add(number, cluster);
            }

            return cluster;
        }

        private string ReadNextMap(bool hexenHack)
        {
            if (Eof)
                return "";

            var token = Take();
            if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
                return hexenHack ? $"&wt@{number:00}" : $"MAP{number:00}";

            return Unquote(token).ToUpperInvariant();
        }

        private string ReadLumpName()
        {
            if (Eof || Peek(",") || Peek("}") || Peek("map") || Peek("cluster"))
                return "";
            return Unquote(Take());
        }

        private double ReadOptionalSkySpeed(bool hexenHack)
        {
            if (Peek(","))
                Take();
            if (Eof || !double.TryParse(PeekToken(), NumberStyles.Float, CultureInfo.InvariantCulture, out var speed))
                return 0;
            if (PeekToken().Any(char.IsLetter))
                return 0;

            Take();

            if (hexenHack)
                speed /= 256.0;

            return speed * (TicRate / 1000.0);
        }

        private (string Text, bool Lookup) ReadText()
        {
            if (Eof)
                return ("", false);

            var token = Take();
            if (token.Equals("lookup", StringComparison.OrdinalIgnoreCase))
            {
                var label = Eof ? "" : Unquote(Take());
                return (label, true);
            }

            var text = Unquote(token);
            if (text.StartsWith('$'))
                return (text[1..], true);
            return (text, false);
        }

        private void SkipValue()
        {
            if (Eof || Peek("}") || Peek("map") || Peek("cluster"))
                return;
            if (Peek("{"))
            {
                SkipBrace(out _);
                return;
            }

            Take();
            while (Peek(","))
            {
                Take();
                if (!Eof && !Peek("}") && !Peek("map") && !Peek("cluster"))
                    Take();
            }
        }

        private bool SkipBrace(out string? error)
        {
            error = null;
            if (!Take("{"))
            {
                error = "Expected '{'.";
                return false;
            }

            var depth = 1;
            while (!Eof && depth > 0)
            {
                var token = Take();
                if (token == "{")
                    depth++;
                else if (token == "}")
                    depth--;
            }

            if (depth != 0)
            {
                error = "Unterminated block.";
                return false;
            }

            return true;
        }

        private bool TryTakeInt(out int value)
        {
            value = 0;
            if (Eof)
                return false;
            if (!int.TryParse(PeekToken(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return false;
            Take();
            return true;
        }

        private bool TryTakeDouble(out double value)
        {
            value = 0;
            if (Eof)
                return false;
            if (!double.TryParse(PeekToken(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return false;
            Take();
            return true;
        }

        private void Optional(string token)
        {
            if (Peek(token))
                Take();
        }

        private bool Take(string token)
        {
            if (!Peek(token))
                return false;
            Take();
            return true;
        }

        private string Take()
        {
            if (Eof)
                return "";
            return _tokens[_index++];
        }

        private bool Peek(string token) =>
            !Eof && _tokens[_index].Equals(token, StringComparison.OrdinalIgnoreCase);

        private string PeekToken() => Eof ? "" : _tokens[_index];

        private bool Eof => _index >= _tokens.Count;

        private void Tokenize(string text)
        {
            var index = 0;
            while (index < text.Length)
            {
                var ch = text[index];
                if (char.IsWhiteSpace(ch))
                {
                    index++;
                    continue;
                }

                if (ch == '/' && index + 1 < text.Length && text[index + 1] == '/')
                {
                    index += 2;
                    while (index < text.Length && text[index] != '\n')
                        index++;
                    continue;
                }

                if (ch == '/' && index + 1 < text.Length && text[index + 1] == '*')
                {
                    index += 2;
                    while (index + 1 < text.Length && !(text[index] == '*' && text[index + 1] == '/'))
                        index++;
                    index = Math.Min(text.Length, index + 2);
                    continue;
                }

                if (ch is '{' or '}' or '=' or ',')
                {
                    _tokens.Add(ch.ToString());
                    index++;
                    continue;
                }

                if (ch == '"')
                {
                    var start = index;
                    index++;
                    while (index < text.Length && text[index] != '"')
                    {
                        if (text[index] == '\\' && index + 1 < text.Length)
                            index += 2;
                        else
                            index++;
                    }

                    if (index < text.Length)
                        index++;
                    _tokens.Add(text[start..index]);
                    continue;
                }

                var tokenStart = index;
                while (index < text.Length && !char.IsWhiteSpace(text[index]) && text[index] is not ('{' or '}' or '=' or ','))
                    index++;
                _tokens.Add(text[tokenStart..index]);
            }
        }

        private static string Unquote(string token)
        {
            if (token.Length >= 2 && token[0] == '"' && token[^1] == '"')
                return token[1..^1];
            return token;
        }
    }
}
