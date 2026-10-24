namespace HCDE.Playsim.Tests;

public class VmPMapProjectileCombatTests
{
    [Fact]
    public void RecordSpecialHits_PushesSpechit()
    {
        var spechit = new List<VmPMapProjectileCombat.SpechitEntry>();
        var portal = new List<VmPMapProjectileCombat.SpechitEntry>();
        VmPMapProjectileCombat.RecordSpecialAndPortalHits(new object(), 1, false, (1, 2), (3, 4), spechit, portal);
        Assert.Single(spechit);
        Assert.Empty(portal);
    }

    [Fact]
    public void CheckRipLevel_WithinBand() =>
        Assert.True(VmPMapProjectileCombat.CheckRipLevel(1, 5, 3));

    [Fact]
    public void CheckPortalOpening_AdjustsFloor()
    {
        var ceiling = 200.0;
        var floor = 0.0;
        var drop = 0.0;
        var line = new VmPMapProjectileCombat.LineStub { FrontSector = new(), BackSector = new() };
        var ok = VmPMapProjectileCombat.CheckPortalOpening(
            line,
            8,
            ref ceiling,
            ref floor,
            ref drop,
            (0, 0),
            new VmPMapProjectileCombat.PortalProbeContext
            {
                LinesAt = _ => new[] { new VmPMapProjectileCombat.LineStub { BackSector = new() } },
                LineOpening = (_, _) => (100, 20),
                AddZ = _ => { },
            });
        Assert.True(ok);
        Assert.True(floor > 0);
    }

    [Fact]
    public void ProjectileImmune_SameSpeciesDefaultGroup()
    {
        var a = new VmPMapProjectileCombat.CombatActor { Self = new(), Species = "imp", ProjectileGroup = 0 };
        var b = new VmPMapProjectileCombat.CombatActor { Self = new(), Species = "imp", ProjectileGroup = 0 };
        Assert.True(VmPMapProjectileCombat.ProjectileImmune(a, b));
    }

    [Fact]
    public void CanAttackHurt_BlocksFriendlyInfightZero()
    {
        var victim = new VmPMapProjectileCombat.CombatActor { FriendOfShooter = true };
        var shooter = new VmPMapProjectileCombat.CombatActor { Flags7HarmFriends = 0 };
        Assert.False(VmPMapProjectileCombat.CanAttackHurt(victim, shooter, new() { LevelInfighting = 0 }));
    }

    [Fact]
    public void CanAttackHurt_AllowsHostileMonsterWhenInfightNegative()
    {
        var victim = new VmPMapProjectileCombat.CombatActor { Flags3Monster = 1, HostileToShooter = true };
        var shooter = new VmPMapProjectileCombat.CombatActor { FlagsShootable = 1 };
        Assert.True(VmPMapProjectileCombat.CanAttackHurt(victim, shooter, new() { LevelInfighting = -1 }));
    }

    [Fact]
    public void CheckPortalOpening_LowersCeiling()
    {
        var ceiling = 50.0;
        var floor = 0.0;
        var drop = 0.0;
        var line = new VmPMapProjectileCombat.LineStub { FrontSector = new(), BackSector = new() };
        VmPMapProjectileCombat.CheckPortalOpening(
            line,
            0,
            ref ceiling,
            ref floor,
            ref drop,
            (0, 0),
            new VmPMapProjectileCombat.PortalProbeContext
            {
                LinesAt = _ => new[] { new VmPMapProjectileCombat.LineStub { BackSector = new() } },
                LineOpening = (_, _) => (40, 10),
                AddZ = _ => { },
            });
        Assert.Equal(40, ceiling);
    }

    [Fact]
    public void DoMissileDamage_NegativeDamageGivesBody()
    {
        var healed = 0;
        VmPMapProjectileCombat.DoMissileDamage(
            new() { Self = new() },
            new() { Self = new() },
            new VmPMapProjectileCombat.MissileDamageContext
            {
                MissileDamage = (_, _) => -5,
                DamageMobj = (_, _, _, _) => { },
                PoisonMobj = (_, _) => { },
                GiveBody = (_, amt) => healed = amt,
            });
        Assert.Equal(5, healed);
    }
}
