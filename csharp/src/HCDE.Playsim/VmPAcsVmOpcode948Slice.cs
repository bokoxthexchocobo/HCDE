namespace HCDE.Playsim;

/// <summary>
/// <c>DLevelScript::RunScript</c> actor/sector query and translation opcodes from <c>p_acs.cpp</c> (**9900–10049**).
/// Exclusive to chunk **948**; <c>AcsVm</c> dispatch is not matrix credit.
/// </summary>
public static class VmPAcsVmOpcode948Slice
{
    public const int MaxAcsTranslations = 65535;
    public const int PlayMovieDisabledResult = -1;
    public const int SetActorPositionStackPop = 4;
    public const int SectorFloorCeilingStackPop = 2;
    public const int PlaneWatcherStackPop = 8;
    public const int StartTranslationStackPop = 1;

    public static double AcsToDouble(int fixedValue) => fixedValue / 65536.0;

    public static int DoubleToAcs(double value) => (int)(value * 65536.0);

    public static int ResolvePlayMovieStackValue(int previousTop) =>
        previousTop < 0 ? previousTop : PlayMovieDisabledResult;

    public static int SetActorPositionTidIndex(int sp) => sp - 5;

    public static int SetActorPositionInterpolateIndex(int sp) => sp - 1;

    public static bool SetActorPositionInterpolate(int stackValue) => stackValue != 0;

    public static void ApplySetActorPositionStackPop(ref int sp) => sp -= SetActorPositionStackPop;

    public static bool SetActorPositionSucceeded(object? actor, bool moved) =>
        actor is not null && moved;

    public static int ResolveActorAcsScalar(object? actor, int valueWhenPresent)
    {
        if (actor is null)
            return 0;
        return valueWhenPresent;
    }

    public static int ResolveActorZWithBob(object? actor, double z, double bob) =>
        actor is null ? 0 : DoubleToAcs(z + bob);

    public static int SelectActorAxisAcs(int opcode, double x, double y)
    {
        if (opcode == (int)HCDE.MapLoader.AcsPcode.GetActorY)
            return DoubleToAcs(y);
        return DoubleToAcs(x);
    }

    public static int LineRowOffsetWhenActivationMissing(bool hasActivationLine, int offset) =>
        hasActivationLine ? offset : 0;

    public static int SectorTagIndex(int sp) => sp - 3;

    public static bool SectorHeightUsesPointInSector(int tag) => tag == 0;

    public static int ResolveSectorIndexFromTag(int tag, int taggedSector, int pointSector) =>
        tag != 0 ? taggedSector : pointSector;

    public static bool SectorIndexValid(int secnum) => secnum >= 0;

    public static int SelectSectorPlaneHeight(int opcode, double floorZ, double ceilingZ) =>
        opcode == (int)HCDE.MapLoader.AcsPcode.GetSectorCeilingZ
            ? DoubleToAcs(ceilingZ)
            : DoubleToAcs(floorZ);

    public static void ApplySectorFloorCeilingStackPop(ref int sp) => sp -= SectorFloorCeilingStackPop;

    public static int ResolveSectorLightLevel(int secnum, int lightLevel) =>
        secnum >= 0 ? lightLevel : -1;

    public static bool IsCeilingPlaneWatcher(int opcode) =>
        opcode == (int)HCDE.MapLoader.AcsPcode.SetCeilingTrigger;

    public static void ApplyPlaneWatcherStackPop(ref int sp) => sp -= PlaneWatcherStackPop;

    public static bool TranslationIndexInRange(int index) =>
        index >= 1 && index <= MaxAcsTranslations;

    public static int TranslationTableSlot(int index) => index - 1;

    public static void ApplyStartTranslationStackPop(ref int sp) => sp -= StartTranslationStackPop;
}
