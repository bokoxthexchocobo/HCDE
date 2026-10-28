#!/usr/bin/env python3
import json
from pathlib import Path

path = Path(__file__).resolve().parents[1] / "docs/data/loc_tally.json"
tally = json.loads(path.read_text())
file = "src/common/scripting/backend/codegen.cpp"
sym = "FxMemberFunctionCall::Resolve"

chunk71 = {
    "name": "zcc-member-function-call-chunk71",
    "bucket": "conversion",
    "managedLoc": 52,
    "note": "FxMemberFunctionCall::Resolve prologue: class context, null args, static type names.",
    "nativeRanges": [
        {"file": file, "symbol": sym, "start": 8875, "end": 8881,
         "tests": ["ZccMemberFunctionCallResolveTests.RequiresEnclosingClass_RejectsGlobalContext"]},
        {"file": file, "symbol": sym, "start": 8883, "end": 8890,
         "tests": ["ZccMemberFunctionCallResolveTests.TryValidateNonNullArguments_RejectsEmptySlots"]},
        {"file": file, "symbol": sym, "start": 8899, "end": 8902,
         "tests": ["ZccMemberFunctionCallResolveTests.TryMapStaticTypeName_MatchesNativeSubstitutions"]},
        {"file": file, "symbol": sym, "start": 8915, "end": 8919,
         "tests": ["ZccMemberFunctionCallResolveTests.ConstantContextCallError_MatchesNativeMessage"]},
    ],
    "conversionManagedCommit": "7eed8e8c83f4a80a8374d359c6426fc301ddb038",
}
chunk72 = {
    "name": "zcc-member-function-call-chunk72",
    "bucket": "conversion",
    "managedLoc": 18,
    "note": "FxMemberFunctionCall readonly const pointer and zero-arg sound/texture builtins.",
    "nativeRanges": [
        {"file": file, "symbol": sym, "start": 8949, "end": 8952,
         "tests": ["ZccMemberFunctionCallResolveTests.ShouldMarkReadonlyConstPointer_Requires412"]},
        {"file": file, "symbol": sym, "start": 8998, "end": 9003,
         "tests": ["ZccMemberFunctionCallResolveTests.TryValidateSoundIsValid_RejectsExtraArgs"]},
        {"file": file, "symbol": sym, "start": 9017, "end": 9022,
         "tests": ["ZccMemberFunctionCallResolveTests.TryValidateZeroArgBuiltin_RejectsTextureIsNullArity"]},
    ],
    "conversionManagedCommit": "b39845c821fbebfd17d1e90e8f676758cc7ec5cf",
}
for c in (chunk71, chunk72):
    c["nativeLoc"] = sum(r["end"] - r["start"] + 1 for r in c["nativeRanges"])

for i, ch in enumerate(tally["chunks"]):
    if ch["name"] == chunk71["name"]:
        tally["chunks"][i] = chunk71
    if ch["name"] == chunk72["name"]:
        tally["chunks"][i] = chunk72
path.write_text(json.dumps(tally, indent=2) + "\n")
print("chunk71", chunk71["nativeLoc"], "chunk72", chunk72["nativeLoc"])
