namespace HCDE.MapLoader;

public enum WadLumpNamespace
{
    Global = 0,
    Sprites = 1,
    Flats = 2,
    Colormaps = 3,
    AcsLibrary = 4,
    NewTextures = 5,
    StrifeVoices = 6,
    Hires = 7,
    Voxels = 8,
    FirstSkin = 100,
}

public sealed class WadLumpMetadata
{
    public WadLumpNamespace Namespace { get; set; } = WadLumpNamespace.Global;
    public bool MaybeFlat { get; set; }
    public bool IsLzssCompressed { get; set; }
    public bool HasShortName { get; set; } = true;
}
