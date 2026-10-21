namespace HCDE.Scripting;

/// <summary><c>FxStackVariable</c> and <c>FxStructMember::RequestAddress</c> from <c>codegen.cpp</c> (7517–7743).</summary>
public static class ZccStackVariableEmit
{
    public readonly record struct StackEmitPlan(
        bool AddressRequested,
        bool UseOutPointerLoad,
        bool AddOffsetToFrame,
        int OffsetReg,
        string LoadOp,
        bool UseBitLoad,
        int BitMask);

    public static bool StackVariableWritable(bool addressWritable, bool ctxWritable) =>
        addressWritable && ctxWritable;

    public static int StackOffsetRegister(int memberOffset) => memberOffset != 0 ? memberOffset : -1;

    public static string StackLoadOpForMember(string memberLoadOp) =>
        memberLoadOp == "OP_LO" ? "OP_LP" : memberLoadOp;

    public static bool StackUseBitLoad(int bitValue) => bitValue != -1;

    public static StackEmitPlan PlanStackVariableEmit(
        bool isOutVariable,
        bool addressRequested,
        int regOffset,
        int memberOffset,
        int bitValue,
        string memberLoadOp)
    {
        if (isOutVariable)
        {
            if (!addressRequested)
                return new StackEmitPlan(false, true, regOffset != 0, regOffset, memberLoadOp, false, 0);
            if (regOffset == 0)
                return new StackEmitPlan(true, false, false, 0, "REGT_POINTER", false, 0);
            return new StackEmitPlan(true, false, true, memberOffset, "OP_ADDA_RK", false, 0);
        }

        var loadOp = StackLoadOpForMember(memberLoadOp);
        if (StackUseBitLoad(bitValue))
            return new StackEmitPlan(addressRequested, false, memberOffset != 0, memberOffset, "OP_LBIT", true, 1 << bitValue);
        return new StackEmitPlan(addressRequested, false, memberOffset != 0, memberOffset, loadOp, false, 0);
    }

    public static bool StructMemberMetaReadOnly(int memberFlags) => (memberFlags & 0x10000) != 0;

    public static bool StructMemberWritable(
        bool addressWritable,
        bool ctxWritable,
        int memberFlags,
        bool classIsConstPointer,
        bool scopeBarrierWritable,
        bool isConst) =>
        addressWritable
        && ctxWritable
        && !classIsConstPointer
        && scopeBarrierWritable
        && !isConst
        && !StructMemberMetaReadOnly(memberFlags);

    public static bool ComputeStructMemberWritable(
        bool addressWritable,
        bool ctxWritable,
        int memberFlags,
        int defFileNo,
        bool classIsConstPointer,
        int outerFunctionFlags,
        int memberSideFlags,
        string symbolName,
        bool scopeBarrierWritable,
        bool isConst) =>
        StructMemberWritable(addressWritable, ctxWritable, memberFlags, classIsConstPointer, scopeBarrierWritable, isConst);

    public static bool StructMemberResolveReadable(bool scopeBarrierReadable) => scopeBarrierReadable;

    public static bool StaticReadonlyPromotesToGlobal(int memberFlags) =>
        (memberFlags & (0x4 | 0x8 | 0x10000)) == (0x4 | 0x8);

    public static int VirtualScopeOuterFlags(int functionFlags, bool virtualScopeVirtual, int classScopeFlags) =>
        virtualScopeVirtual ? classScopeFlags : functionFlags;

    public static int StructMemberInheritBarrierSide(bool nestedStructMember, int parentSide, int selfSide) =>
        nestedStructMember && selfSide == 0 ? parentSide : selfSide;
}
