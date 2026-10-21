# Chunk 650 — HCDE tic/client-input allow-lists and canonical string helpers (`d_net.cpp` 3512–3666)

## Scope

HCDE tic/client-input allow-lists and canonical string helpers (`d_net.cpp` 3512–3666)

## Native ranges

- `src/d_net.cpp` **HCDEIsAllowedTicEventType** 3512–3585 (74 lines)
- `src/d_net.cpp` **HCDEIsAllowedClientInputEventType** 3587–3640 (54 lines)
- `src/d_net.cpp` **HCDEAppendCanonicalString** 3642–3648 (7 lines)
- `src/d_net.cpp` **HCDEAppendCanonicalNullString** 3651–3666 (16 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`DemoCommand.cs`
Managed LOC (tally): 124.
Conversion commit: `d611bbf7f8b68dc9befb79a5162ec4d249ca30e4`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `IsAllowedTicEvent` | `HCDEIsAllowedTicEventType` 3512–3585 |
| `IsAllowedClientInput` | `HCDEIsAllowedClientInputEventType` 3587–3640 |
| `HCDEAppendCanonicalString` | 3642–3648 |
| `HCDEAppendCanonicalNullString` | 3651–3666 |


**Post-review ratio:** 124/151 managed/native ≈ 82% (src file LOC at HEAD; see tally).

## Tests

- `DemoCommandPolicyTests.IsAllowedTicEvent_RejectsUnknownOpcode`
- `DemoCommandPolicyTests.IsAllowedTicEvent_AllowsSayAndTaunt`
- `DemoCommandPolicyTests.IsAllowedClientInput_BlocksServerOnlyChangemap`
- `DemoCommandPolicyTests.IsAllowedClientInput_AllowsGiveCheatOnWire`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 650 --to 650` (0 hard / 0 soft).

## Seams

`HCDEAppendBytes` and live buffer append are injected; allow-list mirrors `EDemoCommand` constants only.

## Credit rationale

Whole `HCDEIsAllowedTicEventType`, `HCDEIsAllowedClientInputEventType`, and `HCDEAppendCanonicalNullString` bodies.
