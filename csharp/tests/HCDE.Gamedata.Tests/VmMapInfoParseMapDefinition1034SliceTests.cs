namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMapDefinition1034SliceTests
{
    [Fact]
    public void DispatchSetFlag_ClearsBitWhenAssignZero()
    {
        var level = new VmMapInfoLevelFlags { Flags = 0xff };
        VmMapInfoParseMapDefinition1034Slice.DispatchMapFlagHandler(
            level,
            VmMapInfoFlagHandlerType.SetFlag,
            0x10,
            0,
            hasAssign: true,
            assignNumber: 0);
        Assert.Equal(0xefu, level.Flags);
    }

    [Fact]
    public void DispatchScFlags2_MergesHandlerMasks()
    {
        var level = new VmMapInfoLevelFlags();
        VmMapInfoParseMapDefinition1034Slice.DispatchMapFlagHandler(
            level,
            VmMapInfoFlagHandlerType.SetScFlags2,
            0x100,
            0xfffffeff,
            hasAssign: false,
            assignNumber: 0);
        Assert.Equal(0x100u, level.Flags2);
    }

    [Fact]
    public void ApplyClearFlag3_AndScFlags3_MirrorNativeArms()
    {
        var level = new VmMapInfoLevelFlags { Flags3 = 0xff };
        VmMapInfoParseMapDefinition1034Slice.DispatchMapFlagHandler(
            level,
            VmMapInfoFlagHandlerType.ClearFlag3,
            0x10,
            0x20,
            hasAssign: false,
            assignNumber: 0);
        Assert.Equal(0xefu | 0x20u, level.Flags3);

        level.Flags3 = 0;
        VmMapInfoParseMapDefinition1034Slice.DispatchMapFlagHandler(
            level,
            VmMapInfoFlagHandlerType.SetScFlags3,
            0x4,
            0xfffffffb,
            hasAssign: false,
            assignNumber: 0);
        Assert.Equal(0x4u, level.Flags3);
    }

    [Fact]
    public void ResolveCompatSetValue_OldFormatUsesCheckNumber()
    {
        var set = VmMapInfoParseMapDefinition1034Slice.ResolveCompatSetValue(
            formatType: 0,
            newFormatUsesCheckAssign: false,
            hasAssignFromCheck: false,
            numberFromMustGet: 0,
            oldFormatHadNumber: true,
            numberFromCheckNumber: 0);
        Assert.Equal(0, set);
    }

    [Fact]
    public void ApplyCompatHandler_ClearsWhenSetZero()
    {
        var level = new VmMapInfoLevelFlags { CompatFlags = 0xff, CompatFlags2 = 0xff };
        VmMapInfoParseMapDefinition1034Slice.ApplyCompatHandler(level, 0x10, 0x01, set: false);
        Assert.Equal(0xefu, level.CompatFlags);
        Assert.Equal(0x10u, level.CompatMask);
    }

    [Fact]
    public void TryDispatchRegisteredOption_RejectsOldFormatOnlyOption()
    {
        var opts = new[]
        {
            new VmMapInfoOptionRegistration { Name = "newopt", RequiresNewFormat = true }
        };
        Assert.Throws<InvalidOperationException>(() =>
            VmMapInfoParseMapDefinition1034Slice.TryDispatchRegisteredOption(
                "newopt",
                formatType: 0,
                opts,
                out _));
    }

    [Fact]
    public void TryDispatchRegisteredOption_MatchesKnownToken()
    {
        var opts = new[]
        {
            new VmMapInfoOptionRegistration { Name = "sky1", RequiresNewFormat = false }
        };
        Assert.True(VmMapInfoParseMapDefinition1034Slice.TryDispatchRegisteredOption(
            "SKY1",
            VmMapInfoParseMapDefinition1034Slice.FmtNew,
            opts,
            out var matched));
        Assert.Equal("sky1", matched!.Name);
    }

    [Fact]
    public void ShouldBreakMapBodyLoop_WhenCloseBraceSeen()
    {
        Assert.True(VmMapInfoParseMapDefinition1034Slice.ShouldBreakMapBodyLoop(closeBraceParsed: true));
        Assert.True(VmMapInfoParseMapDefinition1034Slice.ShouldEmitUnknownPropertyMessage(closeBraceParsed: false));
    }

    [Fact]
    public void HandlerTypeIsIgnore_SkipsMutation()
    {
        var level = new VmMapInfoLevelFlags { Flags = 0x55 };
        VmMapInfoParseMapDefinition1034Slice.DispatchMapFlagHandler(
            level,
            VmMapInfoFlagHandlerType.Ignore,
            0,
            0,
            hasAssign: false,
            assignNumber: 0);
        Assert.Equal(0x55u, level.Flags);
    }
}
