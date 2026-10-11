using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PoisonCloudPainTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CloudPainRollsNativePoisonStreamAndQueuesConfiguredHowl(bool configured)
    {
        var howls = 0;
        for (var seed = 0; seed < 32; seed++)
        {
            var sim = Room(seed); var actor = sim.AddBot(100, 0); actor.Brain = null;
            if (configured) actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Howl] = "howl" };
            var random = NativeStateRandom.Seed((uint)seed, 0x15ba5b61u);
            var howl = (NativeStateRandom.Next(ref random) & 255) < 128;
            var combat = sim.CombatRandomState;
            ActorDamage.Apply(actor, 1, damageType: "poisoncloud", inflictor: new Actor { ForcePain = true });
            Assert.Equal(random, sim.CaptureState().PoisonDamageRandomState);
            Assert.Equal(combat, sim.CombatRandomState); Assert.Equal(actor.PainState, actor.States.Current);
            if (!howl || !configured) { Assert.Empty(sim.PendingSoundRequests); continue; }
            howls++; var request = Assert.Single(sim.PendingSoundRequests);
            Assert.Equal("howl", request.Sound); Assert.Equal(4, request.Channel);
            Assert.Equal(0, request.Flags);
            Assert.True(request.OnlyIfSoundNotPlaying);
        }
        if (configured) Assert.True(howls > 0);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void NonMonstersAndSuppressedPainDoNotDrawPoison(bool monster, bool noPain)
    {
        var sim = Room(42); var actor = sim.AddBot(100, 0); actor.Brain = null;
        actor.IsMonster = monster; actor.NoPain = noPain;
        ActorDamage.Apply(actor, 1, damageType: "PoisonCloud", inflictor: new Actor { ForcePain = true });
        Assert.Null(sim.CaptureState().PoisonDamageRandomState);
    }

    [Fact]
    public void SaveRestoresPoisonDrawSequenceAndRejectsRecursivePriorVersion()
    {
        var sim = Room(42); var legacy = SimSavegame.Write(sim);
        sim.NextPoisonDamageByte(); var bytes = SimSavegame.Write(sim);
        Assert.Equal(157, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var next = sim.NextPoisonDamageByte(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(bytes, SimSavegame.Write(sim)); Assert.Equal(next, sim.NextPoisonDamageByte());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 157);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-poisondamagerandom-header", error);
        SimSavegame.Apply(sim, legacy); Assert.Null(sim.CaptureState().PoisonDamageRandomState);
    }

    private static AuthoritySimulation Room(int seed) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: seed);
}
