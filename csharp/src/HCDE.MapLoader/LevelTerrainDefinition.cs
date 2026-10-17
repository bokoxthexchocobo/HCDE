namespace HCDE.MapLoader;

public sealed record LevelTerrainDefinition(string Name, string DamageType = "None", int DamageAmount = 0, int DamageTimeMask = 31,
    bool DamageOnLand = false, string Splash = "", double Friction = 0, double MoveFactor = 0, double FootClip = 0,
    bool AllowProtection = false, bool IsLiquid = false, float StepVolume = 0, int WalkStepTics = 0, int RunStepTics = 0,
    double StepDistance = 0, double StepDistanceMinVel = 0,
    string StepSound = "", string LeftStepSound = "", string RightStepSound = "");
public sealed record LevelFloorTerrain(string Texture, string Terrain);
public sealed record LevelTerrainSplash(string Name, string NormalSplashSound = "", string SmallSplashSound = "", bool NoAlert = false);
public sealed record LevelTerrainData(IReadOnlyList<LevelTerrainDefinition> Definitions,
    IReadOnlyList<LevelFloorTerrain> Floors, string DefaultTerrain = "", IReadOnlyList<string>? Splashes = null,
    IReadOnlyList<LevelTerrainSplash>? SplashDefinitions = null);
