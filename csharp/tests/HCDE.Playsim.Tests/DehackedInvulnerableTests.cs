using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedInvulnerableTests
{
    [Theory]
    [InlineData("INVULNERABLE")]
    [InlineData("invulnerable+NOTELEPORT+6")]
    public void ExtendedInvulnerabilityBlocksNormalDamageButNotTelefrag(string bits)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nBits = {bits}\n");
        Assert.Empty(patch.Errors);
        var actor = Spawn(patch); var health = actor.Health;
        Assert.True(actor.Invulnerable); Assert.True(actor.Shootable);
        ActorDamage.Apply(actor, 1); Assert.Equal(health, actor.Health);
        ActorDamage.Apply(actor, ActorDamage.TelefragDamage); Assert.True(actor.IsDead);
    }

    [Fact]
    public void ReplacingExtendedSetClearsEarlierFlagAndPreservesBaseline()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = NOTELEPORT+INVULNERABLE\n");
        var second = DehackedPatch.Apply("Thing 2\nBits = INVULNERABLE\n", first);
        Assert.True(Spawn(first).NoTeleport);
        Assert.False(Spawn(second).NoTeleport); Assert.True(Spawn(second).Invulnerable);
        var third = DehackedPatch.Apply("Thing 2\nBits = NOTELEPORT\n", second);
        Assert.True(Spawn(third).NoTeleport); Assert.False(Spawn(third).Invulnerable);
    }

    [Fact]
    public void NumericFirstSetPreservesExtendedInvulnerability()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = INVULNERABLE\n");
        var second = DehackedPatch.Apply("Thing 2\nBits = 6\n", first);
        Assert.True(Spawn(second).Invulnerable);
        Assert.False(first.Actors.Single(actor => actor.Index == 2).BitsPatched);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
