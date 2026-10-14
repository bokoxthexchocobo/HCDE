using System.Buffers.Binary;
using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapMonsterRestoreTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(3001)]
    public void ReconstructsDeadMonsterWithoutRepeatingDeathOrDrops(int type)
    {
        var sim = Room(type); var monster = Monster(sim);
        Assert.True(ActorDamage.Apply(monster, 1000, sim.Players.Single(), DamageFlags.Forced).Killed);
        var before = Save(sim); var deaths = monster.DeathCount;
        var expected = Enumerable.Range(0, 25).Select(_ => Step(sim)).ToArray();
        monster.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        Assert.Equal(deaths, Monster(sim).DeathCount); Assert.True(Monster(sim).Health <= 0);
        Assert.Equal(before, Save(sim)); Assert.Equal(expected, Enumerable.Range(0, 25).Select(_ => Step(sim)).ToArray());
    }

    [Fact]
    public void DuplicateMapOriginsAreRejectedBeforeMutation()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 200 }, new LevelThing { Type = 3004, X = 400 }]
        });
        var before = Save(sim); var state = sim.CaptureState(true);
        var monsters = state.Actors.Where(pose => pose.MapMonsterThingIndex.HasValue).ToArray();
        monsters[1].MapMonsterThingIndex = monsters[0].MapMonsterThingIndex;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state)); Assert.Equal(before, Save(sim));
    }

    [Theory]
    [InlineData(3004, false, false)]
    [InlineData(3004, false, true)]
    [InlineData(3004, true, false)]
    [InlineData(3004, true, true)]
    [InlineData(3001, false, false)]
    [InlineData(3001, false, true)]
    [InlineData(3001, true, false)]
    [InlineData(3001, true, true)]
    [InlineData(66, false, false)]
    [InlineData(66, false, true)]
    [InlineData(66, true, false)]
    [InlineData(66, true, true)]
    [InlineData(64, false, false)]
    [InlineData(64, false, true)]
    [InlineData(64, true, false)]
    [InlineData(64, true, true)]
    [InlineData(71, false, false)]
    [InlineData(71, false, true)]
    [InlineData(71, true, false)]
    [InlineData(71, true, true)]
    [InlineData(3006, false, false)]
    [InlineData(3006, false, true)]
    [InlineData(3006, true, false)]
    [InlineData(3006, true, true)]
    public void RebuildsMapMonsterAndReplaysActiveBrain(int type, bool established, bool remove)
    {
        var sim = Room(type); var monster = Monster(sim); monster.ThingId = 99;
        sim.Players.Single().Invulnerable = true;
        sim.Thinkers.ChangeStatNum(monster, 120);
        if (established) for (var i = 0; i < 8; i++) sim.Tick();
        var before = Save(sim); Assert.True(ArchiveLayerFixture.TrailerStart(before, 221) >= 16);
        var expected = Enumerable.Range(0, 40).Select(_ => Step(sim)).ToArray();
        SimSavegame.Apply(sim, before); monster = Monster(sim);
        monster.Destroy(); if (remove) sim.Tick(); SimSavegame.Apply(sim, before);
        var restored = Assert.IsType<Actor>(Monster(sim));
        Assert.Equal(2, restored.MapMonsterThingIndex); Assert.Equal(monster.Id, restored.Id);
        Assert.Equal(99, restored.ThingId); Assert.Equal(120, restored.StatNum); Assert.NotNull(restored.Brain);
        Assert.Equal(before, Save(sim)); Assert.Equal(expected, Enumerable.Range(0, 40).Select(_ => Step(sim)).ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ReconstructsAbsentBasicAndReplacementProfileBrains(int kind)
    {
        var sim = Room(); var monster = Monster(sim);
        monster.Brain = kind switch
        {
            0 => null,
            1 => new MonsterBrain(MonsterAttack.Fireball) { Enabled = false },
            _ => MonsterBrain.ForType(66)
        };
        var before = Save(sim); var expected = Enumerable.Range(0, 40).Select(_ => Step(sim)).ToArray();
        monster.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        Assert.Equal(before, Save(sim)); Assert.Equal(expected, Enumerable.Range(0, 40).Select(_ => Step(sim)).ToArray());
    }

    [Fact]
    public void PatchedMapDefaultsSurviveReconstruction()
    {
        var patch = DehackedPatch.Apply("Thing 2\nHit points = 88\nWidth = 786432\nHeight = 1572864\nMass = 123\nReaction time = 17\n");
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing
                { Type = 3004, X = 200, Z = 16, Health = 2, Gravity = 0.5, Friendly = true, Ambush = true, Dormant = true }]
        }, dehacked: patch);
        var before = Save(sim); Monster(sim).Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        var restored = Monster(sim);
        Assert.Equal(176, restored.ResurrectionHealth); Assert.Equal(Fixed.FromInt(12), restored.ResurrectionRadius);
        Assert.Equal(Fixed.FromInt(24), restored.ResurrectionHeight); Assert.Equal(123, restored.Mass);
        Assert.Equal(17, restored.SpawnReactionTime); Assert.Equal(16, restored.SpawnZOffset);
        Assert.True(restored.Dormant); Assert.True(restored.Friendly); Assert.True(restored.Ambush);
        Assert.Equal(before, Save(sim));
    }

    [Fact]
    public void ReconnectsPuffSourceProjectileOwnerAndActorPointers()
    {
        var sim = Room(); var monster = Monster(sim); var player = sim.Players.Single();
        player.TracerId = monster.Id; monster.MasterId = player.Id;
        var puff = sim.SpawnHitscanPuff(100, 0, 20); puff.DamageSource = monster;
        var missile = sim.SpawnProjectile(monster, ProjectileKind.Plasma); var before = Save(sim);
        monster.Destroy(); puff.Destroy(); missile.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        var restored = Monster(sim);
        Assert.Equal(monster.Id, player.TracerId); Assert.Equal(player.Id, restored.MasterId);
        Assert.Same(restored, sim.Actors.OfType<PuffActor>().Single().DamageSource);
        Assert.Same(restored, sim.Actors.OfType<ProjectileActor>().Single().Owner);
        Assert.Equal(before, Save(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NotificationsRespectFreshSpawn(bool established)
    {
        var sim = Room(); if (established) sim.Tick(); var before = Save(sim);
        var monster = Monster(sim); monster.Destroy(); sim.Tick(); var events = 0;
        sim.WorldThingSpawned += actor => { if (actor.Id == monster.Id) events++; };
        SimSavegame.Apply(sim, before); Assert.Equal(0, events); sim.Tick(); Assert.Equal(established ? 0 : 1, events);
    }

    [Fact]
    public void RestoringSnapshotWithoutMonsterRemovesRebuiltMonster()
    {
        var sim = Room(); var before = Save(sim); Monster(sim).Destroy(); sim.Tick(); var after = Save(sim);
        SimSavegame.Apply(sim, before); Assert.NotNull(Monster(sim)); SimSavegame.Apply(sim, after);
        Assert.DoesNotContain(sim.Actors, actor => actor.MapMonsterThingIndex.HasValue); Assert.Equal(after, Save(sim));
    }

    [Theory]
    [InlineData(0, 221, "save-map-monster-header")]
    [InlineData(4, -1, "save-map-monster-header")]
    [InlineData(12, -2, "save-map-monster-value")]
    public void RejectsMalformedTrailer(int offset, int value, string expected)
    {
        var bytes = Save(Room()); var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    [Fact]
    public void WrongMapOriginIsRejectedBeforeMutation()
    {
        var sim = Room(); var before = Save(sim); var state = sim.CaptureState(true);
        state.Actors.Single(pose => pose.MapMonsterThingIndex.HasValue).MapMonsterThingIndex = int.MaxValue;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(before, Save(sim));
    }

    [Fact]
    public void OrdinarySaveAndIdentityWithoutThinkerOrderRemainSupported()
    {
        var sim = Room(); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        Assert.All(state.Actors, pose => Assert.Null(pose.MapMonsterThingIndex)); Assert.Equal(bytes, SimSavegame.Write(state));
        bytes = SimSavegame.Write(sim, true, false); Monster(sim).Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(bytes, SimSavegame.Write(sim, true, false));
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, true);
    private static uint Step(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static Actor Monster(AuthoritySimulation sim) => sim.Actors.Single(actor => actor.MapMonsterThingIndex.HasValue);
    private static AuthoritySimulation Room(int type = 3004) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 0 }, new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 200, Id = 77 }]
    });
}
