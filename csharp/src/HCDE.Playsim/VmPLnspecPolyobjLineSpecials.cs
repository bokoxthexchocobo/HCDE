namespace HCDE.Playsim;

/// <summary>Damage-type mapping and polyobj specials from <c>p_lnspec.cpp</c> (60–225).</summary>
public static class VmPLnspecPolyobjLineSpecials
{
    public const int GameHexen = 2;

    public static readonly byte[] NamedAcsToNormal =
    [
        226, 227, 228, 229, 230, 231, 232,
    ];

    public static int MapAcsNameIndex(int index) =>
        index >= 0 && index < NamedAcsToNormal.Length ? NamedAcsToNormal[index] : 0;

    public static string ModToDamageTypeName(int mod) => mod switch
    {
        9 => "BFGSplash",
        12 => "Drowning",
        13 => "Slime",
        14 => "Fire",
        15 => "Crush",
        16 => "Telefrag",
        17 => "Falling",
        18 => "Suicide",
        20 => "Exit",
        22 => "Melee",
        23 => "Railgun",
        24 => "Ice",
        25 => "Disintegrate",
        26 => "Poison",
        27 => "Electric",
        1000 => "Massacre",
        _ => string.Empty,
    };

    public static bool ModIsNone(int mod) => ModToDamageTypeName(mod).Length == 0;

    public enum CrushMode
    {
        Doom,
        Hexen,
        Slowdown,
    }

    public static CrushMode CeilingCrushType(int mode, bool withSlowdown, int gameType)
    {
        if (mode >= 1 && mode <= 3)
            return (CrushMode)(mode - 1);
        if (gameType == GameHexen)
            return CrushMode.Hexen;
        return withSlowdown ? CrushMode.Slowdown : CrushMode.Doom;
    }

    public static bool CrushTypeDoomHexen(int mode, int gameType) =>
        mode == 1 ? false : mode == 2 ? true : gameType == GameHexen;

    public readonly record struct RotatePolyCall(int PolyId, int Speed, int Angle, int Direction, bool Overlap);

    public static RotatePolyCall PackRotateLeft(int poly, int speed, int angle) =>
        new(poly, speed, angle, 1, false);

    public static RotatePolyCall PackRotateRight(int poly, int speed, int angle) =>
        new(poly, speed, angle, -1, false);

    public static RotatePolyCall PackOrRotateLeft(int poly, int speed, int angle) =>
        new(poly, speed, angle, 1, true);

    public readonly record struct MovePolyCall(int PolyId, double Speed, double AngleDeg, int Distance, bool Overlap);

    public static MovePolyCall PackMove(int poly, int speed, int byteAngle, int distance, bool overlap) =>
        new(poly, VmPLnspecArgMacros.Speed(speed), byteAngle * (360.0 / 256.0), distance, overlap);

    public static MovePolyCall PackMoveTimes8(int poly, int speed, int byteAngle, int distance, bool overlap) =>
        PackMove(poly, speed, byteAngle, distance * 8, overlap);

    public readonly record struct MoveToPolyCall(int PolyId, double Speed, double X, double Y, bool Overlap);

    public static MoveToPolyCall PackMoveTo(int poly, int speed, int x, int y, bool overlap) =>
        new(poly, VmPLnspecArgMacros.Speed(speed), x, y, overlap);

    public static bool MoveToSpotRequiresActor(object? spot) => spot != null;

    public readonly record struct PolyDoorCall(int PolyId, int Speed, double AngleDeg, int Delay, int Distance, int Kind);

    public const int PodoorSwing = 0;
    public const int PodoorSlide = 1;

    public static PolyDoorCall PackDoorSwing(int poly, int speed, int byteAngle, int delay) =>
        new(poly, speed, byteAngle * (360.0 / 256.0), delay, 0, PodoorSwing);

    public static PolyDoorCall PackDoorSlide(int poly, int speed, int byteAngle, int distance, int delay) =>
        new(poly, speed, byteAngle * (360.0 / 256.0), delay, distance, PodoorSlide);

}
