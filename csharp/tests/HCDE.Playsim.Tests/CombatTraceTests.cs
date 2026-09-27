using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CombatTraceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WallStopsBulletsAndMelee(bool melee)
    {
        var sim = Range(Wall(60), targetX: 80);
        var player = sim.Players.Single();
        if (melee) player.Inventory.Selected = WeaponKind.Fist;
        Fire(sim);
        Assert.Equal(60, Target(sim).Health);
        Assert.Equal(melee ? 50 : 49, player.Inventory.Bullets);
    }

    [Theory]
    [InlineData(0, 128, 50)]
    [InlineData(LevelLine.BlockingFlag, 128, 50)]
    [InlineData(LevelLine.BlockEverythingFlag, 128, 60)]
    [InlineData(LevelLine.BlockHitscanFlag, 128, 60)]
    [InlineData(0, 0, 60)]
    public void TwoSidedLineUsesShotFlagsAndSectorOpening(int flags, int ceiling, int health)
    {
        var sim = Range(Wall(100, twoSided: true, flags: flags), ceiling: (short)ceiling);
        Fire(sim);
        Assert.Equal(health, Target(sim).Health);
    }

    [Fact]
    public void TraceUsesRestoredDoorHeight()
    {
        var sim = Range(Wall(100, twoSided: true), ceiling: 0);
        Fire(sim);
        Assert.Equal(60, Target(sim).Health);
        var state = sim.CaptureState();
        state.Sectors[1] = (0, 128);
        sim.RestoreState(state);
        Fire(sim);
        Assert.Equal(50, Target(sim).Health);
    }

    [Theory]
    [InlineData(100, 64, 100, 128)]
    [InlineData(100, 128, 100, 64)]
    [InlineData(100, 64, 150, 64)]
    public void WallEndpointsAndCollinearWallsBlock(double x1, double y1, double x2, double y2)
    {
        var sim = Range(new LevelLine { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, SideBack = -1 });
        Fire(sim);
        Assert.Equal(60, Target(sim).Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(250)]
    public void WallBehindShooterOrTargetDoesNotBlock(int x)
    {
        var sim = Range(Wall(x));
        Fire(sim);
        Assert.Equal(50, Target(sim).Health);
    }

    [Fact]
    public void SelectsNearestSurfaceInsteadOfNearestCenter()
    {
        var level = new PlayLevel
        {
            Things = new[] { Thing(1, 32), Thing(3001, 140), Thing(3002, 200) },
        };
        var sim = AuthoritySimulation.Start(level);
        sim.Actors[2].Radius = Fixed.FromInt(100);
        Assert.Same(sim.Actors[2], CombatTrace.FindTarget(sim, sim.Players.Single(), 2048));
    }

    [Fact]
    public void MeleeDoesNotHitToTheSide()
    {
        var sim = Range(targetX: 40);
        Target(sim).Y = Fixed.FromInt(120);
        sim.Players.Single().Inventory.Selected = WeaponKind.Fist;
        Fire(sim);
        Assert.Equal(60, Target(sim).Health);
    }

    [Fact]
    public void RangeEndsAtActorSurface()
    {
        var sim = Range(targetX: 116);
        var player = sim.Players.Single();
        Assert.Same(Target(sim), CombatTrace.FindTarget(sim, player, 64));
        Target(sim).X = Fixed.FromInt(117);
        Assert.Null(CombatTrace.FindTarget(sim, player, 64));
    }

    [Fact]
    public void DeadActorDoesNotShieldLivingTarget()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = new[] { Thing(1, 32), Thing(3001, 100), Thing(3002, 200) },
        });
        sim.Actors[1].Health = 10;
        Fire(sim);
        Assert.True(sim.Actors[1].IsDead);
        Assert.False(sim.Actors[1].BlocksActors);
        Fire(sim);
        Assert.Equal(140, sim.Actors[2].Health); // Demon starts at native health 150.
        var state = sim.CaptureState();
        sim.Actors[1].Health = 30;
        sim.RestoreState(state);
        Assert.False(sim.Actors[1].BlocksActors);
    }

    [Fact]
    public void DeadPlayerCannotMoveFireOrCollectHealth()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = new[] { Thing(1, 32), Thing(2011, 32), Thing(3001, 200) },
        });
        var player = sim.Players.Single();
        player.Health = 0;
        player.AttackPressed = true;
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192, YawDelta = 8192, Attack = true });
        sim.Tick();
        Assert.Equal(32, player.X.ToDouble());
        Assert.Equal(0u, player.Angle.Raw);
        Assert.Equal(0, player.Health);
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(3, sim.Actors.Count);
        Assert.False(HitscanCombat.Fire(sim, player));
        Assert.False(player.AttackPressed);
    }

    [Fact]
    public void DeadBotStopsWalking()
    {
        var sim = Range();
        var bot = sim.AddBot(400, 64);
        bot.Health = 0;
        sim.Tick();
        Assert.Equal(400, bot.X.ToDouble());
    }

    private static void Fire(AuthoritySimulation sim)
    {
        foreach (var actor in sim.Actors) actor.Brain = null; // Trace geometry is independent of AI movement.
        while (sim.Players.Single().WeaponCooldown > 1) sim.Tick();
        sim.QueueCommand(0, new PlayerCommand { Attack = true });
        sim.Tick();
    }

    private static LevelThing Thing(short type, short x) => new() { Type = type, X = x, Y = 64 };
    private static Actor Target(AuthoritySimulation sim) => sim.Actors.Single(a => a is not PlayerPawn and not ProjectileActor);
    private static LevelLine Wall(int x, bool twoSided = false, int flags = 0) => new()
    {
        X1 = x, Y1 = 0, X2 = x, Y2 = 128, SideFront = 0, SideBack = twoSided ? 1 : -1, Flags = flags,
    };
    private static AuthoritySimulation Range(LevelLine? line = null, short targetX = 200, short ceiling = 128) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Things = new[] { Thing(1, 32), Thing(3001, targetX) },
            Lines = line == null ? Array.Empty<LevelLine>() : new[] { line },
            Sides = new[] { new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 } },
            Sectors = new[] { new LevelSector { CeilingHeight = 128 }, new LevelSector { CeilingHeight = ceiling } },
        });
}
