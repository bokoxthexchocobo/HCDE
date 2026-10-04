using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsCallFunctionTests
{
    [Fact]
    public void CallFunc_FixedSqrt_ReturnsFixedPointRoot()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var fixedNine = 9 << 16;
        var fixedThree = 3 << 16;
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, fixedNine,
            (int)AcsPcode.CallFunc, 1, 49,
            (int)AcsPcode.PushNumber, fixedThree,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_VectorLength_ReturnsFixedHypotenuse()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var fixedThree = 3 << 16;
        var fixedFour = 4 << 16;
        var fixedFive = 5 << 16;
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, fixedThree,
            (int)AcsPcode.PushNumber, fixedFour,
            (int)AcsPcode.CallFunc, 2, 50,
            (int)AcsPcode.PushNumber, fixedFive,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_Sqrt_ReturnsIntegerRoot()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 80,
            (int)AcsPcode.CallFunc, 1, 48,
            (int)AcsPcode.PushNumber, 8,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_DamageActor_SkipsInvulnerableTarget()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        var health = imp.Health;
        Run(sim, player, ["None"],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 11,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.SetActorProperty,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 25,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 6, 201);
        Assert.Equal(health, imp.Health);
        Assert.True(imp.Invulnerable);
    }

    [Fact]
    public void CallFunc_DamageActor_ReducesTidTargetHealth()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        var before = imp.Health;
        Run(sim, player, ["None"],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 10,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 6, 201);
        Assert.Equal(before - 10, imp.Health);
    }

    [Fact]
    public void CallFunc_CheckClass_RecognizesKnownSpawnClass()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, ["DoomImp"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, 200,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_CheckActorClass_MatchesImpByTid()
    {
        var sim = RoomWithImp();
        var player = sim.Players.Single();
        Run(sim, player, ["DoomImp"],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 27,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_IsTidUsed_DetectsCoopPlayerTid()
    {
        var sim = TwoPlayerRoom();
        var player = sim.Players.First();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 9,
            (int)AcsPcode.CallFunc, 1, 47,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_CheckActorProperty_MatchesImpHealth()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, imp.Health,
            (int)AcsPcode.CallFunc, 3, 22,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 56,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_GetArmorType_ReturnsWornAmount()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Armor = 150;
        player.Inventory.ArmorType = "GreenArmor";
        player.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        Run(sim, player, ["GreenArmor"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 19,
            (int)AcsPcode.PushNumber, 150,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_SetActorVelocity_ThenGetVelXMatches()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        var velFixed = (int)Math.Floor(2.0 * 65536.0);
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, velFixed,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 6, 23,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.CallFunc, 1, 9,
            (int)AcsPcode.PushNumber, velFixed,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 72,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(velFixed, (int)Math.Floor(imp.VelocityX.ToDouble() * 65536.0));
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_GetActorViewHeight_ReturnsPlayerEyeHeight()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var expected = (int)Math.Floor(PlayerPawn.StandingViewHeight * 65536.0);
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, 14,
            (int)AcsPcode.PushNumber, expected,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_GetChar_ReadsStringIndex()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, ["abc"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.CallFunc, 2, 15,
            (int)AcsPcode.PushNumber, 'b',
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_GetActorVelX_ReturnsFixedVelocity()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.VelocityX = Fixed.FromDouble(4.25);
        var expected = (int)Math.Floor(4.25 * 65536.0);
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, 9,
            (int)AcsPcode.PushNumber, expected,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_DropInventory_RemovesOneActivatorAmmo()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Bullets = 80;
        Run(sim, player, ["Clip"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 82);
        Assert.Equal(79, player.Inventory.Bullets);
    }

    [Fact]
    public void CallFunc_GetArmorInfo_ReturnsMegaSavePercent()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.Inventory.Armor = PlayerInventory.MegaArmorAmount;
        player.Inventory.ArmorSavePercent = PlayerInventory.MegaSavePercent;
        var globals = new AcsGlobalStrings();
        var stack = new List<int> { 2 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], globals, AcsCallFunctions.GetArmorInfo, 1, out var savePercent));
        Assert.Equal((int)(0.5 * 65536), savePercent);
    }

    [Fact]
    public void CallFunc_ChangeActorAngle_SetsTidFacing()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 16384,
            (int)AcsPcode.CallFunc, 2, 79);
        Assert.Equal(BamAngle.Angle90, imp.Angle.Raw);
    }

    [Fact]
    public void CallFunc_ChangeActorPitch_SetsTidPitch()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 8192,
            (int)AcsPcode.CallFunc, 2, 80);
        Assert.Equal(45, imp.PitchDegrees, precision: 3);
    }

    [Fact]
    public void CallFunc_CheckActorState_FindsSpawnLabelOnImp()
    {
        var sim = RoomWithImp();
        var player = sim.Players.Single();
        Run(sim, player, ["Spawn"],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 99,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_CheckActorState_RejectsUnknownLabel()
    {
        var sim = RoomWithImp();
        var player = sim.Players.Single();
        Run(sim, player, ["Missile"],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 99,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_GetActorPowerupTics_ReturnsPowerBuddhaTimer()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.PowerBuddhaTics = 1234;
        Run(sim, player, ["PowerBuddha"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 78,
            (int)AcsPcode.PushNumber, 1234,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_SoundVolume_AcceptsArguments()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var volumeFixed = 1 << 16;
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, volumeFixed,
            (int)AcsPcode.CallFunc, 3, 70,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 56,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_PlayActorSound_ReturnsActivatorCount()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 71,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_SpawnDecal_ReturnsActivatorCount()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 72,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_SetSectorDamage_UpdatesTaggedSectors()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors =
            [
                new LevelSector { Index = 0, Tag = 7, CeilingHeight = 128 },
                new LevelSector { Index = 1, Tag = 3, CeilingHeight = 128 },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var stack = new List<int> { 7, 10, 14, 1, 128 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.SetSectorDamage, 5, out var count));
        Assert.Equal(1, count);
        Assert.Equal(10, sim.Level.Sectors[0].DamageAmount);
        Assert.Equal("Fire", sim.Level.Sectors[0].DamageType);
        Assert.Equal(0, sim.Level.Sectors[1].DamageAmount);
    }

    [Fact]
    public void CallFunc_SetMusicVolume_StoresScale()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var half = 1 << 15;
        var stack = new List<int> { half };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.SetMusicVolume, 1, out var result));
        Assert.Equal(1, result);
        Assert.Equal(0.5, sim.MusicVolume, 3);
    }

    [Fact]
    public void CallFunc_StopSound_AcceptsTidAndChannel()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 62,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_PlaySound_ReturnsActivatorCount()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, ["world/doom/sounds"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 61,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_LineAttack_DamagesActorAlongView()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 3001, Id = 5, X = 96, Y = 64 },
            ],
        });
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        var health = imp.Health;
        var stack = new List<int> { 0, 0, 0, 25 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.LineAttack, 4, out var hit));
        Assert.Equal(0, hit);
        Assert.True(imp.Health < health);
        Assert.Contains(sim.Actors, actor => actor is PuffActor);
    }

    [Fact]
    public void CallFunc_LineAttack_SpawnsPuffOnWallMiss()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, FloorHeight = 0, CeilingHeight = 128 }],
            Lines =
            [
                new LevelLine
                {
                    X1 = 50, X2 = 50, Y1 = -128, Y2 = 128,
                    SideFront = 0, SideBack = 0, Flags = LevelLine.BlockHitscanFlag,
                },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 0 }],
        });
        var player = sim.Players.Single();
        var stack = new List<int> { 0, 0, 0, 10 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.LineAttack, 4, out var hit));
        Assert.Equal(0, hit);
        var puff = Assert.Single(sim.Actors.OfType<PuffActor>());
        Assert.True(puff.X.ToDouble() > 32);
        Assert.Equal(0, puff.ThingId);
        sim.Tick();
        sim.Tick();
        Assert.Contains(puff, sim.Actors);
        for (var tic = 2; tic < 16; tic++) sim.Tick();
        Assert.DoesNotContain(puff, sim.Actors);
    }

    [Fact]
    public void CallFunc_QuakeEx_AcceptsMinimumArguments()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var stack = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.QuakeEx, 8, out var result));
        Assert.Equal(1, result);
    }

    [Fact]
    public void CallFunc_RadiusQuake2_AcceptsMinimumArguments()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var stack = new List<int> { 1, 2, 3, 4, 5, 6 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.RadiusQuake2, 6, out var result));
        Assert.Equal(1, result);
    }

    [Fact]
    public void CallFunc_CheckSight_ActivatorSeesSelf()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var stack = new List<int> { 0, 0, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.CheckSight, 3, out var result));
        Assert.Equal(1, result);
    }

    [Fact]
    public void CallFunc_GetPolyobjX_ReturnsFixedMaxWithoutPolyobjects()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var stack = new List<int> { 1 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.GetPolyobjX, 1, out var result));
        Assert.Equal(AcsCallFunctions.FixedMax, result);
    }

    [Fact]
    public void CallFunc_CheckSight_BlockedBySightLine()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, FloorHeight = 0, CeilingHeight = 128 }],
            Lines =
            [
                new LevelLine
                {
                    X1 = 50, X2 = 50, Y1 = -128, Y2 = 128,
                    SideFront = 0, SideBack = 0, Flags = LevelLine.BlockSightFlag,
                },
            ],
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 0 },
                new LevelThing { Type = 3001, Id = 5, X = 96, Y = 0 },
            ],
        });
        var player = sim.Players.Single();
        var stack = new List<int> { 0, 5, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.CheckSight, 3, out var result));
        Assert.Equal(0, result);
    }

    [Fact]
    public void CallFunc_SpawnParticle_AcceptsArguments()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 96,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_SetSectorTerrain_ThenGetActorFloorTerrainReadsName()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var setStack = new List<int> { 7, 0, SectorTerrain.Floor };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, setStack, new AcsActivatorBinding { Value = player }, ["Water"], new AcsGlobalStrings(),
            AcsCallFunctions.SetSectorTerrain, 3, out var count));
        Assert.Equal(1, count);
        var globals = new AcsGlobalStrings();
        var getStack = new List<int> { 0 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, getStack, new AcsActivatorBinding { Value = player }, [], globals, AcsCallFunctions.GetActorFloorTerrain, 1, out var stringId));
        Assert.Equal("Water", globals.Get(stringId));
    }

    [Fact]
    public void CallFunc_SetActorTeleFog_StoresNamesOnActivator()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var stack = new List<int> { 0, 0, 1 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, ["SrcFog", "DstFog"], new AcsGlobalStrings(),
            AcsCallFunctions.SetActorTeleFog, 3, out var count));
        Assert.Equal(1, count);
        Assert.Equal("SrcFog", player.TeleFogSource);
        Assert.Equal("DstFog", player.TeleFogDest);
    }

    [Fact]
    public void CallFunc_SwapActorTeleFog_SwapsStoredNames()
    {
        var sim = Room();
        var player = sim.Players.Single();
        player.TeleFogSource = "A";
        player.TeleFogDest = "B";
        var stack = new List<int> { 0 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.SwapActorTeleFog, 1, out var count));
        Assert.Equal(1, count);
        Assert.Equal("B", player.TeleFogSource);
        Assert.Equal("A", player.TeleFogDest);
    }

    [Fact]
    public void CallFunc_SetActorRoll_SetsRollOnTidMatch()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        imp.ThingId = 5;
        var player = sim.Players.Single();
        var rollRaw = BamAngle.Angle90;
        var stack = new List<int> { 5, 16384 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.SetActorRoll, 2, out _));
        Assert.Equal(rollRaw, imp.Roll.Raw);
    }

    [Fact]
    public void CallFunc_GetNetId_ReturnsActorNetworkId()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        imp.NetworkId = 42;
        var player = sim.Players.Single();
        var stack = new List<int> { 5, 0 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.GetNetId, 2, out var netId));
        Assert.Equal(42, netId);
    }

    [Fact]
    public void CallFunc_SetActivatorByNetId_SwitchesActivator()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        imp.NetworkId = 77;
        var player = sim.Players.Single();
        Run(sim, player, ["DoomImp"],
            (int)AcsPcode.PushNumber, 77,
            (int)AcsPcode.CallFunc, 1, AcsCallFunctions.SetActivatorByNetId,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, AcsCallFunctions.GetActorClass,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, AcsCallFunctions.StrCmp,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 72,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_GetLineHealth_ReturnsFirstTaggedLineHealth()
    {
        var line = new LevelLine { Index = 0, Tag = 42, Health = 150, SideFront = 0, SideBack = LevelLine.NoSide };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [line],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var stack = new List<int> { 42 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.GetLineHealth, 1, out var health));
        Assert.Equal(150, health);
    }

    [Fact]
    public void CallFunc_GetLineHealth_UsesPooledGroupHealth()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Index = 1, Sector = 0 }],
            Lines =
            [
                new LevelLine { Index = 0, Tag = 9, Health = 80, HealthGroup = 5, SideFront = 0, SideBack = LevelLine.NoSide },
                new LevelLine { Index = 1, Tag = 10, Health = 120, HealthGroup = 5, SideFront = 1, SideBack = LevelLine.NoSide },
            ],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var stack = new List<int> { 9 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.GetLineHealth, 1, out var health));
        Assert.Equal(120, health);
    }

    [Fact]
    public void CallFunc_GetSectorHealth_ReturnsFloorHealthForTag()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, HealthFloor = 88, LightLevel = 128, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var stack = new List<int> { 7, AcsDestructibleHealth.SectorPartFloor };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(),
            AcsCallFunctions.GetSectorHealth, 2, out var health));
        Assert.Equal(88, health);
    }

    [Fact]
    public void CallFunc_GetLineX_ReturnsMidpointAlongTaggedLine()
    {
        var line = new LevelLine
        {
            Index = 0,
            Tag = 42,
            X1 = 0,
            Y1 = 0,
            X2 = 64,
            Y2 = 0,
            SideFront = 0,
            SideBack = LevelLine.NoSide,
        };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [line],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var half = 1 << 15;
        var expected = 32 << 16;
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 42,
            (int)AcsPcode.PushNumber, half,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 3, AcsCallFunctions.GetLineX,
            (int)AcsPcode.PushNumber, expected,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 64,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_GetLineY_AppliesPerpendicularOffset()
    {
        var line = new LevelLine
        {
            Index = 0,
            Tag = 9,
            X1 = 0,
            Y1 = 0,
            X2 = 64,
            Y2 = 0,
            SideFront = 0,
            SideBack = LevelLine.NoSide,
        };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [line],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        var oneFixed = 1 << 16;
        var expectedY = -oneFixed;
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 9,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, oneFixed,
            (int)AcsPcode.CallFunc, 3, AcsCallFunctions.GetLineY,
            (int)AcsPcode.PushNumber, expectedY,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 64,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_SetLineActivation_ThenGetLineActivationMatches()
    {
        var line = new LevelLine { Index = 0, Tag = 42, SideFront = 0, SideBack = LevelLine.NoSide };
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }],
            Lines = [line],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 42,
            (int)AcsPcode.PushNumber, AcsLineActivation.SpacUse,
            (int)AcsPcode.CallFunc, 2, 76,
            (int)AcsPcode.PushNumber, 42,
            (int)AcsPcode.CallFunc, 1, 77,
            (int)AcsPcode.PushNumber, AcsLineActivation.SpacUse,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 56,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        var simLine = sim.Level.Lines[0];
        Assert.True(simLine.PlayerUse);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_CheckFont_RecognizesSmallFont()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, ["SmallFont"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, 73,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_CheckFont_RejectsUnknownFont()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, ["MissingFont"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, 73,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 32,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_IsPointerEqual_PlayerSelectorsMatchActivator()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, AcsActorPointer.Player1,
            (int)AcsPcode.PushNumber, AcsActorPointer.Player1,
            (int)AcsPcode.CallFunc, 2, 84,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_IsPointerEqual_ImpAndPlayerDefaultPointersDiffer()
    {
        var sim = RoomWithImp();
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, AcsActorPointer.Default,
            (int)AcsPcode.PushNumber, AcsActorPointer.Default,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.CallFunc, 3, 84,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_DropItem_SpawnsCatalogPickupAtActivator()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var clipsBefore = sim.Actors.Count(actor => actor.DoomEdNum == PickupCatalog.Clip);
        Run(sim, player, ["Clip"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 74,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(clipsBefore + 1, sim.Actors.Count(actor => actor.DoomEdNum == PickupCatalog.Clip));
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_DropItem_CustomAmountAddsBulletsOnPickup()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var bulletsBefore = player.Inventory.Bullets;
        Run(sim, player, ["Clip"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 99,
            (int)AcsPcode.PushNumber, 256,
            (int)AcsPcode.CallFunc, 4, 74);
        sim.Tick();
        Assert.Equal(bulletsBefore + 99, player.Inventory.Bullets);
    }

    [Fact]
    public void CallFunc_CanRaiseActor_ReportsRaisableImpCorpse()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        imp.Health = 0;
        while (imp.States.Current != ActorStateMachine.Corpse && !imp.Destroyed)
            sim.Tick();
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.CallFunc, 1, 85,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_CanRaiseActor_RejectsBlockedCorpse()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        imp.Health = 0;
        while (imp.States.Current != ActorStateMachine.Corpse && !imp.Destroyed)
            sim.Tick();
        var blocker = sim.AddBot(imp.X.ToDouble(), imp.Y.ToDouble());
        blocker.Brain = null;
        var player = sim.Players.Single();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.CallFunc, 1, 85,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_PickActor_AssignsTidToImpInLineOfFire()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64, Angle = 0 },
                new LevelThing { Type = 3001, X = 96, Y = 64 },
            ],
        });
        var player = sim.Players.Single();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var rangeFixed = 256 << 16;
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, rangeFixed,
            (int)AcsPcode.PushNumber, 42,
            (int)AcsPcode.CallFunc, 5, 83,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 64,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(42, imp.ThingId);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_StrArg_ReturnsSpawnOptionStringInGlobalPool()
    {
        var sim = AuthoritySimulation.Start(
            new PlayLevel
            {
                Format = MapDataFormat.HexenBinary,
                MapName = "MAP01",
                Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
                Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
            },
            spawnOptions: new SpawnOptions(StrArgs: ["+warp", "MAP02"]));
        var player = sim.Players.Single();
        var globals = new AcsGlobalStrings();
        var stack = new List<int> { 1 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], globals, AcsCallFunctions.StrArg, 1, out var stringId));
        Assert.Equal("MAP02", globals.Get(stringId));
    }

    [Fact]
    public void CallFunc_ChangeActorRoll_ThenGetActorRollMatches()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        var rollRaw = BamAngle.Angle90;
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 16384,
            (int)AcsPcode.CallFunc, 2, 89,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.CallFunc, 1, 90,
            (int)AcsPcode.PushNumber, 16384,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 64,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(rollRaw, imp.Roll.Raw);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_GetActorFloorTexture_ReadsSectorFlat()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128, FloorPic = "FLAT1" }],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        });
        var player = sim.Players.Single();
        Run(sim, player, ["FLAT1"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, 204,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 63,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 48,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_FloorRoundCeil_QuantizeFixedValues()
    {
        var sim = Room();
        var twoAndHalf = (2 << 16) + (1 << 15);
        var threeFixed = 3 << 16;
        var globals = new AcsGlobalStrings();

        var floorStack = new List<int> { twoAndHalf };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, floorStack, new AcsActivatorBinding(), [], globals, AcsCallFunctions.Floor, 1, out var floor));
        Assert.Equal(2 << 16, floor);

        var roundStack = new List<int> { twoAndHalf };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, roundStack, new AcsActivatorBinding(), [], globals, AcsCallFunctions.Round, 1, out var round));
        Assert.Equal(threeFixed, round);

        var ceilStack = new List<int> { twoAndHalf };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, ceilStack, new AcsActivatorBinding(), [], globals, AcsCallFunctions.Ceil, 1, out var ceil));
        Assert.Equal(threeFixed, ceil);
    }

    [Fact]
    public void CallFunc_GetWeapon_ReturnsReadyWeaponClass()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, ["Pistol"],
            (int)AcsPcode.CallFunc, 0, 69,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 63,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_CheckProximity_FindsNearbyImp()
    {
        var sim = RoomWithImp();
        var player = sim.Players.Single();
        var radius = 128 << 16;
        Run(sim, player, ["DoomImp"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, radius,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.CallFunc, 4, 98,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 56,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_StrLeft_MatchesPrefixViaStrCmp()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, ["DoomImp", "Doom"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 4,
            (int)AcsPcode.CallFunc, 2, 65,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.CallFunc, 2, 63,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 64,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_SetActorFlag_AndCheckFlag_Invulnerable()
    {
        var sim = RoomWithImp();
        var imp = sim.Actors.First(actor => actor.DoomEdNum == 3001);
        var player = sim.Players.Single();
        Run(sim, player, ["INVULNERABLE"],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.CallFunc, 3, 202,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 75,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 64,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.True(imp.Invulnerable);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_StrCmp_MatchesGetActorClassToModuleString()
    {
        var sim = RoomWithImp();
        var player = sim.Players.Single();
        Run(sim, player, ["DoomImp"],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.CallFunc, 1, 68,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, 63,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 56,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_StrICmp_IgnoresCaseOnModuleStrings()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, ["abc", "ABC"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 1,
            (int)AcsPcode.CallFunc, 2, 64,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_GetActorClass_ReturnsDoomImpForTid()
    {
        var sim = RoomWithImp();
        var player = sim.Players.Single();
        var stack = new List<int> { 5 };
        var globals = new AcsGlobalStrings();
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], globals, AcsCallFunctions.GetActorClass, 1, out var stringId));
        Assert.True(AcsStringIds.IsGlobalPool(stringId));
        Assert.Equal("DoomImp", globals.Get(stringId));
    }

    [Fact]
    public void CallFunc_GetActorClass_ActivatorPlayerIsDoomPlayer()
    {
        var sim = Room();
        var player = sim.Players.Single();
        var stack = new List<int> { 0 };
        var globals = new AcsGlobalStrings();
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], globals, AcsCallFunctions.GetActorClass, 1, out var stringId));
        Assert.Equal("DoomPlayer", globals.Get(stringId));
    }

    [Fact]
    public void CallFunc_SetActivator_SwitchesScriptActivator()
    {
        var sim = RoomWithImp();
        var player = sim.Players.Single();
        Run(sim, player, ["DoomImp"],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.CallFunc, 1, AcsCallFunctions.SetActivator,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, AcsCallFunctions.GetActorClass,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, AcsCallFunctions.StrCmp,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 72,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_SetActivatorToTarget_UsesMonsterChaseTarget()
    {
        var sim = RoomWithImp();
        var player = sim.Players.Single();
        Run(sim, player, ["DoomPlayer", "None"],
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.PushNumber, 10,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 6, AcsCallFunctions.DamageActor,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.CallFunc, 1, AcsCallFunctions.SetActivator,
            (int)AcsPcode.PushNumber, 5,
            (int)AcsPcode.CallFunc, 1, AcsCallFunctions.SetActivatorToTarget,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, AcsCallFunctions.GetActorClass,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 2, AcsCallFunctions.StrCmp,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 128,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_UniqueTid_RandomStartFindsUnusedTid()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
        }, rngSeed: 4242);
        var player = sim.Players.Single();
        var stack = new List<int> { 0, 40 };
        Assert.True(AcsCallFunctions.TryInvoke(
            sim, stack, new AcsActivatorBinding { Value = player }, [], new AcsGlobalStrings(), AcsCallFunctions.UniqueTid, 2, out var tid));
        Assert.NotEqual(0, tid);
        Assert.False(AcsActorTid.IsTidUsed(sim, tid));
    }

    [Fact]
    public void CallFunc_UniqueTid_SkipsTakenTid()
    {
        var sim = TwoPlayerRoom();
        var player = sim.Players.First();
        Run(sim, player, [],
            (int)AcsPcode.PushNumber, 9,
            (int)AcsPcode.PushNumber, 10,
            (int)AcsPcode.CallFunc, 2, 46,
            (int)AcsPcode.PushNumber, 10,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfNotGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void CallFunc_CheckClass_RejectsUnknownClass()
    {
        var sim = Room();
        var player = sim.Players.Single();
        Run(sim, player, ["NotARealDoomClass"],
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.CallFunc, 1, 200,
            (int)AcsPcode.PushNumber, 0,
            (int)AcsPcode.Eq,
            (int)AcsPcode.IfGoto, 40,
            (int)AcsPcode.Lspec2Direct, 112, 7, 35);
        Assert.Equal(35, sim.LightOf(0));
    }

    private static void Run(AuthoritySimulation sim, Actor activator, string[] strings, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, StringTable = strings, Code = bytes });
        Assert.True(sim.Acs.Enqueue(1, ReadOnlySpan<int>.Empty, activator));
        sim.Acs.Tick(sim);
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
    });

    private static AuthoritySimulation TwoPlayerRoom() => AuthoritySimulation.Start(
        new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            MapName = "MAP01",
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
            Things =
            [
                new LevelThing { Type = 1, X = 32, Y = 64 },
                new LevelThing { Type = 2, Id = 9, X = 96, Y = 64, Single = false, Coop = true },
            ],
        },
        spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));

    private static AuthoritySimulation RoomWithImp() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things =
        [
            new LevelThing { Type = 1, X = 32, Y = 64 },
            new LevelThing { Type = 3001, Id = 5, X = 96, Y = 64 },
        ],
    });
}
