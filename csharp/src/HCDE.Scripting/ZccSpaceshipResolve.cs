namespace HCDE.Scripting;

/// <summary><c>FxSpaceship::Resolve</c> from <c>codegen.cpp</c> (4704–4752).</summary>
public static class ZccSpaceshipResolve
{
    public const int TkLtEqGt = 0x200;
    public const int TkLtGtEq = 0x201;

    public enum VersionCheck
    {
        Ok,
        DeprecatedLtGtEq,
        Requires4151,
        NonNumericOperands,
    }

    public static VersionCheck OperatorVersionPolicy(int op, int version)
    {
        if (version >= ZccTypeCastResolve.MakeVersion(4, 15, 1))
        {
            if (op == TkLtGtEq) return VersionCheck.DeprecatedLtGtEq;
            return VersionCheck.Ok;
        }
        if (op == TkLtEqGt) return VersionCheck.Requires4151;
        return VersionCheck.Ok;
    }

    public static bool ShouldDeleteOnVersionError(VersionCheck check) => check == VersionCheck.Requires4151;

    public static bool ShouldWarnDeprecatedOperator(VersionCheck check) => check == VersionCheck.DeprecatedLtGtEq;

    public static bool BothNumeric(bool leftNumeric, bool rightNumeric) => leftNumeric && rightNumeric;

    public static VersionCheck ClassifyOperands(bool leftNumeric, bool rightNumeric) =>
        BothNumeric(leftNumeric, rightNumeric) ? VersionCheck.Ok : VersionCheck.NonNumericOperands;

    public static string NonNumericErrorOperatorName(int op) => op == TkLtEqGt ? "<=>" : "<>=";

    public static bool ShouldPromoteNumeric(bool leftNumeric, bool rightNumeric) => BothNumeric(leftNumeric, rightNumeric);

    public static int FoldSpaceshipCompare(double v1, double v2) =>
        v1 < v2 ? -1 : v1 > v2 ? 1 : 0;

    public static int? TryFoldConstantSpaceship(bool bothConstant, double v1, double v2) =>
        bothConstant ? FoldSpaceshipCompare(v1, v2) : null;

}
