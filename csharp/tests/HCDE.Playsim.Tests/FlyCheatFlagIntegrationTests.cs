using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FlyCheatFlagIntegrationTests
{
    [Theory]
    [InlineData("flycheat")]
    [InlineData("FLYCHEAT")]
    [InlineData("Actor.flycheat")]
    [InlineData("Actor.FLYCHEAT")]
    public void NativeFlagNamesSetQueryAndClear(string name)
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, name, true));
        Assert.True(AcsActorFlags.TryGet(actor, name, out var enabled)); Assert.True(enabled);
        Assert.True(actor.FlyCheat); Assert.False(actor.Fly); Assert.False(actor.NoGravity);
        Assert.True(AcsActorFlags.TrySet(actor, name, false));
        Assert.True(AcsActorFlags.TryGet(actor, name, out enabled)); Assert.False(enabled);
    }

    [Fact]
    public void PropertyAndActorFlagShareMarkerWithoutAliasingFlight()
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorSpecialActions.CallSpecial(player, 191, 0, 1, 3);
        Assert.True(AcsActorFlags.TryGet(player, "FLYCHEAT", out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(player, "FLYCHEAT", false));
        Assert.True(player.Fly); Assert.True(player.NoGravity); Assert.False(player.FlyCheat);
    }

    [Fact]
    public void ScriptFlagSaveRestoresExactMarkerState()
    {
        var sim = Room(); var actor = sim.Actors.Single(a => a is not PlayerPawn);
        Assert.True(AcsActorFlags.TrySet(actor, "Actor.FLYCHEAT", true));
        var saved = SimSavegame.Write(sim, true);
        AcsActorFlags.TrySet(actor, "FLYCHEAT", false); SimSavegame.Apply(sim, saved);
        Assert.True(actor.FlyCheat); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ResurrectionResetsMarkerOnlyWhenSuccessful(bool archvile, bool blocked)
    {
        var sim = Room(); var corpse = sim.Actors.Single(a => a is not PlayerPawn);
        corpse.Health = 0; corpse.States.Enter(corpse, ActorStateMachine.Corpse);
        Assert.True(AcsActorFlags.TrySet(corpse, "FLYCHEAT", true));
        if (blocked) sim.Ceilings[0] = 10;
        Assert.Equal(!blocked, archvile
            ? ArchvileActions.TryRaise(sim, new Actor(), sim.Players.Single())
            : ThingRaise.Execute(sim, 17, corpse, 0, 0) == true);
        Assert.Equal(blocked, corpse.FlyCheat);
    }

    [Fact]
    public void MalformedMarkerValueRejectsArchive()
    {
        var sim = Room(); sim.Players.Single().FlyCheat = true;
        var saved = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-flycheat-state", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }] });
}
