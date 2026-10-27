namespace HCDE.Playsim;

/// <summary>
/// <c>SF_SetWeapon</c> tail, <c>SF_MoveCamera</c>, and act/value SF_* head from <c>fragglescript/t_func.cpp</c> (**2685–2834**); chunk **937**.
/// </summary>
public static class VmFScriptParserMoveCamera937Slice
{
    public const double CameraColliderScale = 1.0 / 8192.0;

    public static bool SetWeaponRequiresTwoArgs(int argc) => argc >= 2;

    public static bool MoveCameraRequiresSixArgs(int argc) => argc >= 6;

    public static bool MoveCameraRejectInvalidActors(object? camera, object? target) =>
        camera == null || target == null;

    public static double VectorLength(double dx, double dy, double dz) =>
        Math.Sqrt(dx * dx + dy * dy + dz * dz);

    public static (double X, double Y, double Z) MoveToward(
        double cx,
        double cy,
        double cz,
        double tx,
        double ty,
        double tz,
        double speed)
    {
        var dx = tx - cx;
        var dy = ty - cy;
        var dz = tz - cz;
        var len = VectorLength(dx, dy, dz);
        if (len == 0 || speed >= len)
            return (tx, ty, tz);
        var scale = speed / len;
        return (cx + dx * scale, cy + dy * scale, cz + dz * scale);
    }

    public static bool MoveFinished(double speed, double distance) =>
        distance == 0 || speed >= distance;

    public static double NormalizeDegrees(double degrees)
    {
        var d = degrees % 360.0;
        return d < 0 ? d + 360.0 : d;
    }

    public static double DeltaAngleDegrees(double fromDeg, double toDeg)
    {
        var diff = NormalizeDegrees(toDeg - fromDeg);
        if (diff > 180.0)
            diff -= 360.0;
        return diff;
    }

    public static (double YawDegrees, bool Finished) StepCameraYaw(
        double currentDeg,
        double targetDeg,
        double moveSpeed,
        double angleSpeedDeg,
        double moveDistance,
        bool moveFinished)
    {
        var diff = DeltaAngleDegrees(currentDeg, targetDeg);
        if (diff == 0)
            return (targetDeg, true);

        if (moveSpeed > 0 && angleSpeedDeg == 0)
        {
            if (!moveFinished)
            {
                var yawStep = diff * (moveSpeed / Math.Max(moveDistance, double.Epsilon));
                return (NormalizeDegrees(currentDeg + yawStep), false);
            }
            return (targetDeg, true);
        }

        var step = diff > 0 ? angleSpeedDeg : -angleSpeedDeg;
        var next = NormalizeDegrees(currentDeg + step);
        var diffAfter = DeltaAngleDegrees(next, targetDeg);
        if (diff * diffAfter <= 0)
            return (targetDeg, true);
        return (next, false);
    }

    public static int MoveCameraReturn(bool finishedMove, bool finishedAngle) =>
        finishedMove && finishedAngle ? 0 : 1;

    public static object? AwakenActor(int argc, object? arg0, object? trigger) =>
        argc > 0 ? arg0 : trigger;

    public static bool AmbientSoundRequiresArgs(int argc) => argc >= 1;

    public static void MarkSecretExit(AuthoritySimulation? sim) => sim?.MarkExited(secret: true);

    public static bool MobjValueRequiresArgs(int argc) => argc >= 1;

    public readonly record struct MoveCameraInvoke(
        double CamX,
        double CamY,
        double CamZ,
        double YawDegrees,
        int ReturnCode,
        double ColliderScale);

    public static MoveCameraInvoke? BuildMoveCamera(
        object? camera,
        object? target,
        double targetHeight,
        double camX,
        double camY,
        double camZ,
        double targetX,
        double targetY,
        double moveSpeed,
        double currentYawDeg,
        double targetYawDeg,
        double angleSpeedDeg)
    {
        if (MoveCameraRejectInvalidActors(camera, target))
            return null;

        var targZ = targetHeight;
        var dx = targetX - camX;
        var dy = targetY - camY;
        var dz = targZ - camZ;
        var dist = VectorLength(dx, dy, dz);

        var finishedMove = MoveFinished(moveSpeed, dist);
        var (nx, ny, nz) = MoveToward(camX, camY, camZ, targetX, targetY, targZ, moveSpeed);

        var normalizedTargetYaw = NormalizeDegrees(targetYawDeg);
        var (yaw, finishedAngle) = StepCameraYaw(
            currentYawDeg,
            normalizedTargetYaw,
            moveSpeed,
            angleSpeedDeg,
            dist,
            finishedMove);

        if (NormalizeDegrees(currentYawDeg) == normalizedTargetYaw)
            finishedAngle = true;

        return new(
            nx,
            ny,
            nz,
            yaw,
            MoveCameraReturn(finishedMove, finishedAngle),
            CameraColliderScale);
    }
}
