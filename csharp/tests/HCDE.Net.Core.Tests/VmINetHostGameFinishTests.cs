namespace HCDE.Net.Core.Tests;

public class VmINetHostGameFinishTests
{
    [Fact]
    public void IGetGamePortNative_ReturnsPort() =>
        Assert.Equal(5029, VmINetHostGameFinish.IGetGamePortNative(5029));

    [Fact]
    public void ReadVerificationErrorNative_EngineMismatch()
    {
        var msg = VmINetHostGameFinish.ReadVerificationErrorNative(new ReadVerificationErrorContext
        {
            Stream = [1, 2, 3, 4, 5, 6, 7],
            VeEngine = 1,
        });
        Assert.Contains("Engine mismatch", msg);
    }
}
