using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NoClipChargeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoclipChargeCrossesWallButStillSlamsTarget(bool noclip)
    {
        var (sim, soul, target) = Room(true); soul.NoClip = noclip;
        soul.Brain!.StartCharge(soul, target);
        for (var i = 0; i < 16 && soul.Brain.Charging; i++) soul.Tick();
        Assert.False(soul.Brain.Charging);
        Assert.Equal(noclip, target.Health < 1000);
        Assert.Equal(noclip, sim.CaptureState().MissileDamageRandomState.HasValue);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PositionProbeRetainsActorBlockingOnlyDuringCharge(bool charging)
    {
        var (sim, soul, target) = Room(false); soul.NoClip = true;
        if (charging) soul.Brain!.StartCharge(soul, target);
        soul.X = target.X; soul.Y = target.Y; soul.Z = target.Z;
        Assert.Equal(!charging, ActorPhysics.CanOccupy(sim, soul));
        var blocker = ActorPhysics.FindPositionBlocker(sim, soul, out var wall);
        Assert.Equal(charging ? target : null, blocker); Assert.False(wall);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChargingNoClipSkipsDestinationHeightRestriction(bool noclip)
    {
        var (sim, soul, target) = Room(false); soul.NoClip = noclip;
        soul.Brain!.StartCharge(soul, target); sim.Floors[1] = 96;
        target.X = Fixed.FromInt(110); target.Y = Fixed.FromInt(100);
        Assert.Equal(noclip, ActorPhysics.TryMove(sim, soul, 64, 0, out _));
    }

    [Fact]
    public void SavedNoclipChargeReplaysWallPassageAndSlam()
    {
        var (sim, soul, target) = Room(true); soul.NoClip = true;
        soul.Brain!.StartCharge(soul, target); soul.Tick();
        var saved = SimSavegame.Write(sim, true);
        Finish(); Assert.True(target.Health < 1000); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Finish(); Assert.Equal(expected, SimSavegame.Write(sim, true));
        void Finish() { for (var i = 0; i < 16 && soul.Brain!.Charging; i++) soul.Tick(); }
    }

    private static (AuthoritySimulation, Actor, PlayerPawn) Room(bool wall)
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var lines = geometry.Lines.ToArray();
        if (wall) lines[6] = new LevelLine { X1 = 0, Y1 = -128, X2 = 0, Y2 = 128, SideFront = 0, SideBack = -1 };
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = lines, Things = [new LevelThing { Type = 1, X = 64 }] });
        var soul = sim.AddBot(-64, 0, 3006); var target = sim.Players.Single(); target.Health = 1000; target.NoPain = true;
        return (sim, soul, target);
    }
}
