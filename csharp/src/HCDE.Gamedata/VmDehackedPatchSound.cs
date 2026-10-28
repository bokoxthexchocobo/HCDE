namespace HCDE.Gamedata;

/// <summary>Disabled <c>PatchSound</c> line consumer from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchSound
{
    public static int ConsumePatchLines(Func<int> readLine) =>
        readLine() switch { 1 => ConsumePatchLines(readLine), var done => done };
}
