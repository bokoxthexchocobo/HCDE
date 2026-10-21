namespace HCDE.Scripting.Tests;

public class ZccIdentifierResolveTests
{
    [Fact]
    public void LocalVariable_HighestPriority()
    {
        Assert.Equal(ZccIdentifierResolve.LocalDispatch.LocalVariable,
            ZccIdentifierResolve.ClassifyLocal(true, false, true, 1));
        Assert.Equal(ZccIdentifierResolve.ResolvePath.Local,
            ZccIdentifierResolve.PlanIdentifierResolve(
                ZccIdentifierResolve.LocalDispatch.LocalVariable,
                false, false, false, false, false, false, false, false, false, false, false));
    }

    [Fact]
    public void GlobalConstant_VersionGate() =>
        Assert.True(ZccIdentifierResolve.GlobalVersionAccessible(100, 200));

    [Fact]
    public void RejectsUserinfoCvar() =>
        Assert.True(ZccIdentifierResolve.UserinfoCvarForbidden(true));

    [Fact]
    public void MemberField_PrivateAccessDenied() =>
        Assert.True(ZccIdentifierResolve.PrivateMemberDenied(true, false));

    [Fact]
    public void MemberFunction_RequiresCallParens() =>
        Assert.True(ZccIdentifierResolve.MemberFunctionRequiresCall(true));
}
