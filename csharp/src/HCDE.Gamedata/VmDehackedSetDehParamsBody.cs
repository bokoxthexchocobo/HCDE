namespace HCDE.Gamedata;

/// <summary>MBF emitter shapers and <c>SetDehParams</c> body from <c>d_dehacked.cpp</c> (815–1188).</summary>
public static class VmDehackedSetDehParamsBody
{
    public static (object? itemType, float distance, float height, int useAmmo, int transferTranslation) SpawnItemArgs(
        object? itemType,
        int misc2) =>
        (itemType, 0f, (float)misc2, 0, 0);

    public static float TurnAngleDegrees(int misc1) => misc1;

    public static (float angle, int flags, int ptr) FaceAngleArgs(int misc1) => (misc1, 0, 0);

    public static (int damage, int hitSound, int missSound, int damageType, bool bleed) ScratchMeleeArgs(int misc1, int misc2) =>
        (misc1, misc2 - 1, 0, 0, true);

    public static (int sound, int channel, float volume, bool loop, float attenuation, bool local, float pitch) PlaySoundArgs(
        int misc1,
        int misc2) =>
        (misc1 - 1, 0, 1f, false, misc2 != 0 ? 0f : 1f, false, 0f);

    public static (int maxChance, int jumpLabel) RandomJumpArgs(int misc1, int misc2) => (misc2, misc1);

    public static (int special, int tag) LineEffectArgs(int misc1, int misc2) => (misc1, misc2);

    public static bool SetDehParamsValidatePointerNative(int codepointer, int tableSize, out bool returnsState)
    {
        returnsState = codepointer == 6;
        return codepointer >= 0 && codepointer < tableSize;
    }

    public static bool SetDehParamsValidateFactoryNative(int codepointer, int factoryCount)
    {
        if (codepointer < 0)
            return false;
        return (uint)codepointer < (uint)factoryCount;
    }

    public static int ImplicitArgCountNative(bool returnsState, int numargs) =>
        returnsState ? numargs : numargs;

    public static string SanitizePatchNameNative(string patchName)
    {
        var sanitized = patchName;
        foreach (var ch in new[] { ':', '.', '-', ' ', '\\', '/', '*', '?', '"' })
            sanitized = sanitized.Replace(ch, '_');
        return sanitized;
    }

    public static string BuildPrintableDehackedNameNative(
        string sanitizedPatch,
        int index,
        string pointerName,
        int value1,
        int value2) =>
        $"Dehacked.{sanitizedPatch}.{index}.{pointerName}.{value1}.{value2}";

    public static (bool Ok, bool ReturnsState, int Value1, int Value2) TryValidateSetDehParamsNative(
        int codepointer,
        int pointerTableSize,
        int factoryCount,
        int value1,
        int value2)
    {
        if (!SetDehParamsValidatePointerNative(codepointer, pointerTableSize, out var returnsState))
            return (false, false, 0, 0);

        if (!SetDehParamsValidateFactoryNative(codepointer, factoryCount))
            return (false, false, 0, 0);

        return (true, returnsState, value1, value2);
    }

    public static bool SetDehParamsReturnsStateNative(int codepointer) => codepointer == 6;

    public static int MbfFactoryTableEntryCountNative() => 38;

    public static void EmitStandardActionParametersNative(int numArgs, Action<int> addParameterPointer)
    {
        for (var i = 0; i < numArgs; i++)
            addParameterPointer(i);
    }


    public static int EncodeReturnRegisterNative(bool returnsState, int whereRegNum) =>
        returnsState ? whereRegNum : 0;
}
