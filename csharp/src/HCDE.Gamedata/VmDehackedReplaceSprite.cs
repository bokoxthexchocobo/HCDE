namespace HCDE.Gamedata;

/// <summary><c>ReplaceSpriteInData</c> pickup map reorder from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedReplaceSprite
{
    public static bool IsPlayerSpritePrefix(string oldSprite) =>
        oldSprite.Length >= 4 && oldSprite.StartsWith("PLAY", StringComparison.Ordinal);

    public static void BubbleSortSpriteMappings(List<string> sprites, string newSprite)
    {
        for (var i = 1; i < sprites.Count; i++)
        {
            while (i > 0 && string.Compare(sprites[i - 1], newSprite, StringComparison.Ordinal) > 0)
            {
                (sprites[i - 1], sprites[i]) = (sprites[i], sprites[i - 1]);
                i--;
            }

            while (i + 1 < sprites.Count &&
                   string.Compare(sprites[i + 1], newSprite, StringComparison.Ordinal) < 0)
            {
                (sprites[i + 1], sprites[i]) = (sprites[i], sprites[i + 1]);
                i++;
            }
        }
    }
}
