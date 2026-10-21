namespace HCDE.Scripting;

public static class ZccArrayElementResolve
{
    public const uint InvalidSizeAddr = uint.MaxValue;

    public static bool IndexIsFloatReg(int regType, int floatReg) => regType == floatReg;

public static bool PointerToDynArray(bool isRealPointer, bool pointedIsDynArray) =>
        isRealPointer && pointedIsDynArray;

public static bool PointerToArray(bool hasPointer, bool pointedIsArray) =>
        hasPointer && pointedIsArray;

    public static bool InvalidArraySubscript(bool hasPointer, bool pointedIsArray) =>
        !hasPointer || !pointedIsArray;

    public static bool StaticArrayInClassOrStruct(int exprType, int classMember, int structMember, int global, int stack) =>
        exprType == classMember || exprType == structMember || exprType == global || exprType == stack;

    public static bool NestedArrayElement(int exprType, int arrayElement, int outVarDeref) =>
        exprType == arrayElement || exprType == outVarDeref;

    public static uint StaticArraySizeAddr(uint parentOffset, int pointerSize) =>
        parentOffset + (uint)pointerSize;

    public static bool ConstantIndexOutOfBounds(uint indexVal, long elementCount) =>
        indexVal >= (uint)elementCount;

    public static bool ConstantIndexInBounds(uint indexVal, long elementCount) =>
        indexVal < (uint)elementCount;

    public static bool CanFoldConstantField(
        bool indexConstant,
        bool hasArrayType,
        bool arrayIsPointer,
        int exprType,
        int classMember,
        int structMember,
        int global,
        int stack) =>
        indexConstant && hasArrayType && !arrayIsPointer && StaticArrayInClassOrStruct(exprType, classMember, structMember, global, stack);

    public static long ConstantFieldOffset(long indexVal, int elementSize, long parentOffset) =>
        indexVal * elementSize + parentOffset;

    public static bool RequestAddressFailed(bool requestOk) => !requestOk;

public static bool SetAddressWritable(bool writable, bool value)
    {
        writable = value;
        return writable;
    }

    public static bool ResolveStaticResizableError(int exprType, int classMember, int structMember, int global, int stack, int arrayElement, int outVarDeref) =>
        !StaticArrayInClassOrStruct(exprType, classMember, structMember, global, stack) &&
        !NestedArrayElement(exprType, arrayElement, outVarDeref);

public static int FloatToIntCastFromDecorate(bool fromDecorate) => fromDecorate ? 1 : 0;

    public static bool DeleteOnResolveFailure(bool resolved) => !resolved;

    public static bool ResolveSucceeded(bool requestAddressOk, bool hasElementType) =>
        requestAddressOk && hasElementType;

    public static bool DynArrayPointerRewrite(bool dynamicArray, bool arrayIsPointer) =>
        dynamicArray && !arrayIsPointer;
}
