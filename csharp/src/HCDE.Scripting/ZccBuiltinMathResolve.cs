namespace HCDE.Scripting;

/// <summary>Abs, atan2, new, and min/max resolve from <c>codegen.cpp</c> (5576–6092).</summary>
public static class ZccBuiltinMathResolve
{
    public const int RegtInt = 1;
    public const int RegtFloat = 2;
    public const int SideVirtual = 0;
    public const int SidePlainData = 1;
    public const string NameMin = "Min";
    public const string NameMax = "Max";

    public enum AbsOutcome
    {
        PassthroughBool,
        ErrorNonNumeric,
        Folded,
        PropagateType,
    }

    public static AbsOutcome ClassifyAbs(bool isBool, bool isNumeric, bool isConstant) =>
        isBool ? AbsOutcome.PassthroughBool :
        !isNumeric ? AbsOutcome.ErrorNonNumeric :
        isConstant ? AbsOutcome.Folded : AbsOutcome.PropagateType;

    public static int FoldAbsInt(int v) => Math.Abs(v);

    public static double FoldAbsFloat(double v) => Math.Abs(v);

    public static bool Atan2RequiresNumeric(bool yNumeric, bool xNumeric) => yNumeric && xNumeric;

    public static double FoldAtan2Degrees(double y, double x) =>
        Math.Atan2(y, x) * (180.0 / Math.PI);

    public static bool NeedsFloatCast(bool isConstant, int regType) =>
        !isConstant && regType != RegtFloat;

    public static IReadOnlyList<bool> PlanAtan2FloatCasts(bool yConst, int yReg, bool xConst, int xReg)
    {
        var plan = new List<bool>(2);
        plan.Add(NeedsFloatCast(yConst, yReg));
        plan.Add(NeedsFloatCast(xConst, xReg));
        return plan;
    }

public static int ResolveOuterSide(int functionSide, int classScopeSide) =>
        functionSide == SideVirtual ? classScopeSide : functionSide;

    public static bool NewScopeMismatch(int outerSide, int innerSide) =>
        outerSide != innerSide && innerSide != SidePlainData;

public readonly struct MinMaxCounts
    {
        public MinMaxCounts(int floatCount, int intCount, int uintCount)
        {
            FloatCount = floatCount;
            IntCount = intCount;
            UIntCount = uintCount;
        }

        public int FloatCount { get; }
        public int IntCount { get; }
        public int UIntCount { get; }
    }

    public static MinMaxCounts ClassifyMinMaxOperands(IEnumerable<(bool isFloat, bool isInteger, bool isUIntType, bool constPositive)> operands)
    {
        var floatCount = 0;
        var intCount = 0;
        var uintCount = 0;
        foreach (var op in operands)
        {
            if (op.isFloat) floatCount++;
            else if (op.isInteger)
            {
                intCount++;
                if (op.isUIntType || op.constPositive) uintCount++;
            }
            else
            {
                return new MinMaxCounts(0, 0, 0);
            }
        }

        return new MinMaxCounts(floatCount, intCount, uintCount);
    }

    public static string MinMaxValueType(MinMaxCounts counts) =>
        counts.FloatCount > 0 ? "float64" :
        counts.UIntCount == counts.IntCount ? "uint32" : "sint32";

    public static bool MinMaxNeedsIntToFloatCast(int regType) => regType == RegtInt;

    public static double PickMinMaxFloat(string kind, double best, double candidate) =>
        kind == NameMin ? Math.Min(best, candidate) : Math.Max(best, candidate);

    public static int PickMinMaxInt(string kind, int best, int candidate) =>
        kind == NameMin ? Math.Min(best, candidate) : Math.Max(best, candidate);

    public static (double best, IReadOnlyList<int> removedIndices) FoldMinMaxFloatConstants(
        string kind,
        IReadOnlyList<double> values,
        int firstConstIndex)
    {
        var best = values[firstConstIndex];
        var removed = new List<int>();
        for (var j = firstConstIndex + 1; j < values.Count; j++)
        {
            best = PickMinMaxFloat(kind, best, values[j]);
            removed.Add(j);
        }

        return (best, removed);
    }

    public static (int best, IReadOnlyList<int> removedIndices) FoldMinMaxIntConstants(
        string kind,
        IReadOnlyList<int> values,
        int firstConstIndex)
    {
        var best = values[firstConstIndex];
        var removed = new List<int>();
        for (var j = firstConstIndex + 1; j < values.Count; j++)
        {
            best = PickMinMaxInt(kind, best, values[j]);
            removed.Add(j);
        }

        return (best, removed);
    }

public static bool AllChoicesFoldedToSingle(int choiceCount) => choiceCount == 1;

    public static int? FindFirstConstantIndex(IReadOnlyList<bool> isConstant)
    {
        for (var i = 0; i < isConstant.Count; i++)
        {
            if (isConstant[i]) return i;
        }

        return null;
    }
}
