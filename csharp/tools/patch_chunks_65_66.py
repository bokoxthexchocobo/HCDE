#!/usr/bin/env python3
import json
from pathlib import Path

def range_entry(file, symbol, start, end, tests):
    return {"file": file, "symbol": symbol, "start": start, "end": end, "tests": tests}

ACS = "src/playsim/p_acs.cpp"
CF = "DLevelScript::CallFunction"

chunk65 = {
    "name": "p-acs-callfunc-pointer-band-chunk65-recognition",
    "bucket": "recognition",
    "managedLoc": 0,
    "note": "CallFunction pointer/net/activator/char/armor cases with per-case managed handlers.",
    "nativeRanges": [
        range_entry(ACS, "ACSF_GetNetID", 5492, 5497,
                    ["AcsCallFunctionTests.CallFunc_GetNetId_ReturnsActorNetworkId"]),
        range_entry(ACS, "ACSF_SetActivator", 5514, 5524,
                    ["AcsActivatorFailureTests.SetActivatorExistingTidSelectsActor"]),
        range_entry(ACS, "ACSF_SetActivatorToTarget", 5526, 5548,
                    ["AcsCallFunctionTests.CallFunc_SetActivatorToTarget_UsesMonsterChaseTarget"]),
        range_entry(ACS, "ACSF_SetActivatorByNetID", 5550, 5560,
                    ["AcsCallFunctionTests.CallFunc_SetActivatorByNetId_SwitchesActivator"]),
        range_entry(ACS, "ACSF_GetChar", 5578, 5590,
                    ["AcsGetCharByteTests.GetCharReturnsNativeSignedUtf8Byte"]),
        range_entry(ACS, "ACSF_GetArmorType", 5630, 5645,
                    ["AcsCallFunctionTests.CallFunc_GetArmorType_ReturnsWornAmount"]),
        range_entry(ACS, "ACSF_GetArmorInfo", 5647, 5681,
                    ["AcsCallFunctionTests.CallFunc_GetArmorInfo_ReturnsMegaSavePercent"]),
    ],
}
chunk65["nativeLoc"] = sum(r["end"] - r["start"] + 1 for r in chunk65["nativeRanges"])

chunk66 = {
    "name": "p-acs-callfunc-gameplay-band-chunk66-recognition",
    "bucket": "recognition",
    "managedLoc": 0,
    "note": "CallFunction gameplay/math/HUD/CVar cases with evidenced TryInvoke handlers.",
    "nativeRanges": [
        range_entry(ACS, "ACSF_SetActorVelocity", 5695, 5713,
                    ["AcsCallFunctionTests.CallFunc_SetActorVelocity_ThenGetVelXMatches"]),
        range_entry(ACS, "ACSF_SetUserVariable", 5715, 5742,
                    ["AcsScriptUserVariableServiceTests.CallFunc_SetAndGetUserVariable_RoundTripsIndexedSlot"]),
        range_entry(ACS, "ACSF_GetUserVariable", 5744, 5754,
                    ["AcsScriptUserVariableServiceTests.CallFunc_SetAndGetUserVariable_RoundTripsIndexedSlot"]),
        range_entry(ACS, "ACSF_SetUserArray", 5756, 5783,
                    ["AcsHotCallFuncBehaviorTests.UserArrayIndexRoundTrip"]),
        range_entry(ACS, "ACSF_GetUserArray", 5785, 5795,
                    ["AcsHotCallFuncBehaviorTests.UserArrayIndexRoundTrip"]),
        range_entry(ACS, "ACSF_CheckActorClass", 5802, 5807,
                    ["AcsCallFunctionTests.CallFunc_CheckActorClass_MatchesImpByTid"]),
        range_entry(ACS, "ACSF_GetActorClass", 5809, 5814,
                    ["AcsCallFunctionTests.CallFunc_GetActorClass_ReturnsDoomImpForTid"]),
        range_entry(ACS, "ACSF_SoundSequenceOnActor", 5816, 5841,
                    ["AcsSoundSequenceCallFuncTests.SoundSequenceOnActorSetsActivatorName"]),
        range_entry(ACS, "ACSF_SoundSequenceOnSector", 5843, 5858,
                    ["AcsSoundSequenceCallFuncTests.SoundSequenceOnSectorTagsAllMatchingSectors"]),
        range_entry(ACS, "ACSF_SoundSequenceOnPolyobj", 5860, 5873,
                    ["AcsSoundSequenceCallFuncTests.SoundSequenceOnPolyobjStoresSequenceName"]),
        range_entry(ACS, "ACSF_GetPolyobjX", 5875, 5884,
                    ["AcsCallFunctionTests.CallFunc_GetPolyobjX_ReturnsFixedMaxWithoutPolyobjects"]),
        range_entry(ACS, "ACSF_GetPolyobjY", 5886, 5895,
                    ["AcsCallFunctionTests.CallFunc_GetPolyobjX_ReturnsFixedMaxWithoutPolyobjects"]),
        range_entry(ACS, "ACSF_CheckSight", 5897, 5941,
                    ["AcsCallFunctionTests.CallFunc_CheckSight_ActivatorSeesSelf"]),
        range_entry(ACS, "ACSF_SpawnForced", 5943, 5945,
                    ["AcsHotCallFuncBehaviorTests.SpawnForcedCreatesActorAtFixedCoordinates"]),
        range_entry(ACS, "ACSF_ACS_NamedExecute", 5947, 5963,
                    ["AcsAbsentCallFuncChunk11Tests.CallFunc_AcsNamedExecute_StartsScriptByName"]),
        range_entry(ACS, "ACSF_UniqueTID", 5967, 5968,
                    ["AcsCallFunctionTests.CallFunc_UniqueTid_RandomStartFindsUnusedTid"]),
        range_entry(ACS, "ACSF_IsTIDUsed", 5970, 5972,
                    ["AcsCallFunctionTests.CallFunc_IsTidUsed_DetectsCoopPlayerTid"]),
        range_entry(ACS, "ACSF_Sqrt", 5974, 5976,
                    ["AcsCallFunctionTests.CallFunc_Sqrt_ReturnsIntegerRoot"]),
        range_entry(ACS, "ACSF_FixedSqrt", 5978, 5980,
                    ["AcsCallFunctionTests.CallFunc_FixedSqrt_ReturnsFixedPointRoot"]),
        range_entry(ACS, "ACSF_VectorLength", 5982, 5984,
                    ["AcsFixedRoundingTests.VectorLengthRoundsFractionalFixedResults"]),
        range_entry(ACS, "ACSF_SetHUDClipRect", 5986, 5993,
                    ["AcsAbsentCallFuncChunk11Tests.CallFunc_SetHudClipRect_StoresOnSimulation"]),
        range_entry(ACS, "ACSF_SetCVar", 6003, 6005,
                    ["AcsModCvarServiceTests.CallFunc_SetCVar_WritesIntegerModCvar"]),
        range_entry(ACS, "ACSF_SetCVarString", 6007, 6009,
                    ["AcsModCvarServiceTests.CallFunc_SetCVarString_StoresText"]),
        range_entry(ACS, "ACSF_SetUserCVar", 6029, 6031,
                    ["AcsModCvarServiceTests.CallFunc_SetUserCVar_MirrorsConsolePlayerGlobal"]),
    ],
}
chunk66["nativeLoc"] = sum(r["end"] - r["start"] + 1 for r in chunk66["nativeRanges"])

path = Path(__file__).resolve().parents[1] / "docs/data/loc_tally.json"
tally = json.loads(path.read_text())
for i, ch in enumerate(tally["chunks"]):
    if ch["name"] == chunk65["name"]:
        tally["chunks"][i] = chunk65
    if ch["name"] == chunk66["name"]:
        tally["chunks"][i] = chunk66
path.write_text(json.dumps(tally, indent=2) + "\n")
print("chunk65", chunk65["nativeLoc"], "chunk66", chunk66["nativeLoc"])
