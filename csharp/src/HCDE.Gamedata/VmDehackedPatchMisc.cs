namespace HCDE.Gamedata;

/// <summary><c>PatchMisc</c> dispatch helpers from <c>d_dehacked.cpp</c> (2537–2730).</summary>
public static class VmDehackedPatchMisc
{
    public static bool LineReady(int getLineResult) => getLineResult == 1;

    public static bool BfgCellsKey(string line1) =>
        string.Equals(line1, "BFG Cells/Shot", StringComparison.OrdinalIgnoreCase);

    public static bool RocketStyleKey(string line1) =>
        string.Equals(line1, "Rocket Explosion Style", StringComparison.OrdinalIgnoreCase);

    public static bool RocketAlphaKey(string line1) =>
        string.Equals(line1, "Rocket Explosion Alpha", StringComparison.OrdinalIgnoreCase);

    public static bool MonstersInfightKey(string line1) =>
        string.Equals(line1, "Monsters Infight", StringComparison.OrdinalIgnoreCase);

    public static bool MonstersIgnoreKey(string line1) =>
        string.Equals(line1, "Monsters Ignore Each Other", StringComparison.OrdinalIgnoreCase);

    public static bool PowerupColorPrefix(string line1) =>
        line1.StartsWith("Powerup Color ", StringComparison.OrdinalIgnoreCase);

    public static string PowerupColorSuffix(string line1) => line1.Length > 14 ? line1[14..] : string.Empty;

    public static int InfightingFromIgnore(bool ignore, int raw) => ignore && raw != 0 ? -1 : raw;

    public static bool ExplosionStyleFound(int style) => style >= 0;

    public static double ParseExplosionAlpha(string line2) =>
        double.TryParse(line2, out var v) ? v : 0;

    public readonly record struct PowerupColorParse(bool Found, int R, int G, int B, float A);

    public static bool PowerupColorValid(PowerupColorParse p) => p.Found && p.A >= 0;

    public static bool NoAutofreezeKey(string key) =>
        string.Equals(key, "No Autofreeze", StringComparison.OrdinalIgnoreCase);

    public static bool StartHealthKey(string key) =>
        string.Equals(key, "Initial Health", StringComparison.OrdinalIgnoreCase);

    public static bool MaxSoulsphereKey(string key) =>
        string.Equals(key, "Max Soulsphere", StringComparison.OrdinalIgnoreCase);

    public static bool GodModeHealthKey(string key) =>
        string.Equals(key, "God Mode Health", StringComparison.OrdinalIgnoreCase);

    public static bool IdfaArmorKey(string key) =>
        string.Equals(key, "IDFA Armor", StringComparison.OrdinalIgnoreCase);

    public static bool IdkfaArmorClassKey(string key) =>
        string.Equals(key, "IDKFA Armor Class", StringComparison.OrdinalIgnoreCase);

    public static bool GreenArmorClassKey(string key) =>
        string.Equals(key, "Green Armor Class", StringComparison.OrdinalIgnoreCase);

    public static bool BlueArmorClassKey(string key) =>
        string.Equals(key, "Blue Armor Class", StringComparison.OrdinalIgnoreCase);

    public static bool MegasphereHealthKey(string key) =>
        string.Equals(key, "Megasphere Health", StringComparison.OrdinalIgnoreCase);

    public static int ParseIntField(string line2) =>
        int.TryParse(line2, out var v) ? v : 0;

    public readonly record struct MiscLineLoop(int GetLineResult, bool Handled);

    public static bool ShouldContinueMiscLoop(MiscLineLoop ctx) =>
        LineReady(ctx.GetLineResult);

    public static bool ApplyHandledMisc(MiscLineLoop ctx) => ctx.Handled;

    public static bool PowerupColorRgbaParsed(int r, int g, int b, float a) =>
        r >= 0 && g >= 0 && b >= 0 && a >= 0;

    public static bool DehInfoOffsetValid(int offset) => offset > 0;

    public static bool SoulsphereHealthKey(string key) =>
        string.Equals(key, "Soulsphere Health", StringComparison.OrdinalIgnoreCase);

    public static bool InitialBulletsKey(string key) =>
        string.Equals(key, "Initial Bullets", StringComparison.OrdinalIgnoreCase);

    public static bool MaxArmorKey(string key) =>
        string.Equals(key, "Max Armor", StringComparison.OrdinalIgnoreCase);

    public static bool MaxHealthKey(string key) =>
        string.Equals(key, "Max Health", StringComparison.OrdinalIgnoreCase);

    public static int ParseMiscInt(string line2, int fallback) =>
        int.TryParse(line2, out var v) ? v : fallback;

    public sealed class DehMiscInfo
    {
        public int StartHealth { get; set; }
        public int StartBullets { get; set; }
        public int MaxHealth { get; set; }
        public int MaxArmor { get; set; }
        public int GreenAc { get; set; }
        public int BlueAc { get; set; }
        public int MaxSoulsphere { get; set; }
        public int SoulsphereHealth { get; set; }
        public int MegasphereHealth { get; set; }
        public int GodHealth { get; set; }
        public int FaArmor { get; set; }
        public int FaAc { get; set; }
        public int KfaArmor { get; set; }
        public int KfaAc { get; set; }
        public int NoAutofreeze { get; set; }
        public int BfgCells { get; set; }
        public int ExplosionStyle { get; set; }
        public double ExplosionAlpha { get; set; }
    }

    public static IReadOnlyList<VmDehackedStructKey> CreateMiscStructKeys(DehMiscInfo deh) =>
    [
        new VmDehackedStructKey { Name = "Initial Health", Setter = v => deh.StartHealth = v },
        new VmDehackedStructKey { Name = "Initial Bullets", Setter = v => deh.StartBullets = v },
        new VmDehackedStructKey { Name = "Max Health", Setter = v => deh.MaxHealth = v },
        new VmDehackedStructKey { Name = "Max Armor", Setter = v => deh.MaxArmor = v },
        new VmDehackedStructKey { Name = "Green Armor Class", Setter = v => deh.GreenAc = v },
        new VmDehackedStructKey { Name = "Blue Armor Class", Setter = v => deh.BlueAc = v },
        new VmDehackedStructKey { Name = "Max Soulsphere", Setter = v => deh.MaxSoulsphere = v },
        new VmDehackedStructKey { Name = "Soulsphere Health", Setter = v => deh.SoulsphereHealth = v },
        new VmDehackedStructKey { Name = "Megasphere Health", Setter = v => deh.MegasphereHealth = v },
        new VmDehackedStructKey { Name = "God Mode Health", Setter = v => deh.GodHealth = v },
        new VmDehackedStructKey { Name = "IDFA Armor", Setter = v => deh.FaArmor = v },
        new VmDehackedStructKey { Name = "IDFA Armor Class", Setter = v => deh.FaAc = v },
        new VmDehackedStructKey { Name = "IDKFA Armor", Setter = v => deh.KfaArmor = v },
        new VmDehackedStructKey { Name = "IDKFA Armor Class", Setter = v => deh.KfaAc = v },
        new VmDehackedStructKey { Name = "No Autofreeze", Setter = v => deh.NoAutofreeze = v },
    ];

    public static readonly string[] PowerupBlendTypeNames =
    [
        "PowerInvulnerable",
        "PowerStrength",
        "PowerInvisibility",
        "PowerIronFeet",
        "PowerLightAmp",
        "PowerWeaponLevel2",
        "PowerSpeed",
        "PowerMinotaur",
    ];

    public sealed class MiscDefaultPatchPlan
    {
        public int GreenArmorSaveAmount { get; set; }
        public double GreenArmorSavePercent { get; set; }
        public int BlueArmorSaveAmount { get; set; }
        public double BlueArmorSavePercent { get; set; }
        public int ArmorBonusMaxSave { get; set; }
        public int SoulsphereAmount { get; set; }
        public int SoulsphereMaxAmount { get; set; }
        public int MegasphereAmount { get; set; }
        public int DoomPlayerHealth { get; set; }
        public int DoomPlayerClipAmount { get; set; }
        public List<MiscPowerupBlendEntry> PowerupBlends { get; } = [];
    }

    public sealed class MiscPowerupBlendEntry
    {
        public required string TypeName { get; init; }
        public uint BlendColor { get; init; }
    }

    public static void ProcessMiscLine(
        DehMiscInfo deh,
        ref int infighting,
        string line1,
        string line2,
        Func<string, int> findStyle,
        MiscDefaultPatchPlan? blendSink,
        Action<string>? logUnknownMisc)
    {
        var intValue = ParseIntField(line2);
        if (VmDehackedStructKeyApply.TryApply(CreateMiscStructKeys(deh), line1, intValue))
            return;

        if (BfgCellsKey(line1))
        {
            deh.BfgCells = intValue;
            return;
        }

        if (RocketStyleKey(line1))
        {
            var style = findStyle(line2.Trim());
            if (ExplosionStyleFound(style))
                deh.ExplosionStyle = style;
            return;
        }

        if (RocketAlphaKey(line1))
        {
            deh.ExplosionAlpha = ParseExplosionAlpha(line2);
            return;
        }

        if (MonstersInfightKey(line1))
        {
            infighting = intValue;
            return;
        }

        if (MonstersIgnoreKey(line1))
        {
            infighting = InfightingFromIgnore(true, intValue);
            return;
        }

        if (PowerupColorPrefix(line1))
        {
            if (!VmDehackedPatchMiscPowerups.TryMapPowerupIndex(line1, out var index))
            {
                logUnknownMisc?.Invoke(line1);
                return;
            }

            if (!VmDehackedPatchMiscPowerups.TryParseRgba(line2, out var r, out var g, out var b, out var a))
            {
                logUnknownMisc?.Invoke($"Bad powerup color \"{line2}\" for {line1}");
                return;
            }

            if (blendSink is null)
                return;

            var typeName = PowerupBlendTypeNames[index];
            if (a > 0)
            {
                var alphaByte = (byte)(Math.Clamp(a, 0f, 1f) * 255f);
                var color = PalEntryPack(alphaByte, (byte)ClampByte(r), (byte)ClampByte(g), (byte)ClampByte(b));
                blendSink.PowerupBlends.Add(new MiscPowerupBlendEntry { TypeName = typeName, BlendColor = color });
            }
            else
            {
                blendSink.PowerupBlends.Add(new MiscPowerupBlendEntry { TypeName = typeName, BlendColor = 0 });
            }

            return;
        }

        logUnknownMisc?.Invoke(line1);
    }

    public static int RunPatchMiscLoop(
        Func<int> getLine,
        Func<string> readLine1,
        Func<string> readLine2,
        DehMiscInfo deh,
        ref int infighting,
        Func<string, int> findStyle,
        MiscDefaultPatchPlan? blendSink = null,
        Action<string>? logUnknownMisc = null)
    {
        int result;
        while ((result = getLine()) == 1)
        {
            ProcessMiscLine(deh, ref infighting, readLine1(), readLine2(), findStyle, blendSink, logUnknownMisc);
        }

        return result;
    }

    public static void FillMiscDefaultPatchPlan(DehMiscInfo deh, MiscDefaultPatchPlan plan)
    {
        plan.GreenArmorSaveAmount = VmDehackedPatchMiscArmor.ArmorSaveAmount(deh.GreenAc);
        plan.GreenArmorSavePercent = VmDehackedPatchMiscArmor.GreenArmorSavePercent(deh.GreenAc);
        plan.BlueArmorSaveAmount = VmDehackedPatchMiscArmor.ArmorSaveAmount(deh.BlueAc);
        plan.BlueArmorSavePercent = VmDehackedPatchMiscArmor.GreenArmorSavePercent(deh.BlueAc);
        plan.ArmorBonusMaxSave = deh.MaxArmor;
        plan.SoulsphereAmount = deh.SoulsphereHealth;
        plan.SoulsphereMaxAmount = deh.MaxSoulsphere;
        plan.MegasphereAmount = deh.MegasphereHealth;
        plan.DoomPlayerHealth = deh.StartHealth;
        plan.DoomPlayerClipAmount = deh.StartBullets;
    }

    public static int FinalizeMiscInfighting(int infighting) =>
        VmDehackedPatchMiscArmor.NormalizeInfighting(infighting);

    static int ClampByte(int v) => Math.Clamp(v, 0, 255);

    static uint PalEntryPack(byte a, byte r, byte g, byte b) =>
        ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | (uint)b;
}
