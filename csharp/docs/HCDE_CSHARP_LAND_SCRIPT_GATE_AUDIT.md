# Land script gate retroactive audit

Retroactive verification after adding `check_port_coverage.sh <N> <port-sha>` to `land_chunk_fast.sh` and `land_one_chunk.sh`.

## Method

For each chunk **N** in range **450–509**:

1. Read `conversionManagedCommit` (+ `portCommits` from `chunkN.json` for measured managed LOC).
2. `bash csharp/tools/check_port_coverage.sh N <port-sha>` — requires ≥50% `csharp/src` additions on the port SHA(s).
3. `python3 csharp/tools/reaudit_chunks.py audit --from N --to N` — **0 hard / 0 soft**.

Chunks **471–504** were landed this session via the fast/one-chunk scripts before the coverage hook was restored.

## Repairs (no gate loosening)

| Chunk | Issue | Fix |
|-------|--------|-----|
| **451** | `managedLoc` mismatch (missing deepen in `portCommits`) | Added deepen SHA `8c575da2…` to `chunk451.json` |
| **452** | Wrong `conversionManagedCommit` (pointed at chunk 453 tally commit) | Set port SHA `f2571f30…` |
| **507** | `A_Pain` / `A_Wander` overlapped prior chunks | Reworked to chase/face/drop-item band (2268–3215) |

## Per-chunk results

| # | Chunk | Port SHA | Native | Managed | Coverage | Reaudit |
|---|-------|----------|--------|---------|----------|---------|
| 451 | d-main-cleanup-restart-chunk451 | `8613014b09f5…` | 154 | 124 | PASS | PASS |
| 452 | p-map-get-friction-chunk452 | `f2571f3076f1…` | 150 | 94 | PASS | PASS |
| 453 | p-map-pit-check-line-chunk453 | `993a264ccc49…` | 278 | 160 | PASS | PASS |
| 454 | p-map-portal-missile-chunk454 | `533cb7993a84…` | 168 | 140 | PASS | PASS |
| 455 | p-map-pit-check-thing-chunk455 | `f3e324ce384f…` | 289 | 194 | PASS | PASS |
| 456 | p-map-missile-slope-chunk456 | `2969a2b005c5…` | 182 | 119 | PASS | PASS |
| 457 | p-map-test-mobj-chunk457 | `2baa42190e93…` | 181 | 133 | PASS | PASS |
| 458 | p-interaction-touch-obit-chunk458 | `a9eaca080f38…` | 219 | 118 | PASS | PASS |
| 459 | p-enemy-valid-look-chunk459 | `0bea479f8d62…` | 152 | 201 | PASS | PASS |
| 460 | p-mobj-bounce-seek-chunk460 | `293cef067604…` | 260 | 177 | PASS | PASS |
| 461 | p-map-bounce-reflect-chunk461 | `17056b38cd40…` | 170 | 107 | PASS | PASS |
| 462 | p-interaction-poison-switch-chunk462 | `f4b7b73256bf…` | 199 | 104 | PASS | PASS |
| 463 | p-enemy-look-wander-chunk463 | `a5543a5137d0…` | 151 | 114 | PASS | PASS |
| 464 | p-mobj-hash-missile-chunk464 | `ef628f22f269…` | 160 | 113 | PASS | PASS |
| 465 | p-map-aim-lineattack-chunk465 | `ef628f22f269…` | 159 | 108 | PASS | PASS |
| 466 | p-mobj-slam-reflect-chunk466 | `0252d7300a88…` | 170 | 186 | PASS | PASS |
| 467 | p-mobj-shade-pitch-chunk467 | `4d6fead63aea…` | 172 | 126 | PASS | PASS |
| 468 | p-interaction-damage-early-chunk468 | `4d6fead63aea…` | 183 | 100 | PASS | PASS |
| 469 | p-enemy-lookex-chunk469 | `5e5d6d3ce3b4…` | 158 | 163 | PASS | PASS |
| 470 | p-map-bounce-traverse-chunk470 | `6229ab63a94a…` | 190 | 104 | PASS | PASS |
| 471 | p-enemy-resurrect-chunk471 | `17ba0115dab2…` | 180 | 196 | PASS | PASS |
| 472 | p-mobj-inventory-portal-chunk472 | `b090291c396d…` | 164 | 123 | PASS | PASS |
| 473 | p-interaction-virtual-damage-chunk473 | `8389c958b4db…` | 161 | 123 | PASS | PASS |
| 474 | p-interaction-damage-late-chunk474 | `bee34c3ecd64…` | 251 | 239 | PASS | PASS |
| 475 | p-map-aim-traverse-chunk475 | `e262efe19e12…` | 249 | 176 | PASS | PASS |
| 476 | p-mobj-calc-bones-chunk476 | `d507e8992087…` | 163 | 122 | PASS | PASS |
| 477 | p-map-aim-traverse-main-chunk477 | `b7e803711d3a…` | 270 | 301 | PASS | PASS |
| 478 | p-mobj-bone-world-tick-chunk478 | `78fa1fea7127…` | 185 | 234 | PASS | PASS |
| 479 | p-mobj-water-spawn-construct-chunk479 | `e797ed16bb6d…` | 224 | 153 | PASS | PASS |
| 480 | p-map-line-trace-chunk480 | `996b2d683031…` | 163 | 147 | PASS | PASS |
| 481 | p-mobj-static-spawn-chunk481 | `bb41c30c91ad…` | 189 | 124 | PASS | PASS |
| 482 | p-map-trace-bleed-chunk482 | `e4dc00b5654a…` | 277 | 185 | PASS | PASS |
| 483 | p-mobj-postbegin-destroy-chunk483 | `b90c0f2d7022…` | 152 | 89 | PASS | PASS |
| 484 | p-map-rail-damage-talk-chunk484 | `72c9ce6ea995…` | 177 | 92 | PASS | PASS |
| 485 | p-mobj-floorclip-morph-chunk485 | `c04bb549c102…` | 155 | 104 | PASS | PASS |
| 486 | p-mobj-morph-spawn-chunk486 | `67070a6ea790…` | 253 | 172 | PASS | PASS |
| 487 | p-mobj-spawnmapthing-bands-chunk487 | `b96a006da667…` | 161 | 137 | PASS | PASS |
| 488 | p-mobj-spawnmapthing-compose-chunk488 | `f56f49a695e2…` | 164 | 100 | PASS | PASS |
| 489 | p-mobj-spawnpuff-blood-chunk489 | `d3dd612a2fe8…` | 311 | 231 | PASS | PASS |
| 491 | p-mobj-hitwater-chunk491 | `6b43ad9c14f0…` | 173 | 120 | PASS | PASS |
| 492 | p-map-offset-talk-puzzle-chunk492 | `33556db8fca9…` | 159 | 92 | PASS | PASS |
| 493 | p-mobj-hitfloor-spawn-chunk493 | `ba9447ff0353…` | 163 | 95 | PASS | PASS |
| 494 | p-mobj-spawn-missile-variants-chunk494 | `f3826f8026cb…` | 156 | 94 | PASS | PASS |
| 495 | p-map-sector-intersectors-chunk495 | `c381772aa64c…` | 187 | 107 | PASS | PASS |
| 496 | p-map-sector-push-chunk496 | `0625a7c98aca…` | 162 | 137 | PASS | PASS |
| 497 | p-map-pit-floor-ceiling-chunk497 | `54a7aec8268d…` | 161 | 102 | PASS | PASS |
| 498 | p-map-change-sector-chunk498 | `117ff441899c…` | 155 | 102 | PASS | PASS |
| 499 | p-map-crush-tail-decal-activate-chunk499 | `5c2d461f354c…` | 205 | 128 | PASS | PASS |
| 500 | p-mobj-team-friend-damage-chunk500 | `65203c4c98b0…` | 180 | 212 | PASS | PASS |
| 501 | p-mobj-crash-revive-chunk501 | `05bfebbe295f…` | 176 | 116 | PASS | PASS |
| 502 | p-mobj-tag-restore-misc-chunk502 | `8371183403ac…` | 229 | 224 | PASS | PASS |
| 503 | p-enemy-look-targets-chunk503 | `d54e1685429d…` | 195 | 215 | PASS | PASS |
| 504 | p-interaction-react-pain-chunk504 | `033d415fe7f8…` | 199 | 172 | PASS | PASS |
| 505 | p-interaction-actor-die-morph-chunk505 | `ff7ef8fa221d…` | 292 | 149 | PASS | PASS |
| 506 | p-interaction-actor-die-states-chunk506 | `96e267fbd6d3…` | 194 | 110 | PASS | PASS |
| 507 | p-enemy-wander-face-chunk507 | `8aa1d7e21a72…` | 157 | 88 | PASS | PASS |

## Aggregate 450–507

```text

NOTE: p-spec-remainder-closure-chunk450 in chunk450.json but not in loc_tally.json

Hard failures: 0, soft warnings: 0
```

## `AActor::Die` split (505–506)

| Chunk | Native lines | Split boundary |
|-------|----------------|----------------|
| **505** | 329–620 | Morph, missile explode, kill/frag/spree through non-player kill credit |
| **506** | 622–817 | Player death, diestate resolution, `Die`/`CallDie` action glue |

Documented in each chunk `auditRationale` and `HCDE_CSHARP_CHUNK505_AUDIT.md` / `CHUNK506_AUDIT.md`.
