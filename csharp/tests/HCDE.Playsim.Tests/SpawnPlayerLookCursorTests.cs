using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnPlayerLookCursorTests
{
    [Theory]
    [InlineData(0, 24, 52)]
    [InlineData(1, 61, 37)]
    [InlineData(42, 25, 47)]
    public void ExplicitFactoryStreamInitializesMonstersInSpawnOrder(int seed, int first, int second)
    {
        var state = NativeStateRandom.Seed((uint)seed, 0x05ec38a2u);
        uint Random() => NativeStateRandom.Next(ref state) & 255;
        var actors = ActorSpawner.Spawn(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001 }]
        }, new ThinkerCollection(), actorSpawnRandom: Random);
        Assert.Equal(first, actors[0].LastLookPlayerNumber);
        Assert.Equal(second, actors[1].LastLookPlayerNumber);
    }

    [Fact]
    public void NonMonsterConsumesOnlyDefaultBobPhaseByte()
    {
        var calls = 0; var actor = new Actor { LastLookPlayerNumber = 17, TidToHate = 9 };
        ActorSpawner.InitializePlayerLookCursor(actor, () => { calls++; return 255; });
        Assert.Equal(1, calls); Assert.Equal(17, actor.LastLookPlayerNumber); Assert.Equal(9, actor.TidToHate);
    }

    [Fact]
    public void MonsterConsumesCursorThenBobByteAndClearsHate()
    {
        var values = new Queue<uint>([255, 123]);
        var actor = new Actor { IsMonster = true, TidToHate = 9 };
        ActorSpawner.InitializePlayerLookCursor(actor, values.Dequeue);
        Assert.Equal(63, actor.LastLookPlayerNumber); Assert.Equal(0, actor.TidToHate); Assert.Empty(values);
    }

    [Fact]
    public void PlayerSpawnDoesNotConsumeMonsterCursorByte()
    {
        var values = new Queue<uint>([24, 118, 52]);
        var actors = ActorSpawner.Spawn(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004 }]
        }, new ThinkerCollection(), actorSpawnRandom: values.Dequeue);
        Assert.Equal(0, actors[0].LastLookPlayerNumber);
        Assert.Equal(54, actors[1].LastLookPlayerNumber); Assert.Empty(values);
    }

    [Fact]
    public void ExistingFactoryCallersKeepTheirDefaultCursor()
    {
        var actors = ActorSpawner.Spawn(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }]
        }, new ThinkerCollection());
        Assert.Equal(0, actors.Single().LastLookPlayerNumber);
    }
}
