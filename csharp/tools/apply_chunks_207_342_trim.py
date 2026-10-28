#!/usr/bin/env python3
"""Trim chunks 207-342 with thin_managed_coverage to tested symbol ranges."""
import json
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TALLY = REPO / "docs/data/loc_tally.json"
TOOLS = REPO / "tools"

# Rows fully superseded by earlier chunks after overlap-safe trim (drop from tally).
DROP_CHUNKS = {328, 333, 337}

TRIMS: dict[int, dict] = {
    212: {
        "commit": "1509d90300cfd1ec535a43fd9dff5a3febd3bb9c",
        "note": "R_InitSpriteDefs texture hash chain only (trimmed re-audit)",
        "auditRationale": "Credits `SpriteNameId` hash bucket linking in `R_InitSpriteDefs` — not the full voxel lump name scan loop.",
        "ranges": [
            {
                "file": "src/r_data/sprites.cpp",
                "symbol": "R_InitSpriteDefs hash",
                "start": 328,
                "end": 341,
                "tests": ["VmSpriteTextureHasherTests.BuildSpriteTextureChains_LinksSamePrefix"],
            },
        ],
    },
    216: {
        "commit": "7a78aca3d301f3bebcdeb22796d9375f033fc1c3",
        "note": "HU font constants and InitDoomFonts small-font path (trimmed)",
        "auditRationale": "Credits `HU` font size constants and small-font lump preference — not the full fallback chain.",
        "ranges": [
            {
                "file": "src/gamedata/doomfont.cpp",
                "symbol": "HU font constants",
                "start": 30,
                "end": 34,
                "tests": ["VmDoomFontPlannerTests.HuFontSize_MatchesNativeRange"],
            },
            {
                "file": "src/gamedata/doomfont.cpp",
                "symbol": "InitDoomFonts small font",
                "start": 36,
                "end": 92,
                "tests": ["VmDoomFontPlannerTests.PlanSmallFont_PrefersFontAWhenPresent"],
            },
        ],
    },
    222: {
        "commit": "868d46235f564f4c230307d0847b2217d7d6fbdf",
        "note": "UMAPINFO token helpers and doomedmap strife tail (trimmed)",
        "auditRationale": "Credits small UMAPINFO parse branches and Strife spawnable clear — not full schema tables or CCMD bodies.",
        "ranges": [
            {
                "file": "src/gamedata/umapinfo.cpp",
                "symbol": "ParseMultiString clear branch",
                "start": 112,
                "end": 118,
                "tests": ["VmUMapInfoTokenParserTests.TryParseMultiString_Clear"],
            },
            {
                "file": "src/gamedata/umapinfo.cpp",
                "symbol": "ParseLumpName validation",
                "start": 130,
                "end": 136,
                "tests": ["VmUMapInfoTokenParserTests.TryParseLumpName_RejectsLongName"],
            },
            {
                "file": "src/gamedata/umapinfo.cpp",
                "symbol": "ParseStandardProperty header",
                "start": 156,
                "end": 164,
                "tests": ["VmUMapInfoStandardPropertyParserTests.TryApplyProperty_SetsLevelName"],
            },
            {
                "file": "src/gamedata/g_doomedmap.cpp",
                "symbol": "Strife type clear",
                "start": 406,
                "end": 419,
                "tests": ["VmDoomEdSpawnInitTests.ClearStrifeTypes_EmptiesMap"],
            },
            {
                "file": "src/gamedata/g_doomedmap.cpp",
                "symbol": "Spawn CCMD tail",
                "start": 452,
                "end": 457,
                "tests": ["VmDoomEdSpawnInitTests.InitSpawnablesFromMapinfo_ResolvesClasses"],
            },
        ],
    },
    228: {
        "commit": "524c57aec6a349af74bd68f3efe5d1b4b995530c",
        "note": "g_skill globals and TeamInfoOptions table (trimmed)",
        "auditRationale": "Credits skill list globals, name token assign, and team option table — not `FTeam` accessor surface or `teamlist` CCMD.",
        "ranges": [
            {
                "file": "src/gamedata/g_skill.cpp",
                "symbol": "AllSkills and DefaultSkill",
                "start": 35,
                "end": 45,
                "tests": ["VmGSkillGlobalsTests.CreateEmptySkillList_StartsEmpty"],
            },
            {
                "file": "src/gamedata/g_skill.cpp",
                "symbol": "ParseSkill name assignment",
                "start": 83,
                "end": 87,
                "tests": ["VmSkillMapinfoParserTests.AssignSkillNameFromToken_SetsName"],
            },
            {
                "file": "src/gamedata/teaminfo.cpp",
                "symbol": "TeamInfoOptions table",
                "start": 57,
                "end": 97,
                "tests": ["VmTeamInfoParserTests.ParseTeam_ReadsOptions"],
            },
        ],
    },
    230: {
        "commit": "797d52b69f89e166ce3586d434a59edd79dc13a7",
        "note": "Terrain outer keyword table (trimmed)",
        "auditRationale": "Credits the `OuterKeywords` catalog mirrored by `VmTerrainKeywordSchema` — not splash prototype bodies.",
        "ranges": [
            {
                "file": "src/gamedata/p_terrain.cpp",
                "symbol": "Terrain enums and file header",
                "start": 1,
                "end": 78,
                "tests": ["VmTerrainKeywordSchemaTests.OuterKeywords_MatchesNativeTable"],
            },
        ],
    },
    235: {
        "commit": "91499df55478b48ada6d426351cae028ebb4920e",
        "note": "GameNames and border preset tables (trimmed)",
        "auditRationale": "Credits `GameNames[]` and border preset lookup — not the `GAMEINFOKEY` macro catalog or file banner.",
        "ranges": [
            {
                "file": "src/gamedata/gi.cpp",
                "symbol": "GameNames and border tables",
                "start": 73,
                "end": 111,
                "tests": [
                    "VmGameInfoStaticSchemaTests.GameNames_IncludesDoom",
                    "VmGameInfoStaticSchemaTests.ResolveBorderPreset_HereticUsesFourBySixteen",
                ],
            },
        ],
    },
    313: {
        "commit": "40ede3e2aa1f0990885b9acfc68278aac13df185",
        "note": "DehSupp SoundMap InfoNames ThingBits and load errors (trimmed)",
        "auditRationale": "Credits secondary DehSupp sections and lump error gates — not the full `StateMap` actor/state expansion loop.",
        "ranges": [
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "SoundMap",
                "start": 3653,
                "end": 3663,
                "tests": ["VmDehackedDehSuppSoundInfoTests.ParseSoundMapSection_ResolvesEachSound"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "InfoNames",
                "start": 3664,
                "end": 3679,
                "tests": [
                    "VmDehackedDehSuppSoundInfoTests.TryParseInfoNamesSection_AcceptsKnownActors",
                    "VmDehackedDehSuppSoundInfoTests.TryParseInfoNamesSection_RejectsUnknownActor",
                ],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "ThingBits",
                "start": 3680,
                "end": 3707,
                "tests": [
                    "VmDehackedDehSuppThingBitsTests.TryParseEntry_ValidatesBitRange",
                    "VmDehackedDehSuppThingBitsTests.TryParseEntry_TruncatesLongName",
                    "VmDehackedDehSuppThingBitsTests.TryParseThingBitsSection_ParsesRows",
                ],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "LoadDehSupp unknown section",
                "start": 3781,
                "end": 3787,
                "tests": ["VmDehackedDehSuppLumpGateTests.FormatUnknownSectionError_MatchesNativeMessage"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "LoadDehSupp recoverable error",
                "start": 3790,
                "end": 3796,
                "tests": ["VmDehackedDehSuppLumpGateTests.TryHandleRecoverableLoadError_ReturnsFalse"],
            },
        ],
    },
    321: {
        "commit": "f49971c89ddde51423abdc961c7e9ef0f39bf203",
        "note": "MAPINFO scalar/environment option clamps (trimmed)",
        "auditRationale": "Credits skymist/skymistyscale/wavelimit/music and environment scalars exercised by map-option tests — not full DEFINE blocks through sucktime.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "skymist speed hexen hack",
                "start": 1186,
                "end": 1190,
                "tests": ["VmMapInfoMapOptionsScalarsTests.ClampSkyMistYScale_Bounds"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "skymistyscale clamp",
                "start": 1197,
                "end": 1198,
                "tests": ["VmMapInfoMapOptionsScalarsTests.ClampSkyMistYScale_Bounds"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "wavelimit clamp",
                "start": 1254,
                "end": 1256,
                "tests": ["VmMapInfoMapOptionsScalarsTests.ClampWaveLimit_CapsAt3000"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "music pair parse",
                "start": 1273,
                "end": 1276,
                "tests": ["VmMapInfoMapOptionsScalarsTests.ParseMusicPair_PreservesOrder"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "evenlighting",
                "start": 1310,
                "end": 1312,
                "tests": ["VmMapInfoMapOptionsEnvironmentTests.ApplyEvenLighting_ClearsSkySpeeds"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "cdid hex parse",
                "start": 1324,
                "end": 1325,
                "tests": ["VmMapInfoMapOptionsEnvironmentTests.ParseCdIdHex_ReadsHex"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "wallshade halve clamp",
                "start": 1338,
                "end": 1339,
                "tests": ["VmMapInfoMapOptionsEnvironmentTests.ClampWallShade_DividesByTwo"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "nogravity",
                "start": 1357,
                "end": 1358,
                "tests": ["VmMapInfoMapOptionsEnvironmentTests.ApplyNoGravity_ReturnsMax"],
            },
        ],
    },
    322: {
        "commit": "7aaf320d20a8c826a93c8f41d3c1b51ed05ba4cd",
        "note": "MAPINFO inter/exit/enter picture assigns (trimmed)",
        "auditRationale": "Credits string assignment for inter/exit/enter pics — not anim/specialaction tails.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "interpic",
                "start": 1375,
                "end": 1380,
                "tests": ["VmMapInfoMapOptionsPicsTests.ApplyInterPic_SetsLabel"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "exitpic",
                "start": 1382,
                "end": 1387,
                "tests": ["VmMapInfoMapOptionsPicsTests.ApplyExitPic_SetsNextMap"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "enterpic",
                "start": 1389,
                "end": 1394,
                "tests": ["VmMapInfoMapOptionsPicsTests.ApplyInterPic_SetsLabel"],
            },
        ],
    },
    325: {
        "commit": "b087fb7a7966cafc27943be5e340a1b3bcce65ef",
        "note": "EMAPINFO normalize and SplitValueList/ParseBool (trimmed)",
        "auditRationale": "Credits `NormalizeResourceName`, `SplitValueList`, and empty-true `ParseBool` — not the full primitive block through `SetFlag`/`ParseDouble`.",
        "ranges": [
            {
                "file": "src/gamedata/hcde_emapinfo.cpp",
                "symbol": "NormalizeResourceName",
                "start": 104,
                "end": 112,
                "tests": ["VmEMapInfoLineParserTests.NormalizeResourceName_StripsLeadingSlashes"],
            },
            {
                "file": "src/gamedata/hcde_emapinfo.cpp",
                "symbol": "SplitValueList",
                "start": 174,
                "end": 201,
                "tests": ["VmEMapInfoLineParserTests.SplitValueList_SplitsOnComma"],
            },
            {
                "file": "src/gamedata/hcde_emapinfo.cpp",
                "symbol": "ParseBool empty true",
                "start": 205,
                "end": 209,
                "tests": ["VmEMapInfoLineParserTests.TryParseBool_TreatsEmptyAsTrue"],
            },
        ],
    },
    327: {
        "commit": "5e9cfd300693278354535a2051930dc9fed1da76",
        "note": "EMAPINFO ApplyInt/Double/BossSpecials and lump normalize (trimmed)",
        "auditRationale": "Credits typed apply wrappers, `E1M8` boss-special bit, and `NormalizeResourceName` head used by lump resolve tests.",
        "ranges": [
            {
                "file": "src/gamedata/hcde_emapinfo.cpp",
                "symbol": "ApplyIntProperty",
                "start": 405,
                "end": 413,
                "tests": ["VmEMapInfoApplyHelpersTests.ApplyIntProperty_RejectsBadNumber"],
            },
            {
                "file": "src/gamedata/hcde_emapinfo.cpp",
                "symbol": "ApplyDoubleProperty",
                "start": 415,
                "end": 423,
                "tests": ["VmEMapInfoApplyHelpersTests.ApplyDoubleProperty_RejectsBadToken"],
            },
            {
                "file": "src/gamedata/hcde_emapinfo.cpp",
                "symbol": "ApplyBossSpecials E1M8",
                "start": 436,
                "end": 439,
                "tests": ["VmEMapInfoApplyHelpersTests.ApplyBossSpecials_ParsesE1M8"],
            },
            {
                "file": "src/gamedata/hcde_emapinfo.cpp",
                "symbol": "ResolveSiblingLump empty name",
                "start": 519,
                "end": 523,
                "tests": ["VmEMapInfoLumpResolveTests.NormalizeLumpLookupName_NormalizesSlashes"],
            },
        ],
    },
    329: {
        "commit": "9531f1c0f019d32272ee67cc657ad01404522e20",
        "note": "MAPINFO edata/intro and e1m8special row (trimmed)",
        "auditRationale": "Credits `edata`/`intro` string assigns and the `e1m8special` handler catalog row — not the full `MapFlagHandlers` head.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "edata assign",
                "start": 1892,
                "end": 1897,
                "tests": ["VmMapInfoMapOptionsScriptTests.ApplyEdName_SetsField"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "intro cutscene",
                "start": 1906,
                "end": 1909,
                "tests": ["VmMapInfoMapOptionsScriptTests.ApplyIntroCutscene_SetsLump"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "e1m8special handler row",
                "start": 1960,
                "end": 1960,
                "tests": ["VmMapInfoFlagHandlersTests.TryGetHandler_FindsE1M8Special"],
            },
        ],
    },
    330: {
        "commit": "9cd1c34f4eef953957cb97227a3ea12ea4b27c0b",
        "note": "MapFlagHandlers strife/compat rows (trimmed)",
        "auditRationale": "Credits only handler rows referenced by mid-table lookup tests.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "strifefallingdamage row",
                "start": 1983,
                "end": 1983,
                "tests": ["VmMapInfoFlagHandlersMidTests.TryGetHandler_FindsStrifeFallingDamage"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "compat_noid24 row",
                "start": 2091,
                "end": 2091,
                "tests": ["VmMapInfoFlagHandlersMidTests.TryGetHandler_FindsCompatNoid24"],
            },
        ],
    },
    331: {
        "commit": "0465a296c165af1d871e8d49ec2223e87ad58f9f",
        "note": "ParseMapDefinition MITYPE flag apply arms (trimmed)",
        "auditRationale": "Credits `MITYPE_SETFLAG2`, `MITYPE_SCFLAGS2`, and `MITYPE_CLRCOMPATFLAG` switch arms only.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "MITYPE_SETFLAG2",
                "start": 2159,
                "end": 2171,
                "tests": ["VmMapInfoFlagApplyTests.ApplySetFlag2_TogglesWithAssign"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "MITYPE_SCFLAGS2",
                "start": 2178,
                "end": 2180,
                "tests": ["VmMapInfoFlagApplyTests.ApplyScFlags2_MergesTeamplayOn"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "MITYPE_CLRCOMPATFLAG",
                "start": 2205,
                "end": 2210,
                "tests": ["VmMapInfoFlagApplyTests.ApplyClearCompat_SetsMask"],
            },
        ],
    },
    332: {
        "commit": "ffb09f7b037de026d3b2515afca8e5b2bc045eb0",
        "note": "GetDefaultLevelNum and ParseMapHeader title paths (trimmed)",
        "auditRationale": "Credits MAP/E#M# default levelnum, Hexen numeric map token, `$` title prefix, and default levelnum assign — not full `ParseMapHeader` body.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "GetDefaultLevelNum MAP branch",
                "start": 2297,
                "end": 2305,
                "tests": ["VmMapInfoParseMapHeaderTests.AssignDefaultLevelNum_ParsesMap01"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "ParseMapHeader Hexen number token",
                "start": 2341,
                "end": 2345,
                "tests": ["VmMapInfoParseMapHeaderTests.ResolveMapNameToken_OldHexenNumber"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "ParseMapHeader dollar title",
                "start": 2382,
                "end": 2387,
                "tests": ["VmMapInfoParseMapHeaderTests.ApplyLevelTitle_DollarPrefixSetsLookup"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "default levelnum assign",
                "start": 2430,
                "end": 2432,
                "tests": ["VmMapInfoParseMapHeaderTests.AssignDefaultLevelNum_ParsesMap01"],
            },
        ],
    },
    336: {
        "commit": "af911e170e4a161a67570a8748e9905b68f6d162",
        "note": "MBF scratch/jump/line-effect emitters (trimmed)",
        "auditRationale": "Credits `CreateScratchFunc` (avoiding chunk 293 sound-index rows), `CreateRandomJumpFunc`, and `CreateLineEffectFunc` argument packing.",
        "ranges": [
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "CreateScratchFunc body",
                "start": 829,
                "end": 830,
                "tests": ["VmDehackedMbfScratchSoundTests.ScratchMeleeArgs_ResolvesHitSound"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "CreateScratchFunc sound args",
                "start": 833,
                "end": 837,
                "tests": ["VmDehackedMbfScratchSoundTests.ScratchMeleeArgs_ResolvesHitSound"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "CreateRandomJumpFunc",
                "start": 851,
                "end": 858,
                "tests": ["VmDehackedMbfScratchSoundTests.RandomJumpArgs_ResolvesStateLabel"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "CreateLineEffectFunc",
                "start": 861,
                "end": 870,
                "tests": ["VmDehackedMbfScratchSoundTests.LineEffectArgs_PassesSpecialAndTag"],
            },
        ],
    },
    339: {
        "commit": "01ed52cec6f788294d93c451194c49174b1e9c65",
        "note": "MBF tracer/seeker/tracer2/bullet defaults (trimmed)",
        "auditRationale": "Credits default-argument packing for tracer-family emitters and weapon bullet counts — not the full emitter table through melee attack.",
        "ranges": [
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "CreateFindTracerFunc",
                "start": 936,
                "end": 941,
                "tests": ["VmDehackedMbfPointerEmittersTests.FindTracerArgs_DefaultsRange"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "CreateSeekerMissileFunc",
                "start": 943,
                "end": 952,
                "tests": ["VmDehackedMbfPointerEmittersTests.SeekerMissileArgs_AppliesDefaults"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "CreateTracer2Func",
                "start": 954,
                "end": 959,
                "tests": ["VmDehackedMbfPointerEmittersTests.Tracer2Angle_UsesDefaultWhenZero"],
            },
        ],
    },
    340: {
        "commit": "d567e408bbdc4a5c120cd82526bca36c5c05fe95",
        "note": "Automap constants cheat gate and margin clamp (trimmed)",
        "auditRationale": "Credits automap numeric constants, net-game `am_cheat` gate, and `am_emptyspacemargin` clamp — not CVAR registration blocks.",
        "ranges": [
            {
                "file": "src/am_map.cpp",
                "symbol": "Automap numeric constants",
                "start": 66,
                "end": 76,
                "tests": ["VmAutomapCvarModelTests.ClampEmptySpaceMargin_Bounds"],
            },
            {
                "file": "src/am_map.cpp",
                "symbol": "am_cheat net gate",
                "start": 135,
                "end": 139,
                "tests": ["VmAutomapCvarModelTests.ShouldBlockAutomapCheat_WhenNetNoCheats"],
            },
            {
                "file": "src/am_map.cpp",
                "symbol": "am_emptyspacemargin clamp",
                "start": 170,
                "end": 177,
                "tests": ["VmAutomapCvarModelTests.ClampEmptySpaceMargin_Bounds"],
            },
        ],
    },
}


def native_loc(ranges: list[dict]) -> int:
    return sum(int(r["end"]) - int(r["start"]) + 1 for r in ranges)


def commit_src_cs_additions(sha: str) -> int:
    root = REPO.parent
    out = subprocess.check_output(["git", "show", sha, "--numstat", "--format="], cwd=root, text=True)
    total = 0
    for line in out.splitlines():
        parts = line.split("\t")
        if len(parts) != 3:
            continue
        add_s, _del, path = parts
        if not path.endswith(".cs"):
            continue
        if ".Tests/" in path or "/tests/" in path:
            continue
        if not path.startswith("csharp/src/"):
            continue
        total += int(add_s)
    return total


def apply() -> None:
    tally = json.loads(TALLY.read_text())
    tally["chunks"] = [c for c in tally["chunks"] if not any(c["name"].endswith(f"chunk{n}") for n in DROP_CHUNKS)]
    for num in DROP_CHUNKS:
        tool_path = TOOLS / f"chunk{num}.json"
        if tool_path.exists():
            tool_path.unlink()
    for num, spec in TRIMS.items():
        nl = native_loc(spec["ranges"])
        ml = commit_src_cs_additions(spec["commit"])
        name_suffix = f"chunk{num}"
        chunk_name = next(c["name"] for c in tally["chunks"] if c["name"].endswith(name_suffix))
        for ch in tally["chunks"]:
            if ch["name"] != chunk_name:
                continue
            ch["nativeRanges"] = spec["ranges"]
            ch["nativeLoc"] = nl
            ch["managedLoc"] = ml
            ch["conversionManagedCommit"] = spec["commit"]
            ch["note"] = spec["note"]
            ch["auditRationale"] = spec["auditRationale"]
            break
        existing = next(c for c in tally["chunks"] if c["name"] == chunk_name)
        tool_path = TOOLS / f"chunk{num}.json"
        tool_doc = [
            {
                "name": chunk_name,
                "bucket": "conversion",
                "managedLoc": ml,
                "note": spec["note"],
                "auditSeams": existing.get("auditSeams", ""),
                "auditRationale": spec["auditRationale"],
                "nativeRanges": spec["ranges"],
                "conversionManagedCommit": spec["commit"],
            }
        ]
        tool_path.write_text(json.dumps(tool_doc, indent=2) + "\n")
    TALLY.write_text(json.dumps(tally, indent=2) + "\n")
    before = 2728  # documented pre-trim thin_managed native (approx)
    after = sum(native_loc(TRIMS[n]["ranges"]) for n in TRIMS)
    print(f"Applied {len(TRIMS)} trims; native sum {after}")


if __name__ == "__main__":
    apply()
