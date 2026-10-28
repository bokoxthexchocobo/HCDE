# HCDE native-to-managed support matrix

Generated: 2026-10-19 17:24:35 UTC.
Regenerate: `dotnet run --project csharp/src/HCDE.ConversionInventory -- ../../`

## Headline counts

| Category | Executed | Partial | Decoded-only | Absent | Total |
| --- | ---: | ---: | ---: | ---: | ---: |
| acs_callfunc | 59 | 0 | 0 | 56 | 115 |
| acs_opcode | 211 | 0 | 113 | 25 | 349 |
| actor_class | 0 | 18 | 0 | 0 | 18 |
| line_special | 176 | 0 | 0 | 86 | 262 |
| persistence_archive | 0 | 299 | 0 | 0 | 299 |
| zscript_vm_opcode | 243 | 0 | 0 | 3 | 246 |
| **all rows** | **689** | **317** | **113** | **170** | **1289** |

## Native LOC without managed equivalent (estimate)

| Subsystem | Native path | Native LOC | Managed LOC | Managed files |
| --- | --- | ---: | ---: | ---: |
| playsim | `src/playsim` | 98201 | 48798 | 463 |
| → est. unconverted | | **49403** | | |
| maploader | `src/maploader` | 14401 | 5931 | 42 |
| → est. unconverted | | **8470** | | |
| scripting_vm | `src/common/scripting` | 57115 | 3065 | 23 |
| → est. unconverted | | **54050** | | |
| rendering | `src/rendering` | 47490 | 17614 | 207 |
| → est. unconverted | | **29876** | | |
| net | `src/d_net.cpp` | 10667 | 11076 | 83 |
| → est. unconverted | | **0** | | |
| gamedata | `src/gamedata` | 25915 | 2642 | 12 |
| → est. unconverted | | **23273** | | |

Machine-readable export: [`HCDE_NATIVE_MANAGED_MATRIX.json`](HCDE_NATIVE_MANAGED_MATRIX.json), [`HCDE_NATIVE_MANAGED_MATRIX.csv`](HCDE_NATIVE_MANAGED_MATRIX.csv).
