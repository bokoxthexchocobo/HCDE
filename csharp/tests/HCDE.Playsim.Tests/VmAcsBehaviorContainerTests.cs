namespace HCDE.Playsim.Tests;

public class VmAcsBehaviorContainerTests
{
    [Fact]
    public void LoadDefaultModulesNative_LoadsExistingModules()
    {
        var loaded = new List<string>();
        VmAcsBehaviorContainer.LoadDefaultModulesNative(
            ["a", "b"],
            n => n == "a",
            loaded.Add,
            _ => { });
        Assert.Equal(["a"], loaded);
    }

    [Fact]
    public void CheckAllGoodNative_FailsOnBadModule() =>
        Assert.False(VmAcsBehaviorContainer.CheckAllGoodNative([true, false]));

    [Fact]
    public void GetModuleIndexNative_ReturnsNullWhenOutOfRange() =>
        Assert.Null(VmAcsBehaviorContainer.GetModuleIndexNative(3, 3));

    [Fact]
    public void MarkLevelVarStringsNative_WalksModulesAndScripts()
    {
        var hits = new List<int>();
        VmAcsBehaviorContainer.MarkLevelVarStringsNative(
            2,
            hits.Add,
            () => hits.Add(-1),
            () => hits.Add(-2));
        Assert.Equal([0, 1, -1, -2], hits);
    }

    [Fact]
    public void SerializeModuleStatesNative_RejectsCountMismatch() =>
        Assert.Throws<InvalidOperationException>(() =>
            VmAcsBehaviorContainer.SerializeModuleStatesNative(2, 3, _ => { }));
}
