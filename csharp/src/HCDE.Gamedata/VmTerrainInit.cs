namespace HCDE.Gamedata;

/// <summary><c>MakeDefaultTerrain</c> / <c>P_InitTerrainTypes</c> reset from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainInit
{
    public static void ResetDatabase(VmTerrainDatabase db)
    {
        db.Splashes.Clear();
        db.Terrains.Clear();
        db.FloorTerrains.Clear();
        db.DefaultTerrainIndex = 0;
        MakeDefaultTerrain(db);
    }

    public static void MakeDefaultTerrain(VmTerrainDatabase db)
    {
        db.Terrains.Add(new VmTerrainDef
        {
            Name = "Solid",
            SplashIndex = -1,
            DamageTimeMask = 31,
        });
    }
}
