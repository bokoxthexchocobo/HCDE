namespace HCDE.Gamedata;

public sealed class VmDehackedThingCombatExtras
{
    public string? DropItem { get; set; }
    public int BloodColor { get; set; }
    public int BloodTranslation { get; set; } = -1;
    public double MeleeThreshold { get; set; }
    public double MaxTargetRange { get; set; }
    public double MinMissileChance { get; set; } = 200;
    public double MissileChanceMult { get; set; } = 1;
    public double ProjectilePassHeight { get; set; } = -1;
    public double PhysicalHeight { get; set; } = -1;
    public string? Tag { get; set; }
    public string? Obituary { get; set; }
    public string? MeleeObituary { get; set; }
    public string? SelfObituary { get; set; }
}

/// <summary><c>PatchThing</c> blood/drop/crispy fields from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchThingCombatExtrasParser
{
    private static readonly int[] BloodColors =
    [
        0, unchecked((int)0xffcccccc), unchecked((int)0xff63af57), unchecked((int)0xff6357af),
        unchecked((int)0xffffd300), unchecked((int)0xff333333), unchecked((int)0xffff30ff),
        unchecked((int)0xffffffff), unchecked((int)0xffff8000)
    ];

    public static bool TryApplyLine(string key, string value, VmDehackedThingCombatExtras extras)
    {
        if (key.Equals("dropped item", StringComparison.OrdinalIgnoreCase))
        {
            if (value == "-1") extras.DropItem = null;
            else extras.DropItem = value;
            return true;
        }

        if (key.Equals("blood color", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(value, out var idx))
        {
            idx = Math.Clamp(idx, 0, 8);
            extras.BloodColor = BloodColors[idx];
            extras.BloodTranslation = idx == 0 ? -1 : idx;
            return true;
        }

        if (key.Equals("Melee threshold", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var meleeThreshold))
        {
            extras.MeleeThreshold = meleeThreshold;
            return true;
        }

        if (key.Equals("Max target range", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var maxRange))
        {
            extras.MaxTargetRange = maxRange;
            return true;
        }

        if (key.Equals("Min missile chance", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var minChance))
        {
            extras.MinMissileChance = minChance;
            return true;
        }

        if (key.Equals("Missile chance multiplier", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var mult))
        {
            extras.MissileChanceMult = mult;
            return true;
        }

        if (key.Equals("Projectile pass height", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var pph))
        {
            extras.ProjectilePassHeight = pph;
            return true;
        }

        if (key.Equals("Physical height", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var phys))
        {
            extras.PhysicalHeight = phys;
            return true;
        }

        if (key.Equals("Tag", StringComparison.OrdinalIgnoreCase))
        {
            extras.Tag = value.Trim();
            return true;
        }

        if (key.Equals("Obituary", StringComparison.OrdinalIgnoreCase))
        {
            extras.Obituary = value.Trim();
            return true;
        }

        if (key.Equals("Melee obituary", StringComparison.OrdinalIgnoreCase))
        {
            extras.MeleeObituary = value.Trim();
            return true;
        }

        if (key.Equals("Self obituary", StringComparison.OrdinalIgnoreCase))
        {
            extras.SelfObituary = value.Trim();
            return true;
        }

        return false;
    }

    public static uint ParseMbf21Bits(string line, IReadOnlyList<string> flagNames)
    {
        uint value = 0;
        foreach (var token in line.Split([',', '|', '+', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (long.TryParse(token, out var numeric))
            {
                value |= (uint)numeric;
                continue;
            }

            for (var i = 0; i < flagNames.Count; i++)
            {
                if (flagNames[i].Equals(token, StringComparison.OrdinalIgnoreCase))
                {
                    value |= 1u << i;
                    break;
                }
            }
        }

        return value;
    }
}
