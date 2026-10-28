namespace HCDE.Scripting.Tests;

public class ZccExpressionSelfTests
{
    [Fact]
    public void TryResolve_RejectsOutsideMemberFunction()
    {
        Assert.False(ZccExpressionSelf.TryResolve(false, true, out var error));
        Assert.Contains("member function", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryResolve_AllowsMemberContext()
        => Assert.True(ZccExpressionSelf.TryResolve(true, true, out _));
}
