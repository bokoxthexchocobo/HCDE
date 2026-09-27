using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeActorDefaultsTests
{
    [Fact]
    public void RemappedDehackedMonsterKeepsItsBrainAndDimensions()
    {
        var patch = DehackedPatch.Apply("Thing 12\nID # = 4000\n");
        var actor = Assert.Single(AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 4000 }] }, dehacked: patch).Actors);
        Assert.Equal(60, actor.Health);
        Assert.Equal(20, actor.Radius.ToDouble());
        Assert.Equal(MonsterAttack.Fireball, actor.Brain!.Attack);
    }
    [Theory]
    [InlineData(3004, 20, 20, 56)]
    [InlineData(9, 30, 20, 56)]
    [InlineData(65, 70, 20, 56)]
    [InlineData(3001, 60, 20, 56)]
    [InlineData(3002, 150, 30, 56)]
    [InlineData(3005, 400, 31, 56)]
    [InlineData(3003, 1000, 24, 64)]
    [InlineData(69, 500, 24, 64)]
    [InlineData(16, 4000, 40, 110)]
    [InlineData(7, 3000, 128, 100)]
    public void SpawnMatchesNativeDoomDefinitions(int type, int health, int radius, int height)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = type }] });
        var actor = Assert.Single(sim.Actors);
        Assert.Equal(health, actor.Health);
        Assert.Equal(radius, actor.Radius.ToDouble());
        Assert.Equal(height, actor.Height.ToDouble());
        Assert.Equal(type == 3005, actor.NoGravity);
    }

    [Fact]
    public void PartialPatchPreservesNativeDimensionsAndChainingDoesNotMutateBaseline()
    {
        var first = DehackedPatch.Apply("Thing 12\nHit points = 88\n\nFrame 47\nDuration = 7\n");
        var second = DehackedPatch.Apply("Thing 12\nHit points = 99\n\nFrame 47\nDuration = 9\n", first);
        Assert.Equal(88, first.Actors.Single(actor => actor.Index == 12).Health);
        Assert.Equal(7, first.States.Single(state => state.Index == 47).Tics);
        Assert.Equal(9, second.States.Single(state => state.Index == 47).Tics);
        var actor = Assert.Single(AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3001 }] }, dehacked: second).Actors);
        Assert.Equal(99, actor.Health);
        Assert.Equal(20, actor.Radius.ToDouble());
        Assert.Equal(56, actor.Height.ToDouble());
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(256, true)]
    public void PainChanceControlsStateWithoutSuppressingDamage(int chance, bool pain)
    {
        var actor = Assert.Single(AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 3001 }] }).Actors);
        actor.PainChance = chance;
        ActorDamage.Apply(actor, 1);
        Assert.Equal(59, actor.Health);
        Assert.Equal(pain, actor.States.Current == actor.PainState);
    }

    [Fact]
    public void HeldUseOnlyActivatesOnPress()
    {
        var pawn = new PlayerPawn();
        pawn.Pending = new PlayerCommand { Use = true }; pawn.Tick();
        Assert.True(pawn.UsePressed);
        pawn.Pending = new PlayerCommand { Use = true }; pawn.Tick();
        Assert.False(pawn.UsePressed);
        pawn.Tick();
        pawn.Pending = new PlayerCommand { Use = true }; pawn.Tick();
        Assert.True(pawn.UsePressed);
    }

    [Theory]
    [InlineData(26)]
    [InlineData(27)]
    [InlineData(28)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(34)]
    public void LockedDoorNeedsMatchingKeyAndUsesNativeSpeed(int special)
    {
        var line = new LevelLine { Special = special, SideFront = 0, SideBack = 1 };
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Format = MapDataFormat.DoomBinary, Lines = [line],
            Sectors = [new LevelSector { CeilingHeight = 128 }, new LevelSector { Index = 1 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }], Things = [new LevelThing { Type = 1 }],
        });
        var player = sim.Players.Single();
        Assert.False(LineSpecials.ActivateMapLine(sim, player, line, true));
        Assert.Equal(special, line.Special);
        player.Inventory.BlueKey = special is 26 or 32;
        player.Inventory.YellowKey = special is 27 or 34;
        player.Inventory.RedKey = special is 28 or 33;
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        LineSpecials.TickMotions(sim);
        Assert.Equal(2, sim.CeilingOf(1));
        Assert.Equal(special < 30 ? special : 0, line.Special);
    }
}
