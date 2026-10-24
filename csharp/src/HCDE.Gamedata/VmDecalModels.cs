namespace HCDE.Gamedata;

public sealed class VmDecalDefinition
{
    public string Name { get; set; } = "";
    public ushort Id { get; set; }
    public VmDecalTemplate Template { get; } = new();
}

public sealed class VmDecalGroupDefinition
{
    public string Name { get; set; } = "";
    public ushort Id { get; set; }
    public List<(string DecalName, int Weight)> Entries { get; } = [];
}

public sealed class VmDecalGeneratorBinding
{
    public string ActorClass { get; set; } = "";
    public string? DecalName { get; set; }
    public bool Optional { get; set; }
}

public sealed class VmDecalFaderAnimator
{
    public string Name { get; set; } = "";
    public int DecayStartTics { get; set; }
    public int DecayTimeTics { get; set; }
}

public sealed class VmDecalDatabase
{
    public List<VmDecalDefinition> Decals { get; } = [];
    public List<VmDecalGroupDefinition> Groups { get; } = [];
    public List<VmDecalGeneratorBinding> Generators { get; } = [];
    public List<VmDecalFaderAnimator> Faders { get; } = [];
    public List<VmDecalStretchAnimator> Stretchers { get; } = [];
    public List<VmDecalSliderAnimator> Sliders { get; } = [];
    public List<VmDecalColorAnimator> ColorChangers { get; } = [];
    public List<VmDecalCombinerAnimator> Combiners { get; } = [];
    public Dictionary<string, string> AnimatorNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetDecal(string name, out VmDecalDefinition? decal)
    {
        decal = Decals.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return decal is not null;
    }
}

public sealed class VmDecalStretchAnimator
{
    public string Name { get; set; } = "";
    public int StretchStartTics { get; set; }
    public int StretchTimeTics { get; set; }
    public double GoalX { get; set; } = -1;
    public double GoalY { get; set; } = -1;
}

public sealed class VmDecalSliderAnimator
{
    public string Name { get; set; } = "";
    public int SlideStartTics { get; set; }
    public int SlideTimeTics { get; set; }
    public double DistY { get; set; }
}

public sealed class VmDecalColorAnimator
{
    public string Name { get; set; } = "";
    public int FadeStartTics { get; set; }
    public int FadeTimeTics { get; set; }
    public string GoalColor { get; set; } = "";
}

public sealed class VmDecalCombinerAnimator
{
    public string Name { get; set; } = "";
    public List<string> AnimatorNames { get; } = [];
}
