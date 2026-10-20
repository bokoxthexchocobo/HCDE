namespace HCDE.Gamedata;

public sealed class VmSplashDef
{
    public string Name { get; set; } = "";
    public string SmallSplashSound { get; set; } = "";
    public string NormalSplashSound { get; set; } = "";
    public double SmallSplashClip { get; set; } = 12;
    public string SmallSplashClass { get; set; } = "";
    public string SplashBaseClass { get; set; } = "";
    public string SplashChunkClass { get; set; } = "";
    public int ChunkXVelShift { get; set; } = 8;
    public int ChunkYVelShift { get; set; } = 8;
    public int ChunkZVelShift { get; set; } = 8;
    public double ChunkBaseZVel { get; set; } = 1;
    public bool NoAlert { get; set; }
}

public sealed class VmTerrainDef
{
    public string Name { get; set; } = "";
    public int SplashIndex { get; set; } = -1;
    public int DamageAmount { get; set; }
    public string DamageType { get; set; } = "";
    public int DamageTimeMask { get; set; } = 31;
    public double FootClip { get; set; }
    public float StepVolume { get; set; }
    public int WalkStepTics { get; set; }
    public int RunStepTics { get; set; }
    public bool IsLiquid { get; set; }
    public bool AllowProtection { get; set; }
    public bool DamageOnLand { get; set; }
    public double Friction { get; set; }
    public double MoveFactor { get; set; }
    public string StepSound { get; set; } = "";
}

public sealed class VmTerrainDatabase
{
    public List<VmSplashDef> Splashes { get; } = [];
    public List<VmTerrainDef> Terrains { get; } = [];
    public Dictionary<string, int> FloorTerrains { get; } = new(StringComparer.OrdinalIgnoreCase);
    public int DefaultTerrainIndex { get; set; }

    public int FindSplash(string name)
    {
        for (var i = 0; i < Splashes.Count; i++)
        {
            if (Splashes[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public int FindTerrain(string name)
    {
        if (name.Equals("None", StringComparison.OrdinalIgnoreCase) || name.Equals("Null", StringComparison.OrdinalIgnoreCase))
            return -1;
        for (var i = 0; i < Terrains.Count; i++)
        {
            if (Terrains[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public string GetTerrainName(int index)
    {
        if (index < 0 || index >= Terrains.Count) return "";
        return Terrains[index].Name;
    }
}
