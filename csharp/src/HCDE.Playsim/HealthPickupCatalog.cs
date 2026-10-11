namespace HCDE.Playsim;

internal readonly record struct HealthPickupDefinition(string TypeName, int Health, int Maximum, int AutoUseMode);

internal static class HealthPickupCatalog
{
    private static readonly HealthPickupDefinition[] Definitions =
    [new("MedPatch", 10, 20, 3), new("MedicalKit", 25, 15, 3), new("SurgeryKit", -100, 5, 0)];

    internal static bool TryFind(string typeName, out HealthPickupDefinition definition, int defaultMaximum = 25)
    {
        foreach (var item in Definitions)
            if (item.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase))
            { definition = item; return true; }
        if (typeName.Equals("ArtiHealth", StringComparison.OrdinalIgnoreCase))
        { definition = new("ArtiHealth", 25, defaultMaximum, 1); return true; }
        if (typeName.Equals("ArtiSuperHealth", StringComparison.OrdinalIgnoreCase))
        { definition = new("ArtiSuperHealth", 100, defaultMaximum, 2); return true; }
        definition = default; return false;
    }
}
