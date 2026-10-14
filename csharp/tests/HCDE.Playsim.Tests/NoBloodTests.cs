using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoBloodTests
{
    [Theory]
    [InlineData("NOBLOOD")]
    [InlineData("noblood")]
    [InlineData("Actor.NoBlood")]
    public void ScriptFlagCanBeReadSetAndCleared(string flag)
    {
        var actor = new Actor(); Assert.True(AcsActorFlags.TryGet(actor, flag, out var initial)); Assert.False(initial);
        Assert.True(AcsActorFlags.TrySet(actor, flag, true)); Assert.True(actor.NoBlood);
        Assert.True(AcsActorFlags.TryGet(actor, flag, out var value)); Assert.True(value);
        Assert.True(AcsActorFlags.TrySet(actor, flag, false)); Assert.False(actor.NoBlood);
    }

    [Fact]
    public void NativeBarrelDefaultDoesNotBleed()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 2035 }] });
        Assert.True(sim.Actors.Single().NoBlood); Assert.True(sim.Actors.Single().DefaultNoBlood);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DehackedPrimaryBitsControlClassDefault(bool noBlood)
    {
        var sim = Room(noBlood); Assert.Equal(noBlood, Target(sim).NoBlood); Assert.Equal(noBlood, Target(sim).DefaultNoBlood);
        var bot = sim.AddBot(200, 0); Assert.Equal(noBlood, bot.NoBlood); Assert.Equal(noBlood, bot.DefaultNoBlood);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void OverrideSurvivesSaveAndReconstruction(bool classDefault, bool reconstruct)
    {
        var sim = Room(classDefault); var target = Target(sim); target.NoBlood = !classDefault;
        var before = SimSavegame.Write(sim, true); Assert.True(ArchiveLayerFixture.TrailerStart(before, 223) >= 16);
        if (reconstruct) { target.Destroy(); sim.Tick(); } else target.NoBlood = classDefault;
        SimSavegame.Apply(sim, before);
        Assert.Equal(!classDefault, Target(sim).NoBlood); Assert.Equal(classDefault, Target(sim).DefaultNoBlood);
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void SnapshotWithoutOverrideRestoresPatchedDefault()
    {
        var sim = Room(true); var before = SimSavegame.Write(sim);
        Target(sim).NoBlood = false; SimSavegame.Apply(sim, before);
        Assert.True(Target(sim).NoBlood); Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsAndWeaponHitsLeaveVisiblePuffForNoBloodTarget(bool acs)
    {
        var sim = Room(); var target = Target(sim); target.NoBlood = true; target.Health = 1000; target.NoPain = true;
        var source = sim.Players.Single();
        if (acs) Assert.Equal(1, AcsLineAttack.Attack(sim, source, 0, 0, 7, "None", 256, puffTid: 77));
        else Assert.True(HitscanCombat.Fire(sim, source));
        var puff = sim.Actors.OfType<PuffActor>().Single(); Assert.False(puff.Destroyed);
        Assert.True(target.Health < 1000); Assert.Equal(source.Id, target.LastDamageSourceId);
        Assert.Same(source, puff.DamageSource); Assert.Equal(acs ? 77 : 0, puff.ThingId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void ZeroDamageNoBloodHitStillSpawnsVisiblePuff(int damage)
    {
        var sim = Room(); var target = Target(sim); target.NoBlood = true; target.Health = 1000;
        AcsLineAttack.Attack(sim, sim.Players.Single(), 0, 0, damage, "None", 256);
        Assert.Single(sim.Actors.OfType<PuffActor>()); Assert.Equal(1000 - damage, target.Health);
    }

    [Theory]
    [InlineData(0, 223, "save-noblood-header")]
    [InlineData(4, -1, "save-noblood-header")]
    [InlineData(12, 2, "save-noblood-value")]
    public void RejectsMalformedArchive(int offset, int value, string expected)
    {
        var sim = Room(); Target(sim).NoBlood = true; var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    [Fact]
    public void InvalidStructuredFlagIsRejectedBeforeMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState(true);
        state.Actors.Single(pose => pose.MapMonsterThingIndex.HasValue).NoBloodFlag = 2;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void VisibleNoBloodImpactReplaysWithRestoredFlag()
    {
        var sim = Room(); Target(sim).NoBlood = true; Target(sim).Health = 1000;
        var source = sim.Players.Single(); var before = SimSavegame.Write(sim, true);
        Assert.True(HitscanCombat.Fire(sim, source)); sim.Tick(); var expected = SimSavegame.Write(sim, true); var checksum = sim.Checksum;
        SimSavegame.Apply(sim, before); Assert.True(HitscanCombat.Fire(sim, source)); sim.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(checksum, sim.Checksum);
    }

    private static Actor Target(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.MapMonsterThingIndex.HasValue);
    private static AuthoritySimulation Room(bool noBlood = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 }]
    }, dehacked: DehackedPatch.Apply("Thing 2\nBits = " + (4194310 + (noBlood ? 524288 : 0)) + "\n"));
}
