using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RaiseStateOrderingTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RevivalRunsOnlyDestinationActionAfterRestoration(bool archvile, bool explicitRaise)
    {
        var sim = Room(); var corpse = sim.Actors[1]; var raiser = new Actor { Friendly = true };
        corpse.Health = 0; corpse.Killed = true; corpse.DeathDamageType = "Massacre";
        corpse.Dormant = true; corpse.Shootable = false; corpse.Solid = false;
        corpse.VelocityZ = Fixed.FromInt(2);
        corpse.RaiseState = explicitRaise ? 4 : -1;
        var entered = new List<int>();
        void Observe(Actor self, int state)
        {
            entered.Add(state);
            Assert.Equal(self.SpawnHealth(), self.Health);
            Assert.False(self.Corpse); Assert.False(self.Killed); Assert.False(self.Dormant);
            Assert.True(self.Shootable); Assert.True(self.Solid);
            Assert.Null(self.DeathDamageType); Assert.Equal(Fixed.FromInt(2), self.VelocityZ);
            Assert.Equal(MonsterMode.Raise, self.Brain!.Mode);
            Assert.True(self.Friendly);
        }
        corpse.States.Configure(corpse, [new(-1, 0, self => Observe(self, 0)),
            new(4, 0), new(6, 3), new(-1, 3), new(7, 0, self => Observe(self, 4))], 3);
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, raiser, corpse)
            : ActorRaiseActions.RaiseActor(raiser, corpse, 3));
        Assert.Equal(new[] { explicitRaise ? 4 : 0 }, entered);
        Assert.True(corpse.Friendly);
    }

    [Fact]
    public void BlockedRaiseDoesNotRunAnyStateActionsOrRestoreHealth()
    {
        var sim = Room(); var corpse = sim.Actors[1];
        corpse.Health = 0; corpse.RaiseState = 4; corpse.X = sim.Players.Single().X;
        var entered = 0;
        corpse.States.Configure(corpse, [new(-1, 0, _ => entered++), new(4, 0),
            new(6, 3), new(-1, 3), new(-1, 4, _ => entered++)], 3);
        Assert.False(ActorRaiseActions.RaiseSelf(corpse));
        Assert.Equal(0, entered); Assert.Equal(0, corpse.Health);
        Assert.True(corpse.Corpse); Assert.Equal(3, corpse.States.Current);
    }

    [Fact]
    public void OrdinaryHealthAssignmentRetainsItsSpawnTransition()
    {
        var actor = new Actor(); actor.Health = 0; var entered = 0;
        actor.States.Configure(actor, [new(-1, 0, _ => entered++), new(4, 0),
            new(6, 3), new(-1, 3)], 3);
        actor.Health = 100;
        Assert.Equal(1, entered); Assert.Equal(0, actor.States.Current);
        Assert.False(actor.Corpse);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public void LegacyDestinationSeesFinalFriendship(bool archvile, int flags)
    {
        var sim = Room(); var corpse = sim.Actors[1];
        var raiser = new Actor { Friendly = true, FriendPlayer = 2, TidToHate = 37, NoHatePlayers = true };
        corpse.Health = 0;
        var transfer = archvile || (flags & 1) != 0;
        var calls = 0;
        corpse.States.Configure(corpse, [new(-1, 0, self =>
        {
            calls++;
            Assert.Equal(transfer, self.Friendly);
            Assert.Equal(transfer ? 2 : 0, self.FriendPlayer);
            Assert.Equal(transfer ? 37 : 0, self.TidToHate);
            Assert.Equal(transfer, self.NoHatePlayers);
        }), new(4, 0), new(6, 3), new(-1, 3)], 3);
        Assert.True(archvile ? ArchvileActions.TryRaise(sim, raiser, corpse)
            : ActorRaiseActions.RaiseActor(raiser, corpse, flags));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void RevivalHelperRestoresFieldsWithoutEnteringState()
    {
        var sim = Room(); var corpse = sim.Actors[1]; corpse.Health = 0;
        var calls = 0;
        corpse.States.Configure(corpse, [new(-1, 0, _ => calls++), new(4, 0),
            new(6, 3), new(-1, 3)], 3);
        corpse.FullBright = true;
        ActorRaise.ReviveSupported(corpse);
        Assert.Equal(0, calls); Assert.Equal(3, corpse.States.Current);
        Assert.True(corpse.FullBright); Assert.False(corpse.Corpse);
        Assert.Equal(corpse.SpawnHealth(), corpse.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 3004, X = 20 }],
    });
}
