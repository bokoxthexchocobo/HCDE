#!/usr/bin/env python3
"""Trim loc_tally chunks 265-286 to tested native line ranges."""
import json
from pathlib import Path

TALLY = Path(__file__).resolve().parents[1] / "docs/data/loc_tally.json"

TRIMS: dict[str, list[dict]] = {
    "dehacked-patchthing-numeric-chunk265": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing actor resolve",
            "start": 1268,
            "end": 1278,
            "tests": ["VmDehackedPatchThingContextTests.TryResolveThingType_RejectsOutOfRange"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing numeric bounds",
            "start": 1290,
            "end": 1293,
            "tests": ["VmDehackedPatchThingNumericTests.IsSupportedNumeric_RejectsOutOfRange"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing Hit points",
            "start": 1294,
            "end": 1297,
            "tests": ["VmDehackedPatchThingNumericTests.TryApplyNumericProperty_SetsHitPoints"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing Translucency",
            "start": 1306,
            "end": 1313,
            "tests": ["VmDehackedPatchThingNumericTests.TryApplyNumericProperty_AppliesTranslucency"],
        },
    ],
    "dehacked-patchthing-combat-chunk266": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing blood color",
            "start": 1437,
            "end": 1455,
            "tests": ["VmDehackedPatchThingCombatExtrasTests.TryApplyLine_SetsBloodColorIndex"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing MBF21 Bits tokens",
            "start": 1456,
            "end": 1479,
            "tests": ["VmDehackedPatchThingCombatExtrasTests.ParseMbf21Bits_CombinesMnemonicAndNumeric"],
        },
    ],
    "dehacked-patchthing-states-bits-a-chunk267": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing Initial frame",
            "start": 1576,
            "end": 1577,
            "tests": ["VmDehackedPatchThingStateLabelsTests.TryMapFrameLabel_Spawn"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing Death sound",
            "start": 1623,
            "end": 1624,
            "tests": ["VmDehackedPatchThingStateLabelsTests.TryMapSoundLabel_Death"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing Bits MBF bounce remap",
            "start": 1692,
            "end": 1718,
            "tests": ["VmDehackedPatchThingBitsTests.RemapMbfFlags_SetsGrenadeBounceStyle"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing Bits translucent alpha",
            "start": 1805,
            "end": 1809,
            "tests": ["VmDehackedPatchThingBitsTests.ResolveTranslucentAlpha_ReturnsHalf"],
        },
    ],
    "dehacked-patchthing-bits-finalize-chunk268": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing height pair check",
            "start": 1833,
            "end": 1840,
            "tests": ["VmDehackedPatchThingFinalizeTests.ValidateHeightPair_RequiresBoth"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchThing speed rescale",
            "start": 1871,
            "end": 1874,
            "tests": ["VmDehackedPatchThingFinalizeTests.NormalizeSpeed_ScalesLargeFixed"],
        },
    ],
    "dehacked-patchframe-chunk269": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchFrame shotgun flash tics",
            "start": 1981,
            "end": 1988,
            "tests": ["VmDehackedPatchFrameTests.GetDefaultTics_UsesShotgunFlashDefaults"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchFrame Duration",
            "start": 2009,
            "end": 2012,
            "tests": ["VmDehackedPatchFrameTests.TryApplyProperty_SetsDuration"],
        },
    ],
    "dehacked-patchsprite-ammo-weapon-chunk270": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchSprite offset",
            "start": 2144,
            "end": 2145,
            "tests": ["VmDehackedPatchSpriteTests.ComputeNameOffset_SubtractsTableBase"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchAmmo backpack amounts",
            "start": 2204,
            "end": 2205,
            "tests": ["VmDehackedPatchAmmoTests.ApplyBackpackAmounts_DoublesMax"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchWeapon Select/Deselect swap",
            "start": 2294,
            "end": 2295,
            "tests": ["VmDehackedPatchWeaponTests.TryMapWeaponFrameLabel_SwapsSelectDeselect"],
        },
    ],
    "g-mapinfo-light-shadow-sky-chunk271": [
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "lightblendmode Unclamped",
            "start": 1735,
            "end": 1737,
            "tests": ["VmMapInfoLightBlendTonemapTests.TryParseLightBlendMode_Unclamped"],
        },
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "hcde_shadowprofile clamp",
            "start": 1779,
            "end": 1782,
            "tests": ["VmMapInfoHcdeShadowOptionsTests.ClampShadowProfile_Bounds"],
        },
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "hcde_shadowqualitycap normalize",
            "start": 1792,
            "end": 1800,
            "tests": ["VmMapInfoHcdeShadowOptionsTests.NormalizeShadowQualityCap_PicksNearestTier"],
        },
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "notexturefill optional bool",
            "start": 1827,
            "end": 1835,
            "tests": ["VmMapInfoToggleOptionsTests.ParseOptionalBool_FlagWhenOmitted"],
        },
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "lightadditivesurfaces optional bool",
            "start": 1840,
            "end": 1848,
            "tests": ["VmMapInfoToggleOptionsTests.ParseOptionalBool_FlagWhenOmitted"],
        },
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "skyrotate vector",
            "start": 1853,
            "end": 1862,
            "tests": ["VmMapInfoSkyRotateParserTests.TryParseVector_Normalizes"],
        },
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "fs_nocheckposition optional bool",
            "start": 1881,
            "end": 1889,
            "tests": ["VmMapInfoToggleOptionsTests.ParseOptionalBool_FlagWhenOmitted"],
        },
    ],
    "a-weapons-slot-chunk272": [
        {
            "file": "src/gamedata/a_weapons.cpp",
            "symbol": "FWeaponSlot AddWeaponList",
            "start": 105,
            "end": 121,
            "tests": ["VmWeaponSlotRuntimeTests.AddWeaponList_AppendsWeapons"],
        },
        {
            "file": "src/gamedata/a_weapons.cpp",
            "symbol": "FWeaponSlot SetInitialPositions single",
            "start": 160,
            "end": 163,
            "tests": ["VmWeaponSlotRuntimeTests.SetInitialPositions_SingleWeaponCentered"],
        },
    ],
    "hcde-emapinfo-line-parse-chunk273": [
        {
            "file": "src/gamedata/hcde_emapinfo.cpp",
            "symbol": "StripLineComment",
            "start": 124,
            "end": 148,
            "tests": ["VmEMapInfoLineParserTests.StripLineComment_HonorsQuotes"],
        },
        {
            "file": "src/gamedata/hcde_emapinfo.cpp",
            "symbol": "SplitPropertyLine",
            "start": 150,
            "end": 167,
            "tests": ["VmEMapInfoLineParserTests.SplitPropertyLine_ParsesEqualsForm"],
        },
    ],
    "g-mapinfo-flag-handlers-chunk274": [
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "MapFlagHandlers nojump",
            "start": 1979,
            "end": 1979,
            "tests": ["VmMapInfoFlagHandlersTests.TryGetHandler_FindsNoJump"],
        },
    ],
    "dehacked-patchweapon-ammo-chunk275": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchWeapon Ammo type",
            "start": 2306,
            "end": 2323,
            "tests": ["VmDehackedPatchWeaponAmmoTests.ApplyAmmoType_DoublesGiveAmount"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "FinishDehPatch clear AmmoUse",
            "start": 3958,
            "end": 3960,
            "tests": ["VmDehackedPatchWeaponAmmoTests.FinalizeAmmo_ClearsUseWhenNoType"],
        },
    ],
    "dehacked-patchsound-pointer-chunk276": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchSound consume lines",
            "start": 1928,
            "end": 1942,
            "tests": ["VmDehackedPatchSoundTests.ConsumePatchLines_StopsOnNonOne"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchPointer Boom hack",
            "start": 2470,
            "end": 2479,
            "tests": ["VmDehackedPatchPointerTests.ResolveBoomPointerZero_FindsIndex"],
        },
    ],
    "dehacked-patchmisc-keys-chunk277": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchMisc Initial Health key",
            "start": 2540,
            "end": 2540,
            "tests": ["VmDehackedPatchMiscTests.TryApplyKnownKey_SetsStartHealth"],
        },
    ],
    "dehacked-patchmisc-tail-chunk278": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchMisc infighting 0xDD",
            "start": 2718,
            "end": 2722,
            "tests": ["VmDehackedPatchMiscArmorTests.NormalizeInfighting_MapsDd"],
        },
    ],
    "g-mapinfo-flag-apply-chunk279": [
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "ParseMapDefinition MITYPE_SETFLAG",
            "start": 2136,
            "end": 2147,
            "tests": ["VmMapInfoFlagApplyTests.ApplySetFlag_TogglesWithAssign"],
        },
    ],
    "g-mapinfo-option-dispatch-chunk280": [
        {
            "file": "src/gamedata/g_mapinfo.cpp",
            "symbol": "ParseMapDefinition handler match",
            "start": 2123,
            "end": 2125,
            "tests": ["VmMapInfoOptionDispatchTests.TryDispatchOption_MatchesName"],
        },
    ],
    "a-weapons-slots-chunk281": [
        {
            "file": "src/gamedata/a_weapons.cpp",
            "symbol": "FWeaponSlots AddDefaultWeapon",
            "start": 226,
            "end": 240,
            "tests": ["VmWeaponSlotsRuntimeTests.ApplyDefaultSlotWeapons_SkipsWhenSlotsPopulated"],
        },
        {
            "file": "src/gamedata/a_weapons.cpp",
            "symbol": "FWeaponSlots LocateWeapon",
            "start": 251,
            "end": 265,
            "tests": ["VmWeaponSlotsRuntimeTests.TryLocateWeapon_FindsAcrossSlots"],
        },
    ],
    "hcde-emapinfo-primitives-chunk283": [
        {
            "file": "src/gamedata/hcde_emapinfo.cpp",
            "symbol": "GetDefaultLevelNum MAPxx",
            "start": 294,
            "end": 301,
            "tests": ["VmEMapInfoParsePrimitivesTests.GetDefaultLevelNum_ParsesMap01"],
        },
    ],
    "hcde-emapinfo-level-chunk284": [
        {
            "file": "src/gamedata/hcde_emapinfo.cpp",
            "symbol": "ApplyBoolProperty invalid",
            "start": 395,
            "end": 403,
            "tests": ["VmEMapInfoApplyHelpersTests.ApplyBoolProperty_RecordsInvalidOnBadToken"],
        },
    ],
    "dehacked-patchmusic-sprite-chunk285": [
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "PatchMusic line",
            "start": 2880,
            "end": 2887,
            "tests": ["VmDehackedPatchMusicTests.TryParseMusicLine_BuildsMusicKey"],
        },
        {
            "file": "src/gamedata/d_dehacked.cpp",
            "symbol": "ReplaceSpriteInData PLAY",
            "start": 2896,
            "end": 2898,
            "tests": ["VmDehackedReplaceSpriteTests.IsPlayerSpritePrefix_DetectsPlay"],
        },
    ],
    "hcde-emapinfo-lump-apply-chunk286": [
        {
            "file": "src/gamedata/hcde_emapinfo.cpp",
            "symbol": "ResolveSiblingLump relative path",
            "start": 534,
            "end": 538,
            "tests": ["VmEMapInfoLumpResolveTests.TryBuildSiblingRelativePath_JoinsDirectory"],
        },
        {
            "file": "src/gamedata/hcde_emapinfo.cpp",
            "symbol": "gravity fixed scale",
            "start": 664,
            "end": 664,
            "tests": ["VmEMapInfoLumpResolveTests.ScaleFixedGravity_ZeroIsMax"],
        },
    ],
}

REMOVE = {"a-weapons-slots-setup-chunk282"}


def native_loc(ranges: list[dict]) -> int:
    return sum(r["end"] - r["start"] + 1 for r in ranges)


def main() -> None:
    tally = json.loads(TALLY.read_text())
    chunks = tally["chunks"]
    before = 0
    after = 0
    for ch in chunks:
        name = ch.get("name", "")
        if name in TRIMS or name in REMOVE:
            before += ch.get("nativeLoc", 0)
    new_chunks = []
    for ch in chunks:
        name = ch.get("name", "")
        if name in REMOVE:
            continue
        if name in TRIMS:
            ch["nativeRanges"] = TRIMS[name]
            ch["nativeLoc"] = native_loc(TRIMS[name])
            ch["note"] = (ch.get("note", "") + " (re-audit 265-286: tested ranges only)").strip()
        new_chunks.append(ch)
    tally["chunks"] = new_chunks
    for ch in new_chunks:
        name = ch.get("name", "")
        if name in TRIMS:
            after += ch["nativeLoc"]
    TALLY.write_text(json.dumps(tally, indent=2) + "\n")
    print(f"nativeLoc 265-286 before={before} after={after} delta={before - after}")


if __name__ == "__main__":
    main()
