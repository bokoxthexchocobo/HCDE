namespace HCDE.Playsim;

/// <summary><c>FParser::SF_MoveCamera</c> from <c>fragglescript/t_func.cpp</c> (2694–2766).</summary>
public static partial class VmFScriptParser
{
    public static void SfMoveCamera(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 6))
            return;
        var cam = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        var target = AsActor(ctx.ActorValue(ctx.TArgv[1]));
        if (cam == null || target == null)
        {
            ctx.ScriptError("invalid target for camera\n");
            return;
        }

        var targetHeight = ctx.FloatValue(ctx.TArgv[2]);
        var moveSpeed = ctx.FloatValue(ctx.TArgv[3]);
        var camX = cam.X.ToDouble();
        var camY = cam.Y.ToDouble();
        var camZ = cam.Z.ToDouble();
        var targX = target.X.ToDouble();
        var targY = target.Y.ToDouble();
        var targZ = targetHeight;
        var dx = targX - camX;
        var dy = targY - camY;
        var dz = targZ - camZ;
        var movelen = Math.Sqrt(dx * dx + dy * dy + dz * dz);

        var finishedMove = false;
        var finishedAngle = false;
        cam.Radius = Fixed.FromDouble(1.0 / 8192);
        cam.Height = Fixed.FromDouble(1.0 / 8192);

        if (movelen > 1e-8)
        {
            if (moveSpeed >= movelen)
            {
                cam.X = Fixed.FromDouble(targX);
                cam.Y = Fixed.FromDouble(targY);
                cam.Z = Fixed.FromDouble(targZ);
                finishedMove = true;
            }
            else
            {
                var scale = moveSpeed / movelen;
                cam.X = Fixed.FromDouble(camX + dx * scale);
                cam.Y = Fixed.FromDouble(camY + dy * scale);
                cam.Z = Fixed.FromDouble(camZ + dz * scale);
            }
        }
        else
            finishedMove = true;

        var targetAngle = BamAngle.FromDegrees(ctx.FloatValue(ctx.TArgv[4]));
        var angleSpeed = BamAngle.FromDegrees(ctx.FloatValue(ctx.TArgv[5]));
        if (cam.Angle.Raw != targetAngle.Raw)
        {
            var diff = DeltaAngle(cam.Angle, targetAngle);
            if (moveSpeed > 0 && angleSpeed.Raw == 0)
            {
                if (!finishedMove && movelen > 1e-8)
                {
                    var step = diff.ToDegrees() * moveSpeed / movelen;
                    cam.Angle = BamAngle.FromDegrees(cam.Angle.ToDegrees() + step);
                }
                else
                    finishedAngle = true;
            }
            else
            {
                var next = diff.ToDegrees() > 0
                    ? cam.Angle.ToDegrees() + angleSpeed.ToDegrees()
                    : cam.Angle.ToDegrees() - angleSpeed.ToDegrees();
                var anglenow = BamAngle.FromDegrees(next);
                var diff2 = DeltaAngle(anglenow, targetAngle);
                if (diff.ToDegrees() * diff2.ToDegrees() <= 0)
                {
                    cam.Angle = targetAngle;
                    finishedAngle = true;
                }
                else
                    cam.Angle = anglenow;
            }
        }
        else
            finishedAngle = true;

        ctx.TReturn.Type = SvtInt;
        ctx.TReturn.IntValue = finishedMove && finishedAngle ? 0 : 1;
    }

    static BamAngle DeltaAngle(BamAngle from, BamAngle to)
    {
        var diff = to.ToDegrees() - from.ToDegrees();
        while (diff > 180) diff -= 360;
        while (diff < -180) diff += 360;
        return BamAngle.FromDegrees(diff);
    }
}
