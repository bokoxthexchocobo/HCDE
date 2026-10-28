namespace HCDE.Playsim;

/// <summary>`SF_SetCamera` / `SF_ClearCamera` from <c>fragglescript/t_func.cpp</c> (1391–1443); exclusive camera chunk **908**.</summary>
public static partial class VmFScriptParser
{
    public static void SfSetCamera(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var cam = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (cam == null)
        {
            ctx.ScriptError("invalid location object for camera\n");
            return;
        }
        cam.FraggleScriptSpecialF1 = cam.Angle.ToDegrees();
        cam.FraggleScriptSpecialF2 = cam.Z.ToDouble();

        var yaw = ctx.TArgc < 2 ? cam.Angle.ToDegrees() : ctx.FloatValue(ctx.TArgv[1]);
        var floorZ = ctx.Simulation.Level.Sectors[cam.SectorIndex].FloorHeight;
        var z = ctx.TArgc < 3 ? floorZ + 41 : ctx.FloatValue(ctx.TArgv[2]);
        cam.Angle = BamAngle.FromDegrees(yaw);
        cam.Z = Fixed.FromDouble(z);
        if (ctx.TArgc >= 4)
            cam.PitchDegrees = Math.Clamp(ctx.FloatValue(ctx.TArgv[3]), -50, 50) * (20.0 / 32.0);

        ctx.Simulation.CameraThingId = (int)cam.Id;
    }

    public static void SfClearCamera(ParserContext ctx)
    {
        if (ctx.Simulation == null)
            return;
        var playerMo = ctx.Simulation.Actors.OfType<PlayerPawn>().FirstOrDefault()
            ?? ctx.Simulation.Actors.FirstOrDefault();
        if (playerMo == null)
            return;
        if (ctx.Simulation.CameraThingId != 0)
        {
            var cam = ctx.Simulation.Actors.FirstOrDefault(a => a.Id == (uint)ctx.Simulation.CameraThingId);
            if (cam != null)
            {
                cam.Angle = BamAngle.FromDegrees(cam.FraggleScriptSpecialF1);
                cam.Z = Fixed.FromDouble(cam.FraggleScriptSpecialF2);
            }
        }
        ctx.Simulation.CameraThingId = (int)playerMo.Id;
    }
}
