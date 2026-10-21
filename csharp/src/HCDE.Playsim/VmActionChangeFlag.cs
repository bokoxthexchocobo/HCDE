namespace HCDE.Playsim;

/// <summary><c>CheckFlag</c> guard from <c>p_actionfunctions.cpp</c> (2783–2791).</summary>
public static class VmActionChangeFlag
{
    public static bool CheckFlagOnOwner(object? owner, bool flagSet) =>
        owner is not null && flagSet;
}
