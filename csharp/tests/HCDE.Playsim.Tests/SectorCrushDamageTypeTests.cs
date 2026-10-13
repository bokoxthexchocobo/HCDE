using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorCrushDamageTypeTests
{
    [Theory]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("none")]
    [InlineData("NONE")]
    public void CrushPainNoSoundSentinelDoesNotQueuePlayback(string sound)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 1 }] });
        var actor = sim.Players.Single();
        actor.ActorSounds = new Dictionary<ActorSoundType, string>(actor.ActorSounds)
        { [ActorSoundType.CrushPain] = sound };
        ActorSoundActions.CrushPain(actor);
        Assert.Empty(sim.DrainSoundRequests());
    }

    [Theory]
    [InlineData(false, "Crush")]
    [InlineData(true, "Crush")]
    [InlineData(false, "crush")]
    [InlineData(true, "crush")]
    [InlineData(false, "CRUSH")]
    [InlineData(true, "CRUSH")]
    public void TypedRiderCrushUsesDefaultGrindShootability(bool shootable, string damageType)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var carrier = sim.AddBot(0, 0); carrier.Brain = null; carrier.Height = Fixed.FromInt(16);
        var rider = sim.AddBot(0, 0); rider.Brain = null; rider.Z = Fixed.FromInt(16);
        rider.Shootable = shootable;
        rider.ActorSounds = new Dictionary<ActorSoundType, string>(rider.ActorSounds)
        { [ActorSoundType.CrushPain] = "custom/crush" };
        var health = rider.Health;
        ActorPhysics.CrushStandingRiders(sim, carrier, 10, 4, damageType);
        Assert.Equal(shootable ? health - 10 : health, rider.Health);
        Assert.Equal(shootable ? 1 : 0, sim.DrainSoundRequests().Count(sound => sound.Sound == "custom/crush"));
        Assert.Equal(shootable ? 4 : 0, rider.MoverCrushTic);
    }

    [Theory]
    [InlineData(MotionKind.Floor, true)]
    [InlineData(MotionKind.Floor, false)]
    [InlineData(MotionKind.Ceiling, true)]
    [InlineData(MotionKind.Ceiling, false)]
    public void NonShootableActorDoesNotObstructSectorCrunch(MotionKind kind, bool shootable)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 1 }] });
        var actor = sim.Players.Single(); actor.Shootable = shootable;
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = kind, Speed = 16, CrushDamage = -1,
            FloorDirection = 1, TargetFloor = 32, CeilingDirection = -1, TargetCeiling = 32 });
        LineSpecials.TickMotions(sim);
        Assert.Equal(kind == MotionKind.Floor ? shootable ? 0 : 16 : shootable ? 64 : 48,
            kind == MotionKind.Floor ? sim.FloorOf(0) : sim.CeilingOf(0));
        Assert.Equal(100, actor.Health);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void StationaryPinchUsesSolidityIndependentOfHealth(bool dead, bool solid)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 1 }] });
        for (var i = 0; i < 4; i++) sim.Tick();
        sim.DrainSoundRequests(); var actor = sim.Players.Single();
        if (dead) actor.Health = 0;
        actor.Corpse = false; actor.Solid = solid;
        actor.ActorSounds = new Dictionary<ActorSoundType, string>(actor.ActorSounds)
        { [ActorSoundType.CrushPain] = "custom/crush" };
        sim.Ceilings[0] = 40;
        ActorPhysics.FitToSector(sim, actor, carryFloor: false, sectorPinchCrush: true);
        Assert.Equal(solid ? 1 : 0, sim.DrainSoundRequests().Count(sound => sound.Sound == "custom/crush"));
        Assert.Equal(dead ? 0 : 90, actor.Health);
    }

    [Theory]
    [InlineData(MotionKind.Floor, false, false, true)]
    [InlineData(MotionKind.Ceiling, false, false, true)]
    [InlineData(MotionKind.Ceiling, true, false, false)]
    [InlineData(MotionKind.Floor, false, true, false)]
    public void CrusherPlaysConfiguredPainUnlessProtected(MotionKind kind, bool invulnerable, bool dormant, bool plays)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 1 }] });
        var actor = sim.Players.Single(); actor.Invulnerable = invulnerable; actor.Dormant = dormant;
        actor.ActorSounds = new Dictionary<ActorSoundType, string>(actor.ActorSounds)
        { [ActorSoundType.CrushPain] = "custom/crush" };
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = kind, Speed = 16, CrushDamage = 10,
            FloorDirection = 1, TargetFloor = 32, CeilingDirection = -1, TargetCeiling = 32 });
        LineSpecials.TickMotions(sim);
        var sounds = sim.DrainSoundRequests().Where(sound => sound.Sound == "custom/crush").ToArray();
        Assert.Equal(plays ? 1 : 0, sounds.Length);
        if (plays) { Assert.Equal(2, sounds[0].Channel); Assert.True(sounds[0].OnlyIfSoundNotPlaying); }
    }

    [Theory]
    [InlineData(MotionKind.Floor, 0, 100)]
    [InlineData(MotionKind.Floor, 0.5, 95)]
    [InlineData(MotionKind.Ceiling, 0, 100)]
    [InlineData(MotionKind.Ceiling, 0.5, 95)]
    public void SectorCrushersApplyCrushDamageFactor(MotionKind kind, double factor, int health)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 64 }],
            Things = [new LevelThing { Type = 1 }]
        });
        var actor = sim.Players.Single(); actor.SetDamageFactor("Crush", factor);
        sim.Motions.Add(new SectorMotion
        {
            SectorIndex = 0, Kind = kind, Speed = 16, CrushDamage = 10,
            FloorDirection = 1, TargetFloor = 32, CeilingDirection = -1, TargetCeiling = 32
        });
        LineSpecials.TickMotions(sim);
        Assert.Equal(health, actor.Health);
    }
}
