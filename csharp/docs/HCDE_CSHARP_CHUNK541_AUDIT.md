# Chunk 541 — HCDEDemCommandName, HCDEAppendEventRecords, canonical string + legacy weapon/CVar append

## Scope

HCDEDemCommandName, HCDEAppendEventRecords, canonical string + legacy weapon/CVar append

## Native ranges

- `src/d_net.cpp` **HCDEDemCommandName** 3902–3923 (22 lines)
- `src/d_net.cpp` **HCDEAppendEventRecords** 3932–4026 (95 lines)
- `src/d_net.cpp` **HCDEReadCanonicalString** 4028–4051 (24 lines)
- `src/d_net.cpp` **HCDEAppendLegacyWeaponIndex** 4062–4072 (11 lines)
- `src/d_net.cpp` **HCDEAppendLegacyCVarChange** 4074–4109 (36 lines)

**Credited native LOC:** 188 (tally `nativeLoc` 188).

## Managed

`VmDNetAppendEventRecords.cs`, `VmDNetAppendEventRecordsTests.cs`
Managed LOC (tally): 117.
Conversion commit: `1a2446f18c503e5c3e67b203f419826276c7dd90`.

## Tests

- `VmDNetAppendEventRecordsTests.DemCommandName_KnownPause`
- `VmDNetAppendEventRecordsTests.ShouldDropClientInputEvent_WhenNotAllowed`
- `VmDNetAppendEventRecordsTests.TryReadCanonicalString_RoundTrip`
- `VmDNetAppendEventRecordsTests.TryAppendLegacyWeaponIndex_TwoByteForm`
- `VmDNetAppendEventRecordsTests.LegacyCVarHeader_EncodesTypeInHighBits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 541 --to 541` (0 hard / 0 soft).

## Seams

`HCDEAppendCanonicalEventPayload`, `HCDEReadByteField`, and live buffer writes are not hosted.

## Credit rationale

Credits DEM command naming, event-record packing loop, client-input filter rewind, and legacy CVar/weapon/string append helpers from `d_net.cpp` (3902–4109).
