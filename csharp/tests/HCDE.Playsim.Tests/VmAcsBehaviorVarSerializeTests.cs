namespace HCDE.Playsim.Tests;

public class VmAcsBehaviorVarSerializeTests
{
    [Fact]
    public void SerializeVarSetWriteBounds_AllZero_ReturnsEmpty()
    {
        var vars = new int[8];
        var (count, first, last) = VmAcsBehaviorVarSerialize.SerializeVarSetWriteBounds(vars, vars.Length);
        Assert.Equal(0, count);
        Assert.Equal(0, first);
        Assert.Equal(0, last);
    }

    [Fact]
    public void SerializeVarSetWriteBounds_TrimsLeadingTrailingZeros()
    {
        var vars = new[] { 0, 0, 5, 0, 7, 0, 0 };
        var (count, first, last) = VmAcsBehaviorVarSerialize.SerializeVarSetWriteBounds(vars, vars.Length);
        Assert.Equal(3, count);
        Assert.Equal(2, first);
        Assert.Equal(4, last);
    }

    [Fact]
    public void SerializeVarSetReadNative_CapsCountToMax()
    {
        var vars = new int[4];
        VmAcsBehaviorVarSerialize.SerializeVarSetReadNative(vars, max: 4, count: 10, first: 2);
        Assert.All(vars, v => Assert.Equal(0, v));
    }

    [Fact]
    public void SerializeVarsNative_WalksMapAndArrays()
    {
        var arrays = new List<int>();
        VmAcsBehaviorVarSerialize.SerializeVarsNative(
            tryBeginVariablesArray: () => true,
            endVariablesArray: () => { },
            serializeMapVarStore: () => { },
            numArrays: 2,
            serializeArrayStore: arrays.Add);
        Assert.Equal(new[] { 0, 1 }, arrays);
    }

    [Fact]
    public void ParseLocalArrayChunkNative_AccumulatesSizes() =>
        Assert.Equal(13, VmAcsBehaviorVarSerialize.ParseLocalArrayChunkNative(8, [3, 5], offset: 5));

    [Fact]
    public void ResetBehaviorCtorNative_ZerosCoreFields()
    {
        var fields = VmAcsBehaviorVarSerialize.ResetBehaviorCtorNative();
        Assert.Equal(-1, fields.LumpNum);
        Assert.Equal(0, fields.NumScripts);
    }

    [Fact]
    public void InitRejectsTinyLumpNative_Below32() =>
        Assert.True(VmAcsBehaviorVarSerialize.InitRejectsTinyLumpNative(16));

    [Fact]
    public void InitReadsAcsMagicNative_RequiresAcsPrefix()
    {
        Assert.True(VmAcsBehaviorVarSerialize.InitReadsAcsMagicNative("ACS"u8));
        Assert.False(VmAcsBehaviorVarSerialize.InitReadsAcsMagicNative("ZZZ"u8));
    }
}
