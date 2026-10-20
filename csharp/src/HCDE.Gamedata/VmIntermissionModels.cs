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
    public int CdTrack { get; set; }
    public int CdId { get; set; }
    public int DurationTics { get; set; }
    public string Background { get; set; } = "";
    public bool FlatFill { get; set; }
    public string Sound { get; set; } = "";
    public string Subtitle { get; set; } = "";
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
}

public sealed class VmIntermissionDescriptor
{
    public string Link { get; set; } = "";
    public List<VmIntermissionAction> Actions { get; } = [];
}
