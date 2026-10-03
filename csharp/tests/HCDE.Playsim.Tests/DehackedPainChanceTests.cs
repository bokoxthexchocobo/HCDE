using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedPainChanceTests
{
    [Theory]
    [InlineData(256, 256)]
    [InlineData(32767, 32767)]
    [InlineData(32768, -32768)]
    [InlineData(65535, -1)]
    [InlineData(65536, 0)]
    [InlineData(-32769, 32767)]
    public void PatchNarrowsPainChanceToSignedSixteenBits(int supplied, int expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nPain chance = {supplied}\n");
        Assert.Equal(expected, patch.Actors.Single(actor => actor.Index == 2).PainChance);
        Assert.Equal(expected, Spawn(patch).PainChance);
    }

    [Theory]
    [InlineData(65536, false)]
    [InlineData(65792, true)]
    public void NarrowedPainChanceControlsDamageReaction(int supplied, bool pain)
    {
        var actor = Spawn(DehackedPatch.Apply($"Thing 2\nPain chance = {supplied}\n"));
        var health = actor.Health;
        ActorDamage.Apply(actor, 1);
        Assert.Equal(health - 1, actor.Health);
        Assert.Equal(pain, actor.States.Current == actor.PainState);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
