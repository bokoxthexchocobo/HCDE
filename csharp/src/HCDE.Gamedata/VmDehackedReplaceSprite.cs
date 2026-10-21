namespace HCDE.Gamedata;

/// <summary><c>ReplaceSpriteInData</c> from <c>d_dehacked.cpp</c> (2894–2925).</summary>
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

    public static void ReplaceSpriteInDataNative(
        string oldStr,
        string newStr,
        Span<char> playerSprite,
        IList<string> pickupSprites,
        Action<int, string> swapPickupSprites)
    {
        if (IsPlayerSpritePrefix(oldStr))
        {
            for (var i = 0; i < 4 && i < newStr.Length; i++)
                playerSprite[i] = newStr[i];
        }

        for (var i = 0; i < pickupSprites.Count; i++)
        {
            var sprite = pickupSprites[i];
            if (sprite.Length < 4 || string.CompareOrdinal(sprite, 0, oldStr, 0, 4) != 0)
                continue;

            swapPickupSprites(i, newStr);

            var j = i;
            while (j > 0 && string.Compare(pickupSprites[j - 1], newStr, StringComparison.Ordinal) > 0)
            {
                (pickupSprites[j - 1], pickupSprites[j]) = (pickupSprites[j], pickupSprites[j - 1]);
                j--;
            }

            while (j + 1 < pickupSprites.Count &&
                   string.Compare(pickupSprites[j + 1], newStr, StringComparison.Ordinal) < 0)
            {
                (pickupSprites[j + 1], pickupSprites[j]) = (pickupSprites[j], pickupSprites[j + 1]);
                j++;
            }

            break;
        }
    }
}
