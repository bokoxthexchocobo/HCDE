namespace HCDE.Playsim;

/// <summary><c>P_FindLineSpecial</c>, <c>P_ExecuteSpecial</c>, and lookup helpers from <c>p_lnspec.cpp</c> (3878–3982).</summary>
public static class VmPLnspecFindExecuteSpecial
{
    public const int TeleportSpecialNumber = 70;
    public const int TeleportAliasMinArgs = 1;
    public const int TeleportAliasMaxArgs = 3;

    public static int MaxLineSpecialIndex(int tableSize) => tableSize - 1;

    public static bool SpecialInTable(uint special, int tableSize) => special < (uint)tableSize;

    public static int BinarySearchMid(int min, int max) => (min + max) / 2;

    public static int BinarySearchStep(int lexval) => lexval switch
    {
        0 => 0,
        > 0 => 1,
        _ => -1,
    };

    public static (int Min, int Max) SearchBounds(int count) => (0, count - 1);

    public static bool SearchContinue(int min, int max) => min <= max;

    public static int AdvanceMin(int mid) => mid + 1;

    public static int RetreatMax(int mid) => mid - 1;

    public static bool TeleportSpecialAlias(string name) =>
        string.Equals(name, "TeleportSpecial", StringComparison.OrdinalIgnoreCase);

    public static int TeleportAliasNumber() => TeleportSpecialNumber;

    public static int FindFailure() => 0;

    public static bool ExecuteInRange(int num, int count) => num >= 0 && num < count;

    public static int ExecuteOutOfRange() => 0;

    public static bool WriteMinArgs(int? minArgs, int value) => minArgs != null;

    public static bool WriteMaxArgs(int? maxArgs, int value) => maxArgs != null;

    public static int CompareSpecialName(string a, string b) =>
        string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

    public static (int Number, int MinArgs, int MaxArgs) HitEntry(int number, int min, int max) => (number, min, max);

    public static bool VmExecuteSpecialParams(bool lineside) => lineside;

    public static int ActionReturnInt(int result) => result;

    public static int FindLineSpecialStep(string name, string[] sortedNames, int[] numbers, int[] minArgs, int[] maxArgs, out int minOut, out int maxOut)
    {
        var (min, max) = SearchBounds(sortedNames.Length);
        while (SearchContinue(min, max))
        {
            int mid = BinarySearchMid(min, max);
            int lex = CompareSpecialName(name, sortedNames[mid]);
            if (lex == 0)
            {
                minOut = minArgs[mid];
                maxOut = maxArgs[mid];
                return numbers[mid];
            }
            if (lex > 0) min = AdvanceMin(mid);
            else max = RetreatMax(mid);
        }
        if (TeleportSpecialAlias(name))
        {
            minOut = TeleportAliasMinArgs;
            maxOut = TeleportAliasMaxArgs;
            return TeleportAliasNumber();
        }
        minOut = 0;
        maxOut = 0;
        return FindFailure();
    }
}
