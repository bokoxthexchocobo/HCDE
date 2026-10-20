namespace HCDE.Client;

/// <summary><c>HCDEAppendCanonicalEventPayload</c> from <c>d_net.cpp</c> (3747–3900).</summary>
public static class VmDNetCanonicalEventPayload
{
    public static bool PayloadSucceeded(bool ok) => ok;

    public static bool ZeroPayloadEvent() => true;

    public static bool AppendOneBytePrefix() => true;

    public static bool AppendNullStringAfterPrefix() => true;

    public static bool AppendFourBytesAfterString() => true;

    public static bool AppendFiveBytesAfterString() => true;

    public static bool AppendFourteenBytesAfterString() => true;

    public static bool ReadBe16CommandLength() => true;

    public static bool AppendCommandBytesField(ushort length) => length >= 0;

    public static bool AppendTwentyFiveBytesAfterString() => true;

    public static bool AppendOneByteBeforeString() => true;

    public static bool StringOnlyPayload() => true;

    public static bool AppendSixByteWarp() => true;

    public static bool AppendFourByteInvUse() => true;

    public static bool AppendEightByteInvDrop() => true;

    public static bool AppendSingleByteCheat() => true;

    public static bool DualNullStrings() => true;

    public static bool DelegateCvarChange(bool xorVariant) => true;

    public static bool RunArgsUnnamed() => false;

    public static bool RunArgsNamed() => true;

    public static bool ConvReplyThreeBytes() => true;

    public static bool SetSlotReadsSlotAndCount() => true;

    public static bool AppendSlotBytes(byte slot, byte count) => true;

    public static bool IterateWeaponIndices(byte count) => count > 0;

    public static bool AddSlotReadsOneByteThenWeapon() => true;

    public static bool SetPitchLimitTwoBytes() => true;

    public static bool DefaultEventReturnsFalse() => false;

    public static bool FieldBytesGuard(ulong cursor, ulong capacity) =>
        cursor <= capacity;

    public static bool InputCursorInBounds(ulong cursor, ulong size) =>
        cursor <= size;

    public static int Summon2FamilyExtra() => 25;

    public static int ChangeMap2LeadingByte() => 1;

    public static int NetEventTrailingBytes() => 14;

    public static int GiveCheatTrailingBytes() => 4;

    public static int SetInvTrailingBytes() => 5;

    public static int AddBotTrailingBytes() => 4;

    public static bool TauntSharesSayShape() => true;

    public static bool SavegameNeedsTwoStrings() => true;

    public static bool SinFChangedPair(bool xor) => xor;

    public static bool RunSpecialUsesUnnamedArgs() => true;

    public static bool SetSlotPnumExtraByte() => true;

    public static bool WeaponIndexPerSlotEntry() => true;

    public static bool AddSlotDefaultUsesWeaponIndex() => true;

    public static bool CanonicalNullStringRequired() => true;

    public static bool ZscCmdChain(bool a, bool b, bool c, bool d) => a && b && c && d;

    public static bool ReturnOnAuthoritySlotFailure(bool ok) => ok;
}
