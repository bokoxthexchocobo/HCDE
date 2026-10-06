using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpeciesJumpTests
{
    [Theory]
    [InlineData(58, "Demon", true)]
    [InlineData(58, "Spectre", false)]
    [InlineData(69, "BaronOfHell", true)]
    [InlineData(69, "HellKnight", false)]
    [InlineData(3001, "doomimp", true)]
    [InlineData(3001, "None", false)]
    public void SpeciesJumpUsesSupportedNativeMonsterAncestry(int type, string species, bool jumps)
    {
        var sim = Room(); var actor = sim.AddBot(0, 0, type);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0,
            StateAction: self => ActorJumpActions.CheckSpecies(self, 2, species)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(jumps ? 2 : 1, actor.States.Current);
    }

    [Fact]
    public void PlayerPointerChecksSelectedSpeciesAndNullDoesNotMatch()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        Assert.Equal(2, ActorJumpActions.CheckSpecies(actor, 2, "DoomPlayer", AcsActorPointer.Player1));
        Assert.Null(ActorJumpActions.CheckSpecies(actor, 2, "DoomPlayer"));
        Assert.Null(ActorJumpActions.CheckSpecies(actor, 2, "None", AcsActorPointer.Null));
        actor.Destroy();
        Assert.Null(ActorJumpActions.CheckSpecies(actor, 2, "DoomImp"));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 100 }],
    });
}
