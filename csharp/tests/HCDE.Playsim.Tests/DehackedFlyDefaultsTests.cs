using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedFlyDefaultsTests
{
    [Theory]
    [InlineData("FLY + 6", true, false)]
    [InlineData("FLY + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordFlyDefaults(string bits, bool fly, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(fly, actor.Fly); Assert.Equal(fly, actor.DefaultFly);
        Assert.Equal(fly, bot.Fly); Assert.Equal(fly, bot.DefaultFly);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerFlyPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = FLY + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.Fly); Assert.True(player.DefaultFly); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("FLY + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsFlyDefault(string bits, bool fly)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(fly, missile.Fly); Assert.Equal(fly, missile.DefaultFly);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool flyDefault)
    {
        var sim = Room($"Thing 2\nBits = {(flyDefault ? "FLY + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.Fly = !flyDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(flyDefault, actor.Fly); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = FLY + 6\n"); var actor = sim.Actors.Single();
        actor.Fly = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(352, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultFly = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Fly); Assert.True(actor.DefaultFly); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.Fly);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultFly = actor.Fly = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultFly); Assert.False(actor.Fly);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = FLY + 6\n"); var actor = sim.Actors.Single(); actor.Fly = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultFly = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = FLY + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultFly);
        Assert.False(Start(replaced).Actors.Single().DefaultFly);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = FLY + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-flydefaults-value", error);
    }

    [Theory]
    [InlineData("FLY+NOGRAVITY+6", true)]
    [InlineData("PASSMOBJ+NOGRAVITY+6", false)]
    public void PatchedFlyControlsAirborneFrictionAfterSave(string bits, bool fly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); actor.Brain = null;
        actor.Z = Fixed.FromInt(64); actor.OnGround = false; actor.VelocityX = Fixed.FromInt(1);
        var saved = SimSavegame.Write(sim, true); actor.Fly = !fly;
        SimSavegame.Apply(sim, saved); ActorPhysics.Step(sim, actor);
        Assert.Equal(Fixed.FromDouble(fly ? ActorPhysics.FlyingFriction : 1), actor.VelocityX);
        Assert.Equal(64, actor.Z.ToDouble());
    }

    [Fact]
    public void FlyDefaultDoesNotEnableNoGravityOrCheatFlags()
    {
        var sim = Room("Thing 2\nBits = FLY+6\n"); var actor = sim.Actors.Single();
        Assert.True(actor.Fly); Assert.False(actor.NoGravity); Assert.False(actor.FlyCheat);
        actor.FlyCheat = true; actor.Fly = false; ActorRaise.ReviveSupported(actor);
        Assert.True(actor.Fly); Assert.False(actor.FlyCheat); Assert.False(actor.NoGravity);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
