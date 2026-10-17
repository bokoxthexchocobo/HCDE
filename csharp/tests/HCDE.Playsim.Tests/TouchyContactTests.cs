using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TouchyContactTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SolidMoverKillsArmedMineEvenIfMineIsNotSolid(bool solidMine)
    {
        var (sim, mover, mine) = Room(); mine.Solid = solidMine; mine.Invulnerable = true;
        Assert.True(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.True(mine.IsDead); Assert.False(mine.Armed);
        Assert.Null(mine.DamageTypeReceived);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void UnarmedNonSentientAndMasterLinkedMinesDoNotDetonate(int policy)
    {
        var (sim, mover, mine) = Room();
        if (policy == 0) mine.Armed = false;
        if (policy == 1) mine.MasterId = mover.Id;
        if (policy == 2) mover.MasterId = mine.Id;
        ActorPhysics.TryMove(sim, mover, 20, 0, out _);
        Assert.False(mine.IsDead);
    }

    [Fact]
    public void SameClassNonPlayersDoNotTriggerEachOther()
    {
        var (sim, mover, mine) = Room(); mover.DefinitionDoomEdNum = mine.ClassDoomEdNum;
        ActorPhysics.TryMove(sim, mover, 20, 0, out _);
        Assert.False(mine.IsDead); Assert.True(mine.Armed);
    }

    [Theory]
    [InlineData(56, false, true)]
    [InlineData(57, false, false)]
    [InlineData(56, true, false)]
    [InlineData(55, true, true)]
    public void VerticalTouchRespectsHeightPassageBeforeInclusiveTouchTest(int height, bool canPass, bool dies)
    {
        var (sim, mover, mine) = Room(); mover.CanPass = canPass; mover.Height = Fixed.FromInt(56); mine.Z = Fixed.FromInt(height);
        ActorPhysics.TryMove(sim, mover, 20, 0, out _);
        Assert.Equal(dies, mine.IsDead);
    }

    [Fact]
    public void SentientMineDoesNotRequireArmedFlag()
    {
        var (sim, mover, mine) = Room(); mine.Armed = false; mine.SeeState = 0;
        Assert.True(mine.IsSentient());
        ActorPhysics.TryMove(sim, mover, 20, 0, out _);
        Assert.True(mine.IsDead);
    }

    [Fact]
    public void ThruActorsSkipsTouchDeath()
    {
        var (sim, mover, mine) = Room(); mover.ThruActors = true;
        Assert.True(ActorPhysics.TryMove(sim, mover, 20, 0, out _));
        Assert.False(mine.IsDead);
    }

    private static (AuthoritySimulation Sim, Actor Mover, Actor Mine) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1, X = -100 }],
        });
        var mover = sim.AddBot(0, 0, 3001); mover.Brain = null;
        var mine = sim.AddBot(50, 0, 3004); mine.Brain = null;
        mine.Touchy = mine.Armed = true; mine.SeeState = -1;
        return (sim, mover, mine);
    }
}
