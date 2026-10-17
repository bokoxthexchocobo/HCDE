using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerSupportViewTests
{
    [Theory]
    [InlineData(0, -4, 45, 0)]
    [InlineData(0, -9, 45, -0.5)]
    [InlineData(10, -4, 35, 0.75)]
    [InlineData(10, -9, 35, 0.75)]
    public void SupportSnapAdjustsViewAndChoosesLargerRecoveryDelta(int supportZ, int velocity,
        double viewHeight, double delta)
    {
        var (sim, player, support) = Room(); support.Z = Fixed.FromInt(supportZ);
        player.VelocityZ = Fixed.FromInt(velocity);
        ActorPhysics.Step(sim, player);
        Assert.Equal(viewHeight, player.ViewHeight); Assert.Equal(delta, player.DeltaViewHeight);
        Assert.Equal(56 + supportZ, player.Z.ToDouble()); Assert.True(player.OnMobj);
    }

    [Theory]
    [InlineData(800, 1, -8, false, 0)]
    [InlineData(800, 1, -9, false, 1)]
    [InlineData(400, 1, -5, false, 1)]
    [InlineData(800, 0.5, -4, false, 0)]
    [InlineData(800, 0.5, -5, false, 1)]
    [InlineData(800, 1, -9, true, 0)]
    public void SquatHookUsesLevelAndSectorGravityInsteadOfLandingSpeed(double levelGravity,
        double sectorGravity, int velocity, bool noGravity, int calls)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            LevelGravity = levelGravity,
            Sectors = [new LevelSector { CeilingHeight = 256, Gravity = sectorGravity }],
        });
        var support = sim.AddBot(0, 0);
        var player = new ProbePlayer { Simulation = sim, Z = Fixed.FromInt(60), OnGround = false,
            VelocityZ = Fixed.FromInt(velocity), NoGravity = noGravity, LandingSpeed = -1000 };
        ActorPhysics.Step(sim, player);
        Assert.Equal(calls, player.Calls);
        if (calls != 0) { Assert.Same(support, player.Support); Assert.Equal(velocity, player.ImpactVelocity); }
    }

    [Fact]
    public void ExistingLargerDeltaSurvivesSupportHeightAdjustment()
    {
        var (sim, player, support) = Room(); support.Z = Fixed.FromInt(10); player.DeltaViewHeight = 2;
        ActorPhysics.Step(sim, player);
        Assert.Equal(35, player.ViewHeight); Assert.Equal(2, player.DeltaViewHeight);
    }

    [Fact]
    public void UnsupportedPlayerKeepsViewState()
    {
        var (sim, player, support) = Room(); support.X = Fixed.FromInt(200);
        ActorPhysics.Step(sim, player);
        Assert.Equal(41, player.ViewHeight); Assert.Equal(0, player.DeltaViewHeight); Assert.False(player.OnMobj);
    }

    [Fact]
    public void SaveRestoresSupportViewAndFutureRecovery()
    {
        var (sim, player, support) = Room(); support.Z = Fixed.FromInt(10); ActorPhysics.Step(sim, player);
        var saved = SimSavegame.Write(sim, true);
        var (restored, loaded, _) = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(35, loaded.ViewHeight); Assert.Equal(0.75, loaded.DeltaViewHeight);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
        for (var i = 0; i < 10; i++)
        { sim.Tick(); restored.Tick(); Assert.Equal(sim.Checksum, restored.Checksum); }
    }

    private sealed class ProbePlayer : PlayerPawn
    {
        public int Calls { get; private set; }
        public Actor? Support { get; private set; }
        public double ImpactVelocity { get; private set; }
        public override void PlayerSquatView(Actor? onMobj = null)
        { Calls++; Support = onMobj; ImpactVelocity = VelocityZ.ToDouble(); base.PlayerSquatView(onMobj); }
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Support) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); var support = sim.AddBot(0, 0); support.Brain = null;
        player.Z = Fixed.FromInt(60); player.VelocityZ = Fixed.FromInt(-4); player.OnGround = false;
        return (sim, player, support);
    }
}
