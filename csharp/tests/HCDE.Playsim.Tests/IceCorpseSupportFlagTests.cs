using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceCorpseSupportFlagTests
{
    [Theory]
    [InlineData(false, true, 16)]
    [InlineData(true, false, 0)]
    [InlineData(true, true, 16)]
    [InlineData(false, false, 0)]
    public void FrozenNonPlayerUsesCorpseFlagForSupport(bool dead, bool corpseFlag, int expectedHeight)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }]
        });
        var support = sim.AddBot(0, 0, 3004); support.Brain = null;
        if (dead) support.Health = 0;
        support.Corpse = corpseFlag; support.Solid = true; support.Height = Fixed.FromInt(16);
        var mover = new Actor
        {
            IceCorpse = true, Solid = true, OnGround = true, SectorIndex = 0,
            Height = Fixed.FromInt(56), Radius = Fixed.FromInt(20)
        };
        ActorPhysics.Step(sim, mover);
        Assert.Equal(expectedHeight, mover.Z.ToDouble());
        Assert.True(mover.OnGround);
    }
}
