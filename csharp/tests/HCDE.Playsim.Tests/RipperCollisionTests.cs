using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RipperCollisionTests
{
    [Fact]
    public void RipsMultipleTargetsOnceEachAcrossMovementSubsteps()
    {
        var (sim, missile, first) = Setup();
        var second = sim.AddBot(80, 0, 3001); second.Brain = null; second.Health = 1000; second.NoPain = true;
        var calls = 0; missile.DamageExpression = _ => { calls++; return 7; };
        missile.VelocityX = Fixed.FromInt(120); missile.Tick();
        Assert.False(missile.Destroyed); Assert.Equal(120, missile.X.ToDouble());
        Assert.Equal(993, first.Health); Assert.Equal(993, second.Health); Assert.Equal(2, calls);
    }

    [Fact]
    public void OverlapCanRipAgainOnNextMovementPass()
    {
        var (_, missile, target) = Setup(); missile.DamageExpression = _ => 7;
        missile.VelocityX = Fixed.FromInt(15); missile.Tick();
        Assert.Equal(993, target.Health); missile.Tick();
        Assert.Equal(986, target.Health); Assert.False(missile.Destroyed);
    }

    [Fact]
    public void DontRipStopsMissileButRetainsRipperDice()
    {
        var (_, missile, target) = Setup(); target.DontRip = true;
        var (prediction, _, _) = Setup();
        var expected = 5 * (2 + (int)(prediction.NextMissileDamageByte() & 3));
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Equal(1000 - expected, target.Health);
    }

    [Fact]
    public void SolidNonShootableBlockerPreventsRippingTargetsBehindIt()
    {
        var (sim, missile, target) = Setup();
        var blocker = sim.AddBot(10, 0, 3001); blocker.Brain = null; blocker.Shootable = false;
        missile.DamageExpression = _ => 7;
        missile.Tick(); Assert.True(missile.Destroyed); Assert.Equal(1000, target.Health);
    }

    [Fact]
    public void OwnerRemainsExcluded()
    {
        var (_, missile, target) = Setup();
        Assert.IsAssignableFrom<Actor>(missile.Owner).X = Fixed.FromInt(10); Assert.IsAssignableFrom<Actor>(missile.Owner).Health = 1000;
        missile.DamageExpression = _ => 7; missile.Tick();
        Assert.Equal(1000, Assert.IsAssignableFrom<Actor>(missile.Owner).Health); Assert.Equal(993, target.Health);
    }

    [Fact]
    public void FlagsAndLoadedMissileBehaviorRoundTrip()
    {
        var (sim, missile, target) = Setup();
        Assert.True(AcsActorFlags.TrySet(missile, "Actor.RIP", true));
        Assert.True(AcsActorFlags.TrySet(target, "DONTRIP", true));
        sim.DamageTypes.Define("Fire", 0.5); target.SplashGroup = 9;
        var bytes = SimSavegame.Write(sim); Assert.Equal(58, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var (restored, loaded, victim) = Setup(); restored.RestoreState(state);
        Assert.True(loaded.Rip); Assert.True(victim.DontRip); Assert.Equal(9, victim.SplashGroup);
        Assert.True(AcsActorFlags.TryGet(loaded, "rip", out var flag)); Assert.True(flag);
        loaded.Tick(); Assert.True(loaded.Destroyed); Assert.True(victim.Health < 1000);
    }

    [Fact]
    public void LegacyClearsFlagsAndMalformedBitsAreRejected()
    {
        var (sim, missile, target) = Setup(); missile.Rip = false;
        var legacy = SimSavegame.Write(sim); missile.Rip = true; target.DontRip = true;
        var pose = sim.CaptureState(); missile.Rip = target.DontRip = false;
        sim.RestoreState(pose); Assert.True(missile.Rip); Assert.True(target.DontRip);
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), 4);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-rip-flags", error);
        Assert.True(SimSavegame.TryRead(legacy, out var state, out error), error);
        sim.RestoreState(state); Assert.False(missile.Rip); Assert.False(target.DontRip);
        Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(7, false, true)]
    [InlineData(0, false, false)]
    [InlineData(-7, false, false)]
    [InlineData(7, true, true)]
    [InlineData(0, true, true)]
    [InlineData(-7, true, true)]
    public void StoppedRipperRetainsDamageBranchSoundAtImpact(int damage, bool causePain, bool sounds)
    {
        var (sim, missile, target) = Setup(); target.DontRip = true; target.Health = 20;
        missile.CausePain = causePain; missile.DamageExpression = _ => damage;
        missile.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Rip] = "custom/rip" };
        sim.DrainSoundRequests();
        missile.Tick();
        Assert.True(missile.Destroyed);
        var requests = sim.DrainSoundRequests();
        if (!sounds) Assert.Empty(requests);
        else
        {
            var request = Assert.Single(requests);
            Assert.Equal("custom/rip", request.Sound); Assert.Equal(missile.Id, request.ActorId);
            Assert.Equal(4, request.Channel); Assert.Equal(1.001f, request.Attenuation);
            Assert.Equal(1, request.Volume);
            var origin = Assert.IsType<SimulationSoundOrigin>(request.DetachedOrigin);
            var position = missile.SoundPos();
            Assert.Equal(position.X, origin.X); Assert.Equal(position.Y, origin.Y);
            Assert.Equal(position.Z, origin.Z);
        }
        Assert.Equal(damage > 0 ? 20 - damage : causePain ? 20 : 20 - damage, target.Health);
    }

    [Theory]
    [InlineData("")]
    [InlineData("None")]
    [InlineData("nOnE")]
    public void RipperNoSoundIdentityDoesNotQueuePlayback(string sound)
    {
        var (sim, missile, target) = Setup();
        missile.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Rip] = sound };
        missile.DamageExpression = _ => 7; sim.DrainSoundRequests();
        missile.DoMissileDamage(target, ripper: true);
        Assert.Empty(sim.DrainSoundRequests()); Assert.Equal(993, target.Health);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: 7);
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.Rip = true;
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        return (sim, missile, target);
    }
}
