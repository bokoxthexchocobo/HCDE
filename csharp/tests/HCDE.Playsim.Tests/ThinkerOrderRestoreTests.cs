using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ThinkerOrderRestoreTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IdentityCaptureAndSaveAutomaticallyIncludeOrder(bool structured)
    {
        var sim = Room(); var first = sim.AddBot(100, 0); var second = sim.AddBot(300, 0);
        first.Brain!.Enabled = second.Brain!.Enabled = false;
        sim.Thinkers.ChangeStatNum(first, ThinkerStat.Default);
        var bytes = structured ? SimSavegame.Write(sim.CaptureState(includeBotIdentities: true)) : Save(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.Equal(new[] { sim.Players.Single().Id, second.Id, first.Id }, state.ThinkerOrder!.Select(item => item.Id));
        Assert.Equal(bytes, SimSavegame.Write(state));
        Assert.Null(sim.CaptureState().ThinkerOrder);
        Assert.Null(sim.CaptureState(includeBotIdentities: true, includeThinkerOrder: false).ThinkerOrder);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LegacyIdentityRestoreCanBeResavedWithAutomaticOrder(bool established)
    {
        var sim = Room(); var bot = sim.AddBot(100, 0); bot.Brain!.Enabled = false;
        if (established) sim.Tick();
        var legacy = SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false);
        bot.Destroy(); sim.Tick();
        SimSavegame.Apply(sim, legacy);
        Assert.Equal(legacy, SimSavegame.Write(sim, includeBotIdentities: true, includeThinkerOrder: false));
        var upgraded = Save(sim);
        Assert.Equal(216, BinaryPrimitives.ReadUInt16LittleEndian(upgraded.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(upgraded, out var state, out var error), error);
        Assert.NotNull(state.ThinkerOrder);
        SimSavegame.Apply(sim, upgraded);
        Assert.Equal(upgraded, Save(sim));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RestoresNonIdOrderForSurvivingAndReconstructedBots(bool established, bool reconstruct)
    {
        var sim = Room();
        var first = sim.AddBot(100, 0); var second = sim.AddBot(300, 0);
        first.Brain!.Enabled = second.Brain!.Enabled = false;
        if (established) sim.Tick();
        sim.Thinkers.ChangeStatNum(first, first.StatNum);
        var bytes = Save(sim);
        Assert.Equal(216, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var spawned = new List<uint>(); sim.WorldThingSpawned += actor => spawned.Add(actor.Id);
        if (reconstruct) { second.Destroy(); sim.Tick(); }
        else sim.Thinkers.ChangeStatNum(second, second.StatNum);
        spawned.Clear();

        SimSavegame.Apply(sim, bytes);

        Assert.Equal(bytes, Save(sim));
        Assert.Empty(spawned);
        sim.Tick();
        Assert.Equal(new[] { second.Id, first.Id }, sim.Thinkers.ThinkersIn(ThinkerStat.Default).Cast<Actor>().Select(actor => actor.Id));
        Assert.Equal(established ? Array.Empty<uint>() : new[] { sim.Players.Single().Id, second.Id, first.Id }, spawned);
    }

    [Fact]
    public void MixedEstablishedAndFreshGroupsRestoreAndReplay()
    {
        var sim = Room(); var first = sim.AddBot(0, 200, 3004); var second = sim.AddBot(0, -200, 3001);
        sim.Tick(); sim.Thinkers.ChangeStatNum(first, first.StatNum);
        var fresh = sim.AddBot(300, 300, 3005);
        var dormant = sim.AddBot(500, 500); sim.Thinkers.ChangeStatNum(dormant, ThinkerStat.Info);
        var bytes = Save(sim);
        var expected = Enumerable.Range(0, 40).Select(_ => Advance(sim)).ToArray();
        second.Destroy(); fresh.Destroy(); dormant.Destroy(); sim.Tick();

        SimSavegame.Apply(sim, bytes);

        Assert.Equal(bytes, Save(sim));
        Assert.Equal(ThinkerStat.Info, sim.Actors.Single(actor => actor.Id == dormant.Id).StatNum);
        Assert.Equal(expected, Enumerable.Range(0, 40).Select(_ => Advance(sim)).ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void InvalidStructuredOrderIsRejectedBeforeMutation(int kind)
    {
        var sim = Room(); var bot = sim.AddBot(100, 0); var before = Save(sim);
        var state = sim.CaptureState(true, true); var item = state.ThinkerOrder!.Last();
        state.ThinkerOrder = kind switch
        {
            0 => [item, item],
            1 => [item with { Id = uint.MaxValue }],
            2 => [item with { StatNum = 128 }],
            _ => [item with { JustSpawned = !item.JustSpawned }]
        };
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, Save(sim)); Assert.False(bot.Destroyed);
    }

    [Theory]
    [InlineData(0, 216, "save-thinker-order-header")]
    [InlineData(4, int.MaxValue, "save-thinker-order-header")]
    [InlineData(16, 2, "save-thinker-order-value")]
    [InlineData(12, 128, "save-thinker-order-state")]
    public void MalformedBinaryOrderIsRejected(int offset, int value, string expectedError)
    {
        var sim = Room(); var bytes = Save(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(expectedError, error);
    }

    [Fact]
    public void MissingUnsupportedActorAndNonActorThinkersRejectBeforeRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); var bytes = Save(sim);
        player.Destroy(); sim.Tick(); var before = SimSavegame.Write(sim, true);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(before, SimSavegame.Write(sim, true));

        sim = Room(); bytes = Save(sim); sim.Thinkers.Add(new Thinker());
        Assert.Throws<InvalidOperationException>(() => Save(sim));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(0, sim.Thinkers.Clock.Tic);
    }

    [Fact]
    public void ProjectilePlacementAndUnlinkedActorsRoundTrip()
    {
        var sim = Room(); var bot = sim.AddBot(200, 0); bot.Brain!.Enabled = false;
        sim.Tick();
        var missile = sim.SpawnProjectile(bot, ProjectileKind.Plasma);
        sim.Thinkers.ChangeStatNum(missile, 120);
        sim.Thinkers.Remove(bot);
        var bytes = Save(sim);
        sim.Thinkers.Add(bot); sim.Tick();

        SimSavegame.Apply(sim, bytes);

        Assert.Equal(bytes, Save(sim));
        Assert.DoesNotContain(bot, sim.Thinkers.SnapshotOrder());
        var restored = sim.Actors.Single(actor => actor.Id == missile.Id);
        Assert.Equal(120, restored.StatNum); Assert.True(restored.JustSpawned);
        sim.Tick(); Assert.False(restored.JustSpawned);
        Assert.Contains(restored, sim.Thinkers.ThinkersIn(120));
    }

    [Fact]
    public void EmptyActorOrderRoundTrips()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var bytes = Save(sim); sim.AddBot(100, 0);
        SimSavegame.Apply(sim, bytes);
        Assert.Empty(sim.Thinkers.SnapshotOrder()); Assert.Equal(bytes, Save(sim));
    }

    [Fact]
    public void OrderCannotReferenceABotRemovedByIdentityMembershipRestore()
    {
        var sim = Room(); var bot = sim.AddBot(100, 0); var before = Save(sim);
        var state = sim.CaptureState(true, true);
        state.Actors.Single(pose => pose.Id == bot.Id).BotIdentity = null;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, Save(sim)); Assert.False(bot.Destroyed);
    }

    [Fact]
    public void NoncanonicalGroupOrderIsRejectedBeforeMutation()
    {
        var sim = Room(); sim.AddBot(100, 0); var before = Save(sim);
        var state = sim.CaptureState(true, true);
        state.ThinkerOrder = state.ThinkerOrder!.Reverse().ToArray();
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Equal(before, Save(sim));
    }

    [Fact]
    public void LegacyAndOrdinarySavesRemainUnchangedAndOrderRequiresIdentities()
    {
        var sim = Room(); sim.AddBot(100, 0);
        foreach (var bytes in new[] { SimSavegame.Write(sim), SimSavegame.Write(sim, true, false) })
        {
            Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
            Assert.Null(state.ThinkerOrder);
            Assert.Equal(bytes, SimSavegame.Write(state));
        }
        Assert.Throws<ArgumentException>(() => sim.CaptureState(false, true));
        Assert.Throws<ArgumentException>(() => SimSavegame.Write(sim, false, true));
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, includeBotIdentities: true);
    private static uint Advance(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }]
    });
}
