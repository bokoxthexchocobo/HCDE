using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

/// <summary>Exercises every integer/boolean <c>GetActorProperty</c> case credited in chunk 38.</summary>
public class AcsGetActorPropertyCoverageTests
{
    [Fact]
    public void GetActorProperty_CoversEveryCreditedScalarCase()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things =
            [
                new LevelThing { Type = 1, X = 0, Y = 0 },
                new LevelThing { Type = 3001, X = 64, Y = 0, Id = 9 },
                new LevelThing { Type = 3001, X = 128, Y = 0, Id = 10 },
            ],
        };
        var sim = AuthoritySimulation.Start(level);
        var player = sim.Players.First();
        var monster = sim.Actors.First(a => a.IsMonster);
        var tracer = sim.Actors.First(a => a != monster && a != player);

        monster.Health = 77;
        monster.MovementSpeed = new Fixed(65536);
        monster.SetDamage(4);
        monster.Alpha = 0.5;
        monster.RenderStyle = ActorRenderStyle.Normal;
        monster.Ambush = true;
        monster.Invulnerable = true;
        monster.ChaseGoal = true;
        monster.Frightened = true;
        monster.Gravity = new Fixed(32768);
        monster.Friendly = true;
        monster.Dropped = true;
        monster.NoTarget = true;
        monster.NoTrigger = true;
        monster.Dormant = true;
        monster.Score = 9;
        monster.DamageFactor = new Fixed(65536);
        monster.DamageMultiplier = new Fixed(131072);
        monster.ScaleX = 2f;
        monster.ScaleY = 0.5f;
        monster.Mass = 500;
        monster.Accuracy = 3;
        monster.Stamina = 2;
        monster.ReactionTime = 5;
        monster.MeleeRange = new Fixed(65536);
        monster.Friction = new Fixed(65536);
        monster.MaxStepHeight = Fixed.FromDouble(4);
        monster.MaxDropOffHeight = Fixed.FromDouble(8);
        monster.FriendlySeeBlocks = 1;
        monster.WaterLevel = 2;
        monster.WaterDepth = Fixed.FromDouble(1.5);
        monster.StencilFillColor = 0x00ff0000;
        monster.SetTargetPointer(tracer);
        monster.TracerId = tracer.Id;

        player.JumpZ = new Fixed(131072);
        player.MaxHealth = 100;
        player.DefaultViewHeight = new Fixed(65536);
        player.AttackZOffset = new Fixed(32768);

        Assert.Equal(77, Get(monster, AcsActorProperties.Health));
        Assert.Equal(65536, Get(monster, AcsActorProperties.Speed));
        Assert.Equal(4, Get(monster, AcsActorProperties.Damage));
        Assert.Equal(32768, Get(monster, AcsActorProperties.Alpha));
        Assert.Equal(ActorRenderStyle.Normal.ToAcs(), Get(monster, AcsActorProperties.RenderStyle));
        Assert.Equal(1, Get(monster, AcsActorProperties.Ambush));
        Assert.Equal(1, Get(monster, AcsActorProperties.Invulnerable));
        Assert.Equal(131072, Get(player, AcsActorProperties.JumpZ));
        Assert.Equal(1, Get(monster, AcsActorProperties.ChaseGoal));
        Assert.Equal(1, Get(monster, AcsActorProperties.Frightened));
        Assert.Equal(32768, Get(monster, AcsActorProperties.Gravity));
        Assert.Equal(1, Get(monster, AcsActorProperties.Friendly));
        Assert.Equal(1, Get(monster, AcsActorProperties.Dropped));
        Assert.Equal(1, Get(monster, AcsActorProperties.NoTarget));
        Assert.Equal(1, Get(monster, AcsActorProperties.NoTrigger));
        Assert.Equal(1, Get(monster, AcsActorProperties.Dormant));
        Assert.Equal(monster.GetMaxHealth(false), Get(monster, AcsActorProperties.SpawnHealth));
        Assert.Equal(9, Get(monster, AcsActorProperties.Score));
        Assert.Equal(65536, Get(monster, AcsActorProperties.DamageFactor));
        Assert.Equal(131072, Get(monster, AcsActorProperties.DamageMultiplier));
        Assert.Equal(tracer.ThingId, Get(monster, AcsActorProperties.TargetTid));
        Assert.Equal(tracer.ThingId, Get(monster, AcsActorProperties.TracerTid));
        Assert.Equal(2, Get(monster, AcsActorProperties.WaterLevel));
        Assert.Equal(131072, Get(monster, AcsActorProperties.ScaleX));
        Assert.Equal(32768, Get(monster, AcsActorProperties.ScaleY));
        Assert.Equal(500, Get(monster, AcsActorProperties.Mass));
        Assert.Equal(3, Get(monster, AcsActorProperties.Accuracy));
        Assert.Equal(2, Get(monster, AcsActorProperties.Stamina));
        Assert.Equal(monster.Height.Raw, Get(monster, AcsActorProperties.Height));
        Assert.Equal(monster.Radius.Raw, Get(monster, AcsActorProperties.Radius));
        Assert.Equal(5, Get(monster, AcsActorProperties.ReactionTime));
        Assert.Equal(65536, Get(monster, AcsActorProperties.MeleeRange));
        Assert.Equal(65536, Get(player, AcsActorProperties.ViewHeight));
        Assert.Equal(32768, Get(player, AcsActorProperties.AttackZOffset));
        Assert.Equal(0x00ff0000, Get(monster, AcsActorProperties.StencilColor));
        Assert.Equal(65536, Get(monster, AcsActorProperties.Friction));
        Assert.Equal(monster.MaxStepHeight.Raw, Get(monster, AcsActorProperties.MaxStepHeight));
        Assert.Equal(monster.MaxDropOffHeight.Raw, Get(monster, AcsActorProperties.MaxDropOffHeight));
        Assert.Equal(1, Get(monster, AcsActorProperties.FriendlySeeBlocks));
        Assert.Equal(monster.WaterDepth.Raw, Get(monster, AcsActorProperties.WaterDepth));
        Assert.Equal(0, Get(monster, AcsActorProperties.JumpZ));
    }

    [Theory]
    [InlineData(AcsActorProperties.Ambush)]
    [InlineData(AcsActorProperties.Invulnerable)]
    [InlineData(AcsActorProperties.Frightened)]
    [InlineData(AcsActorProperties.Dormant)]
    public void CheckActorProperty_MatchesGetForBooleans(int property)
    {
        var sim = Sim();
        var actor = sim.Actors.First();
        Assert.True(AcsGetActorPropertyService.Check(sim, actor, 0, property, 0));
        Assert.False(AcsGetActorPropertyService.Check(sim, actor, 0, property, 1));
    }

    [Theory]
    [InlineData(20)]
    [InlineData(46)]
    public void StringProperties_UseStringTablePath(int property)
    {
        var sim = Sim();
        var actor = sim.Players.First();
        AcsActorStringProperties.Set(sim, actor, 0, property, "Probe");
        Assert.Equal("Probe", AcsActorStringProperties.Get(sim, actor, 0, property));
    }

    private static int Get(Actor actor, int property)
    {
        var sim = actor.Simulation!;
        return AcsGetActorPropertyService.Get(sim, actor, 0, property);
    }

    private static AuthoritySimulation Sim()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Things = [new LevelThing { Type = 1, X = 0, Y = 0 }, new LevelThing { Type = 3001, X = 64, Y = 0 }],
        };
        return AuthoritySimulation.Start(level);
    }
}
