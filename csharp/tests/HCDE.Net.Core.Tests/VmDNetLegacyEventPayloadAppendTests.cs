namespace HCDE.Net.Core.Tests;

public class VmDNetLegacyEventPayloadAppendTests
{
    [Fact]
    public void AppendLegacyRunArgsNative_NamedMasksHighBit()
    {
        var sink = new VmDNetLegacyEventPayloadAppend.LegacyPayloadSink
        {
            Data = new byte[] { 3, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 },
            AppendLegacyStringFromCanonical = () => true,
        };
        Assert.True(VmDNetLegacyEventPayloadAppend.AppendLegacyRunArgsNative(sink, named: true));
    }

    [Fact]
    public void AppendLegacyEventPayloadNative_PauseZeroPayload()
    {
        var sink = new VmDNetLegacyEventPayloadAppend.LegacyPayloadSink();
        Assert.True(VmDNetLegacyEventPayloadAppend.AppendLegacyEventPayloadNative(
            VmDNetLegacyEventPayload.DemPause, sink));
    }

    [Fact]
    public void AppendLegacyEventPayloadNative_WarpCheatFields()
    {
        var sink = new VmDNetLegacyEventPayloadAppend.LegacyPayloadSink
        {
            Data = new byte[] { 1, 2, 3, 4, 5, 6 },
        };
        Assert.True(VmDNetLegacyEventPayloadAppend.AppendLegacyEventPayloadNative(
            (int)DemoCommand.WarpCheat, sink));
        Assert.Equal(6, sink.Output.Count);
    }
}
