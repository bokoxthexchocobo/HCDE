# Chunk 1391 — The key-name table, the double-click threshold, and the queued-press clear and binding execute from c_bind.cpp (1-182).

## Scope

The key-name table, the double-click threshold, and the queued-press clear and binding execute from c_bind.cpp (1-182).

## Native ranges

- `src/common/console/c_bind.cpp` **KeyNames** 1–182 (182 lines)

**Credited native LOC:** 182 (tally `nativeLoc` 182).

## Managed

`VmKeyBind1391Slice.cs`
Managed LOC (tally): 136.
Conversion commit: `e280769898e34b4ab3165ff958e5ad21c59656c7`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Key name table (**43–137**) | yes |
| Double-click cvar and binding globals (**139–156**) | yes |
| Queued-press clear and binding execute (**158–181**) | yes |
| Band hull (**1–182**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| C_TickQueuedInputs | starts at line 183, outside this hole |
| AddCommandString | callee; the slice records the command and key it would receive |

The name table has 459 entries, NUM_KEYS. Null slots stay null. The non-Apple row stores LWin and RWin. The Apple row stores Command and a null in those two slots. An empty binding returns false and does not issue a command. A key-up of a binding that does not start with + returns false. A key-up of a + command rewrites the first character to - and leaves the caller's string unchanged. A key-down issues the original text, including text that does not start with +. A double click ors KEY_DBLCLICKED (0x8000) into the key. Clear drops the queued press, the queued release, the binding pointer, and the double-click deadline. It leaves the double-clicked bit and the delayed-release bit. The synthetic release delay is 1000 / TICRATE, and TICRATE is 35, so the delay is 28. The double-click threshold defaults to 250 with the archive and global-config bits.

| Managed (`VmKeyBind1391Slice.cs`) | Native (`c_bind.cpp`) |
|------------------------------------|------------------------|
| Key names, threshold, clear, and execute | **1–182** |
| Band hull (exclusive slice) | **1–182** |

### Chunk **1391** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **182** |
| **Net headline** | **+182** vs the prior chunk |


**Post-review ratio:** 136/182 managed/native ≈ 75% (src file LOC at HEAD; see tally).

## Tests

- `VmKeyBind1391SliceTests.NamesMatchScanCodesAndTheAppleRow`
- `VmKeyBind1391SliceTests.ExecFlipsPlusCommandsAndClearDropsTheQueuedPress`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1391 --to 1391` (0 hard / 0 soft).

## Seams

C_TickQueuedInputs starts at line 183 and stays outside this band. AddCommandString is a callee. The queued tick that calls ExecBinding stays later.

## Credit rationale

UNCredited c_bind.cpp 1-182 (+182). After chunk 1390 this is the unique largest hole. s_doomsound.cpp 1-180 is 180. The band is shorter than 400 lines and is not sliced.
