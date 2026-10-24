# Chunk 637 — Unchecked protocol stream primitives (`i_protocol.cpp` 32–181)

## Scope

Unchecked protocol stream primitives (`i_protocol.cpp` 32–181)

## Native ranges

- `src/common/engine/i_protocol.cpp` **UncheckedReadString** 32–181 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmIProtocolUnchecked.cs`
Managed LOC (tally): 98.
Conversion commit: `f5fac074fc2de99ae3632c74bbe8e7662925c1a3`.

## Padding review (2026-10-21)

| Managed | Native (`i_protocol.cpp`) |
|---------|-------------------|
| `UncheckedReadStringNative` … `UncheckedReadDoubleNative` | 32–96 |
| `UncheckedWriteStringNative` … `UncheckedWriteDoubleNative` | 98–165 |
| `AdvanceStreamNative` | `AdvanceStream` 167–171 |

**Post-review ratio:** 98/150 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmIProtocolStreamTests.UncheckedReadStringNative_NullTerminated`
- `VmIProtocolStreamTests.UncheckedReadInt16Native_BigEndian`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 637 --to 637` (0 hard / 0 soft).

## Seams

No heap allocation seams; string copy behavior is inlined.

## Credit rationale

Whole unchecked read/write helpers and `AdvanceStream`.
