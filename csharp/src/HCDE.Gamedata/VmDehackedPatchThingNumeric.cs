namespace HCDE.Gamedata;

public sealed class VmDehackedThingPatchFlags
{
    public bool HadHeight { get; set; }
    public bool HadProjHeight { get; set; }
    public bool HadPhysHeight { get; set; }
    public bool HadTranslucency { get; set; }
    public bool HadStyle { get; set; }
}

public sealed class VmDehackedThingPatchTarget
{
    public int Health { get; set; }
    public int ReactionTime { get; set; }
    public int PainChance { get; set; }
    public double Alpha { get; set; } = 1;
    public int RenderStyle { get; set; }
    public double Height { get; set; } = -1;
    public double ProjectilePassHeight { get; set; } = -1;
    public int Damage { get; set; }
    public double Speed { get; set; }
    public double Radius { get; set; }
    public double Scale { get; set; } = 1;
    public bool NoIceDeath { get; set; }
    public double FastSpeed { get; set; }
    public double MeleeRange { get; set; }
    public int BounceFlags { get; set; }
    public string? DamageType { get; set; }
    public double Gravity { get; set; } = 1;
}

/// <summary><c>PatchThing</c> numeric dispatch from <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedPatchThingNumeric
{
    public static bool IsSupportedNumeric(long value) =>
        value >= int.MinValue && value <= uint.MaxValue;

    public static bool TryApplyNumericProperty(
        string key,
        string valueText,
        VmDehackedThingPatchTarget target,
        VmDehackedThingPatchFlags flags)
    {
        if (!long.TryParse(valueText, out var val) || !IsSupportedNumeric(val))
            return false;

        if (key.Equals("Hit points", StringComparison.OrdinalIgnoreCase) && key.Length == 10)
        {
            target.Health = (int)val;
            return true;
        }

        if (key.Equals("Reaction time", StringComparison.OrdinalIgnoreCase) && key.Length == 13)
        {
            target.ReactionTime = (int)val;
            return true;
        }

        if (key.Equals("Pain chance", StringComparison.OrdinalIgnoreCase) && key.Length == 11)
        {
            target.PainChance = (int)val;
            return true;
        }

        if (key.Equals("Translucency", StringComparison.OrdinalIgnoreCase) && key.Length == 12)
        {
            target.Alpha = VmDehackedNumericConversion.DehToDouble(val);
            target.RenderStyle = 2;
            flags.HadTranslucency = true;
            flags.HadStyle = true;
            return true;
        }

        if (key.Equals("Height", StringComparison.OrdinalIgnoreCase) && key.Length == 6)
        {
            if (!flags.HadPhysHeight)
                target.Height = VmDehackedNumericConversion.DehToDouble(val);
            if (!flags.HadProjHeight)
                target.ProjectilePassHeight = 0;
            flags.HadHeight = true;
            return true;
        }

        if (key.Equals("Missile damage", StringComparison.OrdinalIgnoreCase) && key.Length == 14)
        {
            target.Damage = (int)val;
            return true;
        }

        if (key.Length == 5)
        {
            if (key.Equals("Speed", StringComparison.OrdinalIgnoreCase))
            {
                target.Speed = val;
                return true;
            }

            if (key.Equals("Width", StringComparison.OrdinalIgnoreCase))
            {
                target.Radius = VmDehackedNumericConversion.DehToDouble(val);
                return true;
            }

            if (key.Equals("Alpha", StringComparison.OrdinalIgnoreCase) &&
                double.TryParse(valueText, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var alpha))
            {
                target.Alpha = alpha;
                flags.HadTranslucency = true;
                return true;
            }

            if (key.Equals("Scale", StringComparison.OrdinalIgnoreCase) &&
                double.TryParse(valueText, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var scale))
            {
                target.Scale = Math.Clamp(scale, 0.0001, 256);
                return true;
            }
        }

        if (key.Equals("No Ice Death", StringComparison.OrdinalIgnoreCase) && key.Length == 12)
        {
            target.NoIceDeath = val != 0;
            return true;
        }

        if (key.Equals("fast speed", StringComparison.OrdinalIgnoreCase) && key.Length == 10)
        {
            target.FastSpeed = val >= 256 ? VmDehackedNumericConversion.DehToDouble(val) : val;
            return true;
        }

        if (key.Equals("melee range", StringComparison.OrdinalIgnoreCase) && key.Length == 11)
        {
            target.MeleeRange = VmDehackedNumericConversion.DehToDouble(val) - 20;
            return true;
        }

        return false;
    }
}
