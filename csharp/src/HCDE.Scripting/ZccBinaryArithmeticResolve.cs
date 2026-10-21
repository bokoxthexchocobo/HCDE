namespace HCDE.Scripting;

/// <summary><c>FxAddSub::Resolve</c> and <c>FxMulDiv::Resolve</c> from <c>codegen.cpp</c> (3309–3669).</summary>
public static class ZccBinaryArithmeticResolve
{
    public enum ResolveOutcome { Continue, FoldConstant, Error }

    public static bool CustomAdditionHook(bool hasHook, bool hookReturned) => hasHook && hookReturned;

    public static bool TextureIdPlusInteger(bool leftTextureId, bool rightInteger) =>
        leftTextureId && rightInteger;

    public static bool QuaternionBinary(bool leftQuat, bool rightQuat) => leftQuat && rightQuat;

    public static bool VectorBinaryCompatible(
        bool leftV2, bool leftV3, bool leftV4,
        bool rightV2, bool rightV3, bool rightV4) =>
        (leftV3 || leftV2) && rightV2
        || leftV3 && rightV3
        || leftV4 && rightV4;

    public static bool NumericBinary(bool leftNumeric, bool rightNumeric) => leftNumeric && rightNumeric;

    public static ResolveOutcome ClassifyAddSub(
        bool customHook,
        bool leftTextureId,
        bool rightInteger,
        bool leftQuat,
        bool rightQuat,
        bool leftV2,
        bool leftV3,
        bool leftV4,
        bool rightV2,
        bool rightV3,
        bool rightV4,
        bool bothNumeric)
    {
        if (customHook) return ResolveOutcome.Continue;
        if (TextureIdPlusInteger(leftTextureId, rightInteger)) return ResolveOutcome.Continue;
        if (QuaternionBinary(leftQuat, rightQuat)) return ResolveOutcome.Continue;
        if (VectorBinaryCompatible(leftV2, leftV3, leftV4, rightV2, rightV3, rightV4))
            return ResolveOutcome.Continue;
        if (bothNumeric) return ResolveOutcome.Continue;
        return ResolveOutcome.Error;
    }

    public static double FoldFloatBinary(char op, double v1, double v2)
    {
        if (op != '*' && v2 == 0) return double.NaN;
        return op switch
        {
            '+' => v1 + v2,
            '-' => v1 - v2,
            '*' => v1 * v2,
            '/' => v1 / v2,
            '%' => Math.IEEERemainder(v1, v2),
            _ => 0,
        };
    }

    public static int FoldIntBinary(char op, int v1, int v2)
    {
        if (op != '*' && v2 == 0) return int.MinValue;
        return op switch
        {
            '+' => v1 + v2,
            '-' => v1 - v2,
            '*' => v1 * v2,
            '/' => v1 / v2,
            '%' => v1 % v2,
            _ => 0,
        };
    }

    public static ResolveOutcome TryFoldBinaryConstants(
        bool bothConstant,
        bool isFloat,
        char op,
        double f1,
        double f2,
        int i1,
        int i2,
        out double foldedFloat,
        out int foldedInt)
    {
        foldedFloat = 0;
        foldedInt = 0;
        if (!bothConstant) return ResolveOutcome.Continue;
        if (isFloat)
        {
            foldedFloat = FoldFloatBinary(op, f1, f2);
            if (double.IsNaN(foldedFloat)) return ResolveOutcome.Error;
            return ResolveOutcome.FoldConstant;
        }
        foldedInt = FoldIntBinary(op, i1, i2);
        if (foldedInt == int.MinValue) return ResolveOutcome.Error;
        return ResolveOutcome.FoldConstant;
    }

    public static bool VectorModulusForbidden(char op, bool eitherVector) => op == '%' && eitherVector;

    public static bool VectorDivisionRequiresVectorLeft(char op, bool rightVector) => op == '/' && rightVector;

    public static bool QuatTimesVecOrQuat(char op, bool leftQuat, bool rightV3, bool rightQuat) =>
        op == '*' && leftQuat && (rightV3 || rightQuat);

    public static bool VectorScaleByNumeric(char op, bool leftVector, bool leftQuat, bool rightNumeric) =>
        op == '*' && (leftVector || leftQuat) && rightNumeric;

    public static bool NumericScaleVector(char op, bool rightVector, bool rightQuat, bool leftNumeric) =>
        op == '*' && (rightVector || rightQuat) && leftNumeric;

    public static bool NeedsFloatCastForIntegerScale(bool isInteger) => isInteger;

    public static ResolveOutcome ClassifyMulDivVector(
        char op,
        bool leftVector,
        bool rightVector,
        bool leftQuat,
        bool rightQuat,
        bool leftV3,
        bool rightV3,
        bool leftNumeric,
        bool rightNumeric)
    {
        if (!leftVector && !rightVector && !leftQuat && !rightQuat)
            return ResolveOutcome.Continue;

        if (VectorDivisionRequiresVectorLeft(op, rightVector))
            return ResolveOutcome.Error;

        if (op == '*' || op == '/')
        {
            if (QuatTimesVecOrQuat(op, leftQuat, rightV3, rightQuat))
                return ResolveOutcome.Continue;
            if (VectorScaleByNumeric(op, leftVector, leftQuat, rightNumeric))
                return ResolveOutcome.Continue;
            if (NumericScaleVector(op, rightVector, rightQuat, leftNumeric))
                return ResolveOutcome.Continue;
            if (op == '/')
                return ResolveOutcome.Continue;
        }

        if (VectorModulusForbidden(op, leftVector || rightVector))
            return ResolveOutcome.Error;

        return ResolveOutcome.Error;
    }

    public static ResolveOutcome ClassifyMulDiv(
        char op,
        bool leftVector,
        bool rightVector,
        bool leftQuat,
        bool rightQuat,
        bool leftV3,
        bool rightV3,
        bool leftNumeric,
        bool rightNumeric,
        bool bothConstant,
        bool isFloat,
        double f1,
        double f2,
        int i1,
        int i2,
        out double foldedFloat,
        out int foldedInt)
    {
        foldedFloat = 0;
        foldedInt = 0;
        if (!leftVector && !rightVector && !leftQuat && !rightQuat && leftNumeric && rightNumeric)
        {
            var fold = TryFoldBinaryConstants(bothConstant, isFloat, op, f1, f2, i1, i2, out foldedFloat, out foldedInt);
            if (fold != ResolveOutcome.Continue) return fold;
            return ResolveOutcome.Continue;
        }

        var vector = ClassifyMulDivVector(op, leftVector, rightVector, leftQuat, rightQuat, leftV3, rightV3, leftNumeric, rightNumeric);
        if (vector == ResolveOutcome.Error)
        {
            foldedFloat = 0;
            foldedInt = 0;
            return ResolveOutcome.Error;
        }

        return TryFoldBinaryConstants(bothConstant, isFloat, op, f1, f2, i1, i2, out foldedFloat, out foldedInt);
    }

    public static ResolveOutcome ClassifyAddSubEnd(
        bool bothConstant,
        bool isFloat,
        char op,
        double f1,
        double f2,
        int i1,
        int i2,
        out double foldedFloat,
        out int foldedInt)
    {
        return TryFoldBinaryConstants(bothConstant, isFloat, op, f1, f2, i1, i2, out foldedFloat, out foldedInt);
    }
}
