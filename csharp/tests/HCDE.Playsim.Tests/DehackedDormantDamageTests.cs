using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDormantDamageTests
{
    [Theory]
    [InlineData(1, DamageFlags.None, false)]
    [InlineData(ActorDamage.TelefragDamage, DamageFlags.None, false)]
    [InlineData(1, DamageFlags.BypassInvulnerability, false)]
    [InlineData(1, DamageFlags.Forced, true)]
    public void DormancyRequiresForcedDamageEvenForTelefrags(int damage, DamageFlags flags, bool hurt)
    {
        var actor = Spawn(DehackedPatch.Apply("Thing 2\nBits = dormant+6\n"));
        Assert.True(actor.Dormant); var health = actor.Health;
        var attacker = new Actor { Health = 100, Id = 99 };
        ActorDamage.Apply(actor, damage, attacker, flags);
        Assert.Equal(hurt ? health - damage : health, actor.Health);
        if (!hurt) { Assert.Null(actor.LastDamageSourceId); Assert.Null(actor.Brain!.TargetId); }
    }

    [Fact]
    public void ReplacingExtendedFlagsClearsDormancyAndPreservesBaseline()
    {
        var first = DehackedPatch.Apply("Thing 2\nBits = DORMANT\n");
        var second = DehackedPatch.Apply("Thing 2\nBits = CANSLIDE\n", first);
        Assert.True(Spawn(first).Dormant); Assert.False(Spawn(second).Dormant);
    }

    private static Actor Spawn(DehackedPatchResult patch) => Assert.Single(AuthoritySimulation.Start(new PlayLevel
    {
        Things = [new LevelThing { Type = 3004 }],
    }, dehacked: patch).Actors);
}
