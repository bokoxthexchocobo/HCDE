namespace HCDE.Playsim.Tests;

public class ArmorFullAbsorbBoundaryTests
{
    [Theory]
    [InlineData(9, 0, 9)]
    [InlineData(10, 0, 5)]
    [InlineData(11, 0, 5)]
    [InlineData(5, 4, 5)]
    [InlineData(6, 4, 1)]
    public void TotalCapAppliesOnlyAtOrAboveRemainingFullAllowance(int damage, int count, int expected)
    {
        Assert.Equal(expected, ActorDamage.AbsorbArmor(damage, 100, 50, 5, 10, count));
    }

    [Fact]
    public void ArmorAmountStillCapsFullAbsorptionBranch()
    {
        Assert.Equal(3, ActorDamage.AbsorbArmor(9, 3, 50, 5, 10, 0));
    }
}
