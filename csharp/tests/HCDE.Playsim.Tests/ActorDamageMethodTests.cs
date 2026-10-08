namespace HCDE.Playsim.Tests;

public class ActorDamageMethodTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, -7)]
    [InlineData(0, 9)]
    [InlineData(5, 0)]
    [InlineData(5, -7)]
    [InlineData(5, 9)]
    public void ProjectileDamageMethodsRespectAndClearExpressionsThroughActorReference(int constant, int result)
    {
        var missile = new ProjectileActor(new Actor(), ProjectileKind.Plasma) { Damage = constant };
        var calls = 0;
        missile.DamageExpression = _ => { calls++; return result; };
        Actor actor = missile;
        Assert.False(actor.IsZeroDamage());
        Assert.Equal(0, calls);
        Assert.Equal(result, missile.GetMissileDamage(0, 2));
        Assert.Equal(1, calls);

        actor.SetDamage(-3);
        Assert.Null(missile.DamageExpression);
        Assert.False(actor.IsZeroDamage());
        Assert.Equal(-3, actor.Damage);
        actor.SetDamage(7);
        Assert.Equal(14, missile.GetMissileDamage(0, 2));
        Assert.Equal(1, calls);
        actor.SetDamage(0);
        Assert.True(actor.IsZeroDamage());
        Assert.Equal(0, missile.GetMissileDamage(0, 2));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(-1, false)]
    [InlineData(int.MinValue, false)]
    [InlineData(int.MaxValue, false)]
    public void ConstantDamageSetterAndQueryPreserveSignedValues(int damage, bool zero)
    {
        var actor = new Actor { Damage = 17, Health = 100 };
        actor.SetDamage(damage);
        Assert.Equal(damage, actor.Damage);
        Assert.Equal(zero, actor.IsZeroDamage());
        Assert.Equal(100, actor.Health);
        actor.SetDamage(0);
        Assert.True(actor.IsZeroDamage());
        actor.Damage = -7;
        Assert.False(actor.IsZeroDamage());
    }
}
