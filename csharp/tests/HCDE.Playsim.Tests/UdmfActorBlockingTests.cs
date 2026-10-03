using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UdmfActorBlockingTests
{
    [Theory]
    [InlineData("blockmonsters=true;", "6", false, 2)]
    [InlineData("blockmonsters=true;", "FRIEND+6", true, 2)]
    [InlineData("blockfloaters=true;", "FLOAT+6", false, 0x40000)]
    [InlineData("blockfloaters=true;", "NOGRAVITY+6", true, 0x40000)]
    [InlineData("blockmonsters=false; blockfloaters=false;", "FLOAT+6", true, 0)]
    [InlineData("blockmonsters=true; blockfloaters=true;", "FRIEND+FLOAT+6", false, 0x40002)]
    public void UdmfBlockingFieldsReachActorMovement(string fields, string bits, bool expected, int flags)
    {
        var text = $"namespace=\"ZDoom\"; vertex {{x=0;y=-128;}} vertex {{x=0;y=128;}} linedef {{v1=0;v2=1;sidefront=0;sideback=1;{fields}}}";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var line = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Lines);
        Assert.Equal(flags, line.Flags & (LevelLine.BlockMonstersFlag | LevelLine.BlockFloatersFlag));
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines.Take(6).Append(line).ToArray(),
            Things = [new LevelThing { Type = 3004, X = -40 }],
        }, dehacked: DehackedPatch.Apply($"Thing 2\nBits = {bits}\n"));
        Assert.Equal(expected, ActorPhysics.TryMove(sim, Assert.Single(sim.Actors), 40, 0, out _));
    }
}
