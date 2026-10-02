using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorScrollCarryTests
{
    [Fact]
    public void ControlCarryTracksFloorPlusCeilingAndOppositeMotionCancels()
    {
        var sim = SplitSectorRoom();
        sim.RegisterControlSectorCarry(0, 1, 1, 0, 128);
        var player = sim.Players.Single();
        ActorPhysics.PlaceOnFloor(sim, player);
        var startX = player.X.ToDouble();
        sim.Ceilings[1] += 3;
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
        sim.Floors[1] += 2;
        sim.Ceilings[1] -= 2;
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
        sim.Floors[1] += 1;
        sim.Ceilings[1] += 2;
        sim.Tick();
        Assert.Equal(startX + 6, player.X.ToDouble());
    }

    [Fact]
    public void AcceleratingControlCarryKeepsVelocityWhenControlStops()
    {
        var sim = SplitSectorRoom();
        sim.RegisterControlSectorCarry(0, 1, 2, 0, 128, accelerative: true);
        var player = sim.Players.Single();
        ActorPhysics.PlaceOnFloor(sim, player);
        var startX = player.X.ToDouble();
        sim.Floors[1] = 2;
        sim.Tick();
        Assert.Equal(startX + 4, player.X.ToDouble());
        sim.Tick();
        Assert.Equal(startX + 8, player.X.ToDouble());
        sim.Floors[1] = 0;
        sim.Tick();
        Assert.Equal(startX + 8, player.X.ToDouble());
        sim.Tick();
        Assert.Equal(startX + 8, player.X.ToDouble());
    }

    [Fact]
    public void NonAcceleratingControlCarryStopsWithController()
    {
        var sim = SplitSectorRoom();
        sim.RegisterControlSectorCarry(0, 1, 2, 0, 128);
        var player = sim.Players.Single();
        ActorPhysics.PlaceOnFloor(sim, player);
        var startX = player.X.ToDouble();
        sim.Floors[1] = 2;
        sim.Tick(); sim.Tick();
        Assert.Equal(startX + 4, player.X.ToDouble());
    }

    [Fact]
    public void AcceleratingCarryScrollIncreasesDisplacementEachTic()
    {
        var sim = Room(tag: 7);
        sim.AppendAcceleratingSectorCarryScroll(0, 1, 0);
        var player = sim.Players.Single();
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 1, player.X.ToDouble());
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
        sim.Tick();
        Assert.Equal(startX + 6, player.X.ToDouble());
    }

    [Fact]
    public void ScrollFloorPositiveModeRegistersConstantCarryAtMapLoad()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, FloorHeight = 0, CeilingHeight = 128 }],
            Lines =
            [
                new LevelLine
                {
                    Special = LineSpecials.ScrollFloor,
                    Arg0 = 7,
                    Arg1 = 0,
                    Arg2 = 3,
                    Arg3 = 160,
                    Arg4 = 128,
                },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 1, player.X.ToDouble());
        sim.Tick();
        Assert.Equal(startX + 2, player.X.ToDouble());
    }

    [Fact]
    public void ScrollFloorActivatedModeThreeUsesConstantCarry()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 32,
            Arg2 = 0,
            Arg3 = 3,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 1, player.X.ToDouble());
        sim.Tick();
        Assert.Equal(startX + 2, player.X.ToDouble());
    }

    [Fact]
    public void GroundedPlayerMovesWithSectorScroll()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var startX = player.X.ToDouble();
        sim.SetSectorScroll(0, 3, -2);
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
        Assert.Equal(64 - 2, player.Y.ToDouble());
    }

    [Fact]
    public void ScrollFloorRuntimeSpeedBitsDoNotSelectController()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors =
            [
                new LevelSector { Index = 0, Tag = 7, FloorHeight = 0, CeilingHeight = 128, LightLevel = 128 },
                new LevelSector { Index = 1, FloorHeight = 0, CeilingHeight = 128, LightLevel = 128 },
            ],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 1 }],
            Lines =
            [
                new LevelLine { X1 = -64, Y1 = 0, X2 = 64, Y2 = 0, SideFront = 0, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = 64, Y1 = 0, X2 = 64, Y2 = 128, SideFront = 0, SideBack = 1, Flags = LevelLine.TwoSidedFlag },
                new LevelLine { X1 = 64, Y1 = 128, X2 = -64, Y2 = 128, SideFront = 0, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = -64, Y1 = 128, X2 = -64, Y2 = 0, SideFront = 0, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = 64, Y1 = 0, X2 = 128, Y2 = 0, SideFront = 1, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = 128, Y1 = 0, X2 = 128, Y2 = 128, SideFront = 1, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = 128, Y1 = 128, X2 = 64, Y2 = 128, SideFront = 1, SideBack = LevelLine.NoSide },
            ],
            Things = [new LevelThing { Type = 1, X = 55, Y = 64 }],
        });
        var player = sim.Players.Single();
        ActorPhysics.PlaceOnFloor(sim, player);
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 1,
            Arg3 = 1,
            SideFront = 1,
            X1 = 64,
            Y1 = 0,
            X2 = 128,
            Y2 = 0,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Floors[1] = 8;
        sim.Tick();
        Assert.Equal(startX + 1.0 / 32, player.X.ToDouble());
    }

    [Fact]
    public void ControlSectorDisplacementCarryFollowsFloorMotion()
    {
        var sim = SplitSectorRoom();
        sim.RegisterControlSectorCarry(0, 1, 2, 0, 128);
        var player = sim.Players.Single();
        ActorPhysics.PlaceOnFloor(sim, player);
        var startX = player.X.ToDouble();
        sim.Floors[1] = 8;
        sim.Tick();
        Assert.Equal(startX + 16, player.X.ToDouble());
    }

    [Fact]
    public void MapLoadControlSectorDisplacementCarryFollowsFloorMotion()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors =
            [
                new LevelSector { Index = 0, Tag = 7, FloorHeight = 0, CeilingHeight = 128, LightLevel = 128 },
                new LevelSector { Index = 1, FloorHeight = 0, CeilingHeight = 128, LightLevel = 128 },
            ],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 1 }],
            Lines =
            [
                new LevelLine { X1 = -64, Y1 = 0, X2 = 64, Y2 = 0, SideFront = 0, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = 64, Y1 = 0, X2 = 64, Y2 = 128, SideFront = 0, SideBack = 1, Flags = LevelLine.TwoSidedFlag },
                new LevelLine { X1 = 64, Y1 = 128, X2 = -64, Y2 = 128, SideFront = 0, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = -64, Y1 = 128, X2 = -64, Y2 = 0, SideFront = 0, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = 64, Y1 = 0, X2 = 128, Y2 = 0, SideFront = 1, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = 128, Y1 = 0, X2 = 128, Y2 = 128, SideFront = 1, SideBack = LevelLine.NoSide },
                new LevelLine { X1 = 128, Y1 = 128, X2 = 64, Y2 = 128, SideFront = 1, SideBack = LevelLine.NoSide },
                new LevelLine
                {
                    Special = LineSpecials.ScrollFloor,
                    Arg0 = 7,
                    Arg1 = 5,
                    Arg2 = 1,
                    SideFront = 1,
                    X1 = 64,
                    Y1 = 0,
                    X2 = 128,
                    Y2 = 0,
                },
            ],
            Things = [new LevelThing { Type = 1, X = 55, Y = 64 }],
        });
        var player = sim.Players.Single();
        ActorPhysics.PlaceOnFloor(sim, player);
        Assert.Equal(0, player.SectorIndex);
        var startX = player.X.ToDouble();
        sim.Floors[1] = 8;
        sim.Tick();
        Assert.Equal(startX + 16, player.X.ToDouble());
    }

    [Fact]
    public void MapLoadScrollFloorLineAppliesCarryWithoutActivation()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, FloorHeight = 0, CeilingHeight = 128 }],
            Lines =
            [
                new LevelLine
                {
                    Special = LineSpecials.ScrollFloor,
                    Arg0 = 7,
                    Arg1 = 0,
                    Arg2 = 1,
                    Arg3 = 192,
                    Arg4 = 128,
                },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 2, player.X.ToDouble());
    }

    [Fact]
    public void ScrollFloorLineSpecialSetsCarryOnTaggedSectors()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 96,
            Arg2 = -64,
            Arg3 = 1,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
        Assert.Equal(64 - 2, player.Y.ToDouble());
    }

    [Fact]
    public void ScrollFloorCarryModeTwoSetsCarryLikeNative()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 64,
            Arg2 = 0,
            Arg3 = 2,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 2, player.X.ToDouble());
    }

    [Fact]
    public void HexenScrollAddsToLineDrivenCarryForPlayers()
    {
        var sim = Room(tag: 7, hexenSpecial: HexenSectorScroll.NorthSlow);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 32,
            Arg2 = 0,
            Arg3 = 1,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        var startY = player.Y.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 1, player.X.ToDouble());
        Assert.Equal(startY + 0.5, player.Y.ToDouble());
    }

    [Fact]
    public void ScrollFloorCarryModeZeroClearsScroll()
    {
        var sim = Room(tag: 7);
        sim.SetSectorScroll(0, 5, 0);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 0,
            Arg2 = 0,
            Arg3 = 0,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX, player.X.ToDouble());
    }

    [Fact]
    public void HexenNorthSlowScrollMovesPlayersOnly()
    {
        var sim = Room(hexenSpecial: HexenSectorScroll.NorthSlow);
        var player = sim.Players.Single();
        var bot = sim.AddBot(96, 64);
        var startY = player.Y.ToDouble();
        var botStartY = bot.Y.ToDouble();
        sim.Tick();
        Assert.Equal(startY + 0.5, player.Y.ToDouble());
        Assert.Equal(botStartY, bot.Y.ToDouble());
    }

    [Fact]
    public void ScrollCeilingLineActivatesWithoutCarry()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollCeiling,
            Arg0 = 7,
            Arg1 = 96,
            Arg2 = -64,
            Arg3 = 0,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX, player.X.ToDouble());
        Assert.Equal(64, player.Y.ToDouble());
    }

    [Fact]
    public void GroundedMonsterMovesWithSectorScroll()
    {
        var sim = Room();
        var bot = sim.AddBot(96, 64);
        var startX = bot.X.ToDouble();
        sim.SetSectorScroll(0, 2, 1);
        sim.Tick();
        Assert.Equal(startX + 2, bot.X.ToDouble());
        Assert.Equal(64 + 1, bot.Y.ToDouble());
    }

    [Fact]
    public void PlayerOnlyCarryScrollIgnoresGroundedMonsters()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var bot = sim.AddBot(96, 64);
        var playerStart = player.X.ToDouble();
        var botStart = bot.X.ToDouble();
        sim.SetSectorScroll(0, 4, 0, ScrollCarryAffect.Players);
        sim.Tick();
        Assert.Equal(playerStart + 4, player.X.ToDouble());
        Assert.Equal(botStart, bot.X.ToDouble());
    }

    [Fact]
    public void MonsterOnlyCarryScrollIgnoresPlayers()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var bot = sim.AddBot(96, 64);
        var playerStart = player.X.ToDouble();
        var botStart = bot.X.ToDouble();
        sim.SetSectorScroll(0, 4, 0, ScrollCarryAffect.Monsters);
        sim.Tick();
        Assert.Equal(playerStart, player.X.ToDouble());
        Assert.Equal(botStart + 4, bot.X.ToDouble());
    }

    [Fact]
    public void SplitAffectScrollsOnSameSectorSumPerActorKind()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var bot = sim.AddBot(96, 64);
        sim.AppendSectorCarryScroll(0, 2, 0, ScrollCarryAffect.Players);
        sim.AppendSectorCarryScroll(0, 3, 0, ScrollCarryAffect.Monsters);
        var playerStart = player.X.ToDouble();
        var botStart = bot.X.ToDouble();
        sim.Tick();
        Assert.Equal(playerStart + 2, player.X.ToDouble());
        Assert.Equal(botStart + 3, bot.X.ToDouble());
    }

    [Fact]
    public void MultiSectorCarryAveragesForMonstersByDefault()
    {
        var sim = SplitSectorRoom();
        var player = sim.Players.Single();
        player.X = Fixed.FromDouble(-32);
        ActorPhysics.PlaceOnFloor(sim, player);
        var bot = sim.AddBot(96, 64);
        bot.X = Fixed.FromDouble(55);
        bot.Y = Fixed.FromDouble(64);
        ActorPhysics.PlaceOnFloor(sim, bot);
        sim.SetSectorScroll(0, 2, 0);
        sim.SetSectorScroll(1, 4, 0);
        var startX = bot.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 3, bot.X.ToDouble());
    }

    [Fact]
    public void BoomScrollCompatSumsMultiSectorCarryForMonsters()
    {
        var sim = SplitSectorRoom(CompatSurface.BoomScroll);
        var player = sim.Players.Single();
        player.X = Fixed.FromDouble(-32);
        ActorPhysics.PlaceOnFloor(sim, player);
        var bot = sim.AddBot(96, 64);
        bot.X = Fixed.FromDouble(55);
        bot.Y = Fixed.FromDouble(64);
        ActorPhysics.PlaceOnFloor(sim, bot);
        sim.SetSectorScroll(0, 2, 0);
        sim.SetSectorScroll(1, 4, 0);
        var startX = bot.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 6, bot.X.ToDouble());
    }

    [Fact]
    public void BoomScrollCompatStillAveragesMultiSectorCarryForPlayers()
    {
        var sim = SplitSectorRoom(CompatSurface.BoomScroll);
        var player = sim.Players.Single();
        ActorPhysics.PlaceOnFloor(sim, player);
        player.X = Fixed.FromDouble(55);
        player.Y = Fixed.FromDouble(64);
        ActorPhysics.PlaceOnFloor(sim, player);
        sim.SetSectorScroll(0, 2, 0);
        sim.SetSectorScroll(1, 4, 0);
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
    }

    [Fact]
    public void ScrollFloorLineSpecialUpdatesCarryRate()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var fast = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 96,
            Arg2 = 0,
            Arg3 = 1,
            PlayerUse = true,
        };
        var slow = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 32,
            Arg2 = 0,
            Arg3 = 1,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, fast, use: true));
        sim.Tick();
        var afterFast = player.X.ToDouble();
        Assert.True(LineSpecials.ActivateMapLine(sim, player, slow, use: true));
        sim.Tick();
        Assert.Equal(afterFast + 1, player.X.ToDouble());
    }

    [Fact]
    public void SectorScrollCarryPersistsUntilCleared()
    {
        var sim = Room();
        var player = sim.Players.Single();
        sim.SetSectorScroll(0, 2, 0);
        var startX = player.X.ToDouble();
        sim.Tick();
        sim.Tick();
        Assert.Equal(startX + 4, player.X.ToDouble());
    }

    [Fact]
    public void MultipleCarryScrollsOnSameSectorAccumulateEachTic()
    {
        var sim = Room();
        var player = sim.Players.Single();
        sim.AppendSectorCarryScroll(0, 2, 0);
        sim.AppendSectorCarryScroll(0, 1, 1);
        var startX = player.X.ToDouble();
        var startY = player.Y.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 3, player.X.ToDouble());
        Assert.Equal(startY + 1, player.Y.ToDouble());
    }

    [Fact]
    public void TouchingAdjacentScrollerCarriesWhenRadiusOverlaps()
    {
        var sim = SplitSectorRoom();
        var player = sim.Players.Single();
        ActorPhysics.PlaceOnFloor(sim, player);
        player.X = Fixed.FromDouble(55);
        player.Y = Fixed.FromDouble(64);
        ActorPhysics.PlaceOnFloor(sim, player);
        Assert.Equal(0, player.SectorIndex);
        sim.SetSectorScroll(1, 4, 0);
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 4, player.X.ToDouble());
    }

    [Fact]
    public void FloatingActorIgnoresSectorScroll()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Floating = true;
        var startX = player.X.ToDouble();
        sim.SetSectorScroll(0, 5, 0);
        sim.Tick();
        Assert.Equal(startX, player.X.ToDouble());
    }

    [Fact]
    public void PlayersCarryScrollWithoutInScrollSecFlag()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.InScrollSector = false;
        sim.SetSectorScroll(0, 4, 0);
        var startX = player.X.ToDouble();
        ActorPhysics.ApplySectorScroll(sim, player);
        Assert.Equal(startX + 4, player.X.ToDouble());
    }

    [Fact]
    public void MonstersIgnoreCarryScrollWithoutInScrollSecFlag()
    {
        var sim = Room();
        var bot = sim.AddBot(96, 64);
        bot.InScrollSector = false;
        sim.SetSectorScroll(0, 4, 0);
        var startX = bot.X.ToDouble();
        ActorPhysics.ApplySectorScroll(sim, bot);
        Assert.Equal(startX, bot.X.ToDouble());
        bot.InScrollSector = true;
        ActorPhysics.ApplySectorScroll(sim, bot);
        Assert.Equal(startX + 4, bot.X.ToDouble());
    }

    [Fact]
    public void TickSetsInScrollSecOnMonstersTouchingCarryScroller()
    {
        var sim = Room();
        var bot = sim.AddBot(96, 64);
        sim.SetSectorScroll(0, 3, 0);
        Assert.False(bot.InScrollSector);
        sim.Tick();
        Assert.True(bot.InScrollSector);
    }

    private static AuthoritySimulation SplitSectorRoom(CompatSurface compat = CompatSurface.None) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors =
        [
            new LevelSector { Index = 0, FloorHeight = 0, CeilingHeight = 128, LightLevel = 128 },
            new LevelSector { Index = 1, FloorHeight = 0, CeilingHeight = 128, LightLevel = 128 },
        ],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 1 }],
        Lines =
        [
            new LevelLine { X1 = -64, Y1 = 0, X2 = 64, Y2 = 0, SideFront = 0, SideBack = LevelLine.NoSide },
            new LevelLine { X1 = 64, Y1 = 0, X2 = 64, Y2 = 128, SideFront = 0, SideBack = 1, Flags = LevelLine.TwoSidedFlag },
            new LevelLine { X1 = 64, Y1 = 128, X2 = -64, Y2 = 128, SideFront = 0, SideBack = LevelLine.NoSide },
            new LevelLine { X1 = -64, Y1 = 128, X2 = -64, Y2 = 0, SideFront = 0, SideBack = LevelLine.NoSide },
            new LevelLine { X1 = 64, Y1 = 0, X2 = 128, Y2 = 0, SideFront = 1, SideBack = LevelLine.NoSide },
            new LevelLine { X1 = 128, Y1 = 0, X2 = 128, Y2 = 128, SideFront = 1, SideBack = LevelLine.NoSide },
            new LevelLine { X1 = 128, Y1 = 128, X2 = 64, Y2 = 128, SideFront = 1, SideBack = LevelLine.NoSide },
        ],
        Things = [new LevelThing { Type = 1, X = 55, Y = 64 }],
    }, compat: compat);

    private static AuthoritySimulation Room(int tag = 0, short hexenSpecial = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, Tag = (short)tag, Special = hexenSpecial, FloorHeight = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
    });

    [Fact]
    public void FloorTextureScrollRotatesOffsetWithSectorAngle()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors =
            [
                new LevelSector
                {
                    Index = 0,
                    Tag = 7,
                    FloorHeight = 0,
                    CeilingHeight = 128,
                    FloorTextureAngle = BamAngle.Angle90,
                },
            ],
            Lines =
            [
                new LevelLine
                {
                    Special = LineSpecials.ScrollFloor,
                    Arg0 = 7,
                    Arg1 = 0,
                    Arg2 = 0,
                    Arg3 = 160,
                    Arg4 = 128,
                },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var sector = sim.Level.Sectors[0];
        sim.Tick();
        Assert.Equal(0, sector.FloorTextureOffsetX, 3);
        Assert.Equal(1, sector.FloorTextureOffsetY, 3);
    }

    [Fact]
    public void ScrollFloorModeZeroScrollsFloorTextureAtMapLoad()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, FloorHeight = 0, CeilingHeight = 128 }],
            Lines =
            [
                new LevelLine
                {
                    Special = LineSpecials.ScrollFloor,
                    Arg0 = 7,
                    Arg1 = 0,
                    Arg2 = 0,
                    Arg3 = 160,
                    Arg4 = 128,
                },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var sector = sim.Level.Sectors[0];
        sim.Tick();
        Assert.Equal(-1, sector.FloorTextureOffsetX);
        sim.Tick();
        Assert.Equal(-2, sector.FloorTextureOffsetX);
    }

    [Fact]
    public void ScrollCeilingActivationScrollsCeilingTextureNotCarry()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollCeiling,
            Arg0 = 7,
            Arg1 = 64,
            Arg2 = 0,
            Arg3 = 0,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var sector = sim.Level.Sectors[0];
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX, player.X.ToDouble());
        Assert.Equal(-2, sector.CeilingTextureOffsetX);
    }

    [Fact]
    public void ScrollWallLineScrollsTaggedSideMidTexture()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, FloorHeight = 0, CeilingHeight = 128 }],
            Sides = [new LevelSide { Index = 0, Sector = 0 }],
            Lines =
            [
                new LevelLine
                {
                    Index = 0,
                    Tag = 9,
                    SideFront = 0,
                    SideBack = LevelLine.NoSide,
                    X1 = 0,
                    Y1 = 0,
                    X2 = 64,
                    Y2 = 0,
                },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var side = sim.Level.Sides[0];
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollWall,
            Arg0 = 9,
            Arg1 = 1 << 16,
            Arg2 = 0,
            Arg3 = 0,
            Arg4 = (int)WallScrollParts.Mid,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        sim.Tick();
        Assert.Equal(1, side.MidTextureOffsetX);
        sim.Tick();
        Assert.Equal(2, side.MidTextureOffsetX);
    }

    [Fact]
    public void ScrollWallSkipsMidOnThreeDMidTexLine()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors =
            [
                new LevelSector { Index = 0, FloorHeight = 0, CeilingHeight = 128 },
                new LevelSector { Index = 1, FloorHeight = 0, CeilingHeight = 64 },
            ],
            Sides =
            [
                new LevelSide { Index = 0, Sector = 0 },
                new LevelSide { Index = 1, Sector = 1 },
            ],
            Lines =
            [
                new LevelLine
                {
                    Index = 0,
                    Tag = 4,
                    SideFront = 0,
                    SideBack = 1,
                    Flags = LevelLine.TwoSidedFlag | LevelLine.MidTex3DFlag,
                    X1 = 0,
                    Y1 = 0,
                    X2 = 64,
                    Y2 = 0,
                },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var side = sim.Level.Sides[0];
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollWall,
            Arg0 = 4,
            Arg1 = 1 << 16,
            Arg2 = 0,
            Arg3 = 0,
            Arg4 = (int)WallScrollParts.Mid,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        sim.Tick();
        Assert.Equal(0, side.MidTextureOffsetX);
        Assert.Equal(0, side.MidTextureOffsetY);
    }

    [Fact]
    public void ScrollFloorModeTwoScrollsFloorTextureAndCarry()
    {
        var sim = Room(tag: 7);
        var player = sim.Players.Single();
        var sector = sim.Level.Sectors[0];
        var line = new LevelLine
        {
            Special = LineSpecials.ScrollFloor,
            Arg0 = 7,
            Arg1 = 64,
            Arg2 = 0,
            Arg3 = 2,
            PlayerUse = true,
        };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, use: true));
        var startX = player.X.ToDouble();
        sim.Tick();
        Assert.Equal(startX + 2, player.X.ToDouble());
        Assert.Equal(-2, sector.FloorTextureOffsetX);
    }
}
