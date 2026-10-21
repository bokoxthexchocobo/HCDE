namespace HCDE.Scripting;

/// <summary><c>FxStructMember::Emit</c> and <c>FxArrayElement</c> bands from <c>codegen.cpp</c> (7796–8044).</summary>
public static class ZccStructMemberArrayEmit
{
    public const int DeprecatedFlagsBitThreshold = 64;

    public static bool StructMemberUsesDeprecatedFlagsPath(int bitValue) => bitValue >= DeprecatedFlagsBitThreshold;

    public static int DeprecatedFlagsArgument(int bitValue) => bitValue - DeprecatedFlagsBitThreshold;

    public static bool StructMemberEmitKonstPointerReload(bool objKonst) => objKonst;

    public static bool StructMemberEmitUsesMetaPointer(int memberFlags) => (memberFlags & 0x10000) != 0;

    public static bool StructMemberAddressReturnsObjectDirectly(bool addressRequested, int offset) =>
        addressRequested && offset == 0;

    public static bool StructMemberEmitUsesAddressAdd(bool addressRequested, int offset) =>
        addressRequested && offset != 0;

    public static bool StructMemberEmitUsesBitLoad(int bitValue) => bitValue != -1;

    public static int StructMemberBitMask(int bitValue) => 1 << bitValue;

    public static bool StructMemberBitEmitAddsOffsetBeforeLbit(int memberOffset) => memberOffset != 0;

    public static bool StructMemberDeprecatedFlagsUsesBuiltin(int bitValue) => bitValue >= DeprecatedFlagsBitThreshold;

    public static bool ClassMemberCtorSetsClassExprType(bool setsClassMember) => setsClassMember;

    public static bool ArrayElementCtorStoresNoboundscheck(bool noboundscheck) => noboundscheck;

    public static bool ArrayElementRequestAddress(bool addressRequested, out bool writable, bool addressWritable)
    {
        writable = addressWritable;
        return addressRequested;
    }

    public static bool ArrayIndexNeedsIntCast(bool indexIsFloatReg) => indexIsFloatReg;

    public static bool ArrayIndexMustBeInteger(bool isInteger) => isInteger;

    public static bool ShouldRewriteArrayToOutDereference(bool isRealPointer, bool pointedIsDynArray) =>
        isRealPointer && pointedIsDynArray;

    public static bool ArrayResolvePointerToArray(bool isArray, bool isPointer, bool pointedIsArray) =>
        isArray || (isPointer && pointedIsArray);

    public static bool StaticArrayConstantIndexInBounds(uint index, uint elementCount) => index < elementCount;

    public static bool StaticArrayConstantIndexOutOfBounds(uint index, uint elementCount) => index >= elementCount;

    public static bool ResizableStaticArrayFromMemberExpr(
        bool isClassMember,
        bool isStructMember,
        bool isGlobal,
        bool isStack) =>
        isClassMember || isStructMember || isGlobal || isStack;

    public static bool InvalidResizableArrayExpr(bool isMemberBase, bool isArrayElement, bool isOutDeref) =>
        !(isMemberBase || isArrayElement || isOutDeref);

    public static uint FoldedStaticArrayFieldOffset(uint index, uint elementSize, uint parentOffset) =>
        index * elementSize + parentOffset;

    public static uint StaticArraySizeAddrForMember(uint parentFieldOffset) => parentFieldOffset + 8;

    public static bool StaticArraySizeAddrUnknownForNested(bool isArrayElement, bool isOutDeref) =>
        isArrayElement || isOutDeref;

    public static bool ArrayResolveRequestAddressFailed(bool requestSucceeded) => !requestSucceeded;

    public static bool ArrayElementOutOfBounds(uint index, uint elementCount) => index >= elementCount;

    public static bool ArrayResolveFailsNonIntegerIndex(bool isInteger) => !isInteger;

    public static bool ArrayResolveFailsNonArray(bool isArray, bool pointerToArray) => !isArray && !pointerToArray;

    public static bool ArrayIsDynamic(bool isDynamicArray) => isDynamicArray;

    public static bool ArrayIndexConstantFoldEligible(bool indexConstant, bool hasArrayType, bool arrayIsPointer) =>
        indexConstant && hasArrayType && !arrayIsPointer;

    public static bool ArrayMemberFoldUsesParentOffset(
        bool isClassMember,
        bool isStructMember,
        bool isGlobal,
        bool isStack) =>
        isClassMember || isStructMember || isGlobal || isStack;

    public static bool ArrayResolveUsesDynArrayElementType(bool isDynamicArray) => isDynamicArray;

    public static bool ArrayPointerToStaticArray(bool isPointer, bool pointedIsArray) =>
        isPointer && pointedIsArray;

    public static bool ArrayStaticSizedUsesArrayType(bool isArray, bool arrayIsPointer) =>
        isArray && !arrayIsPointer;

    public static bool ArrayResolveSetsElementType(bool resolved) => resolved;

    public static bool ArrayRequestAddressRequired(bool requestAddressSucceeded) => requestAddressSucceeded;

    public static bool StructMemberResolveCompletes(bool classResolved) => classResolved;

    public static bool StructMemberEmitBitValueBelowDeprecatedThreshold(int bitValue) =>
        bitValue < DeprecatedFlagsBitThreshold;

    public static bool StructMemberEmitAddressRequestedPath(int bitValue, bool addressRequested) =>
        bitValue < DeprecatedFlagsBitThreshold || addressRequested;
}
