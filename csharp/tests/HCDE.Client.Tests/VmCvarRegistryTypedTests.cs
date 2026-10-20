namespace HCDE.Client.Tests;

public class VmCvarRegistryTypedTests
{
    [Fact]
    public void CreateTyped_FloatAndStringRoundTrip()
    {
        var reg = new VmCvarRegistry();
        reg.CreateTyped(VmCvarValueType.Float, "pitch", VmCvarValue.FromFloat(1.25), VmCvarFlags.Archive);
        reg.CreateTyped(VmCvarValueType.String, "title", VmCvarValue.FromString("hcde"), VmCvarFlags.Archive);
        var f = (VmFloatCvar)reg.FindTyped("pitch")!;
        var s = (VmStringCvar)reg.FindTyped("title")!;
        f.ForceSet(VmCvarValue.FromFloat(2.5), VmCvarValueType.Float);
        reg.SyncTypedEntry(f);
        Assert.Equal("2.5", reg.Find("pitch")!.Value);
        Assert.Equal("hcde", s.Value);
    }
}
