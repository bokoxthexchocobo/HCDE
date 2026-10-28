# p_floor / ceiling waggle, donut, elevator — chunk 6 audit

## Scope

| Native | Lines | Managed |
| --- | --- | --- |
| `EV_StartWaggle` / `DWaggleBase` | 1163–1353 | `WaggleActions`, specials 138 / 38 |
| `EV_DoDonut` | 796–863 | `FloorCombinedLineSpecials.Donut` |
| `DElevator::Tick` crush rollback | 941–984 | `ElevatorLineSpecials.Tick` |

**Behavior native LOC credited:** 469.

## Tests

`WaggleActionsTests`, `FloorDonutTests`, `LineSpecialBatchTests.ElevatorRaiseToNearestMatchesNeighborFloor`.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
