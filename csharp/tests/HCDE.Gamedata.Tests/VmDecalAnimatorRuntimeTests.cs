namespace HCDE.Gamedata.Tests;

public class VmDecalAnimatorRuntimeTests
{
    [Fact]
    public void FindAnimator_ReturnsLatestMatch()
    {
        var list = new List<VmDecalAnimator>
        {
            new() { Name = "A", Kind = VmDecalAnimatorKind.Fader, DecayStartTics = 1, DecayTimeTics = 2 },
            new() { Name = "a", Kind = VmDecalAnimatorKind.Fader, DecayStartTics = 3, DecayTimeTics = 4 }
        };
        Assert.Equal(3, VmDecalAnimatorRuntime.FindAnimator(list, "A")!.DecayStartTics);
    }

    [Fact]
    public void BuildThinkerSchedules_CombinerChainsChildren()
    {
        var fader = new VmDecalAnimator { Name = "f", Kind = VmDecalAnimatorKind.Fader, DecayStartTics = 5, DecayTimeTics = 10 };
        var combiner = new VmDecalAnimator { Name = "c", Kind = VmDecalAnimatorKind.Combiner, CombinedAnimatorNames = ["f"] };
        var byName = new Dictionary<string, VmDecalAnimator>(StringComparer.OrdinalIgnoreCase) { ["f"] = fader, ["c"] = combiner };
        var schedules = VmDecalAnimatorRuntime.BuildThinkerSchedules(combiner, 100, byName);
        var comb = Assert.Single(schedules);
        Assert.Equal(VmDecalThinkerKind.Combiner, comb.Kind);
        Assert.Single(comb.Children!);
    }

    [Fact]
    public void TranslationTable_ReusesMatchingRange()
    {
        var tables = new List<(uint, uint, int)>();
        var first = VmDecalTranslationTable.Register(tables, 112, 128);
        var second = VmDecalTranslationTable.Register(tables, 112, 128);
        Assert.Equal(first, second);
    }
}
