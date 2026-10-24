# Chunk 650 — HCDE allow-lists and canonical append primitives (`d_net.cpp` 3512–3684).

## Scope

HCDE allow-lists and canonical append primitives (`d_net.cpp` 3512–3684).

## Native ranges

- `src/d_net.cpp` **HCDEIsAllowedTicEventType** 3512–3585 (74 lines)
- `src/d_net.cpp` **HCDEIsAllowedClientInputEventType** 3587–3640 (54 lines)
- `src/d_net.cpp` **HCDEAppendCanonicalString** 3642–3648 (7 lines)
- `src/d_net.cpp` **HCDEAppendCanonicalNullString** 3651–3666 (16 lines)
- `src/d_net.cpp` **HCDEAppendCanonicalWeaponIndex** 3668–3684 (17 lines)

**Credited native LOC:** 168 (tally `nativeLoc` 168).

## Managed

`DemoCommand.cs`, `VmDNetCanonicalPrimitive.cs`
Managed LOC (tally): 188.
Conversion commit: `4b76b7b4a75b372857afa3b297711db66b24f1b9`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `IsAllowedTicEvent` | `HCDEIsAllowedTicEventType` 3512–3585 |
| `IsAllowedClientInput` | `HCDEIsAllowedClientInputEventType` 3587–3640 |
| `TryAppendCanonicalString` | `HCDEAppendCanonicalString` 3642–3648 |
| `TryAppendCanonicalNullString` | `HCDEAppendCanonicalNullString` 3651–3666 |
| `TryAppendCanonicalWeaponIndex` | `HCDEAppendCanonicalWeaponIndex` 3668–3684 |


**Post-review ratio:** 188/168 managed/native ≈ 112% (src file LOC at HEAD; see tally).

## Tests

- `DemoCommandPolicyTests.IsAllowedTicEvent_RejectsUnknownOpcode`
- `DemoCommandPolicyTests.IsAllowedTicEvent_AllowsSayAndTaunt`
- `DemoCommandPolicyTests.IsAllowedClientInput_BlocksServerOnlyChangemap`
- `DemoCommandPolicyTests.IsAllowedClientInput_AllowsGiveCheatOnWire`
- `VmDNetCanonicalPrimitiveTests.TryAppendCanonicalString_RoundTrip`
- `VmDNetCanonicalPrimitiveTests.TryAppendCanonicalNullString_ReadsTerminator`
- `VmDNetCanonicalPrimitiveTests.TryAppendCanonicalWeaponIndex_TwoByteIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 650 --to 650` (0 hard / 0 soft).

## Seams

`HCDEAppendBytes` and live buffer append stay injected; allow-lists mirror `EDemoCommand` constants.

## Credit rationale

Whole `HCDEIsAllowedTicEventType`, `HCDEIsAllowedClientInputEventType`, `HCDEAppendCanonicalString`, `HCDEAppendCanonicalNullString`, and `HCDEAppendCanonicalWeaponIndex` — chosen before measuring (not padded to 150).
