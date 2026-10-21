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
    public void InvalidGlobalIdentifier_ErrorPath() =>
        Assert.True(ZccIdentifierResolve.InvalidMemberInClass(isFunction: false));

    [Fact]
    public void ShouldDeleteOnUnknownIdentifier() =>
        Assert.True(ZccIdentifierResolve.ShouldDeleteOnError(ZccIdentifierResolve.ResolvePath.Unknown));

    [Fact]
    public void StaticFunction_SeesGlobalIdentifiers() =>
        Assert.True(ZccIdentifierResolve.StaticFunctionSeesGlobal(functionIsStatic: true, hasSelfClass: false));

    [Fact]
    public void MemberField_PrivateAccessDenied() =>
        Assert.True(ZccIdentifierResolve.PrivateMemberDenied(true, false));

    [Fact]
    public void MemberFunction_RequiresCallParens() =>
        Assert.True(ZccIdentifierResolve.MemberFunctionRequiresCall(true));

    [Fact]
    public void FieldFromConstantDeclaration_Blocked() =>
        Assert.True(ZccIdentifierResolve.FieldFromConstantContext(inFunction: false));

    [Fact]
    public void VmFunctionConstant_VersionGate() =>
        Assert.True(ZccIdentifierResolve.VmFunctionConstantAllowed(40011100, ZccIdentifierResolve.Version411100));

    [Fact]
    public void ClassFunctionConstant_VersionGate() =>
        Assert.True(ZccIdentifierResolve.VmFunctionConstantAllowed(50000000, ZccIdentifierResolve.Version411100));

    [Fact]
    public void AmbiguousSelfMember_MarksUnsafe() =>
        Assert.True(ZccIdentifierResolve.MarkUnsafeContext(
            ZccIdentifierResolve.AmbiguousSelfMember(true, true, true, true)));

    [Fact]
    public void ClassFunction_RequiresParensInClass() =>
        Assert.True(ZccIdentifierResolve.FunctionUsedWithoutParens(isFunction: true));

    [Fact]
    public void NoGlobalLookup_StopsResolve() =>
        Assert.True(ZccIdentifierResolve.NoGlobalLookup(noglobal: true));

    [Fact]
    public void DeprecatedGlobal_WarnsWhenVersionOk() =>
        Assert.True(ZccIdentifierResolve.DeprecatedGlobalWarning(deprecated: true, versionOk: true));

    [Fact]
    public void MemberConstant_OnContainer() =>
        Assert.True(ZccIdentifierResolve.MemberConstantOnContainer(isConst: true));

    [Fact]
    public void MemberField_VersionGate() =>
        Assert.True(ZccIdentifierResolve.MemberFieldVersionOk(fieldVersion: 100, ctxVersion: 200));

    [Fact]
    public void ProtectedMember_DeniedOutsideDescendant() =>
        Assert.True(ZccIdentifierResolve.ProtectedMemberDenied(
            isProtected: true, hasClassCtx: true, hasTargetClass: true, descendant: false));
}
