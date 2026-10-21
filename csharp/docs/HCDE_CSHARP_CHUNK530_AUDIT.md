# Chunk 530 — change count flags, raise actions, refire, set angle/pitch/roll

## Native ranges

Whole-function slices from `p_actionfunctions.cpp` (2767–3028): `A_ChangeFlag`, `CheckFlag`, `A_ChangeCountFlags`, raise family, `A_MonsterRefire`, `A_SetAngle` / `A_SetPitch` / `A_SetRoll`.

**Credited native LOC:** 170 (tally `nativeLoc`).

## Managed

`VmActionRaiseAndCountFlags.cs` — src-only credit.

## Seams

`ModActorFlag`, `P_Thing_Raise`, `P_CheckSight`, and hosted angle/pitch/roll setters are not executed.
