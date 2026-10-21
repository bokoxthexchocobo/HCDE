namespace HCDE.Playsim;

/// <summary>Raise actions from <c>p_actionfunctions.cpp</c> (2833–2865).</summary>
public static class VmActionRaise
{
    public static bool ShouldRaiseMaster(object? master) => master is not null;

    public static bool IsChildOf(object? master, object self) =>
        master is not null && ReferenceEquals(master, self) == false;

    public static bool ShouldRaiseSibling(object? selfMaster, object? moMaster, object mo, object self) =>
        selfMaster is not null && ReferenceEquals(moMaster, selfMaster) && !ReferenceEquals(mo, self);
}
