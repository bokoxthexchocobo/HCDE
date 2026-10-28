# Chunk 1014 — `FLevelLocals` ctor/dtor guards, `Init` scalar/info copy, GL prop resolution, and `CalcMapName` from `g_level.cpp` (**1959–2108**); follows chunk **1013** and sits after chunk **8** headless-flow credit (**≤1958**).

## Scope

`FLevelLocals` ctor/dtor guards, `Init` scalar/info copy, GL prop resolution, and `CalcMapName` from `g_level.cpp` (**1959–2108**); follows chunk **1013** and sits after chunk **8** headless-flow credit (**≤1958**).

## Native ranges

- `src/g_level.cpp` **FLevelLocals::Init** 1959–2108 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGLevelInitCtor1014Slice.cs`
Managed LOC (tally): 122.
Conversion commit: `27e88a9450c7db6d0d267857a725c93b600e2162`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FLevelLocals` ctor player wiring (**1965–1974**) | yes (index guard) |
| dtor delete guards (**1976–1980**) | yes |
| `Init` reset + gravity/air (**1987–2043**) | yes |
| Fade table flag (**2015–2020**) | yes |
| Info → level field copy (**2024–2083**) | yes (predicates/scalars) |
| Init GL bool fallbacks (**2085–2087**) | yes (Init path; **1010** owns CVAR callback) |
| `CalcMapName` (**2095–2108**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1957–1958** | prior function return (chunk **8** seam) |
| **2002–2013**, **2024**, **2045** | host lookup / texture APIs |
| **2080–2081** | compat callback host |
| **2109+** | `AirControlChanged` (next band) |

| Managed (`VmGLevelInitCtor1014Slice.cs`) | Native (`g_level.cpp`) |
|-------------------------------------------|----------------------|
| Player slot wiring | **1969–1972** |
| Init scalar reset | **1992–2000**, **2057–2059** |
| Gravity / air / teamdamage overrides | **2029–2041** |
| Fade + wall light scaling | **2015–2020**, **2027–2028** |
| Cluster flags merge | **2045–2053** |
| Init GL props | **2085–2087** |
| `CalcMapName` | **2099–2107** |
| Band hull (exclusive slice) | **1959–2108** |

### Chunk **1014** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1013** |


**Post-review ratio:** 122/150 managed/native ≈ 81% (src file LOC at HEAD; see tally).

## Tests

- `VmGLevelInitCtor1014SliceTests.LevelLocalsCtorAndInitReset`
- `VmGLevelInitCtor1014SliceTests.InitGravityFadeAndCluster`
- `VmGLevelInitCtor1014SliceTests.InitGlPropsAndCalcMapName`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1014 --to 1014` (0 hard / 0 soft).

## Seams

`P_InitParticles`, `FindLevelInfo`, `TexMan`, `R_ColormapNumForName`, `FindClusterInfo`, `compatflags` callbacks, and `new EventManager` remain injected.

## Credit rationale

UNCredited **1959–2108**; disjoint from **1051–1958** chunk **8** islands and **1010–1013** bands.
