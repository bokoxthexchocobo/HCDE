namespace HCDE.Playsim;

/// <summary>Native <c>DScroller::m_Affect</c> carry subset.</summary>
[Flags]
public enum ScrollCarryAffect
{
    StaticObjects = 2,
    Players = 4,
    Monsters = 8,
    All = StaticObjects | Players | Monsters,
}
