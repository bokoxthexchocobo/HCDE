# Chunk 541 — HCDE DEM naming, event-record conversion loop, canonical string + legacy weapon/CVar append

## Scope

HCDE DEM naming, event-record conversion loop, canonical string + legacy weapon/CVar append

## Native ranges

- `src/d_net.cpp` **HCDEDemCommandName** 3902–3923 (22 lines)
- `src/d_net.cpp` **HCDEAppendEventRecords** 3932–4026 (95 lines)
- `src/d_net.cpp` **HCDEReadCanonicalString** 4028–4051 (24 lines)
- `src/d_net.cpp` **HCDEAppendLegacyWeaponIndex** 4062–4072 (11 lines)
- `src/d_net.cpp` **HCDEAppendLegacyCVarChange** 4074–4109 (36 lines)

**Credited native LOC:** 188 (tally `nativeLoc` 188).

## Managed

`VmDNetAppendEventRecords.cs`, `VmDNetAppendEventRecordsTests.cs`  
Managed LOC (tally): see `fix_chunk_tally.py` after trim (whole file maps to credited functions only).

## Method-to-native map

| Managed member | Native source |
|----------------|---------------|
| `DemCommandName` | `HCDEDemCommandName` 3902–3923 |
| `AppendEventRecords`, `ValidateAppendPreconditions`, `EventCountOverflow`, `ShouldDropClientInputEvent`, `RewindToEventStart`, `PayloadTooLarge`, `WriteEventCountBE16`, `WritePayloadSizeBE16`, `TryReadByte`, `TryAppendOutputByte` | `HCDEAppendEventRecords` 3932–4026 |
| `TryReadCanonicalString` | `HCDEReadCanonicalString` 4028–4051 |
| `TryAppendLegacyWeaponIndex` | `HCDEAppendLegacyWeaponIndex` 4062–4072 |
| `TryAppendLegacyCVarChange`, `LegacyCVarHeaderByte`, `SinChangedXorEvent`, `TryCopyFieldBytes`, `TryAppendLegacyStringFromCanonical` | `HCDEAppendLegacyCVarChange` 4074–4109 |

## Tests

- `VmDNetAppendEventRecordsTests.DemCommandName_KnownPause`
- `VmDNetAppendEventRecordsTests.AppendEventRecords_*` (loop paths)
- `VmDNetAppendEventRecordsTests.TryReadCanonicalString_RoundTrip`
- `VmDNetAppendEventRecordsTests.TryAppendLegacyWeaponIndex_TwoByteForm`
- `VmDNetAppendEventRecordsTests.TryAppendLegacyCVarChange_BoolPayload`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 541 --to 541` (0 hard / 0 soft).

## Seams

`HCDEAppendCanonicalEventPayload`, `HCDEReadByteField`, `HCDEAppendByte`, and live `HCDELiveWriteBE16` buffer mutation are injected at seams; debug trace logging is not hosted.

## Port fidelity review

The loop port mirrors native control flow (preconditions, per-event header, payload size BE16, client-input rewind, final count write). Uncredited debug-only helpers (`DebugPackLabel`, orphan precondition wrappers) were removed so `managedSourceFiles` LOC tracks credited functions only—not padding.
