namespace HCDE.Playsim;

/// <summary><c>P_TryMove</c> body from <c>p_map.cpp</c> (2368–2882), split across chunk709/713 native bands.</summary>
public static class VmPMapTryMove
{
    public const int MfTeleport = 1 << 0;
    public const int MfNoclip = 1 << 1;
    public const int MfDropoff = 1 << 2;
    public const int MfFloat = 1 << 3;
    public const int MfMissile = 1 << 4;
    public const int MfShootable = 1 << 5;
    public const int Mf2Passmobj = 1;
    public const int Mf2Fly = 1 << 1;
    public const int Mf2Onmobj = 1 << 2;
    public const int Mf2Blasted = 1 << 3;
    public const int Mf2Cantleavefloorpic = 1 << 4;
    public const int Mf2Floorclip = 1 << 5;
    public const int Mf2Mcross = 1 << 6;
    public const int Mf2Pcross = 1 << 7;
    public const int Mf3Floorhugger = 1;
    public const int Mf3Ceilinghugger = 1 << 1;
    public const int Mf4Scrollmove = 1;
    public const int Mf5Nodropoff = 1;
    public const int Mf5Avoidingdropoff = 1 << 1;
    public const int Mf6Intrymove = 1;
    public const int Mf6Stepmissile = 1 << 1;
    public const int Mf6Notrigger = 1 << 2;
    public const int MfNogravity = 1 << 8;
    public const int CompatfNoPassmobj = 1;
    public const int CompatfCrossdropoff = 1 << 1;
    public const int CfPredicting = 1;
    public const int BounceMbf = 1;

    public sealed class CheckPositionTm
    {
        public bool FloatOk { get; set; }
        public bool PortalStep { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double PosZ { get; set; }
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double DropoffZ { get; set; }
        public object? FloorPic { get; set; }
        public object? FloorSector { get; set; }
        public object? CeilingPic { get; set; }
        public object? CeilingSector { get; set; }
        public int FloorTerrain { get; set; }
        public object? Sector { get; set; }
        public int ThingFlags2 { get; set; }
        public int PortalGroup { get; set; }
    }

    public sealed class SpechitEntry
    {
        public object? Line { get; init; }
        public double OldRefX { get; init; }
        public double OldRefY { get; init; }
        public double RefX { get; init; }
        public double RefY { get; init; }
        public int LineSpecial { get; init; }
    }

    public sealed class PortalHitEntry
    {
        public object? Line { get; init; }
        public double OldRefX { get; init; }
        public double OldRefY { get; init; }
        public double RefX { get; init; }
        public double RefY { get; init; }
        public int FrontPortalGroup { get; init; }
    }

    public sealed class TryMoveActor
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Height { get; set; }
        public double Top => Z + Height;
        public double MaxStepHeight { get; set; }
        public double MaxDropOffHeight { get; set; }
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags4 { get; set; }
        public int Flags5 { get; set; }
        public int Flags6 { get; set; }
        public int BounceFlags { get; set; }
        public object? FloorPic { get; set; }
        public object? FloorSector { get; set; }
        public object? CeilingPic { get; set; }
        public object? CeilingSector { get; set; }
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double DropoffZ { get; set; }
        public int FloorTerrain { get; set; }
        public object? Sector { get; set; }
        public object? OldSector { get; set; }
        public object? Target { get; set; }
        public double TargetZ { get; set; }
        public double VelZ { get; set; }
        public bool HasPlayer { get; set; }
        public int PlayerCheats { get; set; }
        public bool BlockingIsPlayer { get; set; }
        public double BlockingTop { get; set; }
        public bool BlockingMobjPresent { get; set; }
        public bool Sentient { get; set; }
        public bool BotDangerousSector { get; set; }
        public double ViewHeight { get; set; }
        public bool HeightSecPresent { get; set; }
        public bool OldAboveFakeFloor { get; set; }
        public bool OldAboveFakeCeiling { get; set; }
        public double FakeFloorZ { get; set; }
        public double FakeCeilingZ { get; set; }
        public bool FakeFloorOnly { get; set; }
        public object? SecActTarget { get; set; }
    }

    public sealed class TryMoveState
    {
        public required TryMoveActor Actor { get; init; }
        public required CheckPositionTm Tm { get; init; }
        public double PosX { get; init; }
        public double PosY { get; init; }
        public int Dropoff { get; set; }
        public bool MissileCheck { get; init; }
        public bool OnFloor { get; init; }
        public double OnFloorZ { get; init; }
        public int LevelCompatFlags { get; init; }
        public List<SpechitEntry> Spechit { get; } = new();
        public List<PortalHitEntry> PortalHits { get; } = new();
        public Func<TryMoveActor, double, double, CheckPositionTm, bool>? CheckPosition { get; init; }
        public Action<TryMoveActor, object?>? CollidedWith { get; init; }
        public Func<TryMoveActor, bool, bool>? TestMobjZ { get; init; }
        public Action<TryMoveActor, SpechitEntry, int, int>? ActivateCrossLine { get; init; }
        public Action<TryMoveActor, SpechitEntry, int>? CheckForPushSpecial { get; init; }
        public Func<TryMoveState, bool>? ResolvePortalCrossing { get; init; }
        public Action<TryMoveActor>? CommitMove { get; init; }
        public Action<TryMoveActor>? AdjustFloorClip { get; init; }
        public Action<TryMoveActor, object?>? CheckSectorTransition { get; init; }
        public Action<TryMoveActor, object?>? TriggerSectorActions { get; init; }
        public Action<TryMoveActor>? PortalStepRelink { get; init; }
        public Action<TryMoveActor>? DamageBlastedSelf { get; init; }
        public bool PushLineRequested { get; set; }
    }

    public static bool ShouldCollideWhenBlocked(bool hasBlocker, bool blockerIsPlayer, bool actorHasPlayer) =>
        hasBlocker && (!actorHasPlayer || (actorHasPlayer && !blockerIsPlayer));

    public static bool PassMobjBlocked(int thingFlags2, int compatFlags) =>
        (thingFlags2 & Mf2Passmobj) == 0 || (compatFlags & CompatfNoPassmobj) != 0;

    public static bool HeightCrush(double ceiling, double floor, double height) => ceiling - floor < height;

    public static bool MustLowerToFit(bool teleport, double ceiling, double top, int flags3, bool fly, bool nogravity) =>
        !teleport && ceiling < top && (flags3 & Mf3Ceilinghugger) == 0 && (!(fly && nogravity));

    public static bool FlyCeilingBlocks(double top, double ceiling, bool fly, bool nogravity) =>
        fly && nogravity && top > ceiling;

    public static bool MissileStepBlocked(bool missile, bool stepMissile, double floorz, double z, int flags3) =>
        missile && !stepMissile && floorz > z && (flags3 & Mf3Floorhugger) == 0;

    public static bool StepUpTooHigh(double floorz, double z, double maxStep) => floorz - z > maxStep;

    public static bool DropoffCompatDisables(int compat, int flags4) =>
        (compat & CompatfCrossdropoff) != 0 && (flags4 & Mf4Scrollmove) == 0;

    public static bool LargeDogDropoffCancel(int dropoff, double floorz, double dropoffz, object? target, double targetZ) =>
        dropoff == 2 && (floorz - dropoffz > 128 || target == null || targetZ > dropoffz);

    public static bool DropoffBlocked(
        int dropoff,
        int flags,
        int flags5,
        double floorz,
        double dropoffz,
        double maxDrop,
        bool blasted,
        bool missileCheck) =>
        (dropoff == 0 && (flags & (MfDropoff | MfFloat | MfMissile)) == 0 || (flags5 & Mf5Nodropoff) != 0)
        && floorz - dropoffz > maxDrop
        && !blasted
        && !missileCheck;

    public static bool CantLeaveFloorPic(int flags2, object? floorPic, object? tmFloorPic, double floorDelta) =>
        (flags2 & Mf2Cantleavefloorpic) != 0 && (!ReferenceEquals(floorPic, tmFloorPic) || floorDelta != 0);

    public static bool BotDangerMoveBlocked(bool hasPlayer, bool shootable, bool dangerous) =>
        hasPlayer && shootable && dangerous;

    public static bool MbfBouncerStepBlocked(int bounce, int flags, bool nogravity, bool sentient, double floorDelta) =>
        (bounce & BounceMbf) != 0
        && (flags & (MfMissile | MfNogravity)) == 0
        && !sentient
        && floorDelta > 16;

    public static bool TryMove(TryMoveState st)
    {
        var thing = st.Actor;
        var tm = st.Tm;
        var oldz = thing.Z;
        tm.FloatOk = false;
        tm.PortalStep = false;
        thing.Flags6 |= Mf6Intrymove;

        if (st.OnFloor)
            thing.Z = st.OnFloorZ;

        if (st.CheckPosition?.Invoke(thing, st.PosX, st.PosY, tm) != true)
        {
            if (ShouldCollideWhenBlocked(thing.BlockingMobjPresent, thing.BlockingIsPlayer, thing.HasPlayer)
                && (thing.HasPlayer == false || (thing.PlayerCheats & CfPredicting) == 0))
                st.CollidedWith?.Invoke(thing, thing);

            if (!thing.BlockingMobjPresent || thing.BlockingIsPlayer || !thing.HasPlayer)
                st.PushLineRequested = true;
            else if (thing.BlockingTop - thing.Z > thing.MaxStepHeight
                     || tm.CeilingZ - thing.BlockingTop < thing.Height)
                st.PushLineRequested = true;
            else
                st.PushLineRequested = false;

            if (st.PushLineRequested || PassMobjBlocked(tm.ThingFlags2, st.LevelCompatFlags))
            {
                thing.Z = oldz;
                thing.Flags6 &= ~Mf6Intrymove;
                return false;
            }
        }

        if ((thing.Flags3 & Mf3Floorhugger) != 0)
            thing.Z = tm.FloorZ;
        else if ((thing.Flags3 & Mf3Ceilinghugger) != 0)
            thing.Z = tm.CeilingZ - thing.Height;

        if (st.OnFloor && ReferenceEquals(tm.FloorSector, thing.FloorSector))
            thing.Z = tm.FloorZ;

        if ((thing.Flags & MfNoclip) == 0)
        {
            if (HeightCrush(tm.CeilingZ, tm.FloorZ, thing.Height))
            {
                st.PushLineRequested = true;
                goto pushline;
            }

            tm.FloatOk = true;

            if (MustLowerToFit((thing.Flags & MfTeleport) != 0, tm.CeilingZ, thing.Top, thing.Flags3,
                    (thing.Flags2 & Mf2Fly) != 0, (thing.Flags & MfNogravity) != 0))
            {
                st.PushLineRequested = true;
                goto pushline;
            }

            if (FlyCeilingBlocks(thing.Top, tm.CeilingZ, (thing.Flags2 & Mf2Fly) != 0, (thing.Flags & MfNogravity) != 0))
            {
                st.PushLineRequested = true;
                goto pushline;
            }

            if ((thing.Flags & MfTeleport) == 0 && ((thing.Flags3 & Mf3Floorhugger) == 0 || (thing.Flags5 & Mf5Nodropoff) != 0))
            {
                if (MissileStepBlocked((thing.Flags & MfMissile) != 0, (thing.Flags6 & Mf6Stepmissile) != 0, tm.FloorZ, thing.Z, thing.Flags3))
                {
                    st.PushLineRequested = true;
                    goto pushline;
                }

                if (StepUpTooHigh(tm.FloorZ, thing.Z, thing.MaxStepHeight))
                {
                    st.PushLineRequested = true;
                    goto pushline;
                }

                if (thing.Z < tm.FloorZ)
                {
                    var savedz = thing.Z;
                    thing.Z = tm.FloorZ;
                    if (st.TestMobjZ?.Invoke(thing, false) != true)
                    {
                        thing.Z = savedz;
                        st.PushLineRequested = true;
                        goto pushline;
                    }

                    if ((thing.Flags6 & Mf6Stepmissile) != 0)
                    {
                        thing.Z = tm.FloorZ;
                        if (thing.VelZ < 0)
                            thing.VelZ = 0;
                    }
                }
            }

            if (DropoffCompatDisables(st.LevelCompatFlags, thing.Flags4))
                st.Dropoff = 0;

            if (LargeDogDropoffCancel(st.Dropoff, tm.FloorZ, tm.DropoffZ, thing.Target, thing.TargetZ))
                st.Dropoff = 0;

            var floorzForDrop = tm.FloorZ;
            if ((thing.Flags2 & Mf2Onmobj) != 0)
                floorzForDrop = Math.Max(thing.Z, tm.FloorZ);

            if (DropoffBlocked(st.Dropoff, thing.Flags, thing.Flags5, floorzForDrop, tm.DropoffZ, thing.MaxDropOffHeight,
                    (thing.Flags2 & Mf2Blasted) != 0, st.MissileCheck))
            {
                thing.Z = oldz;
                thing.Flags6 &= ~Mf6Intrymove;
                return false;
            }

            if (CantLeaveFloorPic(thing.Flags2, thing.FloorPic, tm.FloorPic, tm.FloorZ - thing.Z))
            {
                thing.Z = oldz;
                thing.Flags6 &= ~Mf6Intrymove;
                return false;
            }

            if (BotDangerMoveBlocked(thing.HasPlayer, (thing.Flags & MfShootable) != 0, thing.BotDangerousSector))
            {
                thing.Z = oldz;
                thing.Flags6 &= ~Mf6Intrymove;
                return false;
            }

            if (MbfBouncerStepBlocked(thing.BounceFlags, thing.Flags, (thing.Flags & MfNogravity) != 0, thing.Sentient, tm.FloorZ - thing.Z))
            {
                thing.Flags6 &= ~Mf6Intrymove;
                return false;
            }
        }

        return TryMoveAfterPortals(st, oldz);

    pushline:
        thing.Flags6 &= ~Mf6Intrymove;
        thing.Z = oldz;
        if (thing.HasPlayer && (thing.PlayerCheats & CfPredicting) != 0)
            return false;
        if ((thing.Flags & (MfTeleport | MfNoclip)) == 0)
        {
            if ((thing.Flags2 & Mf2Blasted) != 0)
                st.DamageBlastedSelf?.Invoke(thing);
            for (var i = st.Spechit.Count - 1; i >= 0; i--)
            {
                var spec = st.Spechit[i];
                st.CheckForPushSpecial?.Invoke(thing, spec, 0);
            }
        }

        return false;
    }

    public static bool TryMoveAfterPortals(TryMoveState st, double oldz)
    {
        var thing = st.Actor;
        var tm = st.Tm;
        if (st.ResolvePortalCrossing?.Invoke(st) == false)
        {
            thing.Z = oldz;
            thing.Flags6 &= ~Mf6Intrymove;
            return false;
        }

        st.CommitMove?.Invoke(thing);

        if ((thing.Flags2 & Mf2Floorclip) != 0)
            st.AdjustFloorClip?.Invoke(thing);

        if ((thing.Flags & (MfTeleport | MfNoclip)) == 0)
        {
            while (st.Spechit.Count > 0)
            {
                var spec = st.Spechit[^1];
                st.Spechit.RemoveAt(st.Spechit.Count - 1);
                st.ActivateCrossLine?.Invoke(thing, spec, 0, 1);
            }
        }

        if (thing.HasPlayer && (thing.PlayerCheats & CfPredicting) != 0)
        {
            thing.Flags6 &= ~Mf6Intrymove;
            return true;
        }

        if (thing.HeightSecPresent && thing.SecActTarget != null)
        {
            var eyez = thing.Z + thing.ViewHeight;
            if (!thing.OldAboveFakeFloor && eyez > thing.FakeFloorZ)
                st.TriggerSectorActions?.Invoke(thing, "EyesSurface");
            else if (thing.OldAboveFakeFloor && eyez <= thing.FakeFloorZ)
                st.TriggerSectorActions?.Invoke(thing, "EyesDive");

            if (!thing.FakeFloorOnly)
            {
                if (!thing.OldAboveFakeCeiling && eyez > thing.FakeCeilingZ)
                    st.TriggerSectorActions?.Invoke(thing, "EyesAboveC");
                else if (thing.OldAboveFakeCeiling && eyez <= thing.FakeCeilingZ)
                    st.TriggerSectorActions?.Invoke(thing, "EyesBelowC");
            }
        }

        if (tm.PortalStep)
            st.PortalStepRelink?.Invoke(thing);

        st.CheckSectorTransition?.Invoke(thing, thing.OldSector);
        thing.Flags6 &= ~Mf6Intrymove;
        return true;
    }

    public static bool SpechitCrossed(int oldSide, int newSide) => oldSide != newSide;

    public static bool ShouldActivateCross(bool predicting, bool hasPlayer) => !predicting || hasPlayer;

    public static bool TeleportLineSpecial(int special) =>
        special is 39 or 40 or 243;

    public static bool UseMonsterCross(int flags2) => (flags2 & Mf2Mcross) != 0;

    public static bool UsePlayerCross(int flags2) => (flags2 & Mf2Pcross) != 0;
}
