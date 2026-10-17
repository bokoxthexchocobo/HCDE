using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedReflectiveDefaultsTests
{
    [Theory]
    [InlineData("REFLECTIVE + 6", true, false)]
    [InlineData("REFLECTIVE + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordReflectionDefaults(string bits, bool reflective, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(reflective, actor.Reflective); Assert.Equal(reflective, actor.DefaultReflective);
        Assert.Equal(reflective, bot.Reflective); Assert.Equal(reflective, bot.DefaultReflective);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerReflectivePatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = REFLECTIVE + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.Reflective); Assert.True(player.DefaultReflective); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("REFLECTIVE + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsReflectionDefault(string bits, bool reflective)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(reflective, missile.Reflective); Assert.Equal(reflective, missile.DefaultReflective);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool reflectiveDefault)
    {
        var sim = Room($"Thing 2\nBits = {(reflectiveDefault ? "REFLECTIVE + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.Reflective = !reflectiveDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(reflectiveDefault, actor.Reflective); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = REFLECTIVE + 6\n"); var actor = sim.Actors.Single();
        actor.Reflective = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(336, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultReflective = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.Reflective); Assert.True(actor.DefaultReflective); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.Reflective);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultReflective = actor.Reflective = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultReflective); Assert.False(actor.Reflective);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = REFLECTIVE + 6\n"); var actor = sim.Actors.Single(); actor.Reflective = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultReflective = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = REFLECTIVE + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultReflective);
        Assert.False(Start(replaced).Actors.Single().DefaultReflective);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = REFLECTIVE + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-reflectivedefaults-value", error);
    }

    [Theory]
    [InlineData("REFLECTIVE + 6", true)]
    [InlineData("6", false)]
    public void PatchedReflectorControlsMissileOwnershipAfterSave(string bits, bool reflective)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var reflector = sim.Actors.Single();
        var owner = sim.AddBot(-200, 0, 3001); var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        missile.SetXYZ(20, 0, 20);
        var saved = SimSavegame.Write(sim, true); reflector.Reflective = !reflective;
        SimSavegame.Apply(sim, saved); Assert.Equal(reflective, reflector.Reflective);
        Assert.Equal(reflective, ActorReflectionActions.ReflectOffActor(missile, reflector));
        Assert.Same(reflective ? reflector : owner, missile.Owner);
    }

    [Fact]
    public void NamedSecondaryTopBitDoesNotBecomePrimaryTranslucency()
    {
        var patch = DehackedPatch.Apply("Thing 2\nBits = REFLECTIVE\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == 2);
        Assert.Equal(0x80000000u, definition.Bits2); Assert.True(definition.Bits2Patched);
        Assert.False(definition.BitsPatched); Assert.Null(definition.Alpha);
        var sim = Start(patch); Assert.Equal(1, sim.Actors.Single().Alpha);
        var replacement = DehackedPatch.Apply("Thing 2\nBits = SHOOTABLE\n", patch);
        Assert.True(Start(replacement).Actors.Single().Reflective);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
