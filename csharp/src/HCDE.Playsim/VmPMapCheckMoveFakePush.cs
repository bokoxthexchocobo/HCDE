namespace HCDE.Playsim;

/// <summary><c>P_CheckMove</c>, <c>P_FakeZMovement</c>, and <c>CheckForPushSpecial</c> from <c>p_map.cpp</c>.</summary>
public static class VmPMapCheckMoveFakePush
{
    public const int MfPickup = 1 << 0;
    public const int MfNoclip = 1 << 1;
    public const int MfTeleport = 1 << 2;
    public const int MfFloat = 1 << 3;
    public const int MfMissile = 1 << 4;
    public const int MfDropoff = 1 << 5;
    public const int MfSkullfly = 1 << 6;
    public const int MfNogravity = 1 << 7;
    public const int Mf3Floorhugger = 1;
    public const int Mf3Ceilinghugger = 1 << 1;
    public const int Mf2Fly = 1;
    public const int Mf2Cantleavefloorpic = 1 << 1;
    public const int Mf2Pushwall = 1 << 2;
    public const int Mf2Impact = 1 << 3;
    public const int Mf5Nodropoff = 1;
    public const int Mf6Stepmissile = 1;
    public const int Mf6Notrigger = 1 << 1;
    public const int PcmNoactors = 1;
    public const int PcmNolines = 1 << 1;
    public const int PcmDropoff = 1 << 2;
    public const int Compatf2Pushwindow = 1;
    public const int Ml3dmidtex = 1;
    public const int FfSolid = 1;
    public const int FfExists = 1 << 1;
    public const int Level2Missilesactivateimpact = 1;

    public sealed class CheckPositionTm
    {
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public double DropoffZ { get; set; }
        public object? FloorPic { get; set; }
        public object? BlockingMobj { get; set; }
        public object? BlockingLine { get; set; }
    }

    public sealed class CheckMoveActor
    {
        public int Flags { get; set; }
        public int Flags2 { get; set; }
        public int Flags3 { get; set; }
        public int Flags5 { get; set; }
        public int Flags6 { get; set; }
        public double Z { get; set; }
        public double Height { get; set; }
        public double Top => Z + Height;
        public double MaxStepHeight { get; set; }
        public double MaxDropOffHeight { get; set; }
        public object? FloorPic { get; set; }
        public FloatTargetSample? Target { get; set; }
        public object? FloorPicCheck { get; set; }
        public double PosX { get; set; }
        public double PosY { get; set; }
        public double VelZ { get; set; }
        public double FloatSpeed { get; set; }
        public double FloorZ { get; set; }
        public double CeilingZ { get; set; }
        public bool IsNoClip2 { get; set; }
        public int MapTime { get; set; }
        public object? Level { get; set; }
    }

    public sealed class FloatTargetSample
    {
        public double X { get; init; }
        public double Y { get; init; }
        public double CenterZ { get; init; }
    }

    public sealed class CheckMoveState
    {
        public required CheckMoveActor Actor { get; init; }
        public required CheckPositionTm Tm { get; init; }
        public int Flags { get; init; }
        public Func<bool>? CheckPosition { get; init; }
        public Func<CheckMoveActor, bool>? TestMobjZ { get; init; }
        public Action<double>? SetZ { get; init; }
    }

    public static bool CheckMove(CheckMoveState s)
    {
        var thing = s.Actor;
        var tm = s.Tm;
        var newz = thing.Z;
        var f1 = thing.Flags & MfPickup;
        thing.Flags &= ~MfPickup;
        var res = s.CheckPosition?.Invoke() == true;
        thing.Flags |= f1;
        if (!res)
        {
            if (!ShouldContinueAfterFailedPosition(s.Flags))
                return false;
            if ((s.Flags & PcmNoactors) == 0 && tm.BlockingMobj != null)
                return false;
            if ((s.Flags & PcmNolines) == 0 && tm.BlockingLine != null)
                return false;
        }

        newz = ResolveNewZ(thing, tm, newz);

        if ((thing.Flags & MfNoclip) != 0)
            return true;

        if (tm.CeilingZ - tm.FloorZ < thing.Height)
            return false;

        if (CeilingBlocksMove(thing, tm, newz))
            return false;

        if ((thing.Flags2 & Mf2Fly) != 0 && (thing.Flags & MfNogravity) != 0 && thing.Top > tm.CeilingZ)
            return false;

        if ((thing.Flags & MfTeleport) == 0 && FloorStepGate(thing))
        {
            if (tm.FloorZ - newz > thing.MaxStepHeight)
                return false;
            if ((thing.Flags & MfMissile) != 0 && (thing.Flags6 & Mf6Stepmissile) == 0 && tm.FloorZ > newz
                && (thing.Flags3 & Mf3Floorhugger) == 0)
                return false;
            if (newz < tm.FloorZ)
            {
                var savedz = thing.Z;
                newz = tm.FloorZ;
                thing.Z = newz;
                s.SetZ?.Invoke(newz);
                var good = s.TestMobjZ?.Invoke(thing) == true;
                thing.Z = savedz;
                s.SetZ?.Invoke(savedz);
                if (!good)
                    return false;
            }
            else if ((s.Flags & PcmDropoff) != 0 && DropoffBlocks(thing, newz, tm.DropoffZ))
                return false;
        }

        if ((thing.Flags2 & Mf2Cantleavefloorpic) != 0 && FloorPicChanged(thing, tm, newz))
            return false;

        return true;
    }

    public static bool ShouldContinueAfterFailedPosition(int flags) =>
        flags != 0 && (flags & (PcmNoactors | PcmNolines)) != (PcmNoactors | PcmNolines);

    public static double ResolveNewZ(CheckMoveActor thing, CheckPositionTm tm, double newz)
    {
        if ((thing.Flags3 & Mf3Floorhugger) != 0)
            return tm.FloorZ;
        if ((thing.Flags3 & Mf3Ceilinghugger) != 0)
            return tm.CeilingZ - thing.Height;
        return newz;
    }

    public static bool CeilingBlocksMove(CheckMoveActor thing, CheckPositionTm tm, double newz) =>
        (thing.Flags & MfTeleport) == 0
        && tm.CeilingZ - newz < thing.Height
        && (thing.Flags3 & Mf3Ceilinghugger) == 0
        && ((thing.Flags2 & Mf2Fly) == 0 || (thing.Flags & MfNogravity) == 0);

    public static bool FloorStepGate(CheckMoveActor thing) =>
        (thing.Flags3 & Mf3Floorhugger) == 0 || (thing.Flags5 & Mf5Nodropoff) != 0;

    public static bool DropoffBlocks(CheckMoveActor thing, double newz, double dropoffz) =>
        (thing.Flags & (MfFloat | MfDropoff)) == 0 && newz - dropoffz > thing.MaxDropOffHeight;

    public static bool FloorPicChanged(CheckMoveActor thing, CheckPositionTm tm, double newz) =>
        !ReferenceEquals(tm.FloorPic, thing.FloorPicCheck) || tm.FloorZ - newz != 0;

    public static void FakeZMovement(CheckMoveActor mo)
    {
        mo.Z += mo.VelZ;
        if ((mo.Flags & MfFloat) != 0 && mo.Target != null)
        {
            if ((mo.Flags & MfSkullfly) == 0 && (mo.Flags & MfFloat) != 0)
            {
                var dist = Distance2D(mo, mo.Target);
                var delta = TargetCenter(mo.Target) - mo.Z;
                if (delta < 0 && dist < -(delta * 3))
                    mo.Z -= mo.FloatSpeed;
                else if (delta > 0 && dist < delta * 3)
                    mo.Z += mo.FloatSpeed;
            }
        }

        if (mo.Target != null && (mo.Flags & MfNogravity) != 0 && mo.Z > mo.FloorZ && !mo.IsNoClip2)
            mo.Z += Math.Sin(mo.MapTime * 4.5 * Math.PI / 180.0);

        if (mo.Z <= mo.FloorZ)
            mo.Z = mo.FloorZ;
        if (mo.Top > mo.CeilingZ)
            mo.Z = mo.CeilingZ - mo.Height;
    }

    public static double Distance2D(CheckMoveActor mo, FloatTargetSample target) =>
        Math.Sqrt(Math.Pow(target.X - mo.PosX, 2) + Math.Pow(target.Y - mo.PosY, 2));

    public static double TargetCenter(FloatTargetSample target) => target.CenterZ;

    public sealed class PushLineContext
    {
        public int Special { get; init; }
        public int Side { get; init; }
        public int ThingFlags6 { get; init; }
        public int ThingFlags2 { get; init; }
        public int ThingFlags { get; init; }
        public int CompatFlags2 { get; init; }
        public object? BackSector { get; init; }
        public int LineFlags { get; init; }
        public int LevelFlags2 { get; init; }
        public object? Target { get; init; }
        public double ThingZ { get; init; }
        public double ThingTop { get; init; }
        public bool WindowCheck { get; init; }
        public Func<bool>? MidtexBlocks { get; init; }
        public Func<bool>? FfloorBlocks { get; init; }
        public Action<int>? ActivatePush { get; init; }
        public Action<int>? ActivateImpact { get; init; }
    }

    public static bool CheckForPushSpecial(PushLineContext ctx)
    {
        if (ctx.Special == 0 || (ctx.ThingFlags6 & Mf6Notrigger) != 0)
            return false;

        if (ctx.WindowCheck && PushWindowAllowsPass(ctx))
            return false;

        if ((ctx.ThingFlags2 & Mf2Pushwall) != 0)
        {
            ctx.ActivatePush?.Invoke(ctx.Side);
            return true;
        }

        if ((ctx.ThingFlags2 & Mf2Impact) != 0)
        {
            if ((ctx.LevelFlags2 & Level2Missilesactivateimpact) != 0
                || (ctx.ThingFlags & MfMissile) == 0
                || ctx.Target == null)
                ctx.ActivateImpact?.Invoke(ctx.Side);
            else
                ctx.ActivateImpact?.Invoke(ctx.Side);
            return true;
        }

        return false;
    }

    public static bool PushWindowAllowsPass(PushLineContext ctx)
    {
        if ((ctx.CompatFlags2 & Compatf2Pushwindow) != 0 || ctx.BackSector == null)
            return false;
        if (!ctx.WindowCheck)
            return false;
        if (ctx.MidtexBlocks?.Invoke() == true)
            return false;
        if (ctx.FfloorBlocks?.Invoke() == true)
            return false;
        return true;
    }
}
