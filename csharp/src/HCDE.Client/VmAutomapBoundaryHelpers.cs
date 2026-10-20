namespace HCDE.Client;

/// <summary>
/// <c>AM_Check3DFloors</c> and automap special boundary helpers from <c>am_map.cpp</c> (2418–2622).
/// </summary>
public static class VmAutomapBoundaryHelpers
{
    [Flags]
    public enum FFloorFlags : uint
    {
        Exists = 1,
        ThisInside = 4,
    }

    public readonly struct FFloorRecord
    {
        public FFloorRecord(object? model, FFloorFlags flags, int alpha)
        {
            Model = model;
            Flags = flags;
            Alpha = alpha;
        }

        public object? Model { get; }
        public FFloorFlags Flags { get; }
        public int Alpha { get; }
    }

    public static bool IsRenderableFloor(FFloorRecord floor) =>
        (floor.Flags & FFloorFlags.ThisInside) == 0
        && (floor.Flags & FFloorFlags.Exists) != 0
        && floor.Alpha != 0;

    public static int CountRenderableFloors(IReadOnlyList<FFloorRecord> floors)
    {
        var count = 0;
        for (var i = 0; i < floors.Count; i++)
        {
            if (IsRenderableFloor(floors[i]))
                count++;
        }

        return count;
    }

    public static bool Check3DFloors(IReadOnlyList<FFloorRecord> front, IReadOnlyList<FFloorRecord> back)
    {
        if (back.Count == 0 && front.Count == 0)
            return false;

        var realFront = CountRenderableFloors(front);
        var realBack = CountRenderableFloors(back);
        if (realFront != realBack)
            return true;

        for (var i = 0; i < front.Count; i++)
        {
            var rover = front[i];
            if (!IsRenderableFloor(rover))
                continue;

            var found = false;
            for (var j = 0; j < back.Count; j++)
            {
                var rover2 = back[j];
                if (!IsRenderableFloor(rover2))
                    continue;
                if (Equals(rover.Model, rover2.Model) && rover.Flags == rover2.Flags)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                return true;
        }

        return false;
    }

    public const int SpacPlayerActivate = 1 << 0;

    public readonly struct SectorAction
    {
        public SectorAction(int healthFlags, int special, int[] args, bool friendly)
        {
            HealthFlags = healthFlags;
            Special = special;
            Args = args;
            Friendly = friendly;
        }

        public int HealthFlags { get; }
        public int Special { get; }
        public int[] Args { get; }
        public bool Friendly { get; }
    }

    public const int SecSpacUse = 1;
    public const int SecSpacUseWall = 2;

    public static bool TryFindSectorAction(
        IEnumerable<SectorAction> actions,
        Func<int, int[], bool> predicate,
        bool needUseActivated,
        out int special,
        out int[] args)
    {
        special = 0;
        args = Array.Empty<int>();
        foreach (var action in actions)
        {
            var useOk = (action.HealthFlags & (SecSpacUse | SecSpacUseWall)) != 0 || !needUseActivated;
            if (!useOk || action.Friendly)
                continue;
            if (!predicate(action.Special, action.Args))
                continue;
            special = action.Special;
            args = action.Args;
            return true;
        }

        return false;
    }

    public static bool CheckSpecialBoundary(
        int lineActivation,
        int lineSpecial,
        int[] lineArgs,
        bool hasBackSector,
        IEnumerable<SectorAction> frontSectorActions,
        IEnumerable<SectorAction>? backSectorActions,
        Func<int, int[], bool> predicate,
        out int special,
        out int[] args)
    {
        if ((lineActivation & SpacPlayerActivate) != 0 && predicate(lineSpecial, lineArgs))
        {
            special = lineSpecial;
            args = lineArgs;
            return true;
        }

        if (TryFindSectorAction(frontSectorActions, predicate, !hasBackSector, out special, out args))
            return true;

        if (hasBackSector && backSectorActions != null
            && TryFindSectorAction(backSectorActions, predicate, false, out special, out args))
            return true;

        special = 0;
        args = Array.Empty<int>();
        return false;
    }

    public const int SpecialTeleport = 11;
    public const int SpecialTeleportNoFog = 196;
    public const int SpecialTeleportZombie = 125;
    public const int SpecialTeleportLine = 154;
    public const int SpecialTeleportNewMap = 74;
    public const int SpecialTeleportEndGame = 75;
    public const int SpecialExitNormal = 52;
    public const int SpecialExitSecret = 124;
    public const int SpecialDoorOpen = 1;
    public const int SpecialDoorClose = 2;
    public const int SpecialDoorCloseWaitOpen = 16;
    public const int SpecialDoorRaise = 4;
    public const int SpecialDoorAnimated = 174;
    public const int SpecialGenericDoor = 300;
    public const int SpecialDoorLockedRaise = 13;
    public const int SpecialAcsLockedExecute = 226;
    public const int SpecialAcsLockedExecuteDoor = 227;
    public const int SpecialFsExecute = 193;

    public static bool IsTeleportSpecial(int special) =>
        special is SpecialTeleport or SpecialTeleportNoFog or SpecialTeleportZombie or SpecialTeleportLine;

    public static bool IsExitSpecial(int special) =>
        special is SpecialTeleportNewMap or SpecialTeleportEndGame or SpecialExitNormal or SpecialExitSecret;

    public static bool IsGenericTriggerSpecial(int special, int maxArgs, bool strictDoors)
    {
        if (maxArgs < 0)
            return false;
        if (!strictDoors)
            return true;
        return special is not (SpecialDoorOpen or SpecialDoorClose or SpecialDoorCloseWaitOpen
            or SpecialDoorRaise or SpecialDoorAnimated or SpecialGenericDoor);
    }

    public static bool IsLockSpecial(int special, int[] args)
    {
        return special switch
        {
            SpecialDoorLockedRaise => true,
            SpecialAcsLockedExecute => true,
            SpecialAcsLockedExecuteDoor => true,
            SpecialDoorAnimated => args.Length > 3 && args[3] != 0,
            SpecialGenericDoor => args.Length > 4 && args[4] != 0,
            SpecialFsExecute => args.Length > 2 && args[2] != 0,
            _ => false,
        };
    }

    public static int ResolveLockNumberFromSpecial(int special, int[] args)
    {
        return special switch
        {
            SpecialFsExecute => args.Length > 2 ? args[2] : 0,
            SpecialDoorAnimated or SpecialDoorLockedRaise => args.Length > 3 ? args[3] : 0,
            _ => args.Length > 4 ? args[4] : 0,
        };
    }

    public static bool TryResolveLockBoundary(
        int lineLockNumber,
        int lineActivation,
        int lineSpecial,
        int[] lineArgs,
        bool hasBackSector,
        IEnumerable<SectorAction> frontActions,
        IEnumerable<SectorAction>? backActions,
        out int lockNumber)
    {
        if (lineLockNumber != 0)
        {
            lockNumber = lineLockNumber;
            return true;
        }

        if (!CheckSpecialBoundary(
                lineActivation,
                lineSpecial,
                lineArgs,
                hasBackSector,
                frontActions,
                backActions,
                IsLockSpecial,
                out var special,
                out var args))
        {
            lockNumber = 0;
            return false;
        }

        lockNumber = ResolveLockNumberFromSpecial(special, args);
        return true;
    }
}
