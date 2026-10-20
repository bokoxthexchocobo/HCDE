namespace HCDE.Gamedata;

/// <summary><c>dsda_LumpOptions</c> from <c>options.cpp</c>.</summary>
public static class VmDsdaOptionsParser
{
    private static readonly (string Key, Action<VmDsdaOptions, int> Setter, int Min, int Max)[] Options =
    [
        ("monsters_remember", (o, v) => o.MonstersRemember = v, 0, 1),
        ("monster_infighting", (o, v) => o.MonsterInfighting = v, 0, 1),
        ("monster_backing", (o, v) => o.MonsterBacking = v, 0, 1),
        ("monster_avoid_hazards", (o, v) => o.MonsterAvoidHazards = v, 0, 1),
        ("monster_friction", (o, v) => o.MonsterFriction = v, 0, 1),
        ("comp_dropoff", (o, v) => o.CompDropoff = v, 0, 1),
        ("comp_vile", (o, v) => o.CompVile = v, 0, 1),
        ("comp_pain", (o, v) => o.CompPain = v, 0, 1),
        ("comp_doorlight", (o, v) => o.CompDoorlight = v, 0, 1),
        ("comp_model", (o, v) => o.CompModel = v, 0, 1),
        ("comp_floors", (o, v) => o.CompFloors = v, 0, 1),
        ("comp_staylift", (o, v) => o.CompStaylift = v, 0, 1),
        ("comp_stairs", (o, v) => o.CompStairs = v, 0, 1),
        ("comp_ledgeblock", (o, v) => o.CompLedgeblock = v, 0, 1),
        ("help_friends", (o, v) => o.HelpFriends = v, 0, 1),
        ("player_helpers", (o, v) => o.PlayerHelpers = v, 0, 3),
        ("friend_distance", (o, v) => o.FriendDistance = v, 0, 999),
        ("dog_jumping", (o, v) => o.DogJumping = v, 0, 1),
        ("comp_pursuit", (o, v) => o.CompPursuit = v, 0, 1),
        ("comp_friendlyspawn", (o, v) => o.CompFriendlyspawn = v, 0, 1),
        ("comp_reservedlineflag", (o, v) => o.CompReservedLineflag = v, 0, 1),
    ];

    public static bool TryParse(string text, out VmDsdaOptions options)
    {
        options = new VmDsdaOptions();
        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var value)) continue;
            foreach (var (key, setter, min, max) in Options)
            {
                if (!parts[0].Equals(key, StringComparison.OrdinalIgnoreCase)) continue;
                setter(options, Math.Clamp(value, min, max));
                break;
            }
        }

        return true;
    }
}

public sealed class VmDsdaOptions
{
    public int MonstersRemember { get; set; } = -1;
    public int MonsterInfighting { get; set; } = -1;
    public int MonsterBacking { get; set; } = -1;
    public int MonsterAvoidHazards { get; set; } = -1;
    public int MonsterFriction { get; set; } = -1;
    public int CompDropoff { get; set; } = -1;
    public int CompVile { get; set; } = -1;
    public int CompPain { get; set; } = -1;
    public int CompDoorlight { get; set; } = -1;
    public int CompModel { get; set; } = -1;
    public int CompFloors { get; set; } = -1;
    public int CompStaylift { get; set; } = -1;
    public int CompStairs { get; set; } = -1;
    public int CompLedgeblock { get; set; } = -1;
    public int HelpFriends { get; set; }
    public int PlayerHelpers { get; set; }
    public int FriendDistance { get; set; } = 128;
    public int DogJumping { get; set; } = -1;
    public int CompPursuit { get; set; } = -1;
    public int CompFriendlyspawn { get; set; } = -1;
    public int CompReservedLineflag { get; set; } = -1;
}
