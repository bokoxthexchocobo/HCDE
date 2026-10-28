namespace HCDE.Playsim;

/// <summary><c>LS_Floor_RaiseToCeiling</c> from <c>p_lnspec.cpp</c> (484–501); chunk 413 surviving slice only.</summary>
public static class VmPLnspecFloorRaiseToCeiling
{
    public readonly record struct FloorInvoke(int Tag, double Speed, int Height, int Crush, int Change, bool DirectionUp);

    public static FloorInvoke RaiseToCeiling(int tag, int speed, int change, int crush, int gap) =>
        new(tag, VmPLnspecArgMacros.Speed(speed), gap, VmPLnspecArgMacros.Crush(crush), VmPLnspecArgMacros.Change(change), true);
}
