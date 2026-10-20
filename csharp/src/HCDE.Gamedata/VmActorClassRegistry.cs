namespace HCDE.Gamedata;

/// <summary><c>PClassActor::StaticInit</c> / replacement helpers from <c>info.cpp</c>.</summary>
public static class VmActorClassRegistry
{
    public static void EnsureDefaultSprites(VmSpriteCatalog catalog)
    {
        catalog.GetSpriteIndex("TNT1", true);
        catalog.GetSpriteIndex("----", true);
        catalog.GetSpriteIndex("####", true);
    }

    public static bool TrySetReplacement(Dictionary<string, string> replacees, string actorName, string replaceeName, HashSet<string> knownActors)
    {
        if (!knownActors.Contains(replaceeName)) return false;
        replacees[actorName] = replaceeName;
        return true;
    }

    public static void SetDamageFactor(Dictionary<string, double> factors, string damageType, double factor)
    {
        factors[damageType] = factor;
    }

    public static void SetPainChance(Dictionary<string, int> chances, string painType, int chance)
    {
        if (chance >= 0)
            chances[painType] = Math.Min(chance, 256);
    }
}
