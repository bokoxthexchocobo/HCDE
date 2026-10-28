namespace HCDE.Playsim.Tests;

public class VmAcsGlobalStringsInventoryTests
{
    [Fact]
    public void PresentNamedScript_WhenNegativeIdResolves()
    {
        Assert.Equal("script \"foo\"", VmAcsScriptPresentation.PresentNative(-1, _ => "foo"));
    }

    [Fact]
    public void ClearACSVars_PurgesGlobalsWhenAlsoGlobal()
    {
        var steps = new List<string>();
        VmAcsClearACSVars.ClearACSVarsNative(
            true,
            () => steps.Add("world"),
            () => steps.Add("global"),
            () => steps.Add("purge"),
            () => steps.Add("mark"),
            () => steps.Add("purge2"));
        Assert.Equal(["world", "global", "purge"], steps);
    }

    [Fact]
    public void DoUseInvNative_BypassesFrozenForPlayers()
    {
        Assert.True(VmAcsInventoryNative.DoUseInvNative(
            true,
            true,
            VmAcsInventoryNative.CfTotallyFrozen,
            () => true,
            out var cheats));
        Assert.Equal(VmAcsInventoryNative.CfTotallyFrozen, cheats);
    }

    [Fact]
    public void CheckInventoryNative_HealthMaxUsesActivator()
    {
        Assert.Equal(200, VmAcsInventoryNative.CheckInventoryNative(
            false,
            "Health",
            true,
            () => 200,
            () => 50,
            _ => false,
            _ => false,
            () => 0,
            () => 0,
            () => 0));
    }

    [Fact]
    public void ResolveAcsPlayerNative_RejectsReservedSlots()
    {
        Assert.False(VmAcsInventoryNative.ResolveAcsPlayerNative(
            1,
            x => x,
            8,
            slot => slot == 1,
            _ => true,
            out _));
    }

    [Fact]
    public void CollectGlobalStrings_SkipsZeroStackPointer()
    {
        var marked = 0;
        VmAcsCollectGlobalStrings.CollectFromStacksNative(
            [0, 3],
            (_, sp) => marked += sp,
            (_, _) => IntPtr.Zero,
            () => { },
            () => { },
            () => { });
        Assert.Equal(3, marked);
    }
}
