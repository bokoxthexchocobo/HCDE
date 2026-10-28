namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawBar::Tick</c> tail, fill normalization/interpolation, and <c>ParseComparator</c> head from
/// <c>sbarinfo_commands.cpp</c> (**2701–2850**). Chunk **1091**; **1090** owns ammo1/ammo2 empty-bar rules (**2684–2700**).
/// </summary>
public static class VmSbarinfoCommandDrawBar1091Slice
{
    public const double EqualEpsilon = 1e-6;

    public readonly record struct DrawBarAdditionalDataDefaults(bool UseMaximumConstant);

    public static DrawBarAdditionalDataDefaults CreateDrawBarAdditionalDataDefaults() =>
        new(UseMaximumConstant: false);

    public static (double Value, int Max) ResolveDrawBarInventoryItemTick(
        bool itemFound,
        int amount,
        int maxAmount) =>
        itemFound ? (amount, maxAmount) : (0, 0);

    public static (double Value, int Max) ResolveDrawBarFragsTick(double fragCount, int fragLimit) =>
        (fragCount, fragLimit);

    public static (double Value, int Max) ResolveDrawBarLevelStatTick(int found, int total) => (found, total);

    public static int ClampDrawBarAirtimeRemaining(int airFinished, int mapTime) =>
        Math.Clamp(airFinished - mapTime, 0, int.MaxValue);

    public static (double Value, int Max) ApplyDrawBarPowerupEffectDisplayTicks(int effectTics, int effectMax) =>
        (effectTics + 1, effectMax + 1);

    public static (int Value, int Max) ComputeDrawBarSavePercentTick(
        double hexenArmorSlotSum,
        bool basicArmorPresent,
        double basicSavePercent)
    {
        var add = hexenArmorSlotSum;
        if (basicArmorPresent)
            add += basicSavePercent * 100;
        return ((int)add, 100);
    }

    public static double NormalizeDrawBarFillRatio(double value, int max)
    {
        if (max != 0 && value > 0)
            return Math.Min(value / max, 1.0);
        return 0;
    }

    public static bool ShouldApplyDrawBarTickInterpolation(
        int interpolationSpeed,
        bool hudChanged,
        int levelTime) =>
        interpolationSpeed != 0 && (!hudChanged || levelTime == 1);

    public static double ResolveDrawBarAnimationPixelStep(
        bool pixelUnset,
        double foregroundDisplayWidth,
        double equalEpsilon) =>
        pixelUnset && foregroundDisplayWidth > 0
            ? Math.Max(equalEpsilon, 1.0 / foregroundDisplayWidth)
            : equalEpsilon;

    public static double StepDrawBarDrawValueInterpolation(
        double drawValue,
        double targetValue,
        double pixelThreshold,
        int interpolationSpeed,
        double equalEpsilon)
    {
        if (Math.Abs(drawValue - targetValue) < pixelThreshold)
            return targetValue;
        if (targetValue < drawValue)
            return drawValue - Math.Clamp((drawValue - targetValue) / 4, equalEpsilon, interpolationSpeed / 100.0);
        if (drawValue < targetValue)
            return drawValue + Math.Clamp((targetValue - drawValue) / 4, equalEpsilon, interpolationSpeed / 100.0);
        return drawValue;
    }

    public static double ResolveDrawBarTickDrawValue(
        double drawValue,
        double normalizedTarget,
        int interpolationSpeed,
        bool hudChanged,
        int levelTime,
        bool pixelUnset,
        double foregroundDisplayWidth) =>
        ShouldApplyDrawBarTickInterpolation(interpolationSpeed, hudChanged, levelTime)
            ? StepDrawBarDrawValueInterpolation(
                drawValue,
                normalizedTarget,
                ResolveDrawBarAnimationPixelStep(pixelUnset, foregroundDisplayWidth, EqualEpsilon),
                interpolationSpeed,
                EqualEpsilon)
            : normalizedTarget;

    public static bool ShouldReplaceDrawBarComparatorInventoryClass(bool classFound, bool isInventoryKind) =>
        !classFound || !isInventoryKind;

    public static (bool Applied, bool UseMaximumConstant, int MaximumConstant)
        TryApplyDrawBarComparatorMaximumConstant(bool extendedSyntax, bool hasIntConst, int number) =>
        extendedSyntax && hasIntConst
            ? (Applied: true, UseMaximumConstant: true, MaximumConstant: number)
            : (Applied: false, UseMaximumConstant: false, MaximumConstant: 0);
}
