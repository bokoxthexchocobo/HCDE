namespace HCDE.Gamedata;

internal static class DehackedOriginalHeights
{
    // One-based Doom OrgHeights from wadsrc/static/dehsupp.txt.
    public static int? Find(int index) => index >= 1 && index <= Values.Length ? Values[index - 1] : null;

    private static readonly int[] Values =
    [
        56, 56, 56, 56, 16, 56, 8, 16, 64, 8, 56, 56,
        56, 56, 56, 64, 8, 64, 56, 100, 64, 110, 56, 56,
        72, 16, 32, 32, 32, 16, 42, 8, 8, 8, 8, 8,
        8, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
        16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
        16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
        16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
        16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16,
        16, 16, 16, 16, 16, 68, 84, 84, 68, 52, 84, 68,
        52, 52, 68, 16, 16, 16, 16, 16, 16, 16, 16, 16,
        16, 16, 16, 16, 16, 16, 16, 16, 88, 88, 64, 64,
        64, 64, 16, 16, 16,
    ];
}
