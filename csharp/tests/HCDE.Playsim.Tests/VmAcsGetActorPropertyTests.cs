namespace HCDE.Playsim.Tests;

public class VmAcsGetActorPropertyTests
{
    [Fact]
    public void MissingActor_ReturnsZero() =>
        Assert.Equal(0, VmAcsGetActorProperty.MissingActorResult());

    [Fact]
    public void PlanGet_Health_IsHealthKind()
    {
        var plan = VmAcsGetActorProperty.PlanGet(VmAcsDoSetActorProperty.Aprop.Health);
        Assert.Equal(VmAcsGetActorProperty.GetKind.Health, plan.Kind);
    }

    [Fact]
    public void PlanCheck_Boolean_NormalizesValue()
    {
        var plan = VmAcsGetActorProperty.PlanCheck(VmAcsDoSetActorProperty.Aprop.Friendly);
        Assert.Equal(VmAcsGetActorProperty.CheckKind.BooleanEquality, plan.Kind);
        Assert.True(VmAcsGetActorProperty.BooleanCheckMatches(true, 1));
    }

    [Fact]
    public void LegacyRenderStyleIndex_MatchesTable()
    {
        ReadOnlySpan<int> styles = [1, 2, 99];
        ReadOnlySpan<int> indices = [10, 20, 30];
        Assert.Equal(20, VmAcsGetActorProperty.LegacyRenderStyleIndex(2, styles, indices));
    }

    [Fact]
    public void PlayerOnlyZero_WhenNotPlayer()
    {
        Assert.True(VmAcsGetActorProperty.PlayerOnlyZero(true, false));
    }
}
