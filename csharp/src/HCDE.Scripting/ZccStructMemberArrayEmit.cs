namespace HCDE.Scripting;

/// <summary><c>FxStructMember::Emit</c> and <c>FxArrayElement</c> bands from <c>codegen.cpp</c> (7796–8044).</summary>
public static class ZccStructMemberArrayEmit
{
    public const int DeprecatedFlagsBitThreshold = 64;

    public const int EfxClassMember = 1;
    public const int EfxStructMember = 2;
    public const int EfxGlobalVariable = 3;
    public const int EfxStackVariable = 4;
    public const int EfxArrayElement = 5;
    public const int EfxOutVarDereference = 6;

    public readonly record struct StructMemberEmitPlan(
        bool DereferenceConstPointer,
        bool EmitMetaPointer,
        bool AddressRequested,
        bool OffsetIsZero,
        bool UseBitLoad,
        int MemberOffset,
        int BitMask,
        string LoadOp);

    public readonly record struct DeprecatedFlagsEmitPlan(string BuiltinName, int BitArgument, string ReturnReg);

    public static bool StructMemberUsesDeprecatedFlagsPath(int bitValue) => bitValue >= DeprecatedFlagsBitThreshold;

    public static int DeprecatedFlagsArgument(int bitValue) => bitValue - DeprecatedFlagsBitThreshold;

    public static DeprecatedFlagsEmitPlan PlanDeprecatedFlagsEmit(int bitValue) =>
        new("CheckDeprecatedFlags", DeprecatedFlagsArgument(bitValue), "REGT_INT");

    public static StructMemberEmitPlan PlanStructMemberEmit(
        bool constPointerKonst,
        bool memberIsMeta,
        bool addressRequested,
        int memberOffset,
        int bitValue,
        string memberLoadOp)
    {
        if (StructMemberUsesDeprecatedFlagsPath(bitValue))
            return new StructMemberEmitPlan(false, false, false, false, false, 0, 0, "CheckDeprecatedFlags");

        return new StructMemberEmitPlan(
            constPointerKonst,
            memberIsMeta,
            addressRequested,
            memberOffset == 0,
            bitValue != -1,
            memberOffset,
            bitValue != -1 ? 1 << bitValue : 0,
            bitValue == -1 ? memberLoadOp : "OP_LBIT");
    }

    public static bool StructMemberAddressReturnsObjectDirectly(bool addressRequested, int offset) =>
        addressRequested && offset == 0;

    public static bool StructMemberBitLoadNeedsOffsetPointer(bool useBitLoad) => useBitLoad;

    public static string StructMemberAddressOpcode(bool offsetIsZero) => offsetIsZero ? "REGT_POINTER" : "OP_ADDA_RK";

    public static bool ClassMemberSetsExprType(int exprType) => exprType == EfxClassMember;

    public static bool ArrayElementCtorSetsNoboundscheck(bool nob) => nob;

    public static bool ArrayElementRequestAddress(bool addressRequested, out bool writable, bool addressWritable)
    {
        writable = addressWritable;
        return addressRequested;
    }

    public static bool ArrayIndexNeedsIntCast(int indexRegTypeFloat) => indexRegTypeFloat != 0;

    public static bool ArrayIndexMustBeInteger(bool isInteger) => isInteger;

    public static bool ArrayResolveAcceptsDynamicArray(bool isRealPointer, bool pointedIsDynArray) =>
        isRealPointer && pointedIsDynArray;

    public static bool ArrayResolvePointerToArray(bool isArray, bool isPointer, bool pointedIsArray) =>
        isArray || (isPointer && pointedIsArray);

    public static bool StaticArrayConstantIndexInBounds(uint index, uint elementCount) => index < elementCount;

    public static bool StaticArrayCanFoldMember(int arrayExprType, bool isStaticArray, bool indexIsConstant, bool hasArrayType) =>
        isStaticArray && indexIsConstant && hasArrayType && IsMemberArrayExpr(arrayExprType);

    public static bool IsMemberArrayExpr(int arrayExprType) =>
        arrayExprType is EfxClassMember or EfxStructMember or EfxGlobalVariable or EfxStackVariable;

    public static bool ResizableStaticArrayMember(int arrayExprType) => IsMemberArrayExpr(arrayExprType);

    public static bool InvalidResizableArrayExpr(int arrayExprType) =>
        arrayExprType is not (EfxClassMember or EfxStructMember or EfxGlobalVariable or EfxStackVariable or EfxArrayElement or EfxOutVarDereference);

    public static uint FoldedStaticArrayFieldOffset(uint index, uint elementSize, uint parentOffset) =>
        index * elementSize + parentOffset;

    public static uint StaticArraySizeAddrOffset(uint parentOffset) => parentOffset + 8;

    public static bool ArrayResolveNeedsAddress(bool requestAddressSucceeded) => requestAddressSucceeded;

    public static bool ArrayElementOutOfBounds(uint index, uint elementCount) => index >= elementCount;

    public static bool DynamicArrayPromotesToPointer(bool isDynamicArray) => isDynamicArray;

    public static int ChooseArrayElementValueType(bool dynamicArray, bool pointerArray) =>
        dynamicArray || pointerArray ? 1 : 0;

    public static bool ArrayIndexFloatAllowedFromDecorate(bool fromDecorate) => fromDecorate;

    public static bool ShouldRewriteArrayToOutDereference(bool isRealPointer, bool pointedIsDynArray) =>
        isRealPointer && pointedIsDynArray;

    public static bool ArrayResolveFailsNonIntegerIndex(bool isInteger) => !isInteger;

    public static bool ArrayResolveFailsNonArray(bool isArray, bool pointerToArray) => !isArray && !pointerToArray;

    public static uint SizeAddrUnset => uint.MaxValue;

    public static bool SizeAddrIsTracked(uint sizeAddr) => sizeAddr != SizeAddrUnset;

    public static bool NestedArrayElementExpr(int exprType) =>
        exprType is EfxArrayElement or EfxOutVarDereference;
}
