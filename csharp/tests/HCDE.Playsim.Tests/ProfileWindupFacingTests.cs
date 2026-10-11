using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProfileWindupFacingTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(9)]
    [InlineData(65)]
    [InlineData(3001)]
    [InlineData(3005)]
    [InlineData(16)]
    [InlineData(3006)]
    [InlineData(71)]
    [InlineData(64)]
    [InlineData(84)]
    [InlineData(3002)]
    [InlineData(58)]
    [InlineData(3003)]
    [InlineData(69)]
    [InlineData(68)]
    [InlineData(7)]
    [InlineData(67)]
    public void NativeWindupEntryFacesEvenWhenChaseTurningIsDisabled(int type)
    {
        var (sim, hunter) = Room(type);
        hunter.Angle = BamAngle.FromDegrees(180); hunter.Ambush = true;
        hunter.PitchDegrees = 17;
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoDirectionTurn);
        Assert.Equal(MonsterMode.Windup, hunter.Brain.Mode);
        Assert.False(hunter.Ambush);
        Assert.Equal(0, hunter.Angle.ToDegrees(), 5);
        Assert.Equal(17, hunter.PitchDegrees);
    }

    [Theory]
    [InlineData(3001, 8)]
    [InlineData(3005, 5)]
    [InlineData(16, 18)]
    [InlineData(84, 10)]
    [InlineData(84, 24)]
    [InlineData(71, 5)]
    [InlineData(71, 10)]
    [InlineData(64, 18)]
    [InlineData(64, 58)]
    [InlineData(3002, 8)]
    [InlineData(58, 8)]
    [InlineData(3003, 8)]
    [InlineData(69, 8)]
    [InlineData(67, 30)]
    [InlineData(67, 75)]
    public void IntermediateFacingOccursOnItsNativeFrameBoundary(int type, int faceTic)
    {
        var (sim, hunter) = Room(type);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoDirectionTurn);
        for (var tic = 1; tic < faceTic; tic++) hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoDirectionTurn);
        hunter.Angle = BamAngle.FromDegrees(180); hunter.Ambush = true;
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoDirectionTurn);
        Assert.Equal(0, hunter.Angle.ToDegrees(), 5);
        Assert.False(hunter.Ambush);
    }

    [Fact]
    public void NonFacingWindupTickDoesNotInvokeFacingSideEffects()
    {
        var (sim, hunter) = Room(3001);
        hunter.Brain!.Tick(sim, hunter, MonsterChaseFlags.NoDirectionTurn);
        hunter.Angle = BamAngle.FromDegrees(180); hunter.Ambush = true;
        hunter.Brain.Tick(sim, hunter, MonsterChaseFlags.NoDirectionTurn);
        Assert.Equal(180, hunter.Angle.ToDegrees(), 5);
        Assert.True(hunter.Ambush);
    }

    private static (AuthoritySimulation, Actor) Room(int type)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = type is 3002 or 58 ? 40 : 500 }, new LevelThing { Type = type }],
        });
        var hunter = sim.Actors.Single(actor => actor.DoomEdNum == type);
        hunter.ReactionTime = 0;
        return (sim, hunter);
    }
}
