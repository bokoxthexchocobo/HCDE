namespace HCDE.Playsim;

/// <summary>Camera, sector sounds, and floor height/move from <c>t_func.cpp</c> (1381–1586).</summary>
public static partial class VmFScriptParser
{
    static IEnumerable<int> SectorsForTag(ParserContext ctx, int tag)
    {
        if (ctx.FraggleThinker != null)
            return ctx.FraggleThinker.SectorsByTag(tag);
        if (ctx.Simulation == null)
            return Array.Empty<int>();
        var list = new List<int>();
        for (var i = 0; i < ctx.Simulation.Level.Sectors.Count; i++)
            if (ctx.Simulation.Level.Sectors[i].MatchesTag(tag))
                list.Add(i);
        return list;
    }

    static bool SectorFloorActive(AuthoritySimulation sim, int sectorIndex) =>
        sim.Motions.Any(m => m.SectorIndex == sectorIndex && !m.MovesCeiling && !m.Completed);

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

    public static void SfStartSound(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2))
            return;
        var mo = AsActor(ctx.ActorValue(ctx.TArgv[0]));
        if (mo == null)
            return;
        var name = ctx.StringValue(ctx.TArgv[1]);
        ActorSoundActions.StartSound(mo, name, channel: 0);
    }

    public static void SfFloorHeight(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 1) || ctx.Simulation == null)
            return;
        var sim = ctx.Simulation;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        if (ctx.TArgc > 1)
        {
            var dest = ctx.FloatValue(ctx.TArgv[1]);
            var crush = ctx.TArgc >= 3 ? ctx.IntValue(ctx.TArgv[2]) : 0;
            var returnVal = 1.0;
            foreach (var sectorIndex in SectorsForTag(ctx, tag))
            {
                if (SectorFloorActive(sim, sectorIndex))
                    continue;
                var current = sim.FloorOf(sectorIndex);
                var crushDamage = crush != 0 ? 10 : -1;
                if (crush != 0 && dest < current && TryCrushLoweringFloor(sim, sectorIndex, dest, crushDamage))
                    returnVal = 0;
                else if (!SectorFloorActive(sim, sectorIndex))
                    sim.Floors[sectorIndex] = Math.Min(sim.Ceilings[sectorIndex], dest);
            }
            ctx.TReturn.Type = SvtFixed;
            ctx.TReturn.FixedValue = (long)(returnVal * 65536.0);
            return;
        }

        var sectors = SectorsForTag(ctx, tag).ToList();
        if (sectors.Count == 0)
        {
            ctx.ScriptError($"sector not found with tagnum {tag}\n");
            return;
        }
        ReturnFixed(ctx, sim.FloorOf(sectors[0]));
    }

    public static void SfMoveFloor(ParserContext ctx)
    {
        if (!CheckArgs(ctx, 2) || ctx.Simulation == null)
            return;
        var tag = ctx.IntValue(ctx.TArgv[0]);
        var dest = ctx.FloatValue(ctx.TArgv[1]);
        var speed = ctx.TArgc > 2 ? ctx.FloatValue(ctx.TArgv[2]) : 1.0;
        var crush = ctx.TArgc > 3 ? ctx.IntValue(ctx.TArgv[3]) : -1;
        var crushArg = crush >= 0 ? crush + 1 : 0;
        var speedArg = (int)Math.Clamp(speed * 8.0, 1, int.MaxValue / 8);
        LineSpecials.ExecuteFloorSpecial(ctx.Simulation, 279, tag, speedArg, (int)dest, crushArg, 0);
    }

    static bool TryCrushLoweringFloor(AuthoritySimulation sim, int sectorIndex, double dest, int crushDamage)
    {
        var actors = sim.Actors.Where(a => !a.Destroyed && a is not ProjectileActor
            && ActorPhysics.TouchesSector(sim, a, sectorIndex)).ToArray();
        foreach (var actor in actors)
        {
            var support = ActorPhysics.SectorSupportFloor(sim, actor);
            if (support + actor.Height.ToDouble() > dest + 0.01)
            {
                if (crushDamage > 0)
                {
                    actor.MarkMoverCrush(0);
                    ActorDamage.Apply(actor, crushDamage, damageType: "Crush");
                    ActorSoundActions.CrushPain(actor);
                }
                return true;
            }
        }
        return false;
    }

    static void ReturnFixed(ParserContext ctx, double value)
    {
        ctx.TReturn.Type = SvtFixed;
        ctx.TReturn.FixedValue = (long)(value * 65536.0);
    }
}
