namespace HCDE.Playsim;

/// <summary>Named <c>p_actionfunctions.cpp</c> / enemy entry points for managed state tables and regression tests.</summary>
public static class ActorActionDispatch
{
    private static readonly IReadOnlyDictionary<string, Action<Actor>> NameActions = new Dictionary<string, Action<Actor>>(StringComparer.OrdinalIgnoreCase)
    {
        ["A_BulletAttack"] = ActorCombatActions.BulletAttack,
        ["A_CustomBulletAttack"] = actor => ActorCombatActions.CustomBulletAttack(actor, 5.625, 0, 1, 3),
        ["A_CustomMissile"] = actor => ActorCombatActions.CustomMissile(actor, "DoomImpBall"),
        ["A_CustomMeleeAttack"] = actor => ActorCombatActions.CustomMeleeAttack(actor, 3),
        ["A_CustomComboAttack"] = actor => ActorCombatActions.CustomComboAttack(actor, "DoomImpBall", 32, 3),
        ["A_Look"] = actor => ActorLookActions.LookEx(actor),
        ["A_LookEx"] = actor => ActorLookActions.LookEx(actor),
        ["A_Look2"] = actor => ActorLook2Actions.Look2(actor),
        ["A_ClearLastHeard"] = actor => ActorClearLastHeardActions.ClearLastHeard(actor),
        ["A_Chase"] = actor => ActorChaseActions.Chase(actor),
        ["A_Pain"] = ActorSoundActions.Pain,
        ["A_MonsterRail"] = ActorMonsterRailActions.MonsterRail,
        ["A_BossDeath"] = BossDeathActions.BossDeath,
        ["A_ClassBossHealth"] = ActorClassBossHealthActions.ClassBossHealth,
        ["A_NoBlocking"] = actor => ActorUnblockActions.NoBlocking(actor),
        ["A_Remove"] = actor => ActorRemovalActions.Remove(actor),
        ["A_ResetHealth"] = actor => ActorHealthActions.ResetHealth(actor),
        ["A_FaceTarget"] = actor => ActorOrientationActions.FaceTarget(actor),
        ["A_DamageSelf"] = actor => ActorHealthActions.DamageSelf(actor, 1),
        ["A_DamageTarget"] = actor => ActorHealthActions.DamageTarget(actor, 1),
        ["A_GiveInventory"] = actor => ActorInventoryActions.GiveInventory(actor, "Health"),
        ["A_TakeInventory"] = actor => ActorInventoryActions.TakeInventory(actor, "Health", 1),
        ["A_DropInventory"] = actor => ActorInventoryActions.DropInventory(actor, "Health"),
        ["A_CheckInventory"] = actor => ActorInventoryActions.CheckInventory(actor, "Health", 1),
        ["A_ChangeFlag"] = actor => ActorPropertyActions.ChangeFlag(actor, "SOLID", true),
        ["A_SetTics"] = actor => actor.States.SetTics(1),
        ["A_RadiusDamageSelf"] = actor => ActorRadiusSelfActions.Apply(actor),
        ["A_RadiusGive"] = actor => ActorRadiusGiveActions.RadiusGive(actor, "Health", 128,
            ActorRadiusGiveActions.RadiusGiveFlags.Players | ActorRadiusGiveActions.RadiusGiveFlags.GiveSelf),
        ["A_Explode"] = actor => ActorExplodeActions.Explode(actor, 128, 128),
        ["A_Detonate"] = actor => ActorExplodeActions.Detonate(actor),
        ["A_RadiusThrust"] = actor => ActorExplodeActions.RadiusThrust(actor),
        ["A_MonsterRefire"] = actor => ActorRefireActions.MonsterRefire(actor, 256, actor.SeeState),
        ["A_Jump"] = actor => ActorJumpActions.Jump(actor, 256, actor.SeeState),
        ["A_SpawnProjectile"] = actor => ActorCombatActions.SpawnProjectile(actor, "DoomImpBall", 32, 0, default,
            ActorCombatActions.SpawnProjectileFlags.AimModeMask, default, AcsActorPointer.Target),
    };

    public static bool TryInvoke(string name, Actor actor)
    {
        if (!NameActions.TryGetValue(name, out var action)) return false;
        action(actor);
        return true;
    }

    public static IReadOnlyCollection<string> WiredNames => NameActions.Keys.ToArray();
}
