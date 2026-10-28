namespace HCDE.Gamedata;

public enum VmIntermissionFadeType { In, Out }
public enum VmIntermissionWipeType { Default, Crossfade, Melt, Burn }
public enum VmIntermissionScrollDir { Left, Right, Up, Down }

public sealed class VmIntermissionPatch
{
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public string Condition { get; set; } = "";
}

public class VmIntermissionAction
{
    public string Music { get; set; } = "";
    public int MusicOrder { get; set; }
    public bool MusicLooping { get; set; } = true;
    public int CdTrack { get; set; }
    public int CdId { get; set; }
    public int DurationTics { get; set; }
    public string Background { get; set; } = "";
    public bool FlatFill { get; set; }
    public string Sound { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public bool GotoTitle { get; set; }
    public List<VmIntermissionPatch> Overlays { get; } = [];
}

public sealed class VmIntermissionActionFader : VmIntermissionAction
{
    public VmIntermissionFadeType FadeType { get; set; } = VmIntermissionFadeType.In;
}

public sealed class VmIntermissionActionWiper : VmIntermissionAction
{
    public VmIntermissionWipeType WipeType { get; set; } = VmIntermissionWipeType.Default;
}

public sealed class VmIntermissionActionScroller : VmIntermissionAction
{
    public VmIntermissionScrollDir ScrollDir { get; set; } = VmIntermissionScrollDir.Right;
    public int ScrollDelayTics { get; set; }
    public int ScrollTimeTics { get; set; } = 640;
    public string SecondBackground { get; set; } = "";
}

public sealed class VmIntermissionActionTextscreen : VmIntermissionAction
{
    public int TextX { get; set; } = -1;
    public int TextY { get; set; } = -1;
    public string Text { get; set; } = "";
    public int TextDelayTics { get; set; } = 10;
    public int TextSpeed { get; set; } = 2;
    public string TextColor { get; set; } = "Untranslated";
    public IIntermissionTextLumpResolver? LumpResolver { get; set; }
}

public sealed class VmIntermissionCastSound
{
    public string Sequence { get; set; } = "";
    public int Index { get; set; }
    public string Sound { get; set; } = "";
}

public sealed class VmIntermissionActionCast : VmIntermissionAction
{
    public string CastName { get; set; } = "";
    public string CastClass { get; set; } = "";
    public List<VmIntermissionCastSound> CastSounds { get; } = [];
}

public sealed class VmIntermissionActionCutscene : VmIntermissionAction
{
    public string CutsceneBody { get; set; } = "";
}

public sealed class VmIntermissionDescriptor
{
    public string Link { get; set; } = "";
    public List<VmIntermissionAction> Actions { get; } = [];
}

public sealed class VmIntermissionRegistry
{
    private readonly Dictionary<string, VmIntermissionDescriptor> _descriptors = new(StringComparer.OrdinalIgnoreCase);

    public void Replace(string name, VmIntermissionDescriptor descriptor) => _descriptors[name] = descriptor;

    public bool TryGet(string name, out VmIntermissionDescriptor? descriptor) => _descriptors.TryGetValue(name, out descriptor);

    public void Clear() => _descriptors.Clear();

    public void Deinit() => Clear();

    public IReadOnlyDictionary<string, VmIntermissionDescriptor> Descriptors => _descriptors;
}

public enum VmEndGameType { None = -1, Pic, Bunny, Cast, Demon }

public sealed class VmEndGameSequence
{
    public VmEndGameType EndType { get; set; } = VmEndGameType.None;
    public bool MusicLooping { get; set; } = true;
    public bool PlayTheEnd { get; set; }
    public string PicName { get; set; } = "";
    public string PicName2 { get; set; } = "";
    public string Music { get; set; } = "";
}

public interface IIntermissionTextLumpResolver
{
    bool TryResolve(string lumpName, out string text, out string? hexenLookupKey);
}

public sealed class VmFinaleBuildRequest
{
    public string? Text { get; init; }
    public bool TextInLump { get; init; }
    public bool LookupText { get; init; }
    public string? Flat { get; init; }
    public bool FinalePic { get; init; }
    public string? Music { get; init; }
    public int MusicOrder { get; init; }
    public int CdTrack { get; init; }
    public uint CdId { get; init; }
    public bool Ending { get; init; }
    public string EndSequence { get; init; } = "";
    public bool SkipForChessEnding { get; init; }
}
