namespace HCDE.Playsim;

/// <summary><c>P_ChangeSector</c> iterator and attached-sector policy from <c>p_map.cpp</c> (7116–7271).</summary>
public static class VmPMapChangeSector
{
    public const int FloorMove = 0;
    public const int CeilingMove = 1;
    public const int MidtexMove = 2;
    public const int MfNoblockmap = 1;
    public const int Mf5Movewithsector = 1 << 1;

    public static double AbsMoveAmount(double amt) => amt < 0 ? -amt : amt;

    public static bool ProcessAttached3dFloors(int attachedCount, int floorOrCeil) =>
        attachedCount > 0 && floorOrCeil != MidtexMove;

    public static bool SkipHeightsecAttached(bool heightsecMatches) => heightsecMatches;

    public static bool ShouldMoveThing(int flags, int flags5) =>
        (flags & MfNoblockmap) == 0 || (flags5 & Mf5Movewithsector) != 0;

    public static bool UseInvertedCeilingIterator(int floorOrCeil, double amt) =>
        floorOrCeil == CeilingMove && amt >= 0;

    public static bool FloorDropWhenNegative(double amt) => amt < 0;

    public static bool FloorRaiseWhenPositive(double amt) => amt >= 0;

    public static bool MidtexUsesBothIterators(int floorOrCeil) => floorOrCeil == MidtexMove;

    public static bool InvalidFloorOrCeil(int floorOrCeil) => floorOrCeil < 0 || floorOrCeil > MidtexMove;

    public static bool RunSecondIterator(bool hasSecond) => hasSecond;

    public static bool CheckPortalAfterMove(int floorOrCeil) => floorOrCeil != MidtexMove;

    public static bool UpdateFakeFloorSectors(bool nofit, bool isReset) => !nofit && !isReset;

    public static bool ThingInFakeSector(bool sectorMatches) => sectorMatches;

    public static bool MarkVisited(bool visited) => !visited;

    public readonly record struct IteratorPickContext(int FloorOrCeil, double Amount);

    public static bool PickFloorDrop(IteratorPickContext ctx) =>
        ctx.FloorOrCeil == FloorMove && FloorDropWhenNegative(ctx.Amount);

    public static bool PickFloorRaise(IteratorPickContext ctx) =>
        ctx.FloorOrCeil == FloorMove && FloorRaiseWhenPositive(ctx.Amount);

    public static bool PickCeilingLower(IteratorPickContext ctx) =>
        ctx.FloorOrCeil == CeilingMove && FloorDropWhenNegative(ctx.Amount);

    public static bool PickCeilingRaise(IteratorPickContext ctx) =>
        ctx.FloorOrCeil == CeilingMove && FloorRaiseWhenPositive(ctx.Amount);

    public static bool MidtexMoveFlag(int floorOrCeil) => floorOrCeil == MidtexMove;

    public static bool ReturnNoFit(bool nofit) => nofit;
}
