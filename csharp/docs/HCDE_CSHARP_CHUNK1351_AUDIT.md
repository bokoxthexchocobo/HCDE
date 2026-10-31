# Chunk 1351 — Local echo dump and the self-test harness from d_net_diag.cpp (813-1075).

## Scope

Local echo dump and the self-test harness from d_net_diag.cpp (813-1075).

## Native ranges

- `src/d_net_diag.cpp` **net_echo_dump** 813–1075 (263 lines)

**Credited native LOC:** 263 (tally `nativeLoc` 263).

## Managed

`VmNetDiag1351Slice.cs`
Managed LOC (tally): 273.
Conversion commit: `6b2e534dba2dffd2ad5c750b15b82aa5eea62908`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Local echo dump (**813–866**) | yes |
| Deterministic self-test inputs (**868–925**) | yes |
| Windows harness and trace verdict (**927–1075**) | yes |
| Band hull (**813–1075**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| CreateProcess and Sleep | the host records the command, the error, and the wait |
| Trace files | the host supplies the opened lines |
| FName from a state string | the index for that string is supplied by the caller |

A console slot outside 0 through 63 prints nothing. A null weapon index stays 0, and a valid 0 prints the table entry. WP_NOCHANGE and a name index of -1 both become the all-ones pending index, which prints WP_NOCHANGE. A ready weapon with that index prints None. The weapon sprite walk stops at the first PSP_WEAPON id, including a null state. Tics are stored as int16. State-flag text is not part of this dump. The self-test cvar of 0 leaves the command untouched, and any other value clears it first. Negative tics still take the forward phase because the remainder keeps its sign. The non-Windows branch prints the platform line and returns. A missing module path returns before the log delete. Logs are deleted before the server spawn. A client spawn failure kills only the server. Zero client events suppress the desync verdict. Server lines do not count desyncs. A line can increment both an event and a desync.

| Managed (VmNetDiag1351Slice.cs) | Native (d_net_diag.cpp) |
|---------------------------------|-------------------------|
| Echo dump, inputs, and harness | **813–1075** |
| Band hull (exclusive slice) | **813–1075** |

### Chunk **1351** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **263** |
| **Net headline** | **+263** vs the prior chunk |


**Post-review ratio:** 273/263 managed/native ≈ 104% (src file LOC at HEAD; see tally).

## Tests

- `VmNetDiag1351SliceTests.EchoDumpAndPhases`
- `VmNetDiag1351SliceTests.SelfTestHarness`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1351 --to 1351` (0 hard / 0 soft).

## Seams

The module path, process spawn, sleep, and trace-log bytes stay on the host. FName lookup for a sprite state string stays injected. The name table resolves indexes.

## Credit rationale

UNCredited d_net_diag.cpp 813-1075 (+263). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of i_specialpaths.cpp 1-262.
