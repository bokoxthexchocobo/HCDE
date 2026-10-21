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

    public static double FoldFloatBinary(char op, double v1, double v2) =>
        op == '+' ? v1 + v2 : op == '-' ? v1 - v2 : op == '*' ? v1 * v2 : op == '/' ? v1 / v2 : Math.IEEERemainder(v1, v2);

    public static int FoldIntBinary(char op, int v1, int v2) =>
        op == '+' ? v1 + v2 : op == '-' ? v1 - v2 : op == '*' ? v1 * v2 : op == '/' ? v1 / v2 : v1 % v2;

    public static bool DivisionByZero(char op, double divisor) => op != '*' && divisor == 0;

    public static bool DivisionByZeroInt(char op, int divisor) => op != '*' && divisor == 0;

    public static bool VectorModulusForbidden(char op, bool eitherVector) => op == '%' && eitherVector;

    public static bool VectorDivisionRequiresVectorLeft(char op, bool rightVector) => op == '/' && rightVector;

    public static bool QuatTimesVecOrQuat(char op, bool leftQuat, bool rightV3, bool rightQuat) =>
        op == '*' && leftQuat && (rightV3 || rightQuat);

    public static bool VectorScaleByNumeric(char op, bool leftVector, bool leftQuat, bool rightNumeric) =>
        op == '*' && (leftVector || leftQuat) && rightNumeric;

    public static bool NumericScaleVector(char op, bool rightVector, bool rightQuat, bool leftNumeric) =>
        op == '*' && (rightVector || rightQuat) && leftNumeric;

    public static ResolveOutcome ClassifyMulDivVector(
        char op,
        bool leftVector,
        bool rightVector,
        bool leftQuat,
        bool rightQuat,
        bool leftV3,
        bool rightV3,
        bool leftNumeric,
        bool rightNumeric,
        bool rightInteger,
        bool leftInteger)
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
        }

        if (VectorModulusForbidden(op, leftVector || rightVector))
            return ResolveOutcome.Error;

        return op == '*' || op == '/' ? ResolveOutcome.Continue : ResolveOutcome.Error;
    }

    public static ResolveOutcome ClassifyAddSub(
        bool customHook,
        bool leftTextureId,
        bool rightInteger,
        bool leftQuat,
        bool rightQuat,
        bool vectorOk,
        bool bothNumeric)
    {
        if (customHook) return ResolveOutcome.Continue;
        if (leftTextureId && rightInteger) return ResolveOutcome.Continue;
        if (leftQuat && rightQuat) return ResolveOutcome.Continue;
        if (vectorOk) return ResolveOutcome.Continue;
        if (bothNumeric) return ResolveOutcome.Continue;
        return ResolveOutcome.Error;
    }
}
