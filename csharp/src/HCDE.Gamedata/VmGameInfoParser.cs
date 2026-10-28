namespace HCDE.Gamedata;

public sealed class VmGameInfoDocument
{
    public Dictionary<int, List<string>> WeaponSlots { get; } = new();
    public VmGameBorder Border { get; } = new();
    public string ArmorIcon1 { get; set; } = "";
    public float Armor2Percent { get; set; }
    public string ArmorIcon2 { get; set; } = "";
    public string MapArrow { get; set; } = "";
    public string CheatMapArrow { get; set; } = "";
    public string Dialogue { get; set; } = "";
    public List<string> FinalePages { get; } = [];
}

public sealed class VmGameBorder
{
    public int Size { get; set; } = 8;
    public int Offset { get; set; } = 8;
    public string[] Graphics { get; } = new string[8];
}

public static class VmGameInfoParser
{
    private static readonly Dictionary<string, VmGameBorder> PresetBorders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DoomBorder"] = new VmGameBorder { Size = 8, Offset = 8 },
        ["HereticBorder"] = new() { Size = 4, Offset = 16 },
        ["StrifeBorder"] = new() { Size = 8, Offset = 8 }
    };

    public static bool TryParseKey(string key, IReadOnlyList<string> tokens, ref int i, VmGameInfoDocument doc, List<string> errors)
    {
        if (key.Equals("weaponslot", StringComparison.OrdinalIgnoreCase))
        {
            if (i >= tokens.Count || !int.TryParse(tokens[i++], out var slot) || slot < 0 || slot > 9)
            {
                errors.Add("Weapon slot index must be in range [0..9].");
                return false;
            }

            if (i >= tokens.Count || tokens[i] != ",") { errors.Add("Expected comma after weapon slot."); return false; }
            i++;
            var weapons = new List<string>();
            while (i < tokens.Count && tokens[i] != "}")
            {
                weapons.Add(tokens[i++]);
                if (i < tokens.Count && tokens[i] == ",") i++;
            }

            doc.WeaponSlots[slot] = weapons;
            return true;
        }

        if (key.Equals("border", StringComparison.OrdinalIgnoreCase))
        {
            if (i < tokens.Count && PresetBorders.TryGetValue(tokens[i], out var preset))
            {
                i++;
                doc.Border.Size = preset.Size;
                doc.Border.Offset = preset.Offset;
                return true;
            }

            if (i + 9 >= tokens.Count) { errors.Add("Expected border definition."); return false; }
            doc.Border.Offset = int.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
            if (tokens[i++] != ",") return false;
            doc.Border.Size = int.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
            for (var g = 0; g < 8; g++)
            {
                if (tokens[i++] != ",") return false;
                doc.Border.Graphics[g] = tokens[i++];
            }

            return true;
        }

        if (key.Equals("armoricons", StringComparison.OrdinalIgnoreCase))
        {
            if (i >= tokens.Count) return false;
            doc.ArmorIcon1 = tokens[i++];
            if (i < tokens.Count && tokens[i] == ",")
            {
                i++;
                doc.Armor2Percent = float.Parse(tokens[i++], System.Globalization.CultureInfo.InvariantCulture);
                if (i < tokens.Count && tokens[i] == ",")
                {
                    i++;
                    doc.ArmorIcon2 = tokens[i++];
                }
            }

            return true;
        }

        if (key.Equals("maparrow", StringComparison.OrdinalIgnoreCase))
        {
            if (i >= tokens.Count) return false;
            doc.MapArrow = tokens[i++];
            if (i < tokens.Count && tokens[i] == ",")
            {
                i++;
                doc.CheatMapArrow = tokens[i++];
            }
            else doc.CheatMapArrow = "";

            return true;
        }

        if (key.Equals("dialogue", StringComparison.OrdinalIgnoreCase))
        {
            if (i >= tokens.Count) return false;
            doc.Dialogue = tokens[i++];
            return true;
        }

        errors.Add($"Unknown GAMEINFO key '{key}'.");
        return false;
    }

    public static string? GetFinalePage(IReadOnlyList<string> pages, uint num)
    {
        if (pages.Count == 0) return "-NOFLAT-";
        if (num < 1 || num > pages.Count) return pages[0];
        return pages[(int)num - 1];
    }

    public static bool CheckGame(string token, string activeGame, bool chexIsDoom = true)
    {
        var game = activeGame;
        if (game.Equals("Chex", StringComparison.OrdinalIgnoreCase) && chexIsDoom)
            game = "Doom";
        return token.Equals(game, StringComparison.OrdinalIgnoreCase)
               || token.Equals("Any", StringComparison.OrdinalIgnoreCase);
    }
}
