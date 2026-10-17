using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedTeleStompDefaultsTests
{
    [Theory]
    [InlineData("TELESTOMP + 6", true, false)]
    [InlineData("TELESTOMP + PASSMOBJ", true, false)]
    [InlineData("FRIEND + 6", false, true)]
    [InlineData("1073741830", false, true)]
    [InlineData("6", false, false)]
    public void MapAndDynamicActorsRecordTeleStompDefaults(string bits, bool stomp, bool friendly)
    {
        var sim = Room($"Thing 2\nBits = {bits}\n"); var actor = sim.Actors.Single(); var bot = sim.AddBot(200, 0);
        Assert.Equal(stomp, actor.TeleStomp); Assert.Equal(stomp, actor.DefaultTeleStomp);
        Assert.Equal(stomp, bot.TeleStomp); Assert.Equal(stomp, bot.DefaultTeleStomp);
        Assert.Equal(friendly, actor.Friendly); Assert.Equal(friendly, bot.Friendly);
    }

    [Fact]
    public void PlayerTeleStompPatchKeepsPlayerFriendliness()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply("Thing 1\nBits = TELESTOMP + 6\n"));
        var player = sim.Players.Single(); Assert.True(player.TeleStomp); Assert.True(player.DefaultTeleStomp); Assert.True(player.Friendly);
    }

    [Theory]
    [InlineData("TELESTOMP + 6", true)]
    [InlineData("FRIEND + 6", false)]
    public void ProjectileRecordsTeleStompDefault(string bits, bool stomp)
    {
        var sim = Room($"Thing 34\nBits = {bits}\n"); var owner = sim.Actors.Single();
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        Assert.Equal(stomp, missile.TeleStomp); Assert.Equal(stomp, missile.DefaultTeleStomp);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RevivalRestoresClassDefaultInsteadOfRuntimeFlag(bool stompDefault)
    {
        var sim = Room($"Thing 2\nBits = {(stompDefault ? "TELESTOMP + 6" : "6")}\n"); var actor = sim.Actors.Single();
        actor.TeleStomp = !stompDefault; actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.Equal(stompDefault, actor.TeleStomp); Assert.True(actor.Health > 0);
    }

    [Fact]
    public void DisabledRuntimeFlagAndEnabledClassDefaultSurviveSaveThenRevival()
    {
        var sim = Room("Thing 2\nBits = TELESTOMP + 6\n"); var actor = sim.Actors.Single();
        actor.TeleStomp = false; actor.Health = 0; var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(342, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        actor.DefaultTeleStomp = false; SimSavegame.Apply(sim, bytes);
        Assert.False(actor.TeleStomp); Assert.True(actor.DefaultTeleStomp); Assert.Equal(bytes, SimSavegame.Write(sim, true));
        ActorRaise.ReviveSupported(actor); Assert.True(actor.TeleStomp);
    }

    [Fact]
    public void LegacySaveWithoutDefaultExtensionResetsTheDefault()
    {
        var sim = Room("Thing 2\nBits = 6\n"); var actor = sim.Actors.Single(); var old = SimSavegame.Write(sim, true);
        actor.DefaultTeleStomp = actor.TeleStomp = true;
        SimSavegame.Apply(sim, old); Assert.False(actor.DefaultTeleStomp); Assert.False(actor.TeleStomp);
    }

    [Fact]
    public void DefaultAffectsChecksumEvenWithSameCurrentFlag()
    {
        var sim = Room("Thing 2\nBits = TELESTOMP + 6\n"); var actor = sim.Actors.Single(); actor.TeleStomp = false;
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        actor.DefaultTeleStomp = false; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(checksum, sim.Checksum);
    }

    [Fact]
    public void InheritedSecondaryDefaultIsPreservedUntilReplaced()
    {
        var original = DehackedPatch.Apply("Thing 2\nBits = TELESTOMP + 6\n");
        var inherited = DehackedPatch.Apply("Thing 2\nHit points = 88\n", original);
        var replaced = DehackedPatch.Apply("Thing 2\nBits = PASSMOBJ + 6\n", inherited);
        Assert.True(Start(inherited).Actors.Single().DefaultTeleStomp);
        Assert.False(Start(replaced).Actors.Single().DefaultTeleStomp);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void MalformedDefaultFlagIsRejected(int value)
    {
        var sim = Room("Thing 2\nBits = TELESTOMP + 6\n"); var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-telestompdefaults-value", error);
    }

    [Theory]
    [InlineData("TELESTOMP", true)]
    [InlineData("PASSMOBJ", false)]
    public void PatchedMonsterTelefragsOnlyWithStompDefaultAfterSave(string bits, bool succeeds)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
        }, dehacked: patch);
        var mover = sim.AddBot(-80, 0); var victim = sim.AddBot(200, 0, 3001);
        var saved = SimSavegame.Write(sim, true); mover.TeleStomp = !succeeds;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(succeeds, LineSpecials.Execute(sim, mover, LineSpecials.Teleport, 0));
        Assert.Equal(succeeds, victim.IsDead); Assert.Equal(succeeds ? 200 : -80, mover.X.ToDouble());
    }

    [Theory]
    [InlineData("TELESTOMP", true)]
    [InlineData("PASSMOBJ", false)]
    public void PlayerSecondaryReplacementCanRetainOrClearStockDefault(string bits, bool stomp)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply($"Thing 1\nBits = {bits}\n"));
        var player = sim.Players.Single(); Assert.Equal(stomp, player.TeleStomp); Assert.Equal(stomp, player.DefaultTeleStomp);
        var saved = SimSavegame.Write(sim, true); player.DefaultTeleStomp = player.TeleStomp = !stomp;
        SimSavegame.Apply(sim, saved); Assert.Equal(stomp, player.TeleStomp); Assert.Equal(stomp, player.DefaultTeleStomp);
    }

    [Fact]
    public void StockPlayerNeedsNoDefaultExtensionAndLegacySaveRestoresTrue()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single(); var saved = SimSavegame.Write(sim, true);
        Assert.Null(sim.CaptureState().Actors.Single().TeleStompDefaultsFlag);
        player.DefaultTeleStomp = player.TeleStomp = false;
        SimSavegame.Apply(sim, saved); Assert.True(player.DefaultTeleStomp); Assert.True(player.TeleStomp);
    }

    private static AuthoritySimulation Room(string text) => Start(DehackedPatch.Apply(text));
    private static AuthoritySimulation Start(DehackedPatchResult patch)
    {
        Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3004 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
