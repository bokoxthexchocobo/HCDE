namespace HCDE.Playsim.Tests;

public class SpawnDefenseMovementDefaultTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpawnCapturesMissingDefaultsBeforeCallbackMutations(bool mapSpawn)
    {
        var actor = new MutatingActor { Invulnerable = true, NoTeleport = true };
        ThingActivation.InitializeSpawn(actor, false, mapSpawn);
        Assert.Equal(1, actor.ResurrectionDefenseFlags);
        Assert.Equal(4, actor.ResurrectionMovementFlags);
        Assert.False(actor.Invulnerable); Assert.False(actor.NoTeleport);
        actor.Health = 0;
        ActorRaise.ReviveSupported(actor);
        Assert.True(actor.Invulnerable); Assert.True(actor.NoTeleport);
        Assert.False(actor.NoGravity);
    }

    [Fact]
    public void ExplicitClassMasksTakePrecedenceOverSpawnInstance()
    {
        var actor = new MutatingActor { Invulnerable = true, NoTeleport = true,
            ResurrectionDefenseFlags = 4, ResurrectionMovementFlags = 2 };
        ThingActivation.InitializeSpawn(actor, false);
        Assert.Equal(4, actor.ResurrectionDefenseFlags); Assert.Equal(2, actor.ResurrectionMovementFlags);
        actor.Health = 0; ActorRaise.ReviveSupported(actor);
        Assert.False(actor.Invulnerable); Assert.True(actor.NoRadiusDamage);
        Assert.False(actor.NoTeleport); Assert.True(actor.Floating);
    }

    private sealed class MutatingActor : Actor
    {
        public MutatingActor() { Brain = new MonsterBrain(MonsterAttack.Hitscan); }
        public override void BeginPlay()
        {
            base.BeginPlay(); Invulnerable = false; NoTeleport = false; NoGravity = true;
        }
    }
}
