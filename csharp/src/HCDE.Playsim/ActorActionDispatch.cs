namespace HCDE.Playsim;

/// <summary>Named <c>p_actionfunctions.cpp</c> entry points for managed state tables and regression tests.</summary>
public static class ActorActionDispatch
{
    private static readonly IReadOnlyDictionary<string, Action<Actor>> NameActions = new Dictionary<string, Action<Actor>>(StringComparer.OrdinalIgnoreCase)
    {
        ["A_BulletAttack"] = ActorCombatActions.BulletAttack,
        ["A_Look"] = actor => ActorLookActions.LookEx(actor),
        ["A_Chase"] = actor => ActorChaseActions.ExtChase(actor, useMelee: true, useMissile: true),
        ["A_NoBlocking"] = actor => ActorUnblockActions.NoBlocking(actor),
        ["A_Remove"] = actor => ActorRemovalActions.Remove(actor),
        ["A_ResetHealth"] = actor => ActorHealthActions.ResetHealth(actor),
        ["A_FaceTarget"] = actor => ActorOrientationActions.FaceTarget(actor),
        ["A_DamageSelf"] = actor => ActorHealthActions.DamageSelf(actor, 1),
        ["A_DamageTarget"] = actor => ActorHealthActions.DamageTarget(actor, 1),
        ["A_CustomMeleeAttack"] = actor => ActorCombatActions.CustomMeleeAttack(actor, 3),
        ["A_CustomComboAttack"] = actor => ActorCombatActions.CustomComboAttack(actor, "DoomImpBall", 32, 3),
    };

    public static bool TryInvoke(string name, Actor actor)
    {
        if (!NameActions.TryGetValue(name, out var action)) return false;
        action(actor);
        return true;
    }

    public static IReadOnlyCollection<string> WiredNames => NameActions.Keys.ToArray();
}
