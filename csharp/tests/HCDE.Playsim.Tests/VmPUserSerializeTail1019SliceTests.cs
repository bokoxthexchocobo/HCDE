namespace HCDE.Playsim.Tests;

public class VmPUserSerializeTail1019SliceTests
{
    [Fact]
    public void SafeposUsesLegacyKey_MatchesNativeAlias()
    {
        Assert.True(VmPUserSerializeTail1019Slice.SafeposUsesLegacyKey("safepos"));
        Assert.False(VmPUserSerializeTail1019Slice.SafeposUsesLegacyKey("LastSafePos"));
    }

    [Fact]
    public void SerializeTailArcKeys_IncludesFragsAndPsprites()
    {
        Assert.Contains(VmPUserSerializeTail1019Slice.SerializeTailArcKeys, k =>
            VmPUserSerializeTail1019Slice.SerializeTailUsesFragsArray(k));
        Assert.Contains(VmPUserSerializeTail1019Slice.SerializeTailArcKeys, k =>
            VmPUserSerializeTail1019Slice.SerializeTailUsesPsprites(k));
    }

    [Fact]
    public void PlayerInfoVmExportCount_MatchesNativeDefineBlock()
    {
        Assert.Equal(72, VmPUserSerializeTail1019Slice.PlayerInfoVmExportCount);
        Assert.True(VmPUserSerializeTail1019Slice.TryGetPlayerInfoVmExportName(0, out var first));
        Assert.Equal("mo", first);
        Assert.True(VmPUserSerializeTail1019Slice.TryGetPlayerInfoVmExportName(71, out var last));
        Assert.Equal("BlendA", last);
    }

    [Fact]
    public void PlayerInfoExportUsesNamedAlias_FlagsInconsistent()
    {
        Assert.True(VmPUserSerializeTail1019Slice.PlayerInfoExportUsesNamedAlias("inconsistent"));
        Assert.False(VmPUserSerializeTail1019Slice.PlayerInfoExportUsesNamedAlias("waiting"));
    }

    [Fact]
    public void SerializeTailArcKeyCount_MatchesListLength()
    {
        Assert.Equal(VmPUserSerializeTail1019Slice.SerializeTailArcKeys.Count,
            VmPUserSerializeTail1019Slice.SerializeTailArcKeyCount);
        Assert.True(VmPUserSerializeTail1019Slice.TryGetSerializeTailArcKey(0, out var key));
        Assert.Equal("chickenpeck", key);
    }

    [Fact]
    public void MaxPlayersFragsArray_MatchesNativeMaxplayers()
    {
        Assert.Equal(64, VmPUserSerializeTail1019Slice.MaxPlayersFragsArray);
    }
}
