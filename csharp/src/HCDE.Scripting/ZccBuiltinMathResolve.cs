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
    public const string TypeFloat64 = "float64";
    public const string TypeUInt32 = "uint32";
    public const string TypeSInt32 = "sint32";
    public const string TypeUInt16 = "uint16";
    public const string TypeUInt8 = "uint8";
    public const string TypeBool = "bool";

    public enum AbsOutcome
    {
        PassthroughBool,
        ErrorNonNumeric,
        Folded,
        PropagateType,
    }

    public enum AbsResolveStep
    {
        PassthroughBool,
        ErrorNonNumeric,
        FoldedInt,
        FoldedFloat,
        PropagateType,
        ErrorUnexpectedReg,
    }

    public enum Atan2ResolveStep
    {
        ErrorNonNumeric,
        FoldedConstant,
        ApplyFloatCasts,
        Ready,
    }

    public enum NewResolveStep
    {
        ErrorNotClass,
        ErrorAbstract,
        ErrorScopeBarrier,
        OkNonConstant,
        OkConstant,
    }

    public enum MinMaxResolveStep
    {
        ErrorBadOperand,
        FloatDomain,
        IntDomain,
        FoldedAllConstants,
        ConstantFoldedInPlace,
        Ready,
    }

    public static AbsOutcome ClassifyAbs(bool isBool, bool isNumeric, bool isConstant) =>
        isBool ? AbsOutcome.PassthroughBool :
        !isNumeric ? AbsOutcome.ErrorNonNumeric :
        isConstant ? AbsOutcome.Folded : AbsOutcome.PropagateType;

    public static AbsResolveStep RunAbsResolve(bool isBool, bool isNumeric, bool isConstant, int regType)
    {
        if (isBool)
            return AbsResolveStep.PassthroughBool;
        if (!isNumeric)
            return AbsResolveStep.ErrorNonNumeric;
        if (!isConstant)
            return AbsResolveStep.PropagateType;
        if (regType == RegtInt)
            return AbsResolveStep.FoldedInt;
        if (regType == RegtFloat)
            return AbsResolveStep.FoldedFloat;
        return AbsResolveStep.ErrorUnexpectedReg;
    }

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

    public static Atan2ResolveStep RunAtan2Resolve(
        bool yNumeric,
        bool xNumeric,
        bool yConst,
        bool xConst,
        int yReg,
        int xReg)
    {
        if (!Atan2RequiresNumeric(yNumeric, xNumeric))
            return Atan2ResolveStep.ErrorNonNumeric;
        if (yConst && xConst)
            return Atan2ResolveStep.FoldedConstant;
        if (NeedsFloatCast(yConst, yReg) || NeedsFloatCast(xConst, xReg))
            return Atan2ResolveStep.ApplyFloatCasts;
        return Atan2ResolveStep.Ready;
    }

    public static Atan2ResolveStep RunAtan2VecResolve(bool isVector) =>
        isVector ? Atan2ResolveStep.Ready : Atan2ResolveStep.ErrorNonNumeric;

    public static int ResolveOuterSide(int functionSide, int classScopeSide) =>
        functionSide == SideVirtual ? classScopeSide : functionSide;

    public static bool NewScopeMismatch(int outerSide, int innerSide) =>
        outerSide != innerSide && innerSide != SidePlainData;

    public static NewResolveStep RunNewResolve(
        bool isClassPointer,
        bool isConstant,
        bool isAbstract,
        bool hasFunctionVariants,
        int functionSide,
        int classScopeSide,
        int innerSide)
    {
        if (!isClassPointer)
            return NewResolveStep.ErrorNotClass;
        if (!isConstant)
            return NewResolveStep.OkNonConstant;
        if (isAbstract)
            return NewResolveStep.ErrorAbstract;
        var outer = ResolveOuterSide(hasFunctionVariants ? functionSide : SideVirtual, classScopeSide);
        if (NewScopeMismatch(outer, innerSide))
            return NewResolveStep.ErrorScopeBarrier;
        return NewResolveStep.OkConstant;
    }

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

    public static bool IsUnsignedIntegerType(string valueType) =>
        valueType is TypeUInt32 or TypeUInt16 or TypeUInt8 or TypeBool;

    public static bool CountsAsUIntOperand(bool isUIntType, bool isConstant, int intValue) =>
        isUIntType || (isConstant && intValue > 0);

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

    public static MinMaxCounts ClassifyMinMaxOperand(
        bool isFloat,
        bool isInteger,
        string valueType,
        bool isConstant,
        int intValue,
        MinMaxCounts running)
    {
        if (isFloat)
            return new MinMaxCounts(running.FloatCount + 1, running.IntCount, running.UIntCount);
        if (!isInteger)
            return new MinMaxCounts(0, 0, 0);
        var uintBump = CountsAsUIntOperand(IsUnsignedIntegerType(valueType), isConstant, intValue) ? 1 : 0;
        return new MinMaxCounts(running.FloatCount, running.IntCount + 1, running.UIntCount + uintBump);
    }

    public static string MinMaxValueType(MinMaxCounts counts) =>
        counts.FloatCount > 0 ? TypeFloat64 :
        counts.UIntCount == counts.IntCount ? TypeUInt32 : TypeSInt32;

    public static bool MinMaxNeedsIntToFloatCast(int regType) => regType == RegtInt;

    public static IReadOnlyList<int> PlanMinMaxIntToFloatCastIndices(IReadOnlyList<int> regTypes)
    {
        var indices = new List<int>();
        for (var i = 0; i < regTypes.Count; i++)
        {
            if (MinMaxNeedsIntToFloatCast(regTypes[i]))
                indices.Add(i);
        }

        return indices;
    }

    public static MinMaxResolveStep RunMinMaxDomainPick(MinMaxCounts counts)
    {
        if (counts.FloatCount == 0 && counts.IntCount == 0)
            return MinMaxResolveStep.ErrorBadOperand;
        return counts.FloatCount > 0 ? MinMaxResolveStep.FloatDomain : MinMaxResolveStep.IntDomain;
    }

    public static double PickMinMaxFloat(string kind, double best, double candidate) =>
        kind == NameMin ? Math.Min(best, candidate) : Math.Max(best, candidate);

    public static int PickMinMaxInt(string kind, int best, int candidate) =>
        kind == NameMin ? Math.Min(best, candidate) : Math.Max(best, candidate);

    public static bool CompareMinMaxConstantFloat(string kind, ref double best, double candidate)
    {
        var updated = PickMinMaxFloat(kind, best, candidate);
        if (updated == best)
            return false;
        best = updated;
        return true;
    }

    public static bool CompareMinMaxConstantInt(string kind, ref int best, int candidate)
    {
        var updated = PickMinMaxInt(kind, best, candidate);
        if (updated == best)
            return false;
        best = updated;
        return true;
    }

    public static (double best, IReadOnlyList<int> removedIndices) FoldMinMaxFloatConstants(
        string kind,
        IReadOnlyList<double> values,
        int firstConstIndex)
    {
        var best = values[firstConstIndex];
        var removed = new List<int>();
        for (var j = firstConstIndex + 1; j < values.Count; j++)
        {
            CompareMinMaxConstantFloat(kind, ref best, values[j]);
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
            CompareMinMaxConstantInt(kind, ref best, values[j]);
            removed.Add(j);
        }

        return (best, removed);
    }

    public static bool AllChoicesFoldedToSingle(int choiceCount) => choiceCount == 1;

    public static bool ShouldSwapConstantToSecond(bool firstIsConstant, int choiceCount) =>
        firstIsConstant && choiceCount > 1;

    public static int? FindFirstConstantIndex(IReadOnlyList<bool> isConstant)
    {
        for (var i = 0; i < isConstant.Count; i++)
        {
            if (isConstant[i]) return i;
        }

        return null;
    }

    public static MinMaxResolveStep RunMinMaxResolve(
        string kind,
        IReadOnlyList<bool> isConstant,
        IReadOnlyList<bool> isFloat,
        IReadOnlyList<bool> isInteger,
        IReadOnlyList<int> regTypes,
        IReadOnlyList<string> valueTypes,
        IReadOnlyList<int> intValues,
        IReadOnlyList<double> floatValues,
        out string valueType,
        out bool swapConstantToSecond)
    {
        valueType = TypeSInt32;
        swapConstantToSecond = false;
        var counts = new MinMaxCounts(0, 0, 0);
        for (var i = 0; i < isFloat.Count; i++)
        {
            counts = ClassifyMinMaxOperand(
                isFloat[i],
                isInteger[i],
                valueTypes[i],
                isConstant[i],
                intValues[i],
                counts);
            if (counts.FloatCount == 0 && counts.IntCount == 0 && !isFloat[i] && !isInteger[i])
                return MinMaxResolveStep.ErrorBadOperand;
        }

        var domain = RunMinMaxDomainPick(counts);
        if (domain == MinMaxResolveStep.ErrorBadOperand)
            return domain;

        valueType = MinMaxValueType(counts);
        if (domain == MinMaxResolveStep.FloatDomain && counts.IntCount > 0)
            _ = PlanMinMaxIntToFloatCastIndices(regTypes);

        var firstConst = FindFirstConstantIndex(isConstant);
        if (firstConst == null)
            return MinMaxResolveStep.Ready;

        if (counts.FloatCount > 0)
            FoldMinMaxFloatConstants(kind, floatValues, firstConst.Value);
        else
            FoldMinMaxIntConstants(kind, intValues, firstConst.Value);

        if (isConstant.All(c => c))
            return MinMaxResolveStep.FoldedAllConstants;

        swapConstantToSecond = ShouldSwapConstantToSecond(firstConst == 0, isConstant.Count);
        return MinMaxResolveStep.ConstantFoldedInPlace;
    }
}
