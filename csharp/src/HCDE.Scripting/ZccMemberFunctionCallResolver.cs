namespace HCDE.Scripting;

/// <summary>
/// Managed model of <c>FxMemberFunctionCall::Resolve</c> through the dynarray argument
/// preparation loop (<c>codegen.cpp</c> 8863–9262).
/// </summary>
public static class ZccMemberFunctionCallResolver
{
    private static readonly HashSet<string> TextureZeroArgMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "IsValid", "IsNull", "Exists", "SetInvalid", "SetNull",
    };

    private static readonly HashSet<string> SpriteZeroArgMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "IsValid", "IsEmpty", "IsFixed", "IsKeep", "Exists",
        "SetInvalid", "SetEmpty", "SetFixed", "SetKeep",
    };

    private static readonly HashSet<string> VectorUnaryMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Length", "LengthSquared", "Sum", "Unit", "Angle",
    };

    private static readonly HashSet<string> QuaternionUnaryMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Length", "LengthSquared", "Unit", "Conjugate", "Inverse",
    };

    public static ZccMemberFunctionCallPhase TryResolve(ZccMemberFunctionCallRequest request)
    {
        if (!ZccMemberFunctionCallResolve.RequiresEnclosingClass(request.HasEnclosingClass, out var error))
        {
            request.Error = error;
            return ZccMemberFunctionCallPhase.Failed;
        }

        if (!ZccMemberFunctionCallResolve.TryValidateNonNullArguments(
                request.Arguments ?? Array.Empty<object?>(), out _))
        {
            request.Error = "Empty function argument";
            return ZccMemberFunctionCallPhase.Failed;
        }

        ApplyStaticIdentifierSubstitution(request);

        if (request.SelfResolvedToError && request.ResolvedContainerTypeName is not null)
        {
            if (!TryApplyStaticClassRecovery(request, out var phase))
                return phase;
        }

        ApplyPointerPreprocess(request);

        if (request.SelfExpressionKind == ZccMemberSelfExpressionKind.Super)
        {
            if (!TryApplySuperBinding(request, out var phase))
                return phase;
        }

        if (TryApplyNumericBuiltin(request))
            return PhaseAfterBuiltin(request);

        if (TryApplySoundBuiltin(request))
            return PhaseAfterBuiltin(request);

        if (TryApplyTextureBuiltin(request))
            return PhaseAfterBuiltin(request);

        if (TryApplySpriteBuiltin(request))
            return PhaseAfterBuiltin(request);

        if (TryApplyVectorBuiltin(request))
            return PhaseAfterBuiltin(request);

        if (TryApplyQuaternionBuiltin(request))
            return PhaseAfterBuiltin(request);

        if (TryApplyStringBuiltin(request))
            return PhaseAfterBuiltin(request);

        if (TryApplyDynamicArrayBuiltin(request, out var dynPhase))
            return request.Error is not null ? ZccMemberFunctionCallPhase.Failed : dynPhase;

        if (TryApplyStaticArraySize(request, out var arrayPhase))
            return request.Error is not null ? ZccMemberFunctionCallPhase.Failed : arrayPhase;

        if (TryApplyMapMember(request, out var mapPhase))
            return request.Error is not null ? ZccMemberFunctionCallPhase.Failed : mapPhase;

        if (TryApplyClassIntrospection(request))
            return PhaseAfterBuiltin(request);

        return ZccMemberFunctionCallPhase.Continue;
    }

    private static void ApplyStaticIdentifierSubstitution(ZccMemberFunctionCallRequest request)
    {
        if (request.SelfExpressionKind != ZccMemberSelfExpressionKind.Identifier ||
            string.IsNullOrEmpty(request.StaticIdentifierName))
            return;

        if (ZccMemberFunctionCallResolve.TryMapStaticTypeName(
                request.StaticIdentifierName, out var container))
            request.ResolvedContainerTypeName = container;
    }

    private static bool TryApplyStaticClassRecovery(
        ZccMemberFunctionCallRequest request,
        out ZccMemberFunctionCallPhase phase)
    {
        request.StaticOnly = true;
        phase = ZccMemberFunctionCallPhase.Continue;

        if (!request.StaticContainerIsClass)
            return true;

        if (!request.HasEnclosingFunction)
        {
            request.Error = ZccMemberFunctionCallResolve.ConstantContextCallError(request.MethodName);
            phase = ZccMemberFunctionCallPhase.Failed;
            return false;
        }

        request.NoVirtual = request.EnclosingClassDescendsFromStaticContainer;
        if (!request.NoVirtual)
            return true;

        if (request.StaticContainerHasVirtualMethod)
        {
            request.StaticOnly = false;
            request.NoVirtual = true;
            request.ReboundSelfToEnclosingInstance = true;
            request.SelfValueKind = ZccMemberSelfValueKind.Pointer;
            return true;
        }

        request.NoVirtual = false;
        return true;
    }

    private static void ApplyPointerPreprocess(ZccMemberFunctionCallRequest request)
    {
        if (request.SelfValueKind != ZccMemberSelfValueKind.Pointer)
            return;

        if (ZccMemberFunctionCallResolve.ShouldMarkReadonlyConstPointer(
                request.IsReadonlyConstPointer, request.ScriptMajor, request.ScriptMinor, request.ScriptPatch))
            request.IsReadonlyConstPointer = true;

        if (request.NeedsOutVarDereferenceOnSelf)
            request.NeedsOutVarDereferenceOnSelf = true;
    }

    private static bool TryApplySuperBinding(
        ZccMemberFunctionCallRequest request,
        out ZccMemberFunctionCallPhase phase)
    {
        phase = ZccMemberFunctionCallPhase.Continue;
        if (!request.HasEnclosingFunction)
        {
            request.Error = ZccMemberFunctionCallResolve.ConstantContextCallError(request.MethodName);
            phase = ZccMemberFunctionCallPhase.Failed;
            return false;
        }

        if (string.IsNullOrEmpty(request.SuperParentTypeName))
        {
            request.Error = "Super requires a class type";
            phase = ZccMemberFunctionCallPhase.Failed;
            return false;
        }

        request.SelfExpressionKind = ZccMemberSelfExpressionKind.Identifier;
        request.SelfValueKind = ZccMemberSelfValueKind.Pointer;
        request.NoVirtual = true;
        return true;
    }

    private static bool TryApplyNumericBuiltin(ZccMemberFunctionCallRequest request)
    {
        if (request.SelfValueKind != ZccMemberSelfValueKind.Numeric)
            return false;
        if (!request.MethodName.Equals("ToVector", StringComparison.OrdinalIgnoreCase))
            return false;
        request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.NumericToVector;
        return true;
    }

    private static bool TryApplySoundBuiltin(ZccMemberFunctionCallRequest request)
    {
        if (request.SelfValueKind != ZccMemberSelfValueKind.Sound)
            return false;
        if (!AtLeastVersion(request, 4, 14, 1))
            return false;
        if (!request.MethodName.Equals("IsValid", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!ZccMemberFunctionCallResolve.TryValidateSoundIsValid(request.ArgumentCount, out var error))
        {
            request.Error = error;
            return true;
        }

        request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.SoundIsValidCompare;
        return true;
    }

    private static bool TryApplyTextureBuiltin(ZccMemberFunctionCallRequest request)
    {
        if (request.SelfValueKind != ZccMemberSelfValueKind.TextureId &&
            !(AtLeastVersion(request, 4, 14, 1) && request.SelfValueKind == ZccMemberSelfValueKind.TranslationId))
            return false;

        if (!TextureZeroArgMethods.Contains(request.MethodName))
            return false;

        if (!ZccMemberFunctionCallResolve.TryValidateZeroArgBuiltin(request.ArgumentCount, request.MethodName, out var error))
        {
            request.Error = error;
            return true;
        }

        request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.TextureCompareOrAssign;
        return true;
    }

    private static bool TryApplySpriteBuiltin(ZccMemberFunctionCallRequest request)
    {
        if (!AtLeastVersion(request, 4, 14, 1) || request.SelfValueKind != ZccMemberSelfValueKind.SpriteId)
            return false;
        if (!SpriteZeroArgMethods.Contains(request.MethodName))
            return false;
        if (!ZccSpriteMemberBuiltinValidation.TryValidateZeroArg(request.ArgumentCount, request.MethodName, out var error))
        {
            request.Error = error;
            return true;
        }

        request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.SpriteCompareOrAssign;
        return true;
    }

    private static bool TryApplyVectorBuiltin(ZccMemberFunctionCallRequest request)
    {
        if (request.SelfValueKind is not (ZccMemberSelfValueKind.Vector2 or ZccMemberSelfValueKind.Vector3))
            return false;

        if (VectorUnaryMethods.Contains(request.MethodName))
        {
            if (!ZccMemberVectorBuiltinValidation.TryValidateUnaryBuiltin(request.ArgumentCount, request.MethodName, out var error))
            {
                request.Error = error;
                return true;
            }

            request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.VectorUnaryBuiltin;
            return true;
        }

        if (!request.MethodName.Equals("PlusZ", StringComparison.OrdinalIgnoreCase) ||
            request.SelfValueKind != ZccMemberSelfValueKind.Vector3)
            return false;

        if (!ZccMemberVectorBuiltinValidation.TryValidatePlusZ(request.ArgumentCount, out var plusZError))
        {
            request.Error = plusZError;
            return true;
        }

        request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.VectorPlusZ;
        return true;
    }

    private static bool TryApplyQuaternionBuiltin(ZccMemberFunctionCallRequest request)
    {
        if (request.SelfValueKind != ZccMemberSelfValueKind.Quaternion)
            return false;

        if (QuaternionUnaryMethods.Contains(request.MethodName))
        {
            if (!ZccMemberVectorBuiltinValidation.TryValidateUnaryBuiltin(request.ArgumentCount, request.MethodName, out var error))
            {
                request.Error = error;
                return true;
            }

            request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.QuaternionUnaryBuiltin;
            return true;
        }

        request.RemappedSelfStructType = "QuaternionStruct";
        return false;
    }

    private static bool TryApplyStringBuiltin(ZccMemberFunctionCallRequest request)
    {
        if (request.SelfValueKind != ZccMemberSelfValueKind.String)
            return false;

        if (request.MethodName.Equals("Length", StringComparison.OrdinalIgnoreCase))
        {
            request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.StringLength;
            return true;
        }

        request.RemappedSelfStructType = "StringStruct";
        return false;
    }

    private static bool TryApplyDynamicArrayBuiltin(
        ZccMemberFunctionCallRequest request,
        out ZccMemberFunctionCallPhase phase)
    {
        phase = ZccMemberFunctionCallPhase.Continue;
        if (request.SelfValueKind != ZccMemberSelfValueKind.DynamicArray)
            return false;

        if (ZccDynArraySizeMember.IsSizeMember(request.MethodName))
        {
            request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.DynamicArraySizeRedirect;
            request.RemappedSelfStructType = request.DynArrayBackingTypeName;
            phase = ZccMemberFunctionCallPhase.Finished;
            return true;
        }

        if (request.DynArrayArguments is null)
            return true;

        foreach (var argument in request.DynArrayArguments)
        {
            if (ZccDynArrayMemberArgumentValidation.IsObjectPushArgument(request.MethodName, argument.Index) &&
                !ZccDynArrayMemberArgumentValidation.ValidateObjectElementArgument(
                    request.DynArrayElementIsObjectPointer,
                    argument.IsNullConstant,
                    argument.IsObjectPointer,
                    argument.AncestorMatches,
                    out var error))
            {
                request.Error = error;
                phase = ZccMemberFunctionCallPhase.Failed;
                return true;
            }

            if (argument.IsDynamicArrayValue)
            {
                if (!argument.ElementTypeMatches)
                {
                    request.Error = "Type mismatch in function argument";
                    phase = ZccMemberFunctionCallPhase.Failed;
                    return true;
                }

                if (!argument.CanRequestAddress)
                {
                    request.Error = "Unable to dereference array variable";
                    phase = ZccMemberFunctionCallPhase.Failed;
                    return true;
                }

                argument.PromotedToBackingPointer = true;
                if (request.SelfExpressionKind is ZccMemberSelfExpressionKind.StructMember
                    or ZccMemberSelfExpressionKind.ClassMember
                    or ZccMemberSelfExpressionKind.StackVariable)
                    argument.MemberFieldSwappedToBacking = true;
            }
            else if (argument.IsPointerValue)
            {
                if (!argument.PointerTypeMatchesElement)
                {
                    request.Error =
                        $"Type mismatch in function argument. Got {argument.ArgumentTypeName}, expected {request.DynArrayElementTypeName}";
                    phase = ZccMemberFunctionCallPhase.Failed;
                    return true;
                }
            }
        }

        request.RemappedSelfStructType = request.DynArrayBackingTypeName;
        phase = ZccMemberFunctionCallPhase.Continue;
        return true;
    }

    private static bool TryApplyStaticArraySize(
        ZccMemberFunctionCallRequest request,
        out ZccMemberFunctionCallPhase phase)
    {
        phase = ZccMemberFunctionCallPhase.Continue;
        if (request.SelfValueKind != ZccMemberSelfValueKind.StaticArray)
            return false;
        if (!request.MethodName.Equals("Size", StringComparison.OrdinalIgnoreCase))
            return false;

        if (request.ArgumentCount > 0)
        {
            request.Error = $"Too many parameters in call to {request.MethodName}";
            phase = ZccMemberFunctionCallPhase.Failed;
            return true;
        }

        if (!request.IsResizableStaticArray)
        {
            request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.StaticArraySizeConstant;
            phase = ZccMemberFunctionCallPhase.Finished;
            return true;
        }

        if (request.SelfExpressionKind is ZccMemberSelfExpressionKind.StructMember
            or ZccMemberSelfExpressionKind.ClassMember
            or ZccMemberSelfExpressionKind.GlobalVariable)
        {
            request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.StaticArrayResizableSizeMember;
            phase = ZccMemberFunctionCallPhase.Finished;
            return true;
        }

        request.Error = "Cannot retrieve size of array";
        phase = ZccMemberFunctionCallPhase.Failed;
        return true;
    }

    private static bool TryApplyMapMember(
        ZccMemberFunctionCallRequest request,
        out ZccMemberFunctionCallPhase phase)
    {
        phase = ZccMemberFunctionCallPhase.Continue;
        if (request.SelfValueKind != ZccMemberSelfValueKind.Map)
            return false;

        request.RemappedSelfStructType = request.MapBackingTypeName;
        if (request.MapArguments is null)
            return true;

        foreach (var argument in request.MapArguments)
        {
            if (request.MethodName.Equals("Insert", StringComparison.OrdinalIgnoreCase) &&
                argument.Index == 1 &&
                request.MapValueIsObjectPointer &&
                !ZccDynArrayMemberArgumentValidation.ValidateObjectElementArgument(
                    true,
                    argument.IsNullConstant,
                    argument.IsObjectPointer,
                    argument.AncestorMatches,
                    out var error))
            {
                request.Error = error;
                phase = ZccMemberFunctionCallPhase.Failed;
                return true;
            }

            if (argument.IsMapValue)
            {
                if (!argument.KeyTypeMatches || !argument.ValueTypeMatches)
                {
                    request.Error = "Type mismatch in function argument";
                    phase = ZccMemberFunctionCallPhase.Failed;
                    return true;
                }

                if (!argument.CanRequestAddress)
                {
                    request.Error = "Unable to dereference map variable";
                    phase = ZccMemberFunctionCallPhase.Failed;
                    return true;
                }

                argument.PromotedToBackingPointer = true;
                if (request.SelfExpressionKind is ZccMemberSelfExpressionKind.StructMember
                    or ZccMemberSelfExpressionKind.ClassMember
                    or ZccMemberSelfExpressionKind.StackVariable)
                    argument.MemberFieldSwappedToBacking = true;
            }
            else if (argument.IsPointerValue && !argument.PointerTypeMatchesValue)
            {
                request.Error =
                    $"Type mismatch in function argument. Got {argument.ArgumentTypeName}, expected {request.MapValueTypeName}";
                phase = ZccMemberFunctionCallPhase.Failed;
                return true;
            }
        }

        return true;
    }

    private static bool TryApplyClassIntrospection(ZccMemberFunctionCallRequest request)
    {
        if (request.ArgumentCount != 0)
        {
            request.Error = $"Incorrect number of parameters in call to {request.MethodName}";
            return true;
        }

        if (request.MethodName.Equals("GetParentClass", StringComparison.OrdinalIgnoreCase) &&
            request.SelfValueKind is ZccMemberSelfValueKind.Object or ZccMemberSelfValueKind.ClassPointer)
        {
            request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.GetParentClass;
            return true;
        }

        if (request.MethodName.Equals("GetClassName", StringComparison.OrdinalIgnoreCase) &&
            request.SelfValueKind is ZccMemberSelfValueKind.Object or ZccMemberSelfValueKind.ClassPointer)
        {
            request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.GetClassName;
            return true;
        }

        if (request.MethodName.Equals("IsAbstract", StringComparison.OrdinalIgnoreCase) &&
            request.SelfValueKind == ZccMemberSelfValueKind.ClassPointer)
        {
            request.BuiltinRewrite = ZccMemberBuiltinRewriteKind.IsAbstract;
            return true;
        }

        return false;
    }

    public static bool TryDescribeTextureBuiltin(string methodName, out ZccTextureSpriteBuiltinOp op)
    {
        op = default;
        switch (methodName.ToLowerInvariant())
        {
            case "isvalid":
                op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.GreaterThan, 0, false);
                return true;
            case "isnull":
                op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.Equal, 0, false);
                return true;
            case "exists":
                op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.GreaterOrEqual, 0, false);
                return true;
            case "setinvalid":
                op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.Assign, -1, true);
                return true;
            case "setnull":
                op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.Assign, 0, true);
                return true;
            default:
                return false;
        }
    }

    public static bool TryDescribeSpriteBuiltin(string methodName, out ZccTextureSpriteBuiltinOp op)
    {
        op = default;
        if (methodName.Equals("IsValid", StringComparison.OrdinalIgnoreCase))
        {
            op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.GreaterOrEqual, 0, false);
            return true;
        }

        if (methodName.Equals("Exists", StringComparison.OrdinalIgnoreCase))
        {
            op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.GreaterOrEqual, 3, false);
            return true;
        }

        if (ZccSpriteMemberBuiltinValidation.TryGetEqualityConstant(methodName, out var constant))
        {
            op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.Equal, constant, false);
            return true;
        }

        switch (methodName.ToLowerInvariant())
        {
            case "setinvalid":
                op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.Assign, -1, true);
                return true;
            case "setempty":
                op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.Assign, 0, true);
                return true;
            case "setfixed":
                op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.Assign, 1, true);
                return true;
            case "setkeep":
                op = new ZccTextureSpriteBuiltinOp(ZccCompareOp.Assign, 2, true);
                return true;
            default:
                return false;
        }
    }

    private static ZccMemberFunctionCallPhase PhaseAfterBuiltin(ZccMemberFunctionCallRequest request) =>
        request.Error is not null ? ZccMemberFunctionCallPhase.Failed : ZccMemberFunctionCallPhase.Finished;

    private static bool AtLeastVersion(ZccMemberFunctionCallRequest request, int major, int minor, int patch) =>
        ZccMemberFunctionCallResolve.PackScriptVersion(request.ScriptMajor, request.ScriptMinor, request.ScriptPatch) >=
        ZccMemberFunctionCallResolve.PackScriptVersion(major, minor, patch);
}

public enum ZccCompareOp
{
    GreaterThan,
    GreaterOrEqual,
    Equal,
    Assign,
}

public readonly record struct ZccTextureSpriteBuiltinOp(ZccCompareOp Operation, int Constant, bool IsVoidAssign);
