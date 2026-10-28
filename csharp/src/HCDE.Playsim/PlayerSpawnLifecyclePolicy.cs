namespace HCDE.Playsim;

/// <summary>Native <c>SpawnPlayer</c> reborn, inventory transfer, and presentation hooks.</summary>
public static class PlayerSpawnLifecyclePolicy
{
    public enum PlayerSpawnState
    {
        Enter = 0,
        Reborn = 1,
        Live = 2,
    }

    public sealed class LifecycleContext
    {
        public PlayerSpawnState State { get; init; }
        public bool RememberLastWeaponDmFlag { get; init; }
        public bool TempPlayerSpawn { get; init; }
        public bool OldActorOwnedByPlayer { get; init; }
        public bool PlayerPawnDontTranslate { get; init; }
        public bool PlayerPawnNoSkin { get; init; }
        public int PlayerNumber { get; init; }
        public int SkinSprite { get; init; }
        public int PlayerHealth { get; init; }
    }

    public sealed class LifecyclePlan
    {
        public bool CallPlayerReborn { get; init; }
        public bool TransferVoodooInventory { get; init; }
        public bool CancelOldActorScripts { get; init; }
        public bool RememberReadyWeapon { get; init; }
        public bool ApplyPlayerTranslation { get; init; }
        public int TranslationPlayerIndex { get; init; }
        public bool ApplySkinSprite { get; init; }
        public int SkinSprite { get; init; }
        public int SpawnHealth { get; init; }
    }

    public static LifecyclePlan Plan(LifecycleContext context)
    {
        var rebornOrEnter = context.State is PlayerSpawnState.Reborn or PlayerSpawnState.Enter;
        var transfer = !rebornOrEnter
            && context.OldActorOwnedByPlayer
            && !context.TempPlayerSpawn;

        return new LifecyclePlan
        {
            CallPlayerReborn = rebornOrEnter,
            TransferVoodooInventory = transfer,
            CancelOldActorScripts = transfer,
            RememberReadyWeapon = context.State == PlayerSpawnState.Reborn && context.RememberLastWeaponDmFlag,
            ApplyPlayerTranslation = !context.PlayerPawnDontTranslate,
            TranslationPlayerIndex = context.PlayerNumber,
            ApplySkinSprite = !context.PlayerPawnNoSkin,
            SkinSprite = context.SkinSprite,
            SpawnHealth = context.PlayerHealth,
        };
    }
}
