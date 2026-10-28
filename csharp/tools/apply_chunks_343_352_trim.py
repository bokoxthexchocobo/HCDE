#!/usr/bin/env python3
"""Trim chunks 343-352 to symbol ranges with real managed ports + tests."""
import json
import subprocess
from pathlib import Path

REPO = Path(__file__).resolve().parents[1]
TALLY = REPO / "docs/data/loc_tally.json"
TOOLS = REPO / "tools"

TRIMS: dict[int, dict] = {
    343: {
        "commit": "f7667961b082bd7f47df5dcde6476ae988a43683",
        "note": "Automap togglemap/am_zoom and AMColor (trimmed re-audit)",
        "auditRationale": "Credits only `CCMD(togglemap)` idle guard, `CCMD(am_zoom)` argv parse, and `AMColor` RGB validity — not CVAR registration blocks.",
        "ranges": [
            {
                "file": "src/am_map.cpp",
                "symbol": "CCMD togglemap",
                "start": 246,
                "end": 251,
                "tests": ["VmAutomapCommandsTests.ShouldRunToggleMap_OnlyWhenIdle"],
            },
            {
                "file": "src/am_map.cpp",
                "symbol": "CCMD am_zoom",
                "start": 254,
                "end": 260,
                "tests": ["VmAutomapCommandsTests.ParseZoomDir_UsesArgument"],
            },
            {
                "file": "src/am_map.cpp",
                "symbol": "AMColor pack/valid",
                "start": 326,
                "end": 348,
                "tests": [
                    "VmAutomapColorArgbTests.FromRgb_PacksAlpha",
                    "VmAutomapColorArgbTests.Invalid_IsNotValid",
                ],
            },
        ],
    },
    344: {
        "commit": "049dbe1ec1591792621c1dd447ff7818816e8cf2",
        "note": "Teleport TID rewrite and missing-texture warn cap (trimmed)",
        "auditRationale": "Credits `TranslateTeleportThings` TID/`line.args` rewrite and `SetTexture` warn-limit plus flat-name truncation only.",
        "ranges": [
            {
                "file": "src/maploader/maploader.cpp",
                "symbol": "MISSING_TEXTURE_WARN_LIMIT",
                "start": 60,
                "end": 63,
                "tests": ["VmMapLoaderMissingTexturePolicyTests.ShouldPrintWarning_UntilLimit"],
            },
            {
                "file": "src/maploader/maploader.cpp",
                "symbol": "TranslateTeleportThings TID",
                "start": 100,
                "end": 105,
                "tests": ["VmMapLoaderTeleportTranslateTests.ShouldAssignTid_WhenSectorUntagged"],
            },
            {
                "file": "src/maploader/maploader.cpp",
                "symbol": "TranslateTeleportThings line args",
                "start": 111,
                "end": 130,
                "tests": ["VmMapLoaderTeleportTranslateTests.ApplyTeleportLineArgs_SetsTidWhenTagZero"],
            },
            {
                "file": "src/maploader/maploader.cpp",
                "symbol": "SetTexture warn gate",
                "start": 151,
                "end": 157,
                "tests": ["VmMapLoaderMissingTexturePolicyTests.ShouldPrintWarning_UntilLimit"],
            },
            {
                "file": "src/maploader/maploader.cpp",
                "symbol": "SetTexture flat truncate",
                "start": 187,
                "end": 192,
                "tests": ["VmMapLoaderMissingTexturePolicyTests.TruncateFlatName_EightChars"],
            },
        ],
    },
    345: {
        "commit": "7d8fadf33724f5fde5b2221019e24fc6d1c81dad",
        "note": "WarpTrans secret visit Reset flags2 (trimmed)",
        "auditRationale": "Credits `CheckWarpTransMap`, `SecretLevelVisited`, and Hexen/Doom `flags2` default in `level_info_t::Reset` only.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "CheckWarpTransMap",
                "start": 134,
                "end": 154,
                "tests": ["VmMapInfoWarpTransTests.TryResolveWarpTransMap_UsesLevelRecord"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "SecretLevelVisited",
                "start": 161,
                "end": 168,
                "tests": ["VmMapInfoWarpTransTests.SecretLevelVisited_DetectsVisitedSecret"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "level_info_t::Reset flags2",
                "start": 257,
                "end": 260,
                "tests": [
                    "VmMapInfoLevelResetTests.CreateDefaults_SetsHexenFlags2",
                    "VmMapInfoLevelResetTests.CreateDefaults_SetsDoomLaxMonster",
                ],
            },
        ],
    },
    346: {
        "commit": "6546c28c2e5aea3dd28576b65080a1aa71c80e65",
        "note": "LookupLevelName strip and brace format (trimmed)",
        "auditRationale": "Credits localized title header stripping and `FMapInfoParser` open/close brace plus unexpected-EOF guard.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "LookupLevelName header strip",
                "start": 359,
                "end": 375,
                "tests": ["VmMapInfoLookupLevelNameTests.StripLocalizedHeader_RemovesMapPrefix"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "FMapInfoParser::ParseOpenBrace",
                "start": 480,
                "end": 498,
                "tests": ["VmMapInfoParserFormatTests.ResolveOpenBrace_DetectsNewFormat"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "FMapInfoParser::ParseCloseBrace",
                "start": 505,
                "end": 518,
                "tests": ["VmMapInfoParserFormatTests.ResolveOpenBrace_DetectsNewFormat"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "FMapInfoParser::CheckEndOfFile",
                "start": 661,
                "end": 667,
                "tests": ["VmMapInfoParserFormatTests.EnsureNotUnexpectedEof_ThrowsForNewFormat"],
            },
        ],
    },
    347: {
        "commit": "2b1afe4047fcd37b58c91efe6f965ede42ff76b0",
        "note": "ParseLookupName lookup/$ tokens (trimmed)",
        "auditRationale": "Credits `FMapInfoParser::ParseLookupName` lookup and `$` string-table prefix handling only.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "FMapInfoParser::ParseLookupName",
                "start": 678,
                "end": 694,
                "tests": ["VmMapInfoParseLookupNameTests.TryParseLookupToken_StripsDollarPrefix"],
            },
        ],
    },
    348: {
        "commit": "3996db610eac83f057553c15d536a5906c33f526",
        "note": "Cluster exit remap ParseNextMap shadow cap (trimmed)",
        "auditRationale": "Credits Hexen `CLUS?MSG` remap, `ParseNextMap` numeric/end-sequence formatting, and shadow-quality quantization only.",
        "ranges": [
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "cluster Hexen exit text remap",
                "start": 1008,
                "end": 1027,
                "tests": ["VmMapInfoClusterExitTextRemapTests.TryRemapHexenExitLump_BuildsKey"],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "FMapInfoParser::ParseNextMap",
                "start": 1041,
                "end": 1064,
                "tests": [
                    "VmMapInfoParseNextMapTests.FormatNumericNextMap_HexenUsesWarpTrans",
                    "VmMapInfoParseNextMapTests.FormatEndSequenceMap_EncodesIndex",
                ],
            },
            {
                "file": "src/gamedata/g_mapinfo.cpp",
                "symbol": "NormalizeHcdeShadowQualityCap",
                "start": 1066,
                "end": 1075,
                "tests": ["VmMapInfoShadowQualityCapTests.Normalize_SnapsToKnownStep"],
            },
        ],
    },
    349: {
        "commit": "102214eeceef2c793fa646aac7c714c205c28a42",
        "note": "DeHackEd lookup bounds and Modes table (trimmed)",
        "auditRationale": "Credits `FindInfoName`/`DehFindSound` index guards, `ReplaceSoundName` ds-prefix, and `Modes[]` section names only.",
        "ranges": [
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "FindInfoName index guard",
                "start": 118,
                "end": 122,
                "tests": ["VmDehackedLookupCoreTests.ResolveInfoNameIndex_RespectsBounds"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "DehFindSound index guard",
                "start": 146,
                "end": 147,
                "tests": ["VmDehackedLookupCoreTests.IsValidSoundIndex_RespectsBounds"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "ReplaceSoundName ds prefix",
                "start": 162,
                "end": 163,
                "tests": ["VmDehackedLookupCoreTests.DsSoundLumpName_PrefixesDs"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "Modes section dispatch",
                "start": 467,
                "end": 476,
                "tests": ["VmDehackedPatchModesTests.IsKnownSection_RejectsUnknown"],
            },
        ],
    },
    350: {
        "commit": "225870cb44bd0e0fa754bcccc3189ee35776d68b",
        "note": "DeHackEd lump filter and header sniff (trimmed)",
        "auditRationale": "Credits `isDehFile`, PWAD IWAD lump skip, and DeHackEd v2.3+ header gate only.",
        "ranges": [
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "isDehFile",
                "start": 3213,
                "end": 3220,
                "tests": ["VmDehackedLumpLoaderTests.IsRootDehOrBex_RejectsNestedPaths"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "D_LoadDehLumps IWAD skip",
                "start": 3235,
                "end": 3238,
                "tests": ["VmDehackedLumpLoaderTests.IsRootDehOrBex_RejectsNestedPaths"],
            },
            {
                "file": "src/gamedata/d_dehacked.cpp",
                "symbol": "DoDehPatch header version",
                "start": 3349,
                "end": 3357,
                "tests": ["VmDehackedLumpLoaderTests.IsSupportedDehackedHeader_AcceptsV3"],
            },
        ],
    },
    351: {
        "commit": "18e6c38b056a4787ee8c046efdb2f28b24ce69c1",
        "note": "D_Main timescale and rendermode policy (trimmed)",
        "auditRationale": "Credits `i_timescale` net-game reset and `vid_rendermode` clamp/softpoly remap only.",
        "ranges": [
            {
                "file": "src/d_main.cpp",
                "symbol": "CUSTOM_CVAR i_timescale net",
                "start": 424,
                "end": 428,
                "tests": ["VmDMainCvarPolicyTests.ClampTimeScaleForNetGame_ForcesOne"],
            },
            {
                "file": "src/d_main.cpp",
                "symbol": "CUSTOM_CVAR vid_rendermode clamp",
                "start": 446,
                "end": 453,
                "tests": ["VmDMainCvarPolicyTests.ClampVidRenderMode_RemapsSoftpoly"],
            },
        ],
    },
    352: {
        "commit": "1e946392b9c02e0c95954a7c76dde10e42d00e8c",
        "note": "Automap ColorNames table (trimmed)",
        "auditRationale": "Credits static `ColorNames[]` labels mirrored by `VmAutomapColorsetIndex.ColorNames`.",
        "ranges": [
            {
                "file": "src/am_map.cpp",
                "symbol": "ColorNames table",
                "start": 357,
                "end": 382,
                "tests": [
                    "VmAutomapColorsetIndexTests.ColorNames_MatchesNativeTableLength",
                    "VmAutomapColorsetIndexTests.Indices_AlignWithFirstEntries",
                ],
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
    for num, spec in TRIMS.items():
        nl = native_loc(spec["ranges"])
        ml = commit_src_cs_additions(spec["commit"])
        name_suffix = f"chunk{num}"
        chunk_name = next(
            c["name"] for c in tally["chunks"] if c["name"].endswith(name_suffix)
        )
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
            }
        ]
        tool_path.write_text(json.dumps(tool_doc, indent=2) + "\n")
    TALLY.write_text(json.dumps(tally, indent=2) + "\n")
    print("Applied trims; native sum", sum(native_loc(TRIMS[n]["ranges"]) for n in TRIMS))


if __name__ == "__main__":
    apply()
