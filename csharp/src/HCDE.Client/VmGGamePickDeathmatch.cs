namespace HCDE.Client;

/// <summary>Native <c>PickDeathmatchStart</c> from <c>g_game.cpp</c> (2151–2179).</summary>
public static class VmGGamePickDeathmatch
{
    public const int IntMaxAngle = int.MaxValue;

    public readonly record struct DeathmatchPick(int Angle, double X, double Y, double Z, bool Empty);

    public static DeathmatchPick EmptyStart() => new(IntMaxAngle, 0, 0, 0, true);

    public static DeathmatchPick FromStart(int angle, double x, double y, double z) =>
        new(angle, x, y, z, false);

    public static int SelectIndex(int count, int randomIndex) => count == 0 ? 0 : randomIndex % count;

    public static int ReturnCount(bool wantVector, bool wantAngle) =>
        (wantVector ? 1 : 0) + (wantAngle ? 1 : 0);

    public static bool HasDeathmatchStarts(int count) => count > 0;

    public static double PickX(DeathmatchPick pick) => pick.X;

    public static double PickY(DeathmatchPick pick) => pick.Y;

    public static double PickZ(DeathmatchPick pick) => pick.Z;

    public static int PickAngle(DeathmatchPick pick) => pick.Angle;

    public static bool PickIsEmpty(DeathmatchPick pick) => pick.Empty;

    public static DeathmatchPick PickFromStarts(IReadOnlyList<DeathmatchPick> starts, int index) =>
        starts.Count == 0 ? EmptyStart() : starts[index % starts.Count];

    public static bool WantVector(bool flag) => flag;

    public static bool WantAngle(bool flag) => flag;
}
