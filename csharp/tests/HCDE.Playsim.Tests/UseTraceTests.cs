using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UseTraceTests
{
    [Theory]
    [InlineData(-64, 255)]
    [InlineData(-8, 128)]
    [InlineData(0, 128)]
    [InlineData(64, 128)]
    public void SignedUseRangeTracesOppositeYawWhenNegative(double range, int expected)
    {
        var line = new LevelLine { X1 = 16, X2 = 16, Y1 = 64, Y2 = -64,
            SideFront = 0, SideBack = 1, Special = 138, Tag = 1 };
        var sim = Facing(0, 180, line);
        sim.Players.Single().UseRange = range;
        sim.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(sim);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(CompatSurface.None, 255)]
    [InlineData(CompatSurface.PointOnLine, 128)]
    [InlineData(CompatSurface.UseBlocking, 255)]
    [InlineData(CompatSurface.PointOnLine | CompatSurface.UseBlocking, 128)]
    public void SimulationPointOnLineSelectsVanillaWithoutChangingMapFlags(CompatSurface compat, int expected)
    {
        var line = new LevelLine { X1 = 0, X2 = 0, Y1 = -0.25, Y2 = 0.25,
            SideFront = 0, SideBack = 1, Special = 138, Tag = 1 };
        var sim = FacingWithCompatibility(-1.0 / 65536, 0, compat, line);
        sim.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(sim);
        Assert.Equal(expected, sim.LightOf(0));
        Assert.Equal(0, line.Flags);
        var other = Facing(-1.0 / 65536, 0, line);
        other.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(other);
        Assert.Equal(255, other.LightOf(0));
    }

    [Theory]
    [InlineData(138)]
    [InlineData(9999)]
    public void CompatibilityBackUseStillConsumesThroughTrigger(int special)
    {
        var first = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = special, Tag = 2, PlayerUseBack = true, UseThrough = true };
        var beyond = new LevelLine { X1 = 16, X2 = 16, Y1 = 64, Y2 = -64, SideFront = 1, SideBack = 0,
            Special = 138, Tag = 1 };
        var sim = FacingWithCompatibility(-32, 0, CompatSurface.UseBlocking, first, beyond);
        Press(sim);
        Assert.Equal(128, sim.LightOf(0));
        Assert.Equal(special == 138 ? 255 : 160, sim.LightOf(1));
    }

    [Theory]
    [InlineData(false, 138, 128)]
    [InlineData(true, 138, 255)]
    [InlineData(false, 9999, 128)]
    [InlineData(true, 9999, 255)]
    public void CompatibilityUseThroughWinsOverCombinedOrdinaryUse(bool compatibility, int special, int expected)
    {
        var first = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = special, Tag = 2, PlayerUse = true, UseThrough = true };
        var beyond = new LevelLine { X1 = -16, X2 = -16, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = 138, Tag = 1 };
        var sim = FacingWithCompatibility(32, 180, compatibility ? CompatSurface.UseBlocking : CompatSurface.None, first, beyond);
        Press(sim);
        Assert.Equal(expected, sim.LightOf(0));
        Assert.Equal(special == 138 ? 255 : 160, sim.LightOf(1));
    }

    [Theory]
    [InlineData(false, 138, 255)]
    [InlineData(true, 138, 128)]
    [InlineData(false, 0, 255)]
    [InlineData(true, 0, 255)]
    public void CompatibilityBlocksUnusableSpecialButNotEmptyOpenLine(bool compatibility, int special, int expected)
    {
        var first = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = special, Tag = 2, PlayerUseBack = true };
        var beyond = new LevelLine { X1 = -16, X2 = -16, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = 138, Tag = 1 };
        var sim = FacingWithCompatibility(32, 180, compatibility ? CompatSurface.UseBlocking : CompatSurface.None, first, beyond);
        Press(sim);
        Assert.Equal(expected, sim.LightOf(0));
        Assert.Equal(160, sim.LightOf(1));
    }

    [Theory]
    [InlineData(0, 255)]
    [InlineData(LevelLine.CompatSideFlag, 128)]
    public void UseActivationSelectsCompatibilitySideClassifier(int flags, int expected)
    {
        var line = new LevelLine { X1 = 0, X2 = 0, Y1 = -0.25, Y2 = 0.25,
            SideFront = 0, SideBack = 1, Special = 138, Tag = 1, Flags = flags };
        var sim = Facing(-1.0 / 65536, 0, line);
        sim.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(sim);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0.5, false, 255)]
    [InlineData(1, false, 255)]
    [InlineData(2, false, 128)]
    [InlineData(0.5, true, 128)]
    [InlineData(1, true, 128)]
    [InlineData(2, true, 255)]
    public void UseSideTreatsNativeToleranceAsFront(double length, bool useBack, int expected)
    {
        var line = new LevelLine { X1 = 0, X2 = 0, Y1 = -length / 2, Y2 = length / 2,
            SideFront = 0, SideBack = 1, Special = 138, Tag = 1, PlayerUseBack = useBack };
        var sim = Facing(-1.0 / 65536, 0, line);
        sim.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(sim);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(64, 0.000001, 255)]
    [InlineData(16, 0.000001, 255)]
    [InlineData(8, 0.000001, 128)]
    [InlineData(64, 0.00000001, 128)]
    public void ShortLineEndpointToleranceUsesFullUseRange(double range, double halfLength, int expected)
    {
        var line = new LevelLine { X1 = 0, X2 = 0, Y1 = halfLength, Y2 = -halfLength,
            SideFront = 0, SideBack = 1, Special = 138, Tag = 1 };
        var sim = Facing(-range / 2, 0, line);
        var player = sim.Players.Single();
        player.UseRange = range;
        player.UsePressed = true;
        LineSpecials.ActivateUses(sim);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(true, 255)]
    [InlineData(false, 128)]
    public void ImportedUdmfPassUseContinuesToTheNextTrigger(bool pass, int expected)
    {
        var text = "namespace = \"ZDoom\"; thing { type = 1; x = 32; angle = 180; skill1 = true; skill2 = true; skill3 = true; skill4 = true; skill5 = true; single = true; } "
            + "vertex { x = 0; y = -64; } vertex { x = 0; y = 64; } "
            + "vertex { x = -16; y = -64; } vertex { x = -16; y = 64; } "
            + "sector { id = 1; lightlevel = 128; heightceiling = 128; } "
            + "sector { id = 2; lightlevel = 160; heightceiling = 128; } "
            + "sidedef { sector = 0; } sidedef { sector = 1; } "
            + "linedef { v1 = 0; v2 = 1; sidefront = 0; sideback = 1; special = 112; arg0 = 2; arg1 = 255; "
            + "playeruse = true; passuse = " + pass.ToString().ToLowerInvariant() + "; } "
            + "linedef { v1 = 2; v2 = 3; sidefront = 0; sideback = 1; special = 112; arg0 = 1; arg1 = 255; playeruse = true; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map!, "MAP01"));
        Press(sim);
        Assert.Equal(expected, sim.LightOf(0));
        Assert.Equal(255, sim.LightOf(1));
    }

    [Theory]
    [InlineData(false, false, true, 255)]
    [InlineData(false, true, true, 255)]
    [InlineData(false, true, false, 128)]
    [InlineData(true, true, false, 255)]
    [InlineData(true, false, true, 128)]
    public void HexenUseDispatchHonorsThroughAndBackFlags(bool back, bool useBack, bool through, int expected)
    {
        var line = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = 112, Arg0 = 1, Arg1 = 255, PlayerUseBack = useBack, UseThrough = through };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Things = [new LevelThing { Type = 1, X = back ? -32 : 32 }],
            Sectors = [new LevelSector { Tag = 1, LightLevel = 128, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 0 }], Lines = [line],
        });
        sim.Players.Single().Angle = BamAngle.FromDegrees(back ? 0 : 180);
        Press(sim);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(138)]
    [InlineData(9999)]
    public void UseBackCombinedWithUseThroughAllowsFrontTraversal(int special)
    {
        var first = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = special, Tag = 2, PlayerUseBack = true, UseThrough = true };
        var beyond = new LevelLine { X1 = -16, X2 = -16, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = 138, Tag = 1 };
        var sim = Facing(32, 180, first, beyond);
        Press(sim);
        Assert.Equal(255, sim.LightOf(0));
        Assert.Equal(special == 138 ? 255 : 160, sim.LightOf(1));
    }

    [Theory]
    [InlineData(9999, LevelLine.BlockUseFlag, 255)]
    [InlineData(9999, LevelLine.BlockEverythingFlag, 255)]
    [InlineData(0, LevelLine.BlockUseFlag, 128)]
    public void FailedUseThroughSkipsBlockingButSpentTriggerDoesNot(int special, int flags, int expected)
    {
        var first = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = special, UseThrough = true, Flags = flags };
        var beyond = new LevelLine { X1 = -16, X2 = -16, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = 138, Tag = 1 };
        var sim = Facing(32, 180, first, beyond);
        sim.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(sim);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(138, true, 128)]
    [InlineData(9999, true, 128)]
    [InlineData(138, false, 255)]
    [InlineData(9999, false, 255)]
    public void OrdinaryUseWinsOverCombinedUseThroughRegardlessOfActivationResult(int special, bool use, int expected)
    {
        var first = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = special, Tag = 2, PlayerUse = use, UseThrough = true };
        var beyond = new LevelLine { X1 = -16, X2 = -16, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = 138, Tag = 1 };
        var sim = Facing(32, 180, first, beyond);
        Press(sim); Assert.Equal(expected, sim.LightOf(0));
        Assert.Equal(special == 138 ? 255 : 160, sim.LightOf(1));
    }

    [Theory]
    [InlineData(false, 128)]
    [InlineData(true, 255)]
    public void FailedUseTriggerConsumesTraceUnlessUseThrough(bool through, int expected)
    {
        var failed = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = 9999, PlayerUse = !through, UseThrough = through };
        var beyond = new LevelLine { X1 = -16, X2 = -16, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1, Special = 138, Tag = 1 };
        var sim = Facing(32, 180, failed, beyond);
        Press(sim); Assert.Equal(expected, sim.LightOf(0)); Assert.Equal(9999, failed.Special);
    }

    [Fact]
    public void FailedUseBackConsumesTraceEvenWhenUseThroughIsSet()
    {
        var failed = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1,
            Special = 9999, PlayerUseBack = true, UseThrough = true };
        var beyond = new LevelLine { X1 = 16, X2 = 16, Y1 = 64, Y2 = -64, SideFront = 1, SideBack = 0, Special = 138, Tag = 1 };
        var sim = Facing(-32, 0, failed, beyond);
        Press(sim); Assert.Equal(128, sim.LightOf(0));
    }

    [Theory]
    [InlineData(0, 255)]
    [InlineData(LevelLine.BlockUseFlag, 128)]
    [InlineData(LevelLine.BlockHitscanFlag, 255)]
    [InlineData(LevelLine.BlockSightFlag, 255)]
    [InlineData(LevelLine.BlockProjectileFlag, 255)]
    public void BlockUseStopsUseBeyondAnOtherwiseOpenLine(int flags, int expected)
    {
        var blocking = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1, Flags = flags };
        var beyond = new LevelLine { X1 = -16, X2 = -16, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1, Special = 138, Tag = 1 };
        var sim = Facing(32, 180, blocking, beyond);
        Press(sim); Assert.Equal(expected, sim.LightOf(0));
        Assert.False(blocking.BlocksMovement);
    }

    [Theory]
    [InlineData(0, 16, 255)]
    [InlineData(40, 128, 255)]
    [InlineData(128, 128, 128)]
    [InlineData(129, 128, 128)]
    public void UsePassesAnyPositiveOpeningRegardlessOfPlayerTraceHeight(double floor, double ceiling, int expected)
    {
        var blocking = new LevelLine { X1 = 0, X2 = 0, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1 };
        var beyond = new LevelLine { X1 = -16, X2 = -16, Y1 = -64, Y2 = 64, SideFront = 0, SideBack = 1, Special = 138, Tag = 1 };
        var sim = Facing(32, 180, blocking, beyond);
        sim.Floors[1] = floor; sim.Ceilings[1] = ceiling;
        sim.Players.Single().UsePressed = true;
        LineSpecials.ActivateUses(sim); Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void FrontUseWithinRangeChangesTheTaggedLight()
    {
        var sim = Facing(32, 180, Switch(138, 1));
        Press(sim);
        Assert.Equal(255, sim.LightOf(0));
    }

    [Theory]
    [InlineData(80, 64, 128)]
    [InlineData(80, 96, 255)]
    [InlineData(40, 32, 128)]
    public void UseRangeLimitsTheTrace(double x, double range, int light)
    {
        var sim = Facing(x, 180, Switch(138, 1));
        sim.Players.Single().UseRange = range;
        Press(sim);
        Assert.Equal(light, sim.LightOf(0));
    }

    [Fact]
    public void BackSideDoesNotActivateAFrontOnlySwitch()
    {
        var sim = Facing(-32, 0, Switch(138, 1));
        Press(sim);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void BackSideActivatesWhenTheLineAllowsIt()
    {
        var sim = Facing(-32, 0, Switch(138, 1, useBack: true));
        Press(sim);
        Assert.Equal(255, sim.LightOf(0));
    }

    [Fact]
    public void UseBackOnlyLineIgnoresTheFront()
    {
        var sim = Facing(32, 180, Switch(138, 1, useBack: true));
        Press(sim);
        Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void OpenFrontOnlyLineDoesNotEatTheSwitchBeyondIt()
    {
        var blocking = Switch(138, 1);
        var beyond = new LevelLine
        {
            X1 = 16, Y1 = 64, X2 = 16, Y2 = -64,
            SideFront = 1, SideBack = 0,
            Special = 139, Tag = 2,
        };
        var sim = Facing(-32, 0, blocking, beyond);
        Press(sim);
        Assert.Equal(128, sim.LightOf(0));
        Assert.Equal(35, sim.LightOf(1));
    }

    private static void Press(AuthoritySimulation sim)
    {
        Assert.True(sim.QueueCommand(0, new PlayerCommand { Use = true }));
        sim.Tick();
    }

    private static AuthoritySimulation Facing(double x, double angle, params LevelLine[] lines)
        => FacingWithCompatibility(x, angle, CompatSurface.None, lines);

    private static AuthoritySimulation FacingWithCompatibility(double x, double angle, CompatSurface compat, params LevelLine[] lines)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.DoomBinary,
            Things = [new LevelThing { Type = 1, X = x }],
            Sectors =
            [
                new LevelSector { Tag = 1, LightLevel = 128, CeilingHeight = 128 },
                new LevelSector { Index = 1, Tag = 2, LightLevel = 160, CeilingHeight = 128 },
            ],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
            Lines = lines,
        }, compat: compat);
        sim.Players.Single().Angle = BamAngle.FromDegrees(angle);
        return sim;
    }

    private static LevelLine Switch(int special, int tag, bool useBack = false) => new()
    {
        X1 = 0, Y1 = -64, X2 = 0, Y2 = 64,
        SideFront = 0, SideBack = 1,
        Special = special, Tag = tag, PlayerUseBack = useBack,
    };
}
