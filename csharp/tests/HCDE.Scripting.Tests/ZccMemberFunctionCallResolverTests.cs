namespace HCDE.Scripting.Tests;

public class ZccMemberFunctionCallResolverTests
{
    [Fact]
    public void TryResolve_RejectsMissingEnclosingClass()
    {
        var request = new ZccMemberFunctionCallRequest { HasEnclosingClass = false };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.Equal("Expression is not constant", request.Error);
    }

    [Fact]
    public void TryResolve_RejectsNullArgumentSlots()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            Arguments = new object?[] { 1, null },
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
    }

    [Fact]
    public void TryResolve_StaticIdentifierMapsStringStruct()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfExpressionKind = ZccMemberSelfExpressionKind.Identifier,
            StaticIdentifierName = "String",
            SelfValueKind = ZccMemberSelfValueKind.Error,
            SelfResolvedToError = true,
            ResolvedContainerTypeName = "StringStruct",
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Continue, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.True(request.StaticOnly);
    }

    [Fact]
    public void TryResolve_StaticClassVirtualRebindsToSelf()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            HasEnclosingFunction = true,
            MethodName = "Tick",
            SelfResolvedToError = true,
            ResolvedContainerTypeName = "Actor",
            StaticContainerIsClass = true,
            EnclosingClassDescendsFromStaticContainer = true,
            StaticContainerHasVirtualMethod = true,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Continue, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.True(request.ReboundSelfToEnclosingInstance);
        Assert.False(request.StaticOnly);
    }

    [Fact]
    public void TryResolve_StaticClassFromConstantContextFails()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            HasEnclosingFunction = false,
            MethodName = "Spawn",
            SelfResolvedToError = true,
            ResolvedContainerTypeName = "Actor",
            StaticContainerIsClass = true,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
    }

    [Fact]
    public void TryResolve_ReadonlyPointerMarks412()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.Pointer,
            IsReadonlyConstPointer = true,
            ScriptMajor = 4,
            ScriptMinor = 12,
            ScriptPatch = 0,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Continue, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.True(request.IsReadonlyConstPointer);
    }

    [Fact]
    public void TryResolve_SuperBindsParentPointer()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            HasEnclosingFunction = true,
            SelfExpressionKind = ZccMemberSelfExpressionKind.Super,
            SuperParentTypeName = "Inventory",
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Continue, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.True(request.NoVirtual);
        Assert.Equal(ZccMemberSelfValueKind.Pointer, request.SelfValueKind);
    }

    [Fact]
    public void TryResolve_NumericToVectorRewrites()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.Numeric,
            MethodName = "ToVector",
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Finished, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.Equal(ZccMemberBuiltinRewriteKind.NumericToVector, request.BuiltinRewrite);
    }

    [Fact]
    public void TryResolve_SoundIsValidRequiresZeroArgs()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            ScriptMajor = 4,
            ScriptMinor = 14,
            ScriptPatch = 1,
            SelfValueKind = ZccMemberSelfValueKind.Sound,
            MethodName = "IsValid",
            ArgumentCount = 1,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
    }

    [Fact]
    public void TryResolve_SoundIsValidFoldsCompare()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            ScriptMajor = 4,
            ScriptMinor = 14,
            ScriptPatch = 1,
            SelfValueKind = ZccMemberSelfValueKind.Sound,
            MethodName = "IsValid",
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Finished, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.Equal(ZccMemberBuiltinRewriteKind.SoundIsValidCompare, request.BuiltinRewrite);
    }

    [Theory]
    [InlineData("IsNull")]
    [InlineData("SetInvalid")]
    public void TryResolve_TextureBuiltinsFold(string method)
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.TextureId,
            MethodName = method,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Finished, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.True(ZccMemberFunctionCallResolver.TryDescribeTextureBuiltin(method, out _));
    }

    [Fact]
    public void TryResolve_SpriteIsEmptyUsesZeroConstant()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            ScriptMajor = 4,
            ScriptMinor = 14,
            ScriptPatch = 1,
            SelfValueKind = ZccMemberSelfValueKind.SpriteId,
            MethodName = "IsEmpty",
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Finished, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.True(ZccMemberFunctionCallResolver.TryDescribeSpriteBuiltin("IsEmpty", out var op));
        Assert.Equal(0, op.Constant);
    }

    [Fact]
    public void TryResolve_VectorLengthUnaryBuiltin()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.Vector3,
            MethodName = "Length",
            ArgumentCount = 1,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
    }

    [Fact]
    public void TryResolve_VectorPlusZRequiresOneArgument()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.Vector3,
            MethodName = "PlusZ",
            ArgumentCount = 0,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
    }

    [Fact]
    public void TryResolve_QuaternionConjugateUnaryBuiltin()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.Quaternion,
            MethodName = "Conjugate",
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Finished, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.Equal(ZccMemberBuiltinRewriteKind.QuaternionUnaryBuiltin, request.BuiltinRewrite);
    }

    [Fact]
    public void TryResolve_StringLengthIntrinsic()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.String,
            MethodName = "Length",
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Finished, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.Equal(ZccMemberBuiltinRewriteKind.StringLength, request.BuiltinRewrite);
    }

    [Fact]
    public void TryResolve_DynArraySizeRedirectsToBackingMember()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.DynamicArray,
            MethodName = "Size",
            DynArrayBackingTypeName = "DynArray_i32",
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Finished, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.Equal("DynArray_i32", request.RemappedSelfStructType);
    }

    [Fact]
    public void TryResolve_DynArrayPushRejectsIncompatibleObject()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.DynamicArray,
            MethodName = "Push",
            DynArrayElementIsObjectPointer = true,
            DynArrayArguments = new[]
            {
                new ZccDynArrayArgumentState
                {
                    Index = 0,
                    IsObjectPointer = true,
                    AncestorMatches = false,
                },
            },
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
    }

    [Fact]
    public void TryResolve_StaticArraySizeFoldsConstantCount()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.StaticArray,
            MethodName = "Size",
            IsResizableStaticArray = false,
            StaticArrayElementCount = 12,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Finished, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.Equal(ZccMemberBuiltinRewriteKind.StaticArraySizeConstant, request.BuiltinRewrite);
    }

    [Fact]
    public void TryResolve_ResizableStaticArraySizeRequiresMemberAccess()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.StaticArray,
            MethodName = "Size",
            IsResizableStaticArray = true,
            SelfExpressionKind = ZccMemberSelfExpressionKind.Identifier,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
    }

    [Fact]
    public void TryResolve_MapInsertRejectsIncompatibleValue()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.Map,
            MethodName = "Insert",
            MapBackingTypeName = "Map_str_Actor",
            MapValueIsObjectPointer = true,
            MapArguments = new[]
            {
                new ZccMapArgumentState { Index = 1, IsObjectPointer = true, AncestorMatches = false },
            },
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
    }

    [Fact]
    public void TryResolve_GetClassNameRequiresZeroArgs()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.Object,
            MethodName = "GetClassName",
            ArgumentCount = 1,
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Failed, ZccMemberFunctionCallResolver.TryResolve(request));
    }

    [Fact]
    public void TryResolve_IsAbstractOnClassPointer()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.ClassPointer,
            MethodName = "IsAbstract",
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Finished, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.Equal(ZccMemberBuiltinRewriteKind.IsAbstract, request.BuiltinRewrite);
    }

    [Fact]
    public void TryResolve_DynArrayCopyPromotesBackingPointer()
    {
        var request = new ZccMemberFunctionCallRequest
        {
            HasEnclosingClass = true,
            SelfValueKind = ZccMemberSelfValueKind.DynamicArray,
            MethodName = "Copy",
            SelfExpressionKind = ZccMemberSelfExpressionKind.StructMember,
            DynArrayBackingTypeName = "DynArray_i32",
            DynArrayArguments = new[]
            {
                new ZccDynArrayArgumentState
                {
                    Index = 0,
                    IsDynamicArrayValue = true,
                    ElementTypeMatches = true,
                    CanRequestAddress = true,
                },
            },
        };
        Assert.Equal(ZccMemberFunctionCallPhase.Continue, ZccMemberFunctionCallResolver.TryResolve(request));
        Assert.True(request.DynArrayArguments![0].PromotedToBackingPointer);
        Assert.True(request.DynArrayArguments[0].MemberFieldSwappedToBacking);
    }
}
