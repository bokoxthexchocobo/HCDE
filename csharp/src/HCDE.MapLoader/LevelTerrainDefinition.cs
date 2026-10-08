namespace HCDE.MapLoader;

public sealed record LevelTerrainDefinition(string Name, string DamageType = "None", int DamageAmount = 0, int DamageTimeMask = 31,
    bool DamageOnLand = false, string Splash = "", double Friction = 0, double MoveFactor = 0);
public sealed record LevelFloorTerrain(string Texture, string Terrain);
public sealed record LevelTerrainData(IReadOnlyList<LevelTerrainDefinition> Definitions,
    IReadOnlyList<LevelFloorTerrain> Floors, string DefaultTerrain = "", IReadOnlyList<string>? Splashes = null);
