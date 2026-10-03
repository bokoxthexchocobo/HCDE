using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedSlideTests
{
    [Theory]
    [InlineData("CANSLIDE", true)]
    [InlineData("canslide+NOTELEPORT", true)]
    [InlineData("NOTELEPORT", false)]
    public void ExtendedFlagsControlSlideSelectionAtSpawn(string bits, bool expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n");
        Assert.Empty(patch.Errors);
        Assert.Equal(expected, Assert.Single(Room(patch).Actors).CanSlide);
    }

    [Fact]
    public void LaterExtendedAssignmentClearsSlideWithoutMutatingBaseline()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = CANSLIDE\n");
        var second = DehackedPatch.Apply("Thing 2\nBits = NOTELEPORT\n", first);
        Assert.True(Assert.Single(Room(first).Actors).CanSlide);
        Assert.False(Assert.Single(Room(second).Actors).CanSlide);
    }

    [Fact]
    public void PatchedMonsterUsesSlidePathAtWall()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 3004 }],
            Lines = [new LevelLine { X1 = 24, X2 = 24, Y1 = -128, Y2 = 128, SideBack = -1 }],
        }, dehacked: DehackedPatch.Apply("Thing 2\nBits = CANSLIDE\n"));
        var actor = Assert.Single(sim.Actors); actor.Brain!.Enabled = false;
        actor.VelocityX = Fixed.FromInt(1000); actor.VelocityY = Fixed.FromInt(20);
        sim.Tick();
        Assert.InRange(actor.X.ToDouble(), 0, 4);
        Assert.InRange(actor.Y.ToDouble(), 19.99, 20.01);
        Assert.Equal(0, actor.VelocityX.Raw);
    }

    [Fact]
    public void SlideDefaultsAndChecksumReflectActorFlag()
    {
        Assert.True(new PlayerPawn().CanSlide);
        Assert.False(new Actor().CanSlide);
        var first = Room(null); var second = Room(null);
        Assert.Single(first.Actors).CanSlide = true;
        first.Tick(); second.Tick();
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room(DehackedPatchResult? patch) => AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch);
}
