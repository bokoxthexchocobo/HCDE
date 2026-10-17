using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoClipTargetContactTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void TargetNoClipDisablesOrdinarySolidBlocking(bool noclip, bool moves)
    {
        var (sim, mover, target) = Room(); target.NoClip = noclip;
        Assert.Equal(moves, ActorPhysics.TryMove(sim, mover, 8, 0, out _));
        Assert.Equal(moves ? 8 : 0, mover.X.ToDouble()); Assert.Equal(0, target.VelocityX.Raw);
    }

    [Fact]
    public void NoClipSolidTargetStillReceivesOrdinaryPush()
    {
        var (sim, mover, target) = Room(); target.NoClip = target.Pushable = true;
        mover.VelocityX = Fixed.FromInt(8);
        ActorPhysics.Step(sim, mover);
        Assert.Equal(8, mover.X.ToDouble()); Assert.Equal(2, target.VelocityX.ToDouble());
        Assert.True(target.LastPushFromMovement); Assert.Single(sim.PendingSoundRequests);
    }

    [Fact]
    public void NoClipDoesNotBypassEarlierBlastImpact()
    {
        var (sim, mover, target) = Room(); target.NoClip = true; mover.Blasted = true;
        mover.VelocityX = Fixed.FromInt(4);
        Assert.False(ActorPhysics.TryMove(sim, mover, 4, 0, out _));
        Assert.Equal(997, target.Health); Assert.Equal(999, mover.Health);
    }

    [Fact]
    public void NoClipDoesNotBypassTouchDeath()
    {
        var (sim, mover, target) = Room(); mover.DefinitionDoomEdNum = 3001;
        target.NoClip = target.Touchy = target.Armed = true; target.SeeState = -1;
        Assert.True(ActorPhysics.TryMove(sim, mover, 8, 0, out _));
        Assert.True(target.IsDead); Assert.False(target.Armed);
    }

    [Fact]
    public void RestoredNoClipTargetRetainsPushAndPassThroughBehavior()
    {
        var (sim, mover, target) = Room(); target.NoClip = target.Pushable = true; mover.VelocityX = Fixed.FromInt(8);
        var saved = SimSavegame.Write(sim, true);
        ActorPhysics.Step(sim, mover);
        var (restored, loaded, hit) = Room(); SimSavegame.Apply(restored, saved); ActorPhysics.Step(restored, loaded);
        Assert.Equal(8, loaded.X.ToDouble()); Assert.Equal(2, hit.VelocityX.ToDouble());
        sim.RestoreState(sim.CaptureState()); restored.RestoreState(restored.CaptureState());
        Assert.Equal(sim.Checksum, restored.Checksum);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var mover = sim.AddBot(0, 0); var target = sim.AddBot(40, 0);
        mover.Brain = target.Brain = null; mover.Health = target.Health = 1000;
        mover.DoHarmSpecies = target.DoHarmSpecies = true; mover.PainChance = target.PainChance = 0;
        mover.Mass = 200; target.Mass = 300;
        target.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Push] = "push/test" };
        return (sim, mover, target);
    }
}
