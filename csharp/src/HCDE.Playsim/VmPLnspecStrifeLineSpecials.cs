namespace HCDE.Playsim;

/// <summary>Force field, glass, Strife conversation from <c>p_lnspec.cpp</c> (3249–3412).</summary>
public static class VmPLnspecStrifeLineSpecials
{
    public static bool ForceFieldDamages(object? activator) => activator != null;

    public static int ForceFieldDamage() => 16;

    public static double ForceFieldThrustSpeed() => 7.8125;

    public static int ForceFieldAngleOffset() => 180;

    public static bool ClearForceFieldReturns(bool any) => any;

    public static bool StartConversationNeedsLivingPlayer(object? activator, object? player, object? mo, int health) =>
        activator != null && player != null && ReferenceEquals(mo, activator) && health > 0;

    public static bool StartConversationRejectNoTarget(object? target) => target == null;

    public static int ConversationDialogIndex(int arg1, int resolved) => arg1 != 0 ? resolved : 0;

    public static bool SetConversationRejectUnknown(int arg1, int index) => arg1 != 0 && index == -1;

    public static bool SetConversationIterateTid(int tid) => tid != 0;

    public static bool SetConversationApplyActivator(object? activator) => activator != null;

    public static bool GlassQuestRewards(bool q1, bool q2) => q1 || q2;

    public static string QuestItemClass() => "QuestItem29";

    public static string UpgradeAccuracyClass() => "UpgradeAccuracy";

    public static string UpgradeStaminaClass() => "UpgradeStamina";

    public static bool StartConversationFaceTalker(int arg) => arg != 0;
}
