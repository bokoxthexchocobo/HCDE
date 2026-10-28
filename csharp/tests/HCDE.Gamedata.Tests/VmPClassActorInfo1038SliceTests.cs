namespace HCDE.Gamedata.Tests;

public class VmPClassActorInfo1038SliceTests
{
    [Fact]
    public void ResolveEventHandlerReplacement_ReturnsSelfWhenMismatch()
    {
        var result = VmPClassActorInfo1038Slice.ResolveEventHandlerReplacement(
            eventHandlerMatched: true,
            handlerReplacementType: "Knight",
            selfType: "Baron",
            replaceeClientSide: true,
            replacementClientSide: false);
        Assert.True(result.EventHandlerFinal);
        Assert.True(result.ReturnSelf);
        Assert.Equal("Baron", result.ResolvedType);
    }

    [Fact]
    public void ShouldReturnSelfWithoutReplacement_WhenBothMissing()
    {
        Assert.True(VmPClassActorInfo1038Slice.ShouldReturnSelfWithoutReplacement(
            decorateReplacementMissing: true,
            lookSkill: false,
            skillReplaceName: ""));
    }

    [Fact]
    public void ResolveSkillThenDecorateReplacement_PrefersSkillName()
    {
        var resolved = VmPClassActorInfo1038Slice.ResolveSkillThenDecorateReplacement(
            lookSkill: true,
            skillReplaceName: "SKILL",
            decorateReplacementType: "DECOR",
            (type, _) => type + "_R");
        Assert.Equal("SKILL_R", resolved);
    }

    [Fact]
    public void EvaluateSkillReplacedBy_FailsWhenClassMissing()
    {
        var fail = VmPClassActorInfo1038Slice.EvaluateSkillReplacedBy(
            lookSkill: true,
            skillCount: 4,
            currentSkill: 1,
            typeName: "A",
            replacedByName: "B",
            replacementClassExists: false,
            clientSideMatch: true);
        Assert.True(fail.Failed);
    }

    [Fact]
    public void TryUpdateDamageFactor_UpdatesExisting()
    {
        var list = new List<(string, double)> { ("Fire", 1.0) };
        Assert.True(VmPClassActorInfo1038Slice.TryUpdateDamageFactor(list, "Fire", 0.5));
        Assert.Equal(0.5, list[0].Item2);
    }

    [Fact]
    public void TryUpdatePainChance_ClampsTo256()
    {
        var list = new List<(string, int)> { ("Hit", 1) };
        Assert.True(VmPClassActorInfo1038Slice.TryUpdatePainChance(list, "Hit", 300));
        Assert.Equal(256, list[0].Item2);
    }

    [Fact]
    public void ShouldPushPainChance_RejectsNegative()
    {
        Assert.False(VmPClassActorInfo1038Slice.ShouldPushPainChance(-1));
    }

    [Fact]
    public void FinalizeReplacementClientSide_RejectsMismatch()
    {
        var type = VmPClassActorInfo1038Slice.FinalizeReplacementClientSide(
            "Self", "Other", selfClientSide: false, candidateClientSide: true);
        Assert.Equal("Self", type);
    }

    [Fact]
    public void ClearSkillReplacementNamesAfterFailure_ClearsFlags()
    {
        var look = true;
        var name = "X";
        VmPClassActorInfo1038Slice.ClearSkillReplacementNamesAfterFailure(ref look, ref name);
        Assert.False(look);
        Assert.Equal("", name);
    }
}
