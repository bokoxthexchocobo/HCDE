using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedNonShootableDefaultsTests
{
    [Theory]
    [InlineData("NONSHOOTABLE + 6", true, false)]
    [InlineData("NONSHOOTABLE + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordNonShootableDefaults(string bits, bool nonShootable, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(nonShootable, actor.NonShootable); Assert.Equal(nonShootable, actor.DefaultNonShootable);
        Assert.Equal(nonShootable, bot.NonShootable); Assert.Equal(nonShootable, bot.DefaultNonShootable);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerNonShootablePatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = NONSHOOTABLE + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.NonShootable); Assert.True(player.DefaultNonShootable); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("NONSHOOTABLE + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsNonShootableDefault(string bits, bool nonShootable)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(nonShootable, missile.NonShootable); Assert.Equal(nonShootable, missile.DefaultNonShootable);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool nonShootableDefault)
    {
        var sim = Room($"Thing 2\nBits = {(nonShootableDefault ? "NONSHOOTABLE + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.NonShootable = !nonShootableDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(nonShootableDefault, actor.NonShootable); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = NONSHOOTABLE + 6\n"); var actor = sim.Actors.Single();
        actor.NonShootable = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(338, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultNonShootable = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.NonShootable); Assert.True(actor.DefaultNonShootable); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.NonShootable);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultNonShootable = actor.NonShootable = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultNonShootable); Assert.False(actor.NonShootable);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = NONSHOOTABLE + 6\n"); var actor = sim.Actors.Single(); actor.NonShootable = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultNonShootable = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = NONSHOOTABLE + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultNonShootable);
        Assert.False(Start(replaced).Actors.Single().DefaultNonShootable);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = NONSHOOTABLE + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-nonshootabledefaults-value", error);
    }

    [Theory]
    [InlineData("NONSHOOTABLE", true)]
    [InlineData("PASSMOBJ", false)]
    public void PatchedTargetControlsMissileContactAfterSave(string bits, bool passes)
    {
        var sim = Room($"Thing 12\nBits = {bits}\n"); var owner = sim.Actors.Single();
        owner.SetXYZ(-200, 0, 0); owner.Brain = null;
        var target = sim.AddBot(30, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        Assert.True(target.Shootable); Assert.Equal(passes, target.NonShootable);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.SetXYZ(0, 0, 20); missile.VelocityX = Fixed.FromInt(30);
        missile.VelocityY = missile.VelocityZ = default;
        var saved = SimSavegame.Write(sim, true); target.NonShootable = !passes;
        SimSavegame.Apply(sim, saved); missile.DamageExpression = _ => 7;
        missile.Tick(); Assert.Equal(!passes, missile.Destroyed);
        Assert.Equal(passes ? 1000 : 993, target.Health);
    }

    [Fact]
    public void ShootabilityActionsOverrideRuntimeButPreserveRevivalDefault()
    {
        var sim = Room("Thing 2\nBits = NONSHOOTABLE\n"); var actor = sim.Actors.Single();
        ActorPropertyActions.SetShootable(actor); Assert.False(actor.NonShootable);
        var saved = SimSavegame.Write(sim, true);
        actor.NonShootable = true; SimSavegame.Apply(sim, saved);
        Assert.False(actor.NonShootable); Assert.True(actor.DefaultNonShootable);
        actor.Health = 0; ActorRaise.ReviveSupported(actor);
        Assert.True(actor.NonShootable); Assert.True(actor.Shootable);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
