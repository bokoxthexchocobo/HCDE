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

public sealed class VmTerrainDatabase
{
    public List<VmSplashDef> Splashes { get; } = [];

    public int FindSplash(string name)
    {
        for (var i = 0; i < Splashes.Count; i++)
        {
            if (Splashes[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }
}
