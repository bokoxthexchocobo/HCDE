namespace HCDE.Playsim;

/// <summary><c>P_GetLineSide</c>, <c>P_ClipLineToPortal</c>, and portal translation from <c>portal.cpp</c> (391–524).</summary>
public static class VmPortalClipTranslate
{
    public const double EqualEpsilon = 1.0 / 65536;
    public const int PortfPolyobj = 1;
    public const int PorgFloor = 1;
    public const int PorgCeiling = 2;

    public static int GetLineSide(double posX, double posY, double v1X, double v1Y, double deltaX, double deltaY)
    {
        var v = (posY - v1Y) * deltaX + (v1X - posX) * deltaY;
        if (v < -EqualEpsilon)
            return -1;
        if (v > EqualEpsilon)
            return 1;
        return 0;
    }

    public static bool ClipLineToPortal(
        double lineV1X, double lineV1Y, double lineV2X, double lineV2Y,
        double portalV1X, double portalV1Y, double portalV2X, double portalV2Y,
        double portalDeltaX, double portalDeltaY,
        double viewX, double viewY)
    {
        var behind1 = GetLineSide(lineV1X, lineV1Y, portalV1X, portalV1Y, portalDeltaX, portalDeltaY);
        var behind2 = GetLineSide(lineV2X, lineV2Y, portalV1X, portalV1Y, portalDeltaX, portalDeltaY);

        if (behind1 == 0 && behind2 == 0)
            return true;
        if (behind2 == 0)
            behind2 = behind1;
        else if (behind1 == 0)
            behind1 = behind2;

        if (behind1 > 0 && behind2 > 0)
            return true;
        if (behind1 < 0 && behind2 < 0)
            return false;

        var lineDx = lineV2X - lineV1X;
        var lineDy = lineV2Y - lineV1Y;
        var viewside = GetLineSide(viewX, viewY, lineV1X, lineV1Y, lineDx, lineDy);
        var p1side = GetLineSide(portalV1X, portalV1Y, lineV1X, lineV1Y, lineDx, lineDy);
        var p2side = GetLineSide(portalV2X, portalV2Y, lineV1X, lineV1Y, lineDx, lineDy);
        if (p1side == 0)
            p1side = p2side;
        else if (p2side == 0)
            p2side = p1side;
        return p1side == p2side && viewside != p1side;
    }

    public sealed class TranslatePortalContext
    {
        public bool HasPortal { get; set; }
        public int Flags { get; set; }
        public double SinRot { get; set; }
        public double CosRot { get; set; } = 1;
        public double AngleDiffDegrees { get; set; }
        public double SrcV1X { get; set; }
        public double SrcV1Y { get; set; }
        public double DestV2X { get; set; }
        public double DestV2Y { get; set; }
        public int Alignment { get; set; }
        public double SrcFloorZ { get; set; }
        public double DstFloorZ { get; set; }
        public double SrcCeilingZ { get; set; }
        public double DstCeilingZ { get; set; }
        public Action<VmPortalBuildBlockmap.PortalRotationState>? RefreshPolyRotation { get; init; }
    }

    public static void TranslatePortalXy(TranslatePortalContext ctx, ref double x, ref double y)
    {
        if (!ctx.HasPortal)
            return;
        if ((ctx.Flags & PortfPolyobj) != 0)
        {
            var rot = new VmPortalBuildBlockmap.PortalRotationState
            {
                SinRot = ctx.SinRot,
                CosRot = ctx.CosRot,
                AngleDiffDegrees = ctx.AngleDiffDegrees,
                Flags = ctx.Flags,
            };
            ctx.RefreshPolyRotation?.Invoke(rot);
            ctx.SinRot = rot.SinRot;
            ctx.CosRot = rot.CosRot;
        }

        var nposx = x - ctx.SrcV1X;
        var nposy = y - ctx.SrcV1Y;
        var tx = nposx * ctx.CosRot - nposy * ctx.SinRot;
        var ty = nposy * ctx.CosRot + nposx * ctx.SinRot;
        x = tx + ctx.DestV2X;
        y = ty + ctx.DestV2Y;
    }

    public static void TranslatePortalVxVy(TranslatePortalContext ctx, ref double velx, ref double vely)
    {
        if (!ctx.HasPortal)
            return;
        if ((ctx.Flags & PortfPolyobj) != 0)
        {
            var rot = new VmPortalBuildBlockmap.PortalRotationState { SinRot = ctx.SinRot, CosRot = ctx.CosRot };
            ctx.RefreshPolyRotation?.Invoke(rot);
            ctx.SinRot = rot.SinRot;
            ctx.CosRot = rot.CosRot;
        }
        var ox = velx;
        var oy = vely;
        velx = ox * ctx.CosRot - oy * ctx.SinRot;
        vely = oy * ctx.CosRot + ox * ctx.SinRot;
    }

    public static double TranslatePortalAngle(TranslatePortalContext ctx, double angleDegrees)
    {
        if (!ctx.HasPortal)
            return angleDegrees;
        if ((ctx.Flags & PortfPolyobj) != 0)
        {
            var rot = new VmPortalBuildBlockmap.PortalRotationState { SinRot = ctx.SinRot, CosRot = ctx.CosRot, AngleDiffDegrees = ctx.AngleDiffDegrees };
            ctx.RefreshPolyRotation?.Invoke(rot);
            ctx.AngleDiffDegrees = rot.AngleDiffDegrees;
        }
        var a = angleDegrees + ctx.AngleDiffDegrees;
        while (a < 0)
            a += 360;
        while (a >= 360)
            a -= 360;
        return a;
    }

    public static void TranslatePortalZ(TranslatePortalContext ctx, ref double z)
    {
        switch (ctx.Alignment)
        {
            case PorgFloor:
                z = z - ctx.SrcFloorZ + ctx.DstFloorZ;
                break;
            case PorgCeiling:
                z = z - ctx.SrcCeilingZ + ctx.DstCeilingZ;
                break;
        }
    }
}
