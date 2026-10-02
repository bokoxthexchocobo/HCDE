namespace HCDE.Playsim;

/// <summary>Native <c>EScrollPos</c> wall scroll parts.</summary>
[Flags]
public enum WallScrollParts
{
    Top = 1,
    Mid = 2,
    Bottom = 4,
    All = Top | Mid | Bottom,
}
