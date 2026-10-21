namespace HCDE.Scripting;

/// <summary><c>FxStackVariable</c> and early <c>FxStructMember::RequestAddress</c> from <c>codegen.cpp</c> (7517–7666).</summary>
public static class ZccStackVariableEmit
{
    public static bool StackVariableWritable(bool addressWritable, bool ctxWritable, int memberFlags, int defFileNo) =>
        addressWritable && ctxWritable;

    public static bool StackMetaVariableWritable(int memberFlags, out bool writable)
    {
        writable = false;
        return (memberFlags & 0x10000) != 0;
    }

    public static int StackOffsetRegister(int memberOffset) => memberOffset != 0 ? memberOffset : -1;

    public static bool StackEmitAddressRequested(bool addressRequested, int offsetReg, bool hasFramePointer) =>
        addressRequested && (offsetReg >= 0 || hasFramePointer);

    public static string StackLoadOpForMember(string memberLoadOp) =>
        memberLoadOp == "OP_LO" ? "OP_LP" : memberLoadOp;

    public static bool StackUseBitLoad(int bitValue) => bitValue != -1;

    public static bool StructMemberMetaReadOnly(int memberFlags) => (memberFlags & 0x10000) != 0;

    public static bool StructMemberWritable(
        bool addressWritable,
        bool ctxWritable,
        int memberFlags,
        int defFileNo,
        bool classIsConstPointer,
        bool scopeBarrierWritable,
        bool isConst) =>
        addressWritable
        && ctxWritable
        && !classIsConstPointer
        && scopeBarrierWritable
        && !isConst
        && (memberFlags & 0x10000) == 0;

    public static bool StructMemberResolveReadable(bool scopeBarrierReadable) => scopeBarrierReadable;

    public static int StructMemberInheritBarrierSide(bool isNestedStructMember, int parentSide, int selfSide) =>
        isNestedStructMember && selfSide == 0 ? parentSide : selfSide;

    public static bool StaticReadonlyPromotesToGlobal(int memberFlags) =>
        (memberFlags & (0x4 | 0x8 | 0x10000)) == (0x4 | 0x8);

    public static bool PointerMemberRequiresContainer(bool isPointer, bool pointedTypeIsContainer) =>
        !isPointer || pointedTypeIsContainer;

    public static bool StructMemberRequestAddressSetsFlag(bool requested) => requested;

    public static int VirtualScopeOuterFlags(int functionFlags, bool hasVirtualScopeVirtual, int classScopeFlags) =>
        hasVirtualScopeVirtual ? classScopeFlags : functionFlags;

    public static bool ScopeBarrierAllowsWrite(bool barrierWritable, bool memberWritable, bool isConst) =>
        barrierWritable && memberWritable && !isConst;

    public static string StackEmitAddressOp(int offsetReg) => offsetReg >= 0 ? "OP_ADDA_RK" : "FRAME_POINTER";

    public static int BitShiftForMember(int bitValue) => 1 << bitValue;

    public static bool ResolveChecksSpecialMember(bool hasHook) => hasHook;

    public static bool NestedStructMemberExpr(bool classMember, bool structMember, bool globalVar, bool stackVar) =>
        structMember && (classMember || structMember || globalVar || stackVar);

    public static int CumulativeFieldOffset(int parentOffset, int memberOffset) => parentOffset + memberOffset;

    public static int LocalVectorRegOffset(bool isOutVariable, int memberOffset) =>
        isOutVariable ? memberOffset : memberOffset / 8;

    public static bool LocalVectorMember(bool isVector, bool isQuaternion) => isVector || isQuaternion;

    public static bool EmitUsesFramePointerWhenNoOffset(int offsetReg) => offsetReg < 0;

    public static string GlobalVariableLookupOp => "OP_LKP";

    public static bool MemberWritableFromCtx(bool ctxWritable, int flags, int defFileNo) => ctxWritable;
}
