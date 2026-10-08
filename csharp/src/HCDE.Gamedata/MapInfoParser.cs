using System.Globalization;

namespace HCDE.Gamedata;

public sealed class MapInfoMap
{
    internal MapInfoMap Copy()
    {
        var copy = (MapInfoMap)MemberwiseClone();
        copy.ExitTexts = new Dictionary<string, MapInfoExitText>(ExitTexts, StringComparer.OrdinalIgnoreCase);
        copy.MapIntermissionMusic = new Dictionary<string, MapInfoMusic>(MapIntermissionMusic, StringComparer.OrdinalIgnoreCase);
        return copy;
    }
    public Dictionary<string, MapInfoExitText> ExitTexts { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, MapInfoMusic> MapIntermissionMusic { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
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
    public double? AirControl { get; set; }
    public double? Gravity { get; set; }
    public bool ActivateOwnDeathSpecials { get; set; }
    public bool NoClusterText { get; set; }
    public string Music { get; set; } = "";
    public int MusicOrder { get; set; }
    public string IntermissionMusic { get; set; } = "";
    public int IntermissionMusicOrder { get; set; }
    public int CdTrack { get; set; }
    public uint CdId { get; set; }
}

public sealed class MapInfoCluster
{
    internal MapInfoCluster Copy() => (MapInfoCluster)MemberwiseClone();
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
    public bool AllowIntermission { get; set; }
    public bool EnterTextIsLump { get; set; }
    public bool ExitTextIsLump { get; set; }
    public string Music { get; set; } = "";
    public int MusicOrder { get; set; }
    public int CdTrack { get; set; }
    public uint CdId { get; set; }
}

public sealed class MapInfoSet
{
    public MapInfoMusic IntermissionMusic { get; init; } = new("");
    public string FinaleMusic { get; init; } = "";
    public int FinaleMusicOrder { get; init; }
    public string FinaleFlat { get; init; } = "";
    internal MapInfoMap? Defaults { get; init; }
    public int? DefaultDropStyle { get; init; }
    public IReadOnlyList<MapInfoDamageType> DamageTypes { get; init; } = Array.Empty<MapInfoDamageType>();
    public IReadOnlyList<MapInfoMap> Maps { get; init; } = Array.Empty<MapInfoMap>();
    public IReadOnlyList<MapInfoCluster> Clusters { get; init; } = Array.Empty<MapInfoCluster>();

    public MapInfoMap? FindMap(string mapName) =>
        Maps.FirstOrDefault(map => map.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase));

    public MapInfoCluster? FindCluster(int cluster) =>
        Clusters.FirstOrDefault(entry => entry.Cluster == cluster);
}

public sealed record MapInfoDamageType(string Name, double Factor = 1, bool ReplaceFactor = false,
    bool NoArmor = false, string Obituary = "");

public sealed record MapInfoExitText(string Text, bool Lookup, bool HasText = true,
    string? Music = null, int MusicOrder = 0, string? Background = null, bool BackgroundIsPicture = false);

public sealed record MapInfoMusic(string Resource, int Order = 0);

/// <summary>
/// ZMAPINFO / MAPINFO subset used to boot a map: lump name, level name, next, secret next, sky, and cluster.
/// Mirrors <c>ParseMapHeader</c>, <c>ParseNextMap</c>, <c>sky1</c>, and <c>ParseCluster</c> in <c>g_mapinfo.cpp</c>.
/// </summary>
public static class MapInfoParser
{
    public const int TicRate = 35;

    public static bool TryParse(string text, out MapInfoSet set, out string? error, MapInfoSet? previous = null, IReadOnlyDictionary<int, string>? mapMusic = null)
    {
        var parser = new Parser(text);
        return parser.TryParse(out set, out error, previous, mapMusic);
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

        public bool TryParse(out MapInfoSet set, out string? error, MapInfoSet? previous, IReadOnlyDictionary<int, string>? mapMusic)
        {
            var maps = new Dictionary<string, MapInfoMap>(StringComparer.OrdinalIgnoreCase);
            var clusters = new Dictionary<int, MapInfoCluster>();
            var damageTypes = new Dictionary<string, MapInfoDamageType>(StringComparer.OrdinalIgnoreCase);
            foreach (var map in previous?.Maps ?? Array.Empty<MapInfoMap>()) maps[map.MapName] = map.Copy();
            foreach (var cluster in previous?.Clusters ?? Array.Empty<MapInfoCluster>()) clusters[cluster.Cluster] = cluster.Copy();
            foreach (var type in previous?.DamageTypes ?? Array.Empty<MapInfoDamageType>()) damageTypes[type.Name] = type;
            int? defaultDropStyle = previous?.DefaultDropStyle;
            var finaleMusic = previous?.FinaleMusic ?? "";
            var finaleOrder = previous?.FinaleMusicOrder ?? 0;
            var finaleFlat = previous?.FinaleFlat ?? "";
            var intermissionMusic = previous?.IntermissionMusic ?? new MapInfoMusic("");
            MapInfoMap? defaults = previous?.Defaults?.Copy();
            error = null;

            while (!Eof)
            {
                var token = Take();
                if (token.Equals("map", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseMap(maps, clusters, defaults, mapMusic, out error))
                    {
                        set = new MapInfoSet();
                        return false;
                    }
                }
                else if (token.Equals("defaultmap", StringComparison.OrdinalIgnoreCase)
                    || token.Equals("adddefaultmap", StringComparison.OrdinalIgnoreCase))
                {
                    if (token.Equals("defaultmap", StringComparison.OrdinalIgnoreCase)) defaults = new MapInfoMap();
                    defaults ??= new MapInfoMap();
                    var braced = Take("{");
                    if (!braced && Eof) { set = new(); error = "DefaultMap is missing properties."; return false; }
                    while (!Eof && (braced ? !Peek("}") : !AtDefinition(clusterIsProperty: true)))
                    {
                        if (!ReadMapProperty(defaults, clusters, out error)) { set = new(); return false; }
                    }
                    if (braced && !Take("}")) { set = new(); error = "Unterminated DefaultMap."; return false; }
                }
                else if (token.Equals("GameInfo", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseGameInfo(ref defaultDropStyle, ref finaleMusic, ref finaleOrder, ref finaleFlat, ref intermissionMusic, out error))
                    { set = new MapInfoSet(); return false; }
                }
                else if (token.Equals("DamageType", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryParseDamageType(damageTypes, out error))
                    { set = new MapInfoSet(); return false; }
                }
                else if (token.Equals("cluster", StringComparison.OrdinalIgnoreCase)
                    || token.Equals("clusterdef", StringComparison.OrdinalIgnoreCase))
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
                Defaults = defaults?.Copy(),
                IntermissionMusic = intermissionMusic,
                FinaleMusic = finaleMusic, FinaleMusicOrder = finaleOrder, FinaleFlat = finaleFlat,
                DefaultDropStyle = defaultDropStyle,
                Maps = maps.Values.ToArray(),
                DamageTypes = damageTypes.Values.ToArray(),
                Clusters = clusters.Values.OrderBy(cluster => cluster.Cluster).ToArray(),
            };
            return true;
        }

        private bool TryParseGameInfo(ref int? defaultDropStyle, ref string finaleMusic,
            ref int finaleOrder, ref string finaleFlat, ref MapInfoMusic intermissionMusic, out string? error)
        {
            error = null;
            if (!Take("{")) { error = "GameInfo is missing a block."; return false; }
            while (!Eof && !Peek("}"))
            {
                var property = Take();
                if (property.Equals("DefaultDropStyle", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Take("=") || !TryTakeInt(out var style))
                    { error = "Invalid GameInfo DefaultDropStyle."; return false; }
                    defaultDropStyle = style;
                }
                else if (property.Equals("intermissionmusic", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Take("=") || Eof || !PeekToken().StartsWith('"'))
                    { error = "Invalid GameInfo intermission music string."; return false; }
                    if (!TryReadMusic(Unquote(Take()), out var music, out var order, out error, allowSeparateOrder: false)) return false;
                    intermissionMusic = new MapInfoMusic(music, order);
                }
                else if (property.Equals("finalemusic", StringComparison.OrdinalIgnoreCase)
                    || property.Equals("finaleflat", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Take("=") || Eof || Peek("}") || Peek(","))
                    { error = "Invalid GameInfo finale resource."; return false; }
                    var resource = Unquote(Take());
                    if (property.Equals("finalemusic", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!TryReadMusic(resource, out finaleMusic, out finaleOrder, out error, allowSeparateOrder: false)) return false;
                    }
                    else finaleFlat = resource;
                }
                else if (Peek("{"))
                {
                    if (!SkipBrace(out error)) return false;
                }
                else
                {
                    if (!Take("=") || Eof || Peek("}"))
                    { error = $"Invalid GameInfo property {property}."; return false; }
                    SkipValue();
                }
            }
            if (!Take("}")) { error = "Unterminated GameInfo."; return false; }
            return true;
        }

        private bool TryParseDamageType(Dictionary<string, MapInfoDamageType> definitions, out string? error)
        {
            error = null;
            if (Eof || Peek("{")) { error = "DamageType is missing a name."; return false; }
            var nameToken = Take();
            if (nameToken.StartsWith('"') && !nameToken.EndsWith('"'))
            { error = "Unterminated DamageType name."; return false; }
            var name = Unquote(nameToken);
            if (string.IsNullOrEmpty(name)) { error = "DamageType is missing a name."; return false; }
            if (!Take("{")) { error = $"DamageType {name} is missing a block."; return false; }
            double factor = 1;
            bool replace = false, noArmor = false;
            var obituary = "";
            while (!Eof && !Peek("}"))
            {
                var property = Take();
                if (property.Equals("Factor", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Take("=") || Eof || !double.TryParse(Take(), NumberStyles.Float, CultureInfo.InvariantCulture, out factor) || !double.IsFinite(factor))
                    { error = $"Invalid DamageType {name} factor."; return false; }
                    if (factor == 0) replace = true;
                }
                else if (property.Equals("ReplaceFactor", StringComparison.OrdinalIgnoreCase)) replace = true;
                else if (property.Equals("NoArmor", StringComparison.OrdinalIgnoreCase)) noArmor = true;
                else if (property.Equals("Obituary", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Take("=") || Eof || Peek("}")) { error = $"Invalid DamageType {name} obituary."; return false; }
                    var value = Take();
                    if (value.StartsWith('"') && !value.EndsWith('"'))
                    { error = $"Unterminated DamageType {name} obituary."; return false; }
                    obituary = Unquote(value);
                }
                else { error = $"Unexpected DamageType {name} property {property}."; return false; }
            }
            if (!Take("}")) { error = $"Unterminated DamageType {name}."; return false; }
            definitions[name] = new MapInfoDamageType(name, factor, replace, noArmor, obituary);
            return true;
        }

        private bool TryParseMap(
            Dictionary<string, MapInfoMap> maps,
            Dictionary<int, MapInfoCluster> clusters,
            MapInfoMap? defaults,
            IReadOnlyDictionary<int, string>? mapMusic,
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
            var map = defaults?.Copy() ?? new MapInfoMap { ActivateOwnDeathSpecials = hexenHack };
            map.MapName = mapName;
            map.LevelName = levelName;
            map.LookupLevelName = lookup;
            map.HexenHack = hexenHack;
            if (hexenHack) map.ActivateOwnDeathSpecials = true;
            var defaultLevelNum = DefaultLevelNum(map.MapName, out var id24);
            if (map.LevelNum == 0) map.LevelNum = defaultLevelNum;
            map.Id24LevelNum = id24;
            if (mapMusic is not null && mapMusic.TryGetValue(defaultLevelNum, out var song)) map.Music = song;

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
                while (!Eof && !AtDefinition(clusterIsProperty: true))
                {
                    if (!ReadMapProperty(map, clusters, out error))
                        return false;
                }
            }

            if (map.Sky2.Length == 0 || map.Sky2.Equals("-NOFLAT-", StringComparison.OrdinalIgnoreCase))
                map.Sky2 = map.Sky1;
            foreach (var existing in maps.Values)
                if (existing.LevelNum == map.LevelNum) existing.LevelNum = 0;
            map.Id24LevelNum = 0;
            maps[map.MapName] = map;
            return true;
        }

        private bool AtDefinition(bool clusterIsProperty = false) => Peek("map") || (!clusterIsProperty && Peek("cluster")) || Peek("clusterdef") || Peek("DamageType")
            || Peek("defaultmap") || Peek("adddefaultmap") || Peek("GameInfo");

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
            if (key.Equals("cdtrack", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryTakeInt(out var track)) { error = "Invalid map CD track."; return false; }
                map.CdTrack = track;
                return true;
            }
            if (key.Equals("cdid", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryReadCdId(out var id, out error)) return false;
                map.CdId = id;
                return true;
            }
            if (key.Equals("mapintermusic", StringComparison.OrdinalIgnoreCase))
            {
                if (Eof || Peek("}") || Peek(",")) { error = "MapInterMusic is missing a destination."; return false; }
                var destination = Unquote(Take());
                if (!Take(",")) { error = "MapInterMusic is missing a comma."; return false; }
                if (Eof || Peek("}") || Peek(",")) { error = "MapInterMusic is missing a resource."; return false; }
                if (!TryReadMusic(Unquote(Take()), out var music, out var order, out error)) return false;
                map.MapIntermissionMusic[destination] = new MapInfoMusic(music, order);
                return true;
            }
            if (key.Equals("music", StringComparison.OrdinalIgnoreCase) || key.Equals("intermusic", StringComparison.OrdinalIgnoreCase))
            {
                if (Eof || Peek("}") || Peek(",")) { error = "Map music is missing a resource."; return false; }
                if (!TryReadMusic(Unquote(Take()), out var music, out var order, out error)) return false;
                if (key.Equals("music", StringComparison.OrdinalIgnoreCase)) { map.Music = music; map.MusicOrder = order; }
                else { map.IntermissionMusic = music; map.IntermissionMusicOrder = order; }
                return true;
            }
            if (key.Equals("exittext", StringComparison.OrdinalIgnoreCase)
                || key.Equals("textmusic", StringComparison.OrdinalIgnoreCase)
                || key.Equals("textflat", StringComparison.OrdinalIgnoreCase)
                || key.Equals("textpic", StringComparison.OrdinalIgnoreCase))
            {
                if (Eof || Peek("}") || Peek(",")) { error = "ExitText is missing a destination."; return false; }
                var destination = Unquote(Take());
                if (!Take(",")) { error = "ExitText is missing a comma."; return false; }
                var exists = map.ExitTexts.TryGetValue(destination, out var prior);
                var definition = prior ?? new MapInfoExitText("", false, HasText: false);
                if (key.Equals("exittext", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryReadClusterText(true, out var text, out var lookup, out error)) return false;
                    definition = definition with { Text = text, Lookup = lookup, HasText = true };
                }
                else
                {
                    if (Eof || Peek("}") || Peek(",")) { error = "Exit presentation is missing a resource."; return false; }
                    var resource = Unquote(Take());
                    if (key.Equals("textmusic", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!TryReadMusic(resource, out resource, out var order, out error)) return false;
                        definition = definition with { Music = resource, MusicOrder = order };
                    }
                    else definition = definition with { Background = resource,
                        BackgroundIsPicture = key.Equals("textpic", StringComparison.OrdinalIgnoreCase),
                        Music = exists ? definition.Music : "" };
                }
                map.ExitTexts[destination] = definition;
                return true;
            }
            if (key.Equals("noclustertext", StringComparison.OrdinalIgnoreCase))
            { map.NoClusterText = true; return true; }
            if (key.Equals("nogravity", StringComparison.OrdinalIgnoreCase))
            { map.Gravity = double.MaxValue; return true; }
            if (key.Equals("gravity", StringComparison.OrdinalIgnoreCase))
            {
                if (Eof || !double.TryParse(Take(), NumberStyles.Float, CultureInfo.InvariantCulture, out var gravity)
                    || !double.IsFinite(gravity))
                { error = "Invalid map gravity."; return false; }
                map.Gravity = gravity; return true;
            }
            if (key.Equals("aircontrol", StringComparison.OrdinalIgnoreCase))
            {
                if (Eof || !double.TryParse(Take(), NumberStyles.Float, CultureInfo.InvariantCulture, out var control)
                    || !double.IsFinite(control))
                { error = "Invalid map air control."; return false; }
                map.AirControl = control;
                return true;
            }
            if (key.Equals("activateowndeathspecials", StringComparison.OrdinalIgnoreCase))
            {
                map.ActivateOwnDeathSpecials = true;
                return true;
            }
            if (key.Equals("killeractivatesdeathspecials", StringComparison.OrdinalIgnoreCase))
            {
                map.ActivateOwnDeathSpecials = false;
                return true;
            }
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

            var cluster = new MapInfoCluster { Cluster = number };
            clusters[number] = cluster;
            var braced = Take("{");
            while (!Eof && (braced ? !Peek("}") : !AtDefinition()))
            {
                var key = Take();
                Optional("=");
                if (key.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryReadClusterText(braced, out var name, out var lookup, out error)) return false;
                    cluster.Name = name;
                    cluster.LookupName = lookup;
                }
                else if (key.Equals("entertext", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryReadClusterText(braced, out var text, out var lookup, out error)) return false;
                    cluster.EnterText = text;
                    cluster.LookupEnterText = lookup;
                }
                else if (key.Equals("exittext", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryReadClusterText(braced, out var text, out var lookup, out error)) return false;
                    cluster.ExitText = text;
                    cluster.LookupExitText = lookup;
                }
                else if (key.Equals("flat", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.Flat = ReadLumpName();
                }
                else if (key.Equals("pic", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.Flat = ReadLumpName();
                    cluster.FinaleIsPic = true;
                }
                else if (key.Equals("music", StringComparison.OrdinalIgnoreCase))
                {
                    if (Eof || Peek("}") || Peek(",")) { error = "Cluster music is missing a resource."; return false; }
                    if (!TryReadMusic(Unquote(Take()), out var music, out var order, out error)) return false;
                    cluster.Music = music; cluster.MusicOrder = order;
                }
                else if (key.Equals("hub", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.Hub = true;
                }
                else if (key.Equals("cdtrack", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryTakeInt(out var track)) { error = "Invalid cluster CD track."; return false; }
                    cluster.CdTrack = track;
                }
                else if (key.Equals("cdid", StringComparison.OrdinalIgnoreCase))
                {
                    if (!TryReadCdId(out var id, out error)) return false;
                    cluster.CdId = id;
                }
                else if (key.Equals("allowintermission", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.AllowIntermission = true;
                }
                else if (key.Equals("entertextislump", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.EnterTextIsLump = true;
                }
                else if (key.Equals("exittextislump", StringComparison.OrdinalIgnoreCase))
                {
                    cluster.ExitTextIsLump = true;
                }
                else
                {
                    SkipValue();
                }
            }

            if (braced && !Take("}"))
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

        private bool TryReadCdId(out uint id, out string? error)
        {
            id = 0;
            error = null;
            if (Eof || Peek("}") || Peek(",")) { error = "Missing CD id."; return false; }
            var value = Unquote(Take());
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) value = value[2..];
            if (!uint.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out id))
            { error = "Invalid CD id."; return false; }
            return true;
        }

        private bool TryReadMusic(string resource, out string music, out int order, out string? error, bool allowSeparateOrder = true)
        {
            music = resource; order = 0; error = null;
            var colon = resource.IndexOf(':');
            if (colon >= 0)
            {
                music = resource[..colon]; var suffix = resource[(colon + 1)..].TrimStart();
                var length = 0;
                if (suffix.StartsWith('+') || suffix.StartsWith('-')) length++;
                while (length < suffix.Length && char.IsAsciiDigit(suffix[length])) length++;
                int.TryParse(suffix[..length], NumberStyles.Integer, CultureInfo.InvariantCulture, out order);
            }
            else if (allowSeparateOrder && Take(","))
            {
                if (!TryTakeInt(out order)) { error = "Invalid music order."; return false; }
            }
            else if (allowSeparateOrder && int.TryParse(PeekToken(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            { Take(); order = value; }
            return true;
        }

        private bool TryReadClusterText(bool braced, out string text, out bool lookup, out string? error)
        {
            text = ""; lookup = false; error = null;
            if (Eof || Peek("}") || Peek(",")) { error = "Cluster text is missing a value."; return false; }
            var lookupToken = Peek("lookup");
            if (lookupToken)
            {
                Take(); Optional(",");
                if (Eof || Peek("}") || Peek(",")) { error = "Cluster lookup is missing a label."; return false; }
                text = Unquote(Take()); lookup = true; return true;
            }
            (text, lookup) = ReadText();
            if (!braced || lookup) return true;
            while (Take(","))
            {
                if (Eof || Peek("}") || Peek(",")) { error = "Cluster text list is missing a value."; return false; }
                text += "\n" + Unquote(Take());
            }
            return true;
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
