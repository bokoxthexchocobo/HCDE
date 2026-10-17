using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedInFloatDefaultsTests
{
    [Theory]
    [InlineData("INFLOAT + 6", true, false)]
    [InlineData("INFLOAT + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordInFloatDefaults(string bits, bool infloat, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(infloat, actor.InFloat); Assert.Equal(infloat, actor.DefaultInFloat);
        Assert.Equal(infloat, bot.InFloat); Assert.Equal(infloat, bot.DefaultInFloat);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerInFloatPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = INFLOAT + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.InFloat); Assert.True(player.DefaultInFloat); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("INFLOAT + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsInFloatDefault(string bits, bool infloat)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(infloat, missile.InFloat); Assert.Equal(infloat, missile.DefaultInFloat);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool infloatDefault)
    {
        var sim = Room($"Thing 2\nBits = {(infloatDefault ? "INFLOAT + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.InFloat = !infloatDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(infloatDefault, actor.InFloat); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = INFLOAT + 6\n"); var actor = sim.Actors.Single();
        actor.InFloat = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(349, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultInFloat = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.InFloat); Assert.True(actor.DefaultInFloat); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.InFloat);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultInFloat = actor.InFloat = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultInFloat); Assert.False(actor.InFloat);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = INFLOAT + 6\n"); var actor = sim.Actors.Single(); actor.InFloat = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultInFloat = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedPrimaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = INFLOAT + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultInFloat);
        Assert.False(Start(replaced).Actors.Single().DefaultInFloat);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = INFLOAT + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-infloatdefaults-value", error);
    }

    [Theory]
    [InlineData("FLOAT+NOGRAVITY+6", 60)]
    [InlineData("INFLOAT+FLOAT+NOGRAVITY+6", 56)]
    [InlineData("2114054", 56)]
    public void PatchedInFloatSuppressesTargetHeightAdjustmentAfterSave(string bits, int expected)
    {
        var patch = DehackedPatch.Apply($"Thing 15\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
        var actor = sim.AddBot(0, 0, 3005); var target = sim.AddBot(64, 0);
        actor.Z = Fixed.FromInt(56); target.Z = Fixed.FromInt(100);
        actor.Brain!.SetSpecialTarget(target); actor.Brain.Enabled = false;
        var saved = SimSavegame.Write(sim, true); actor.InFloat = !actor.InFloat;
        SimSavegame.Apply(sim, saved); ActorPhysics.Step(sim, actor);
        Assert.Equal(expected, actor.Z.ToDouble()); Assert.Equal(default, actor.VelocityZ);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
