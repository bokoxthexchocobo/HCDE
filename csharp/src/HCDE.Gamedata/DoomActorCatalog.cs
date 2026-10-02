namespace HCDE.Gamedata;

/// <summary>Default map actor dimensions and health from wadsrc/static/zscript/actors/doom.</summary>
public sealed record DoomActorDefinition(int EditorNumber, int Health, int Radius, int Height, int Speed, int PainChance, bool Floating = false, bool NoRadiusDamage = false);

public static class DoomActorCatalog
{
    /// <summary>Default Doom spawn names from <c>mapinfo/doomitems.txt</c> for catalog editor numbers.</summary>
    private static readonly Dictionary<string, int> SpawnClassNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SpiderMastermind"] = 7,
        ["ShotgunGuy"] = 9,
        ["Cyberdemon"] = 16,
        ["Spectre"] = 58,
        ["Archvile"] = 64,
        ["ChaingunGuy"] = 65,
        ["Revenant"] = 66,
        ["Fatso"] = 67,
        ["Arachnotron"] = 68,
        ["HellKnight"] = 69,
        ["PainElemental"] = 71,
        ["WolfensteinSS"] = 84,
        ["DoomImp"] = 3001,
        ["Demon"] = 3002,
        ["BaronOfHell"] = 3003,
        ["Zombieman"] = 3004,
        ["ZombieMan"] = 3004,
        ["Cacodemon"] = 3005,
        ["LostSoul"] = 3006,
    };

    private static readonly Dictionary<int, DoomActorDefinition> Definitions = new DoomActorDefinition[]
    {
        new(3004,20,20,56,8,200), new(9,30,20,56,8,170), new(65,70,20,56,8,170),
        new(84,50,20,56,8,170), new(3001,60,20,56,8,200),
        new(3002,150,30,56,10,180), new(58,150,30,56,10,180),
        new(3005,400,31,56,8,128,true), new(3003,1000,24,64,8,50), new(69,500,24,64,8,50),
        new(64,700,20,56,15,10), new(66,300,20,56,10,100), new(67,600,48,64,8,80),
        new(68,500,64,64,12,128), new(71,400,31,56,8,128,true), new(3006,100,16,56,8,256,true),
        new(16,4000,40,110,16,20,NoRadiusDamage:true), new(7,3000,128,100,12,40,NoRadiusDamage:true),
    }.ToDictionary(definition => definition.EditorNumber);

    public static DoomActorDefinition? Find(int editorNumber) => Definitions.GetValueOrDefault(editorNumber);

    /// <summary>Resolves a Doom spawn class name to an editor number when this port tracks it.</summary>
    public static bool TryEditorNumberForClassName(string? className, out int editorNumber)
    {
        editorNumber = 0;
        if (string.IsNullOrWhiteSpace(className)) return false;
        if (!SpawnClassNames.TryGetValue(className.Trim(), out editorNumber)) return false;
        return Definitions.ContainsKey(editorNumber);
    }

    /// <summary>True when <paramref name="editorNumber"/> matches a known spawn class name.</summary>
    public static bool MatchesClassName(int editorNumber, string? className) =>
        TryEditorNumberForClassName(className, out var mapped) && mapped == editorNumber;

    /// <summary>Preferred Doom spawn name for a catalog editor number, when known.</summary>
    public static bool TrySpawnClassName(int editorNumber, out string className)
    {
        foreach (var pair in SpawnClassNames)
        {
            if (pair.Value == editorNumber && Definitions.ContainsKey(editorNumber))
            {
                className = pair.Key;
                return true;
            }
        }

        className = string.Empty;
        return false;
    }

    public static int MassOf(int editorNumber) => editorNumber switch
    {
        3006 => 50, 3002 or 58 or 3005 or 71 => 400, 64 or 66 => 500,
        68 => 600, 3003 or 69 or 67 or 16 or 7 => 1000, _ => 100
    };
}
