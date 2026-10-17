namespace HCDE.Playsim;

internal static class DoomPickupStates
{
    internal static void Apply(Actor actor, int definitionType)
    {
        int[] durations = definitionType switch {
            PickupCatalog.GreenArmor => [6, 7], PickupCatalog.MegaArmor => [6, 6],
            PickupCatalog.HealthBonus or PickupCatalog.ArmorBonus => [6, 6, 6, 6, 6, 6],
            PickupCatalog.BlueCard or PickupCatalog.RedCard or PickupCatalog.YellowCard
                or PickupCatalog.BlueSkull or PickupCatalog.RedSkull or PickupCatalog.YellowSkull => [10, 10],
            PickupCatalog.Soulsphere => [6, 6, 6, 6, 6, 6],
            PickupCatalog.Megasphere => [6, 6, 6, 6],
            PickupCatalog.Berserk or PickupCatalog.RadSuit => [-1],
            PickupCatalog.Stimpack or PickupCatalog.Medikit or PickupCatalog.Clip or PickupCatalog.ClipBox
                or PickupCatalog.Rocket or PickupCatalog.RocketBox or PickupCatalog.Cell or PickupCatalog.CellPack
                or PickupCatalog.Shells or PickupCatalog.ShellBox or PickupCatalog.Backpack
                or PickupCatalog.Bfg or PickupCatalog.Chaingun or PickupCatalog.Chainsaw or PickupCatalog.RocketLauncher
                or PickupCatalog.PlasmaRifle or PickupCatalog.Shotgun or PickupCatalog.SuperShotgun or PickupCatalog.Pistol => [-1],
            _ => [],
        };
        if (durations.Length == 0) return;
        actor.HasOnlySpawnLabel = true;
        actor.PainState = -1;
        actor.DeathState = -1;
        actor.GenericCrushState = durations.Length;
        actor.NullState = durations.Length + 1;
        actor.DieFromSpawnState = durations.Length + 2;
        actor.GenericFreezeDeath = durations.Length + 4;
        var alternatingBright = definitionType is PickupCatalog.GreenArmor or PickupCatalog.MegaArmor
            or PickupCatalog.BlueCard or PickupCatalog.RedCard or PickupCatalog.YellowCard
            or PickupCatalog.BlueSkull or PickupCatalog.RedSkull or PickupCatalog.YellowSkull;
        var alwaysBright = definitionType is PickupCatalog.Soulsphere or PickupCatalog.Megasphere
            or PickupCatalog.Berserk or PickupCatalog.RadSuit;
        var frames = durations.Select((tics, index) => new ActorFrame(tics, (index + 1) % durations.Length,
            FullBright: alwaysBright || alternatingBright && index == 1))
            .Append(new ActorFrame(-1, -1, FullBright: false))
            .Append(new ActorFrame(1, -1, FullBright: false))
            .Append(new ActorFrame(1, actor.DieFromSpawnState + 1, FullBright: false))
            .Append(new ActorFrame(1, -1, actor => actor.DieFromSpawn(), FullBright: false))
            .Append(new ActorFrame(5, actor.GenericFreezeDeath + 1, ActorFreezeActions.FreezeDeath, FullBright: false))
            .Append(new ActorFrame(1, actor.GenericFreezeDeath + 1,
                actor => (actor.Simulation ?? throw new InvalidOperationException("FreezeDeathChunks requires an attached simulation."))
                    .SpawnIceChunks(actor), FullBright: false));
        actor.States.ConfigureSpawn(actor, frames, actor.SpawnState);
    }
}
