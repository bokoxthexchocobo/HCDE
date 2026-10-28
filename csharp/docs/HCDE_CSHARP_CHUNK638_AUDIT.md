# Chunk 638 — Bounds-checked protocol stream I/O (`i_protocol.cpp` 175–389)

## Scope

Bounds-checked protocol stream I/O (`i_protocol.cpp` 175–389)

## Native ranges

- `src/common/engine/i_protocol.cpp` **ReadString** 175–389 (215 lines)

**Credited native LOC:** 215 (tally `nativeLoc` 215).

## Managed

`VmIProtocolChecked.cs`
Managed LOC (tally): 166.
Conversion commit: `7ebfde7868a2c014ebd81d2007c4e004ae099902`.

## Padding review (2026-10-21)

| Managed | Native (`i_protocol.cpp`) |
|---------|-------------------|
| `ReadStringNative` … `ReadDoubleNative` | 175–277 |
| `WriteStringNative` … `WriteDoubleNative` | 279–389 |

**Post-review ratio:** 166/215 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmIProtocolStreamTests.ReadStringNative_Checked`
- `VmIProtocolStreamTests.ReadInt32Native_Checked`
- `VmIProtocolStreamTests.WriteInt16Native_Checked`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 638 --to 638` (0 hard / 0 soft).

## Seams

`I_Error` is injected as optional `fatal` callbacks. `ProtocolStream` is uncredited on `VmIProtocolChecked.cs`.

## Credit rationale

Checked read/write family through `WriteDouble`.
