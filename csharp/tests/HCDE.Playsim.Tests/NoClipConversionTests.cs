using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoClipConversionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoClipPassesOneSidedWall(bool enabled)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var lines = geometry.Lines.ToArray();
        lines[6] = new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = -1 };
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = lines, Things = [new LevelThing { Type = 1, X = -64 }] });
        var player = sim.Players.Single(); player.NoClip = enabled;
        Assert.Equal(enabled, ActorPhysics.TryMove(sim, player, 64, 0, out _));
        Assert.Equal(enabled ? 64 : -64, player.X.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoClipPassesActorsAndPositionProbes(bool enabled)
    {
        var sim = Room(); var player = sim.Players.Single(); var obstacle = sim.AddBot(64, 0);
        player.NoClip = enabled;
        Assert.Equal(enabled, ActorPhysics.TryMove(sim, player, 64, 0, out _));
        player.X = obstacle.X; player.Y = obstacle.Y;
        Assert.Equal(enabled, ActorPhysics.CanOccupy(sim, player));
        Assert.Equal(enabled, ActorPhysics.FindZBlocker(sim, player) is null);
    }

    [Fact]
    public void OrdinaryNoClipRetainsFloorAndCeilingClipping()
    {
        var sim = Room(); var player = sim.Players.Single(); player.NoClip = true;
        player.Z = Fixed.FromInt(-20); ActorPhysics.FitToSector(sim, player, false);
        Assert.Equal(0, player.Z.Raw);
        player.Z = Fixed.FromInt(200); ActorPhysics.FitToSector(sim, player, false);
        Assert.Equal(128 - player.Height.ToDouble(), player.Z.ToDouble());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoClipSuppressesCrossingSpecials(bool enabled)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }], Sides = [new LevelSide { Sector = 0 }],
            Things = [new LevelThing { Type = 1, X = -64 }],
            Lines = [new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = 0,
                Special = 191, Arg1 = 1, Arg2 = 21, PlayerCross = true }] });
        var player = sim.Players.Single(); player.NoClip = enabled; player.RememberPosition();
        Assert.True(ActorPhysics.TryMove(sim, player, 64, 0, out _)); LineSpecials.ActivateCrossings(sim);
        Assert.Equal(!enabled, player.GodMode);
    }

    [Theory]
    [InlineData("noclip")]
    [InlineData("Actor.NOCLIP")]
    public void NativeFlagAndPlayerPropertyShareState(string name)
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(ActorSpecialActions.CallSpecial(player, 191, 0, 1, 19));
        Assert.True(AcsActorFlags.TryGet(player, name, out var enabled)); Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(player, name, false)); Assert.False(player.NoClip);
    }

    [Fact]
    public void SavedNoClipMovementReplaysAndOlderSaveClearsIt()
    {
        var sim = Room(); var player = sim.Players.Single(); var older = SimSavegame.Write(sim, true);
        player.NoClip = true; var saved = SimSavegame.Write(sim, true);
        ActorPhysics.TryMove(sim, player, 64, 0, out _); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); ActorPhysics.TryMove(sim, player, 64, 0, out _);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, older); Assert.False(player.NoClip);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void InvalidNoClipSaveRejectsBeforeMutation(int value)
    {
        var sim = Room(); var state = sim.CaptureState(); state.Actors[0].NoClipFlag = value;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.False(sim.Players.Single().NoClip);
    }

    [Fact]
    public void ResurrectionRestoresNoClipDefault()
    {
        var sim = Room(); var corpse = sim.AddBot(64, 0); corpse.Health = 0;
        corpse.States.Enter(corpse, ActorStateMachine.Corpse); corpse.NoClip = true;
        Assert.True(ThingRaise.Execute(sim, 17, corpse, 0, 0)); Assert.False(corpse.NoClip);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = -64 }] });
}
