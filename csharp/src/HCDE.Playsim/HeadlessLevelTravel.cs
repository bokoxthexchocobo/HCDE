namespace HCDE.Playsim;

/// <summary>Native <c>FLevelLocals::StartTravel</c> / <c>G_FinishTravel</c> player carry-over (co-op, non-deathmatch).</summary>
public sealed class HeadlessTravellerSnapshot
{
    public required byte PlayerNum { get; init; }
    public int Health { get; init; }
    public int Armor { get; init; }
    public WeaponKind SelectedWeapon { get; init; }
    public int Bullets { get; init; }
    public int Shells { get; init; }
    public int Rockets { get; init; }
    public int Cells { get; init; }
    public bool BlueKey { get; init; }
    public bool YellowKey { get; init; }
    public bool RedKey { get; init; }
    public double X { get; init; }
    public double Y { get; init; }
    public double Z { get; init; }
    public uint AngleBams { get; init; }

    internal static HeadlessTravellerSnapshot? Capture(PlayerPawn player)
    {
        if (player.Destroyed || player.Health <= 0)
            return null;
        return new HeadlessTravellerSnapshot
        {
            PlayerNum = player.PlayerNum,
            Health = player.Health,
            Armor = player.Armor,
            SelectedWeapon = player.Inventory.Selected,
            Bullets = player.Inventory.Bullets,
            Shells = player.Inventory.Shells,
            Rockets = player.Inventory.Rockets,
            Cells = player.Inventory.Cells,
            BlueKey = player.Inventory.BlueKey,
            YellowKey = player.Inventory.YellowKey,
            RedKey = player.Inventory.RedKey,
            X = player.X.ToDouble(),
            Y = player.Y.ToDouble(),
            Z = player.Z.ToDouble(),
            AngleBams = player.Angle.Raw,
        };
    }

    internal void Apply(PlayerPawn player)
    {
        player.Health = Health;
        player.Armor = Armor;
        player.Inventory.Bullets = Bullets;
        player.Inventory.Shells = Shells;
        player.Inventory.Rockets = Rockets;
        player.Inventory.Cells = Cells;
        player.Inventory.BlueKey = BlueKey;
        player.Inventory.YellowKey = YellowKey;
        player.Inventory.RedKey = RedKey;
        player.Inventory.Selected = SelectedWeapon;
        player.X = Fixed.FromDouble(X);
        player.Y = Fixed.FromDouble(Y);
        player.Z = Fixed.FromDouble(Z);
        player.Angle = new BamAngle(AngleBams);
        ActorPhysics.PlaceOnFloor(player.Simulation!, player);
        player.RememberPosition();
    }
}

internal static class HeadlessLevelTravel
{
    internal static List<HeadlessTravellerSnapshot> StartTravel(AuthoritySimulation sim)
    {
        if (sim.GameMode == SpawnGameMode.Deathmatch)
            return [];
        return sim.Players.Select(HeadlessTravellerSnapshot.Capture).Where(snapshot => snapshot is not null)
            .Cast<HeadlessTravellerSnapshot>().ToList();
    }

    internal static void FinishTravel(AuthoritySimulation sim, IReadOnlyList<HeadlessTravellerSnapshot> travellers)
    {
        if (travellers.Count == 0)
            return;
        foreach (var snapshot in travellers)
        {
            var player = sim.Players.FirstOrDefault(p => p.PlayerNum == snapshot.PlayerNum && !p.Destroyed);
            if (player is null)
                continue;
            snapshot.Apply(player);
        }
    }
}
