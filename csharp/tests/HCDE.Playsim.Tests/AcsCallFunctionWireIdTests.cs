namespace HCDE.Playsim.Tests;

public class AcsCallFunctionWireIdTests
{
    [Theory]
    [InlineData("GetActorVelX", 9)]
    [InlineData("CheckClass", 100)]
    [InlineData("DamageActor", 101)]
    [InlineData("LineAttack", 60)]
    public void WireTableMatchesNativeEacsOrder(string name, int wire)
    {
        Assert.True(AcsCallFunctionWireIds.NameToWire.TryGetValue(name, out var resolved));
        Assert.Equal(wire, resolved);
    }
}
