namespace HCDE.Gamedata.Tests;

public class VmInfoActorState1041SliceTests
{
    [Fact]
    public void EvaluateCheckCallerType_SkipsWhenNoImplicitArgs()
    {
        var req = new VmInfoActorState1041Slice.CheckCallerTypeRequest(
            0, true, "A_Foo", "AActor", null, null, 0);
        Assert.Equal(
            VmInfoActorState1041Slice.CheckCallerTypeOutcome.Skip,
            VmInfoActorState1041Slice.EvaluateCheckCallerType(req));
    }

    [Fact]
    public void EvaluateCheckCallerType_BadPrototypeWhenFirstArgNotObjectPointer()
    {
        var req = new VmInfoActorState1041Slice.CheckCallerTypeRequest(
            1, false, "A_Foo", "AActor", "AActor", null, 0);
        Assert.Equal(
            VmInfoActorState1041Slice.CheckCallerTypeOutcome.BadPrototype,
            VmInfoActorState1041Slice.EvaluateCheckCallerType(req));
    }

    [Fact]
    public void EvaluateCheckCallerType_OkWhenMatches()
    {
        var req = new VmInfoActorState1041Slice.CheckCallerTypeRequest(
            1, true, "A_Foo", "AActor", "AActor", null, 0);
        Assert.Equal(
            VmInfoActorState1041Slice.CheckCallerTypeOutcome.Ok,
            VmInfoActorState1041Slice.EvaluateCheckCallerType(req));
    }

    [Fact]
    public void EvaluateCheckCallerType_NullCallerWhenSelfMissing()
    {
        var req = new VmInfoActorState1041Slice.CheckCallerTypeRequest(
            1, true, "A_Foo", "AActor", null, null, 0);
        Assert.Equal(
            VmInfoActorState1041Slice.CheckCallerTypeOutcome.NullCaller,
            VmInfoActorState1041Slice.EvaluateCheckCallerType(req));
    }

    [Fact]
    public void EvaluateCheckCallerType_InvalidClassWhenKindMismatch()
    {
        var req = new VmInfoActorState1041Slice.CheckCallerTypeRequest(
            1, true, "A_Foo", "AActor", "Pistol", null, 0);
        Assert.Equal(
            VmInfoActorState1041Slice.CheckCallerTypeOutcome.InvalidClass,
            VmInfoActorState1041Slice.EvaluateCheckCallerType(req));
    }

    [Fact]
    public void FormatCheckCallerMessages_MatchNativeTemplates()
    {
        Assert.Contains("Bad function prototype", VmInfoActorState1041Slice.FormatBadPrototypeMessage("A_X"));
        Assert.Contains("without valid caller", VmInfoActorState1041Slice.FormatNullCallerMessage("A_X", "AActor"));
        Assert.Contains("Invalid class Pistol", VmInfoActorState1041Slice.FormatInvalidClassMessage("Pistol", "A_X", "AActor"));
    }

    [Fact]
    public void PrepareStateReturnTracking_TracksPointerOnlyForStateReturn()
    {
        var noProto = VmInfoActorState1041Slice.PrepareStateReturnTracking(true, false, 0, 0);
        Assert.False(noProto.TrackStateReturnPointer);

        var intRet = VmInfoActorState1041Slice.PrepareStateReturnTracking(true, true, 1, 2);
        Assert.False(intRet.TrackStateReturnPointer);

        var stateRet = VmInfoActorState1041Slice.PrepareStateReturnTracking(true, true, 1, VmInfoActorState1041Slice.TypeStateReturn);
        Assert.True(stateRet.TrackStateReturnPointer);
    }

    [Fact]
    public void ResolveVmCallParamCount_PicksDefaultOrImplicitPath()
    {
        Assert.Equal(4, VmInfoActorState1041Slice.ResolveVmCallParamCount(true, 4, 3));
        Assert.Equal(3, VmInfoActorState1041Slice.ResolveVmCallParamCount(false, 0, 3));
    }

    [Fact]
    public void FillImplicitActionParams_WritesThreeSlotLayout()
    {
        var slots = new object?[3];
        VmInfoActorState1041Slice.FillImplicitActionParams(slots, 0, 3, "self", "owner", "info");
        Assert.Equal(["self", "owner", "info"], slots);
    }

    [Fact]
    public void SelectPspriteCallInfoPrefix_WeaponVsOverlay()
    {
        Assert.Equal("", VmInfoActorState1041Slice.SelectPspriteCallInfoPrefix(null, true, true));
        Assert.Equal("weapon ", VmInfoActorState1041Slice.SelectPspriteCallInfoPrefix(
            VmInfoActorState1041Slice.StatePsprite, true, true));
        Assert.Equal("overlay ", VmInfoActorState1041Slice.SelectPspriteCallInfoPrefix(
            VmInfoActorState1041Slice.StatePsprite, true, false));
    }

    [Fact]
    public void FormatCallActionAbortStackTrace_WithAndWithoutOwner()
    {
        Assert.Equal("Called from state Idle\n", VmInfoActorState1041Slice.FormatCallActionAbortStackTrace("Idle", null, ""));
        Assert.Equal(
            "Called from weapon state Fire in Weapon\n",
            VmInfoActorState1041Slice.FormatCallActionAbortStackTrace("Fire", "Weapon", "weapon "));
    }

    [Fact]
    public void ShouldInvokeCallAction_RequiresBinding()
    {
        Assert.False(VmInfoActorState1041Slice.ShouldInvokeCallAction(false));
        Assert.True(VmInfoActorState1041Slice.ShouldInvokeCallAction(true));
    }
}
