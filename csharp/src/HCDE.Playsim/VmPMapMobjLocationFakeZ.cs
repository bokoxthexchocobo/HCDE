namespace HCDE.Playsim;

/// <summary><c>P_TestMobjLocation</c>, <c>P_FakeZMovement</c>, <c>CheckForPushSpecial</c>, and <c>P_TryMove</c> tail (<c>p_map.cpp</c>).</summary>
public static class VmPMapMobjLocationFakeZ
{
    public const int MfPickup = 1;
    public const int MfFloat = 1 << 1;
    public const int MfSkullfly = 1 << 2;
    public const int MfNogravity = 1 << 3;
    public const int MfMissile = 1 << 4;
    public const int Mf2Pushwall = 1;
    public const int Mf2Impact = 1 << 1;
    public const int Mf6Notrigger = 1 << 2;
    public const int Compatf2Pushwindow = 1;

    public sealed class MobjProbe
    {
        public int Flags { get; set; }
        public double Z { get; set; }
        public double Top { get; set; }
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double VelZ { get; set; }
        public bool HasTarget { get; set; }
        public double TargetCenter { get; set; }
        public double FloatSpeed { get; set; }
        public bool InFloat { get; set; }
        public bool HasPlayer { get; set; }
        public bool NoClip2 { get; set; }
        public int MapTime { get; set; }
        public double Dist2d { get; set; }
        public double Height { get; set; }
    }

    public sealed class PushLineContext
    {
        public bool HasSpecial { get; init; }
        public bool NoTrigger { get; init; }
        public bool PushWindowCompat { get; init; }
        public bool HasBackSector { get; init; }
        public double FrontCeil { get; init; }
        public double FrontFloor { get; init; }
        public double BackCeil { get; init; }
        public double BackFloor { get; init; }
        public bool Midtex3d { get; init; }
        public double MidtexTop { get; init; }
        public double MidtexBot { get; init; }
        public IEnumerable<(double bot, double top, bool solid, bool exists)>? Back3dFloors { get; init; }
        public bool PushWall { get; init; }
        public bool Impact { get; init; }
        public bool Missile { get; init; }
        public bool LevelMissilesActivateImpact { get; init; }
        public bool MissileNoTarget { get; init; }
        public Action<int>? ActivatePush { get; init; }
        public Action<int, bool>? ActivateImpact { get; init; }
    }

    public static bool TestMobjLocation(MobjProbe m, Func<MobjProbe, bool> checkPosition)
    {
        var saved = m.Flags;
        m.Flags &= ~MfPickup;
        if (checkPosition(m))
        {
            m.Flags = saved;
            return m.Z >= m.FloorZ && m.Top <= m.CeilingZ;
        }

        m.Flags = saved;
        return false;
    }

    public static void FakeZMovement(MobjProbe m)
    {
        m.Z += m.VelZ;
        if ((m.Flags & MfFloat) != 0 && m.HasTarget)
        {
            if ((m.Flags & MfSkullfly) == 0 && !m.InFloat)
            {
                var delta = m.TargetCenter - m.Z;
                if (delta < 0 && m.Dist2d < -delta * 3)
                    m.Z -= m.FloatSpeed;
                else if (delta > 0 && m.Dist2d < delta * 3)
                    m.Z += m.FloatSpeed;
            }
        }

        if (m.HasPlayer && (m.Flags & MfNogravity) != 0 && m.Z > m.FloorZ && !m.NoClip2)
            m.Z += Math.Sin(m.MapTime * 4.5 * Math.PI / 180.0);

        if (m.Z <= m.FloorZ)
            m.Z = m.FloorZ;
        if (m.Top > m.CeilingZ)
            m.Z = m.CeilingZ - m.Height;
    }

    public static bool WindowAllowsPush(PushLineContext ctx, double top, double z) =>
        ctx.FrontCeil >= top && ctx.BackCeil >= top && ctx.FrontFloor <= z && ctx.BackFloor <= z;

    public static bool MidtexBlocks(double bot, double top, double actorTop, double actorZ) =>
        bot < actorTop && top > actorZ;

    public static bool FfloorBlocks(double bot, double top, double actorTop, double actorZ, bool solid, bool exists) =>
        solid && exists && bot < actorTop && top > actorZ;

    public static void CheckForPushSpecial(PushLineContext ctx, int side, double actorTop, double actorZ)
    {
        if (!ctx.HasSpecial || ctx.NoTrigger)
            return;

        if (ctx.PushWindowCompat && ctx.HasBackSector)
        {
            if (WindowAllowsPush(ctx, actorTop, actorZ))
            {
                if (ctx.Midtex3d && MidtexBlocks(ctx.MidtexBot, ctx.MidtexTop, actorTop, actorZ))
                    goto blocking;
                foreach (var ff in ctx.Back3dFloors ?? Array.Empty<(double, double, bool, bool)>())
                {
                    if (FfloorBlocks(ff.bot, ff.top, actorTop, actorZ, ff.solid, ff.exists))
                        goto blocking;
                }

                return;
            }
        }

    blocking:
        if (ctx.PushWall)
            ctx.ActivatePush?.Invoke(side);
        else if (ctx.Impact)
        {
            var useActivator = ctx.LevelMissilesActivateImpact || !ctx.Missile || ctx.MissileNoTarget;
            ctx.ActivateImpact?.Invoke(side, useActivator);
        }
    }

    public static bool PortalCrossSide(int oldSide, int newSide) => oldSide == 0 && newSide == 1;

    public static bool PortalGroupMatches(int lineGroup, int thingGroup) => lineGroup == thingGroup;

    public static bool ShouldSkipPushWhenWindowOpen(PushLineContext ctx, double actorTop, double actorZ) =>
        ctx.PushWindowCompat && ctx.HasBackSector && WindowAllowsPush(ctx, actorTop, actorZ)
        && !MidtexBlocks(ctx.MidtexBot, ctx.MidtexTop, actorTop, actorZ);

    public static bool ImpactUsesTarget(bool missile, bool noTarget, bool levelFlag) =>
        missile && !noTarget && !levelFlag;

    public static bool TryMoveAfterPortals(VmPMapTryMove.TryMoveState st, double oldz)
    {
        var thing = st.Actor;
        var tm = st.Tm;
        if (st.ResolvePortalCrossing?.Invoke(st) == false)
        {
            thing.Z = oldz;
            thing.Flags6 &= ~VmPMapTryMove.Mf6Intrymove;
            return false;
        }

        st.CommitMove?.Invoke(thing);

        if ((thing.Flags2 & VmPMapTryMove.Mf2Floorclip) != 0)
            st.AdjustFloorClip?.Invoke(thing);

        if ((thing.Flags & (VmPMapTryMove.MfTeleport | VmPMapTryMove.MfNoclip)) == 0)
        {
            while (st.Spechit.Count > 0)
            {
                var spec = st.Spechit[^1];
                st.Spechit.RemoveAt(st.Spechit.Count - 1);
                st.ActivateCrossLine?.Invoke(thing, spec, 0, 1);
            }
        }

        if (thing.HasPlayer && (thing.PlayerCheats & VmPMapTryMove.CfPredicting) != 0)
        {
            thing.Flags6 &= ~VmPMapTryMove.Mf6Intrymove;
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
        thing.Flags6 &= ~VmPMapTryMove.Mf6Intrymove;
        return true;
    }
}
