namespace HCDE.Scripting;

/// <summary><c>FxStackVariable</c>, <c>FxStructMember::RequestAddress</c>, and <c>FxArrayElement::RequestAddress</c> from <c>codegen.cpp</c> (7517–7743, 7914–7919).</summary>
public static class ZccStackVariableEmit
{
    public static bool StackVariableWritable(bool addressWritable, bool ctxWritable) =>
        addressWritable && ctxWritable;

    public static int StackEmitOffsetReg(int memberOffset) => memberOffset != 0 ? memberOffset : -1;

    public static bool StackAddressRequestedUsesFramePointer(bool addressRequested, int offsetReg) =>
        addressRequested && offsetReg < 0;

    public static bool StackAddressRequestedUsesFrameAdd(bool addressRequested, int offsetReg) =>
        addressRequested && offsetReg >= 0;

    public static string StackLoadOpForMember(string memberLoadOp) =>
        memberLoadOp == "OP_LO" ? "OP_LP" : memberLoadOp;

    public static bool StackEmitValueLoadUsesFramePointer(int offsetReg) => offsetReg < 0;

    public static bool StackUseBitLoad(int bitValue) => bitValue != -1;

    public static int StackBitMask(int bitValue) => 1 << bitValue;

    public static bool StackEmitBitPathAddsFrameOffset(int offsetReg) => offsetReg >= 0;

    public static bool StructMemberMetaReadOnly(int memberFlags) => (memberFlags & 0x10000) != 0;

    public static bool StructMemberBaseWritable(
        bool addressWritable,
        bool ctxWritable,
        bool ctxMemberWritable,
        bool classIsConstPointer) =>
        addressWritable && ctxWritable && ctxMemberWritable && !classIsConstPointer;

    public static bool StructMemberWritable(
        bool addressWritable,
        bool ctxWritable,
        int memberFlags,
        bool classIsConstPointer,
        bool scopeBarrierWritable,
        bool isConst) =>
        StructMemberBaseWritable(addressWritable, ctxWritable, true, classIsConstPointer)
        && scopeBarrierWritable
        && !isConst
        && !StructMemberMetaReadOnly(memberFlags);

    public static bool StructMemberResolveReadable(bool scopeBarrierReadable) => scopeBarrierReadable;

    public static bool StaticReadonlyPromotesToGlobal(int memberFlags) =>
        (memberFlags & (0x4 | 0x8 | 0x10000)) == (0x4 | 0x8);

    public static int VirtualScopeOuterFlags(int functionFlags, bool virtualScopeVirtual, int classScopeFlags) =>
        virtualScopeVirtual ? classScopeFlags : functionFlags;

    public static int StructMemberInheritBarrierSide(bool nestedStructMember, int parentSide, int selfSide) =>
        nestedStructMember && selfSide == 0 ? parentSide : selfSide;

    public static bool StructMemberPointerRequiresContainerType(
        bool isPointer,
        bool hasPointerType,
        bool pointedIsContainer)
    {
        if (!isPointer)
            return true;
        return hasPointerType && pointedIsContainer;
    }

    public static bool StructMemberRequiresContainer(bool isPointer, bool pointedIsContainer) =>
        StructMemberPointerRequiresContainerType(isPointer, true, pointedIsContainer);

    public static int StructMemberColorChannelShift(string symbolName) => symbolName switch
    {
        "a" => 24,
        "r" => 16,
        "g" => 8,
        _ => 0,
    };

    public static int ApplyColorChannelExtract(int loadedColor, string symbolName)
    {
        var shift = StructMemberColorChannelShift(symbolName);
        var shifted = shift > 0 ? loadedColor >> shift : loadedColor;
        return shifted & 255;
    }

    public static bool ArrayElementRequestAddress(bool addressRequested, out bool writable, bool addressWritable)
    {
        writable = addressWritable;
        return addressRequested;
    }
}
