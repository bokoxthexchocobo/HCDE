namespace HCDE.Playsim.Tests;

public class FreezeDeathClassificationTests
{
    [Fact]
    public void MonsterWithoutBrainUsesGenericFreezeDeath()
    {
        var actor = Victim();
        actor.IsMonster = true;
        Assert.Null(actor.Brain);
        Assert.True(ActorDamage.Apply(actor, 100, damageType: "Ice").Killed);
        Assert.Equal(3, actor.States.Current);
    }

    [Fact]
    public void OrdinaryActorDoesNotUseGenericFreezeDeath()
    {
        var actor = Victim();
        Assert.True(ActorDamage.Apply(actor, 100, damageType: "Ice").Killed);
        Assert.Equal(2, actor.States.Current);
    }

    [Fact]
    public void BrainAloneDoesNotGrantMonsterFreezeDeath()
    {
        var actor = Victim();
        actor.Brain = new MonsterBrain(MonsterAttack.Melee);
        Assert.False(actor.IsMonster);
        ActorDamage.Apply(actor, 100, damageType: "Ice");
        Assert.Equal(2, actor.States.Current);
    }

    [Fact]
    public void MonsterNoIceDeathStillBlocksGenericFreeze()
    {
        var actor = Victim();
        actor.IsMonster = true;
        actor.NoIceDeath = true;
        ActorDamage.Apply(actor, 100, damageType: "Ice");
        Assert.Equal(2, actor.States.Current);
    }

    private static Actor Victim() => new() { Health = 100, GenericFreezeDeath = 3 };
}
