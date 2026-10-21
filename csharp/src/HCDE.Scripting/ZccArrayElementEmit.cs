namespace HCDE.Scripting;

/// <summary><c>FxArrayElement::Emit</c> from <c>codegen.cpp</c> (8052–8315).</summary>
public static class ZccArrayElementEmit
{
    public const uint InvalidSizeAddr = uint.MaxValue;

    public static bool ArrayIsPointer(bool arrayIsPointer) => arrayIsPointer;

    public static bool NeedsPointerDeref(bool arrayIsPointer, bool hasPointerType) =>
        arrayIsPointer && hasPointerType;

    public static bool InternalArrayError(bool arrayIsPointer, bool hasPointerType) =>
        arrayIsPointer && !hasPointerType;

    public static bool UsesSizeAddrField(uint sizeAddr) => sizeAddr != InvalidSizeAddr;

    public static bool MemberIsMeta(bool isMeta) => isMeta;

    public static bool IsStructOrClassMember(int exprType, int structMember, int classMember) =>
        exprType == structMember || exprType == classMember;

    public static bool IsNestedStaticArray(int exprType, int arrayElement, int outVarDeref, bool isStatic) =>
        (exprType == arrayElement || exprType == outVarDeref) && isStatic;

    public static bool IndexIsConstant(bool constant) => constant;

    public static bool NeedsDynamicBoundCheck(uint sizeAddr, bool nestedArray) =>
        sizeAddr != InvalidSizeAddr || nestedArray;

    public static bool AddressRequestedPath(bool addressRequested) => addressRequested;

    public static bool ShouldOffsetPointerIndex(uint indexVal, bool addressRequested) =>
        addressRequested && indexVal != 0;

    public static bool StartIsFixed(bool fixedReg) => fixedReg;

    public static bool UseTempWhenFixedOffset(bool startFixed, uint indexVal) =>
        startFixed && indexVal != 0;

    public static bool StartIsKonst(bool konst) => konst;

    public static int LoadOpForRegType(int regType) => regType;

    public static int LkOpIndex(int regType) => regType;

    public static bool ForeachSkipsBounds(bool noBoundsCheck) => noBoundsCheck;

    public static bool UseBoundK(long elementCount) => elementCount > 65535;

    public static int ShiftBitsForElementSize(int elementSize)
    {
        var shift = 0;
        while ((1u << shift) < (uint)elementSize)
            shift++;
        return shift;
    }

    public static bool CanUseShiftInsteadOfMul(int elementSize, int shiftBits) =>
        (1u << shiftBits) == (uint)elementSize;

    public static bool NeedsShiftEmit(int shiftBits) => shiftBits > 0;

    public static bool IndexWorkNeedsCopy(bool indexFixed, int elementSize) =>
        indexFixed && elementSize > 1;

    public static bool LargeConstTableImmediate(int startRegNum) => startRegNum <= 255;

    public static bool AddStartRegToIndex(int startRegNum) => startRegNum > 255;

    public static int ElementByteOffset(uint indexVal, int elementSize) =>
        (int)(indexVal * (uint)elementSize);

    public static bool ResizableArrayNeedsBoundEvenWhenConstant(uint sizeAddr, bool nestedArray) =>
        sizeAddr != InvalidSizeAddr || nestedArray;

    public static bool StaticArrayInBounds(uint indexVal, int elementCount, uint sizeAddr, bool nestedArray) =>
        sizeAddr == InvalidSizeAddr && !nestedArray && indexVal < (uint)elementCount;

    public static bool EmitLoadAfterIndex(bool addressRequested, bool startKonst) =>
        !addressRequested && !startKonst;

    public static bool EmitLkFromKonstTable(bool startKonst, bool addressRequested) =>
        startKonst && !addressRequested;

    public static bool MetaBoundUsesDoubleEmit(bool isMeta) => isMeta;

    public static bool GlobalOrStackDoubleEmit(int exprType, int structMember, int classMember) =>
        !IsStructOrClassMember(exprType, structMember, classMember);

    public static bool FreeObjVarAfterMemberEmit() => true;

    public static bool RestoreMemberVarAfterMetaEmit() => true;

    public static int FArrayCountFieldOffset(int offset) => offset;

    public static bool NestedArraySetsFlag(bool nested) => nested;

    public static bool EmitBoundR(uint sizeAddr, bool nestedArray, bool noBoundsCheck) =>
        !noBoundsCheck && (sizeAddr != InvalidSizeAddr || nestedArray);

    public static bool EmitBoundForStatic(int elementCount, bool noBoundsCheck, uint sizeAddr, bool nestedArray) =>
        !noBoundsCheck && sizeAddr == InvalidSizeAddr && !nestedArray;

    public static int LoadOpPlusOne(int baseLoadOp) => baseLoadOp + 1;

    public static int LkrOpIndex(int regType) => regType;

    public static bool PreserveFixedState(bool wasFixed) => wasFixed;

    public static bool DummyVarFixesDoubleEmit() => true;

    public static bool StructMemberUsesDummyVar(int exprType, int structMember) => exprType == structMember;

    public static bool ClassMemberUsesDummyVar(int exprType, int classMember) => exprType == classMember;

    public static bool RestoreClassxAfterEmit() => true;

    public static bool ObjVarMustStayFixed(bool wasFixed) => wasFixed;

    public static bool EmitLpForArrayBase() => true;

    public static bool EmitLwForBoundFromSizeAddr(int sizeAddr) => sizeAddr >= 0;

    public static bool MetaPathDoubleEmitsMemberOnly() => true;

    public static bool GlobalStackPathDoubleEmitsMember() => true;

    public static int RegTypePointer() => 3;

    public static int RegTypeInt() => 0;

    public static bool IndexRegFreedAfterBound() => true;

    public static bool BoundRegFreedAfterBound() => true;

    public static bool ArrayVarFreedAfterNestedStatic() => true;

    public static bool DestUsesValueRegType(bool addressRequested) => !addressRequested;

    public static bool TempRegForAdda(bool startFixed) => startFixed;

    public static bool IndexvFreedAfterLkr() => true;

    public static bool AddRkWhenStartRegLarge(int startRegNum) => startRegNum > 255;

    public static bool ForeachSkipsAllBounds(bool noBoundsCheck) => noBoundsCheck;

    public static bool ElementCountFitsBoundOpcode(int elementCount) => elementCount <= 65535;

    public static bool MulRkWhenNotPowerOfTwo(int elementSize, int shiftBits) =>
        !CanUseShiftInsteadOfMul(elementSize, shiftBits);
}
