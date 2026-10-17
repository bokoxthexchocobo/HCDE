using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedTouchyDefaultsTests
{
    [Theory]
    [InlineData("TOUCHY + 6", true, false)]
    [InlineData("TOUCHY + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordTouchyDefaults(string bits, bool touchy, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(touchy, actor.Touchy); Assert.Equal(touchy, actor.DefaultTouchy);
        Assert.Equal(touchy, bot.Touchy); Assert.Equal(touchy, bot.DefaultTouchy);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerTouchyPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = TOUCHY + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.Touchy); Assert.True(player.DefaultTouchy); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("TOUCHY + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsTouchyDefault(string bits, bool touchy)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(touchy, missile.Touchy); Assert.Equal(touchy, missile.DefaultTouchy);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool touchyDefault)
    {
        var sim = Room($"Thing 2\nBits = {(touchyDefault ? "TOUCHY + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.Touchy = !touchyDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(touchyDefault, actor.Touchy); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = TOUCHY + 6\n"); var actor = sim.Actors.Single();
        actor.Touchy = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(346, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultTouchy = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Touchy); Assert.True(actor.DefaultTouchy); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.Touchy);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultTouchy = actor.Touchy = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultTouchy); Assert.False(actor.Touchy);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = TOUCHY + 6\n"); var actor = sim.Actors.Single(); actor.Touchy = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultTouchy = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void RemappedFlagSurvivesLaterFlagSetReplacements()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = TOUCHY + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultTouchy);
        Assert.True(Start(replaced).Actors.Single().DefaultTouchy);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = TOUCHY + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-touchydefaults-value", error);
    }

    [Theory]
    [InlineData("TOUCHY+6", true)]
    [InlineData("268435462", true)]
    [InlineData("6", false)]
    public void RemappedFlagArmsAndDetonatesOnSolidContactAfterSave(string bits, bool detonates)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == 2);
        Assert.Equal(6u, definition.Bits); Assert.Equal(detonates, definition.Touchy);
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        var mover = sim.Players.Single(); var mine = sim.AddBot(40, 0);
        mine.Brain = null; mine.SeeState = -1; mine.Solid = false;
        ActorPhysics.Step(sim, mine); Assert.Equal(detonates, mine.Armed);
        var saved = SimSavegame.Write(sim, true); mine.Touchy = !detonates;
        SimSavegame.Apply(sim, saved);
        ActorPhysics.TryMove(sim, mover, 20, 0, out _); Assert.Equal(detonates, mine.IsDead);
    }

    [Fact]
    public void LaterPrimaryZeroDoesNotClearNativeRemappedTouchy()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = TOUCHY+6\n");
        var next = DehackedPatch.Apply("Thing 2\nBits = 0\n", original);
        Assert.Empty(next.Errors); Assert.True(Start(next).Actors.Single().Touchy);
        Assert.Equal(0u, next.Actors.Single(a => a.Index == 2).Bits);
        Assert.True(original.Actors.Single(a => a.Index == 2).Touchy);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
