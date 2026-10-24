namespace HCDE.Playsim;

/// <summary><c>LineSpecials[]</c> slots 150–284 and name table init from <c>p_lnspec.cpp</c> (3695–3870).</summary>
public static class VmPLnspecLineSpecialsTableB
{
    public const int MaxSlot = 284;

    public static bool SlotInRange(int index) => index >= 150 && index <= MaxSlot;

    public static bool IsNopSlot(int index) => index is 152 or 153 or 155 or 156 or 160 or 161 or 162 or 163 or 164 or 165 or 166 or 167 or 181 or 182 or 190 or 209 or 210 or 211 or 222 or 225;

    public static bool IsTeleportSlot(int index) => index is 154 or 215;

    public static bool IsScrollSlot(int index) => index is 221 or 223 or 224;

    public static bool IsSectorVisualSlot(int index) =>
        index is 170 or 171 or 183 or 184 or 185 or 186 or 187 or 188 or 189 or 212 or 213 or 277 or 278;

    public static bool IsEnvSlot(int index) =>
        index is 157 or 159 or 216 or 218 or 219 or 220 or 214;

    public static bool IsThingAdvancedSlot(int index) =>
        index is 173 or 174 or 175 or 176 or 177 or 178 or 179 or 180 or 229;

    public static bool IsGenericMoverSlot(int index) =>
        index is 200 or 201 or 202 or 203 or 204 or 205 or 284;

    public static bool IsPlatExtensionSlot(int index) =>
        index is 172 or 206 or 207 or 228 or 230 or 231;

    public static bool IsCeilingExtensionSlot(int index) =>
        index is 168 or 169 or 192 or 193 or 194 or 195 or 196 or 197 or 198 or 199;

    public static bool IsLightExtensionSlot(int index) =>
        index is 232 or 233 or 234;

    public static bool IsExitSlot(int index) => index is 243 or 244;

    public static bool IsElevatorSlot(int index) => index is 245 or 246 or 247;

    public static bool MapsToCallable(int index) => SlotInRange(index) && !IsNopSlot(index);

    public static int LineSpecialNameCompare(string a, string b) =>
        string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

    public static int BinarySearchStep(int lexval) => lexval switch
    {
        0 => 0,
        > 0 => 1,
        _ => -1,
    };

    public static bool AssertUniqueSlot(bool existingNull) => existingNull;

    public static int ResizeInfoTable(int maxNumber) => maxNumber + 1;

    public static bool TeleportSpecialAlias(string name) =>
        string.Equals(name, "TeleportSpecial", StringComparison.OrdinalIgnoreCase);

    public static (int Min, int Max) TeleportAliasArgs() => (1, 3);

    public static int TeleportAliasNumber() => 70;

    public static bool IsFloorTransferSlot(int index) => index is 235 or 236;

    public static bool IsCameraSlot(int index) => index is 237;

    public static bool IsFloorRaiseLowerSlot(int index) =>
        index is 238 or 239 or 240 or 241 or 242 or 256 or 257 or 258 or 259 or 260 or 261;

    public static bool IsCeilingMoveSlot(int index) =>
        index is 252 or 253 or 254 or 255 or 262 or 263 or 264 or 265 or 266 or 267 or 268 or 269;

    public static bool IsStairsExtensionSlot(int index) =>
        index is 270 or 271 or 272 or 273;

    public static bool IsDoorAnimatedSlot(int index) => index is 274;

    public static bool IsMoverStopSlot(int index) => index is 275 or 276;

    public static bool IsGlowSlot(int index) => index is 277 or 278;

    public static bool IsMoveToValueCrushSlot(int index) => index is 279 or 280;

    public static bool IsAutomapSlot(int index) => index is 281 or 282;

    public static bool IsPolyobjSoundSlot(int index) => index is 283;

    public static bool IsHealThingSlot(int index) => index is 248;

    public static bool IsDoorCloseWaitOpenSlot(int index) => index is 249;

    public static bool IsDonutSlot(int index) => index is 250;

    public static bool IsFloorCeilingComboSlot(int index) => index is 251;

    public static int SlotHandlerKind(int index)
    {
        if (!SlotInRange(index)) return 0;
        if (IsNopSlot(index)) return 1;
        if (IsGenericMoverSlot(index)) return 2;
        if (IsTeleportSlot(index)) return 3;
        if (IsScrollSlot(index)) return 4;
        if (IsExitSlot(index)) return 5;
        return 6;
    }

    public static bool MaxSlotScanIncludes(int number, int currentMax) => number > currentMax;
}
