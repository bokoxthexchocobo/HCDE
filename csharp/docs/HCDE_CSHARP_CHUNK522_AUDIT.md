# Chunk 522 — HCDEAppendCanonicalEventPayload

## Scope

HCDEAppendCanonicalEventPayload

## Native ranges

- `src/d_net.cpp` **HCDEAppendCanonicalEventPayload** 3747–3900 (154 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`csharp/src/HCDE.Net.Core/CanonicalEventPayloadCodec.cs` (src-only credit).

Managed LOC (tally): 208.
Conversion commit: `86a1a8889ed5c848d66b05f449491fa4da1973e4`.

## Tests

- `CanonicalEventPayloadCodecTests.GiveCheat_StringPlusInt`
- `CanonicalEventPayloadCodecTests.SetSlot_CanonicalizesSlotCountAndWeaponIndices`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 522 --to 522` (0 hard / 0 soft).

## Seams

`HCDEAppendFieldBytes`, `HCDEAppendCanonicalNullString`, and `HCDEReadBE16Field` are not hosted.

## Credit rationale

Credits canonical demo-event payload translation for SAY/cheat/ZSC/slot families from `d_net.cpp` (3747–3900).

## Padding review (2026-10-21)

| Managed | Native |
|---------|--------|
| `TryBuildFromLegacy` switch + per-command helpers (`TryCanonicalizeSay`, `TryCanonicalizeZscCmd`, `TryCanonicalizeSetSlot`, …) | `HCDEAppendCanonicalEventPayload` switch cases 3747–3900 |
| `TryCopyFixedLegacy`, string codec calls | `HCDEAppendFieldBytes` / null-string helpers (seams) |

**Removed:** `PayloadsEqual` (test-only helper, not in native).

**Why managed > native (~135%):** C# expresses each `case` as `DemoCommand` enum arms and splits string-prefix families into `private static` helpers; native folds many branches inline in one function. No duplicate predicates or stub `=> true` methods remain.
