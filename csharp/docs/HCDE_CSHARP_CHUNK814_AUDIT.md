# Chunk 814 — Whole `P_LineAttack` puff-type overload, `CheckLineTrace`, `P_LineTrace`, and `P_LinePickActor` from `p_map.cpp` (5052–5237). Retired (2026-10-23): out-of-body credit removed; honest **48** < **150**.

## Scope

Whole `P_LineAttack` puff-type overload, `CheckLineTrace`, `P_LineTrace`, and `P_LinePickActor` from `p_map.cpp` (5052–5237). Retired (2026-10-23): out-of-body credit removed; honest **48** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `8541f205fea1f1ab8b84be9de2e6ce77bad3b629`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|----------------------|
| `P_LineAttack` (FName puff) | **5052–5067** |
| `CheckLineTrace` | **5083–5100** |
| `P_LineTrace` | **5102–5200** |
| `P_LinePickActor` | **5208–5237** |

### Out-of-body uncredit (814)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **163** |
| **Out-of-body removed** | **115** |
| **Honest** | **48** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 814 --to 814` (0 hard / 0 soft).

## Seams

`Trace`, `PClass::FindActor`, and sector queries are injected.

## Credit rationale

First tally credit for ACS/trace helpers after chunk710 class `P_LineAttack`; retires recognition-only chunk480 row.
