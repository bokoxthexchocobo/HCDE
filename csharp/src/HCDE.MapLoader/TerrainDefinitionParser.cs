using System.Text;

namespace HCDE.MapLoader;

/// <summary>TERRAIN subset for damage-type definitions and floor mappings.
/// Splash effects, conditionals, and texture validation are not represented.</summary>
public static class TerrainDefinitionParser
{
    public static bool TryParse(string text, out IReadOnlyList<LevelTerrainDefinition> definitions,
        out string? error, IReadOnlyList<LevelTerrainDefinition>? previous = null)
    {
        definitions = Array.Empty<LevelTerrainDefinition>();
        if (!TryParseData(text, out var data, out error,
            new LevelTerrainData(previous ?? Array.Empty<LevelTerrainDefinition>(), Array.Empty<LevelFloorTerrain>()))) return false;
        definitions = data.Definitions;
        return true;
    }

    public static bool TryParseData(string text, out LevelTerrainData data, out string? error, LevelTerrainData? previous = null)
    {
        data = new LevelTerrainData(Array.Empty<LevelTerrainDefinition>(), Array.Empty<LevelFloorTerrain>());
        if (!TryTokenize(text, out var tokens, out error)) return false;
        var entries = new Dictionary<string, LevelTerrainDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in previous?.Definitions ?? Array.Empty<LevelTerrainDefinition>()) entries[entry.Name] = entry;
        var floors = new Dictionary<string, LevelFloorTerrain>(StringComparer.OrdinalIgnoreCase);
        foreach (var floor in previous?.Floors ?? Array.Empty<LevelFloorTerrain>()) floors[floor.Texture] = floor;
        var defaultTerrain = previous?.DefaultTerrain ?? "";
        var splashes = new HashSet<string>(previous?.Splashes ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var index = 0;
        while (index < tokens.Count)
        {
            var directive = tokens[index++];
            if (directive.Equals("splash", StringComparison.OrdinalIgnoreCase))
            {
                if (index >= tokens.Count || tokens[index] is "{" or "}")
                { error = "Splash definition is missing a name."; return false; }
                var splash = tokens[index++];
                if (index < tokens.Count && tokens[index].Equals("modify", StringComparison.OrdinalIgnoreCase)) index++;
                if (index + 1 >= tokens.Count || tokens[index++] != "{" || tokens[index++] != "}")
                { error = "Only empty splash definition blocks are supported."; return false; }
                splashes.Add(splash);
                continue;
            }
            if (directive.Equals("floor", StringComparison.OrdinalIgnoreCase))
            {
                if (index < tokens.Count && tokens[index].Equals("optional", StringComparison.OrdinalIgnoreCase)) index++;
                if (index + 1 >= tokens.Count || tokens[index] is "{" or "}" || tokens[index + 1] is "{" or "}")
                { error = "Floor mapping is missing texture or terrain."; return false; }
                var texture = tokens[index++]; var terrain = tokens[index++];
                if (!entries.ContainsKey(terrain) && !terrain.Equals("None", StringComparison.OrdinalIgnoreCase)
                    && !terrain.Equals("Null", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Trace.TraceWarning($"Unknown floor terrain {terrain}; using default terrain.");
                    terrain = "None";
                }
                floors[texture] = new LevelFloorTerrain(texture, terrain);
                continue;
            }
            if (directive.Equals("defaultterrain", StringComparison.OrdinalIgnoreCase))
            {
                if (index >= tokens.Count || tokens[index] is "{" or "}")
                { error = "DefaultTerrain is missing a name."; return false; }
                defaultTerrain = tokens[index++];
                if (!entries.ContainsKey(defaultTerrain))
                {
                    System.Diagnostics.Trace.TraceWarning($"Unknown default terrain {defaultTerrain}; using neutral terrain.");
                    defaultTerrain = "";
                }
                continue;
            }
            if (!directive.Equals("terrain", StringComparison.OrdinalIgnoreCase))
            { error = "Unsupported TERRAIN directive."; return false; }
            if (index >= tokens.Count || tokens[index] is "{" or "}")
            { error = "Terrain definition is missing a name."; return false; }
            var name = tokens[index++];
            var modify = index < tokens.Count && tokens[index].Equals("modify", StringComparison.OrdinalIgnoreCase);
            if (modify) index++;
            if (index >= tokens.Count || tokens[index++] != "{")
            { error = $"Terrain {name} is missing a block."; return false; }
            var definition = modify && entries.TryGetValue(name, out var prior) ? prior : new LevelTerrainDefinition(name);
            while (index < tokens.Count && tokens[index] != "}")
            {
                var property = tokens[index++];
                if (property.Equals("friction", StringComparison.OrdinalIgnoreCase))
                {
                    if (index >= tokens.Count || !double.TryParse(tokens[index++], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var amount) || !double.IsFinite(amount))
                    { error = $"Invalid terrain {name} friction."; return false; }
                    var friction = Math.Clamp((0x1EB8 * (amount * 100)) / 0x80 + 0xD001, 0, 65536);
                    var moveFactor = friction > 0xE800
                        ? ((0x10092 - friction) * 1024) / 4352 + 568
                        : ((friction - 0xDB34) * 10) / 0x80;
                    definition = definition with { Friction = friction / 65536, MoveFactor = Math.Max(32, moveFactor) / 65536 };
                    continue;
                }
                if (property.Equals("damageonland", StringComparison.OrdinalIgnoreCase))
                { definition = definition with { DamageOnLand = true }; continue; }
                if (property.Equals("splash", StringComparison.OrdinalIgnoreCase))
                {
                    if (index >= tokens.Count || tokens[index] is "{" or "}")
                    { error = $"Terrain {name} is missing a splash name."; return false; }
                    var splash = tokens[index++];
                    if (!splashes.Contains(splash))
                    {
                        System.Diagnostics.Trace.TraceWarning($"Unknown splash {splash}; disabling terrain splash.");
                        splash = "";
                    }
                    definition = definition with { Splash = splash };
                    continue;
                }
                if (property.Equals("damageamount", StringComparison.OrdinalIgnoreCase)
                    || property.Equals("damagetimemask", StringComparison.OrdinalIgnoreCase))
                {
                    if (index >= tokens.Count || !int.TryParse(tokens[index++], System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out var value))
                    { error = $"Invalid terrain {name} {property}."; return false; }
                    if (property.Equals("damageamount", StringComparison.OrdinalIgnoreCase)) definition = definition with { DamageAmount = value };
                    else
                    {
                        if (value < 0) { error = $"Invalid terrain {name} damage time mask."; return false; }
                        definition = definition with { DamageTimeMask = value };
                    }
                    continue;
                }
                if (!property.Equals("damagetype", StringComparison.OrdinalIgnoreCase))
                { error = $"Unsupported terrain {name} property {property}."; return false; }
                if (index >= tokens.Count || tokens[index] is "{" or "}")
                { error = $"Terrain {name} is missing a damage type."; return false; }
                var type = tokens[index++];
                if (type.Equals("Lava", StringComparison.OrdinalIgnoreCase)) type = "Fire";
                definition = definition with { DamageType = type };
            }
            if (index >= tokens.Count || tokens[index++] != "}")
            { error = $"Unterminated terrain {name}."; return false; }
            entries[name] = definition;
        }
        data = new LevelTerrainData(entries.Values.ToArray(), floors.Values.ToArray(), defaultTerrain, splashes.ToArray());
        return true;
    }

    private static bool TryTokenize(string text, out List<string> tokens, out string? error)
    {
        tokens = new(); error = null;
        for (var index = 0; index < text.Length;)
        {
            if (char.IsWhiteSpace(text[index])) { index++; continue; }
            if (text[index] == '/' && index + 1 < text.Length && text[index + 1] == '/')
            {
                index += 2;
                while (index < text.Length && text[index] != '\n') index++;
                continue;
            }
            if (text[index] == '/' && index + 1 < text.Length && text[index + 1] == '*')
            {
                index += 2;
                while (index + 1 < text.Length && !(text[index] == '*' && text[index + 1] == '/')) index++;
                if (index + 1 >= text.Length) { error = "Unterminated terrain comment."; return false; }
                index += 2; continue;
            }
            if (text[index] is '{' or '}') { tokens.Add(text[index++].ToString()); continue; }
            if (text[index] == '"')
            {
                index++; var value = new StringBuilder();
                while (index < text.Length && text[index] != '"')
                {
                    if (text[index] == '\\' && index + 1 < text.Length) index++;
                    value.Append(text[index++]);
                }
                if (index >= text.Length) { error = "Unterminated terrain string."; return false; }
                index++; tokens.Add(value.ToString()); continue;
            }
            var start = index;
            while (index < text.Length && !char.IsWhiteSpace(text[index]) && text[index] is not ('{' or '}' or '"')
                && !(text[index] == '/' && index + 1 < text.Length && text[index + 1] is '/' or '*')) index++;
            tokens.Add(text[start..index]);
        }
        return true;
    }
}
