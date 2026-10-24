# HCDE C# gate regression audit

## 1. Branch regressions (native / managed gates)

| SHA | Date (UTC) | File(s) | Change |
|-----|------------|---------|--------|
| `9f3c4c18` | 2026-10-21 15:08:29 | `check_port_coverage.sh`, `reaudit_chunks.py` | **Weakened** native floor **150 → 40** (doc still said 150). |
| `1e0b968f` | 2026-10-21 15:28:33 | same + tally bounds on all rows | **Partial restore**: floor back to 150; added tally-row bounds (over-scoped vs doc). |
| `61e2f479` | 2026-10-20 (earlier on branch) | `reaudit_chunks.py` | **Weakened** `thin_managed_coverage`: `measured = max(commit_ml, tally_ml, json_ml)`. |
| `6f551b8d` | 2026-10-20 19:39:37 | `reaudit_chunks.py` | **Restored** strict managed measure (removed `max()` / json inflation). |
| `caafaf11` | 2026-10-21 | `land_chunk_fast.sh`, `land_one_chunk.sh` | **Strengthened**: land aborts on any reaudit `warn` (incl. thin_managed / thin_test_band). |
| `4694cbae` | 2026-10-21 12:27:53 | `check_port_coverage.sh`, `reaudit_chunks.py` | **Added** 150–400 ceiling/floor on **chunkN.json** (strict). |
| `b3ec1e4e` | 2026-10-20 23:57:08 | `chunk_port_managed.py`, `reaudit_chunks.py` | **Strengthened**: surviving HEAD managed LOC + commit inflation gate. |
| `72bad8ca` | 2026-10-21 | `reaudit_chunks.py` | **Added** soft `missing_managed_loc_justification` (>150% native). |

**Not weakened on this branch:** overlap/exclusivity (matrix tests), 50% managed ratio (`check_port_coverage.sh` hard-fails; reaudit soft + land blocks warns), test-existence checks (hard).

**Cutover (`d2a0aa20`):** `CHUNK_RULES_MIN_CHUNK = **450**` — first tranche under surviving-HEAD managed LOC (`b3ec1e4e`, 2026-10-20). Native **150–400** on **`loc_tally.nativeLoc`** is a **hard** reaudit fail for chunk numbers **≥ 450**; below 450, bounds are **`legacy_nativeLoc_*` soft** (reported, non-blocking). Land gate **150–400** on `chunkN.json` since `4694cbae`.

Regenerate: `python3 csharp/tools/reaudit_chunks.py breakdown`.

## 2. Re-audit summary (all conversion `nativeLoc > 0`, `reaudit_chunks.py` at HEAD)

- Conversion tally rows with `nativeLoc > 0`: **528**
- **Pre-rules** (chunk number **&lt; 450**): **405** rows — **152** hard events (**17 244** nativeLoc at stake), **673** soft events (**78 410** nativeLoc at stake)
- **Post-rules** (chunk number **≥ 450**): **123** rows — **0** hard events, **95** soft events (**18 778** nativeLoc at stake)
- **Totals:** **152** hard / **768** soft events (nativeLoc sums count a row once per kind, as in `breakdown`)
- Active band **548–587** (588–590 retired into 587 after padding strip): **0 hard / 0 soft**

## 4. Pre vs post-rules breakdown (exact)

### (a) Pre-rules — chunk &lt; 450 (legacy; native bounds soft)

| Kind | Events | nativeLoc at stake |
|------|--------|-------------------|
| **Hard** `chunk_json_nativeLoc_mismatch` | 32 | 856 |
| **Hard** `missing_test` | 120 | 16 388 |
| **Soft** `legacy_nativeLoc_below_floor` | 268 | 15 822 |
| **Soft** `legacy_nativeLoc_above_ceiling` | 4 | 3 202 |
| **Soft** `missing_audit_padding_review` | 209 | 21 504 |
| **Soft** `missing_managed_loc_justification` | 49 | 1 186 |
| **Soft** `thin_managed_coverage` | 18 | 4 743 |
| **Soft** `thin_test_band` | 125 | 31 953 |

### (b) Post-rules — chunk ≥ 450 (should comply; native bounds hard)

| Kind | Events | nativeLoc at stake |
|------|--------|-------------------|
| **Hard** | 0 | 0 |
| **Soft** `missing_audit_padding_review` | 93 | 18 466 |
| **Soft** `missing_managed_loc_justification` | 1 | 151 |
| **Soft** `thin_test_band` | 1 | 161 |

## 3. Per-chunk failures (hard)

| Chunk | nativeLoc | Hard kinds |
|-------|-----------|------------|
| `p-enemy-chunk1` | 305 | missing_test |
| `p-map-radius-chunk1` | 209 | missing_test |
| `p-map-radius-thrust-chunk2` | 58 | missing_test |
| `p-enemy-move-blocked-chunk2` | 21 | missing_test |
| `p-spec-sector-friction-push-chunk4` | 52 | missing_test |
| `thingdef-flag-expression-chunk20` | 69 | missing_test |
| `thingdef-property-params-chunk21` | 191 | missing_test |
| `thingdef-actor-flags-chunk22` | 211 | missing_test |
| `p-mobj-spawn-mapthing-chunk23` | 133 | missing_test |
| `p-acs-thing-count-chunk24` | 80 | missing_test |
| `d-net-full-reset-chunk25` | 91 | missing_test |
| `thingdef-parse-actor-header-chunk26` | 142 | missing_test |
| `thingdef-dispatch-script-property-chunk27` | 83 | missing_test |
| `thingdef-builtin-properties-chunk28` | 427 | missing_test |
| `p-mobj-spawn-compose-chunk29` | 99 | missing_test |
| `d-net-presentation-echo-chunk31` | 15 | missing_test |
| `thingdef-damage-chunk33` | 38 | missing_test |
| `thingdef-parse-actor-body-chunk34` | 61 | missing_test |
| `thingdef-stencil-blood-chunk35` | 5 | missing_test |
| `p-mobj-spawn-lifecycle-chunk36` | 74 | missing_test |
| `p-acs-get-actor-property-chunk38` | 83 | missing_test |
| `p-acs-fade-range-chunk39` | 71 | missing_test |
| `p-acs-set-master-chunk41` | 62 | missing_test |
| `p-acs-check-actor-property-chunk43` | 73 | missing_test |
| `p-acs-check-actor-texture-chunk44` | 31 | missing_test |
| `p-acs-swap-telefog-chunk45` | 27 | missing_test |
| `zcc-constant-fold-chunk46` | 28 | missing_test |
| `zcc-int-cast-fold-chunk48` | 2 | missing_test |
| `file-zip-chunk103` | 93 | nativeLoc_below_floor |
| `terrain-splash-friction-chunk180` | 79 | missing_test |
| `dsda-options-parse-chunk211` | 99 | nativeLoc_below_floor |
| `sprite-init-hash-chunk212` | 14 | nativeLoc_below_floor |
| `doomedmap-resolve-chunk213` | 54 | nativeLoc_below_floor |
| `doomedmap-spawnmap-chunk214` | 77 | nativeLoc_below_floor |
| `umapinfo-token-chunk215` | 58 | nativeLoc_below_floor |
| `doomfont-planner-chunk216` | 62 | nativeLoc_below_floor |
| `p-xlat-flags-chunk217` | 58 | nativeLoc_below_floor |
| `umapinfo-standard-chunk218` | 62 | nativeLoc_below_floor |
| `g-skill-mapinfo-chunk219` | 68 | nativeLoc_below_floor |
| `p-xlat-simple-chunk220` | 60 | nativeLoc_below_floor |
| `umapinfo-schema-doomed-spawn-chunk222` | 43 | nativeLoc_below_floor |
| `g-skill-globals-teaminfo-accessors-chunk228` | 57 | nativeLoc_below_floor |
| `p-terrain-keywords-init-chunk230` | 78 | nativeLoc_below_floor |
| `decallib-tail-gaps-chunk233` | 95 | nativeLoc_below_floor |
| `doomfont-gap-lines-chunk234` | 33 | nativeLoc_below_floor |
| `gi-static-schema-chunk235` | 39 | nativeLoc_below_floor |
| `gi-parsegameinfo-macros-chunk236` | 109 | nativeLoc_below_floor |
| `a-keys-check-chunk238` | 107 | nativeLoc_below_floor |
| `a-keys-lockdef-tail-chunk240` | 85 | nativeLoc_below_floor |
| `statistics-save-chunk242` | 139 | nativeLoc_below_floor |
| `statistics-level-store-chunk243` | 60 | nativeLoc_below_floor |
| `info-state-sprite-althud-chunk246` | 88 | nativeLoc_below_floor |
| `info-staticinit-replace-chunk247` | 36 | nativeLoc_below_floor |
| `info-damage-summon-chunk248` | 63 | nativeLoc_below_floor |
| `hcde-edf-utils-chunk249` | 48 | nativeLoc_below_floor |
| `hcde-edf-issues-chunk250` | 15 | nativeLoc_below_floor |
| `hcde-edf-reverb-chunk251` | 66 | nativeLoc_below_floor |
| `dehacked-mbf21-styles-chunk263` | 122 | chunk_json_nativeLoc_mismatch |
| `g-mapinfo-level-scalars-chunk264` | 65 | nativeLoc_below_floor |
| `dehacked-patchframe-chunk269` | 12 | chunk_json_nativeLoc_mismatch |
| `dehacked-patchsprite-ammo-weapon-chunk270` | 4 | chunk_json_nativeLoc_mismatch |
| `g-mapinfo-light-shadow-sky-chunk271` | 53 | chunk_json_nativeLoc_mismatch |
| `a-weapons-slot-chunk272` | 21 | chunk_json_nativeLoc_mismatch |
| `hcde-emapinfo-line-parse-chunk273` | 43 | chunk_json_nativeLoc_mismatch |
| `g-mapinfo-flag-handlers-chunk274` | 1 | chunk_json_nativeLoc_mismatch |
| `dehacked-patchweapon-ammo-chunk275` | 3 | chunk_json_nativeLoc_mismatch |
| `dehacked-patchsound-pointer-chunk276` | 25 | chunk_json_nativeLoc_mismatch |
| `g-mapinfo-flag-apply-chunk279` | 12 | chunk_json_nativeLoc_mismatch |
| `g-mapinfo-option-dispatch-chunk280` | 3 | chunk_json_nativeLoc_mismatch |
| `a-weapons-slots-chunk281` | 30 | chunk_json_nativeLoc_mismatch |
| `hcde-emapinfo-primitives-chunk283` | 8 | chunk_json_nativeLoc_mismatch |
| `hcde-emapinfo-level-chunk284` | 9 | chunk_json_nativeLoc_mismatch |
| `dehacked-patchmusic-sprite-chunk285` | 11 | chunk_json_nativeLoc_mismatch |
| `hcde-emapinfo-lump-apply-chunk286` | 6 | chunk_json_nativeLoc_mismatch |
| `dehacked-patchtext-chunk287` | 7 | chunk_json_nativeLoc_mismatch |
| `dehacked-patchstrings-chunk288` | 16 | chunk_json_nativeLoc_mismatch |
| `dehacked-sprites-include-chunk289` | 16 | chunk_json_nativeLoc_mismatch |
| `dehacked-include-paths-chunk290` | 21 | chunk_json_nativeLoc_mismatch |
| `dehacked-mbf-params-chunk292` | 5 | chunk_json_nativeLoc_mismatch |
| `dehacked-mbf-emitters-chunk293` | 3 | chunk_json_nativeLoc_mismatch |
| `hcde-emapinfo-property-tail-chunk294` | 15 | chunk_json_nativeLoc_mismatch |
| `hcde-emapinfo-document-chunk295` | 14 | chunk_json_nativeLoc_mismatch, nativeLoc_below_floor |
| `g-mapinfo-scalar-prelude-chunk296` | 9 | chunk_json_nativeLoc_mismatch |
| `g-mapinfo-episode-chunk297` | 48 | chunk_json_nativeLoc_mismatch |
| `a-weapons-setslot-chunk300` | 4 | chunk_json_nativeLoc_mismatch |
| `a-weapons-print-chunk301` | 15 | chunk_json_nativeLoc_mismatch |
| `a-weapons-addslot-chunk302` | 5 | chunk_json_nativeLoc_mismatch, nativeLoc_below_floor |
| `g-mapinfo-cmpgninf-chunk303` | 75 | nativeLoc_below_floor |
| `dehacked-dehsupp-ammo-chunk304` | 22 | nativeLoc_below_floor |
| `dehacked-dehsupp-weapon-chunk305` | 17 | nativeLoc_below_floor |
| `g-mapinfo-complvl-chunk306` | 39 | nativeLoc_below_floor |
| `dehacked-dehsupp-aliases-chunk307` | 16 | nativeLoc_below_floor |
| `g-mapinfo-lump-loop-chunk308` | 35 | nativeLoc_below_floor |
| `dehacked-dehsupp-lump-gate-chunk309` | 10 | chunk_json_nativeLoc_mismatch, nativeLoc_below_floor |
| `dehacked-dehsupp-actionlist-chunk310` | 36 | nativeLoc_below_floor |
| `dehacked-dehsupp-heights-codep-chunk311` | 22 | nativeLoc_below_floor |
| `dehacked-dehsupp-orgsprnames-chunk312` | 28 | nativeLoc_below_floor |
| `dehacked-dehsupp-map-sections-chunk313` | 69 | nativeLoc_below_floor |
| `dehacked-finish-pickup-chunk316` | 82 | nativeLoc_below_floor |
| `dehacked-finish-weapon-ammo-chunk317` | 76 | nativeLoc_below_floor |
| `g-mapinfo-parse-loop-chunk318` | 46 | nativeLoc_below_floor |
| `g-mapinfo-level-lookup-chunk319` | 42 | nativeLoc_below_floor |
| `g-mapinfo-options-core-chunk320` | 94 | nativeLoc_below_floor |
| `g-mapinfo-options-media-env-chunk321` | 23 | nativeLoc_below_floor |
| `g-mapinfo-options-pics-chunk322` | 18 | nativeLoc_below_floor |
| `g-mapinfo-options-late-chunk323` | 140 | nativeLoc_below_floor |
| `g-mapinfo-exit-environment-chunk324` | 81 | nativeLoc_below_floor |
| `hcde-emapinfo-primitives-head-chunk325` | 42 | nativeLoc_below_floor |
| `hcde-emapinfo-level-bootstrap-chunk326` | 92 | nativeLoc_below_floor |
| `hcde-emapinfo-apply-block-chunk327` | 27 | nativeLoc_below_floor |
| `g-mapinfo-script-flags-head-chunk329` | 11 | nativeLoc_below_floor |
| `g-mapinfo-flag-table-mid-chunk330` | 2 | nativeLoc_below_floor |
| `g-mapinfo-flag-apply-switch-chunk331` | 22 | nativeLoc_below_floor |
| `g-mapinfo-parse-header-chunk332` | 23 | nativeLoc_below_floor |
| `hcde-emapinfo-property-tail-chunk334` | 101 | nativeLoc_below_floor |
| `hcde-emapinfo-report-scan-chunk335` | 54 | nativeLoc_below_floor |
| `dehacked-mbf-scratch-sound-chunk336` | 25 | nativeLoc_below_floor |
| `hcde-emapinfo-document-loop-chunk338` | 56 | nativeLoc_below_floor |
| `dehacked-mbf-pointer-emitters-chunk339` | 22 | nativeLoc_below_floor |
| `am-map-cvar-head-chunk340` | 24 | nativeLoc_below_floor |
| `hcde-emapinfo-stats-model-chunk342` | 18 | nativeLoc_below_floor |
| `am-map-commands-colors-chunk343` | 36 | nativeLoc_below_floor |
| `maploader-teleport-textures-chunk344` | 43 | nativeLoc_below_floor |
| `g-mapinfo-warp-reset-chunk345` | 33 | nativeLoc_below_floor |
| `g-mapinfo-lookup-parser-format-chunk346` | 57 | nativeLoc_below_floor |
| `g-mapinfo-parser-mid-chunk347` | 17 | nativeLoc_below_floor |
| `g-mapinfo-cluster-nextmap-chunk348` | 54 | nativeLoc_below_floor |
| `dehacked-lookup-defaults-chunk349` | 9 | nativeLoc_below_floor |
| `dehacked-lump-loader-chunk350` | 21 | nativeLoc_below_floor |
| `d-main-cvar-policy-chunk351` | 13 | nativeLoc_below_floor |
| `am-map-colorset-table-chunk352` | 26 | nativeLoc_below_floor |
| `am-map-colorset-loader-chunk353` | 89 | nativeLoc_below_floor |
| `am-map-colorset-init-chunk354` | 116 | nativeLoc_below_floor |
| `am-map-parse-amcolors-chunk355` | 72 | nativeLoc_below_floor |
| `am-map-arrow-static-init-chunk356` | 57 | nativeLoc_below_floor |
| `maploader-missing-texture-sector-chunk357` | 41 | nativeLoc_below_floor |
| `maploader-texture-blend-chunk358` | 27 | nativeLoc_below_floor |
| `d-main-fraglimit-timescale-chunk359` | 27 | nativeLoc_below_floor |
| `am-map-follow-grid-ccmd-chunk360` | 12 | nativeLoc_below_floor |
| `d-main-wipetype-chunk361` | 4 | nativeLoc_below_floor |
| `g-mapinfo-optional-float-compat-chunk362` | 34 | nativeLoc_below_floor |
| `am-map-texture-mark-ccmd-chunk363` | 10 | nativeLoc_below_floor |
| `maploader-load-segs-chunk375` | 60 | nativeLoc_below_floor |
| `maploader-load-things-chunk380` | 134 | nativeLoc_below_floor |
| `am-map-draw-marker-chunk381` | 30 | nativeLoc_below_floor |
| `d-main-compat-fps-chunk383` | 37 | nativeLoc_below_floor |
| `maploader-node-orchestration-chunk388` | 98 | nativeLoc_below_floor |
| `maploader-create-blockmap-chunk390` | 74 | nativeLoc_below_floor |
| `maploader-sidedefs2-verify-blockmap-chunk391` | 114 | nativeLoc_below_floor |
| `d-main-doomloop-page-chunk392` | 102 | nativeLoc_below_floor |
| `d-main-compat-callbacks-chunk393` | 116 | nativeLoc_below_floor |
| `d-main-debug-ccmds-chunk396` | 112 | chunk_json_nativeLoc_mismatch |
| `maploader-settexture-userdata-chunk401` | 193 | chunk_json_nativeLoc_mismatch |
| `d-main-startup-post-init-chunk433` | 161 | missing_test |
| `d-main-startup-resource-chunk435` | 151 | missing_test |
| `d-doommain-internal-boot-chunk443` | 145 | nativeLoc_below_floor |
| `p-lnspec-find-execute-chunk444` | 75 | nativeLoc_below_floor |
| `acs-random-getcvar-polywait` | 38 | missing_test |
| `combat-trace-ordering` | 280 | missing_test |
| `acs-map-mutations` | 151 | missing_test |
| `line-specials-plat-generic-elevator` | 128 | missing_test |
| `line-specials-gap-closure` | 161 | missing_test |
| `acs-callfunc-wire-dispatch-udmf` | 31 | missing_test |
| `action-combat-port` | 273 | missing_test |
| `acs-hot-callfunc-behavior` | 14 | missing_test |
| `line-special-feasible-gaps` | 51 | missing_test |
| `action-explode-radius-give` | 211 | missing_test |
| `acs-air-userarray-spawnspot` | 50 | missing_test |
| `p-map-use-fail-noway` | 47 | missing_test |

## 4. Per-chunk soft warnings (first 80 rows)

| Chunk | nativeLoc | Soft kinds |
|-------|-----------|------------|
| `p-enemy-chunk1` | 305 | thin_test_band |
| `p-map-radius-chunk1` | 209 | thin_test_band |
| `p-switch-textures-button-chunk5` | 185 | thin_test_band |
| `p-floor-waggle-donut-elevator-chunk6` | 152 | thin_test_band |
| `p-pusher-lighttransfer-chunk6` | 172 | thin_test_band |
| `p-teleport-switch-range-chunk6` | 204 | thin_managed_coverage, thin_test_band |
| `g-level-headless-flow-chunk8` | 1008 | thin_managed_coverage, thin_test_band |
| `g-game-headless-ticker-chunk9` | 1080 | thin_managed_coverage, thin_test_band |
| `p-saveg-acs-archive-chunk10` | 687 | thin_test_band |
| `d-net-reset-commands-chunk12` | 148 | thin_test_band |
| `thingdef-property-params-chunk21` | 191 | thin_test_band |
| `p-mobj-spawn-mapthing-chunk23` | 133 | thin_test_band |
| `thingdef-parse-actor-header-chunk26` | 142 | thin_test_band |
| `thingdef-dispatch-script-property-chunk27` | 83 | thin_test_band |
| `thingdef-builtin-properties-chunk28` | 427 | thin_test_band |
| `p-mobj-spawn-compose-chunk29` | 99 | thin_test_band |
| `thingdef-parse-actor-body-chunk34` | 61 | thin_test_band |
| `p-acs-get-actor-property-chunk38` | 83 | thin_test_band |
| `p-acs-fade-range-chunk39` | 71 | thin_test_band |
| `p-acs-set-master-chunk41` | 62 | thin_test_band |
| `p-acs-check-actor-property-chunk43` | 73 | thin_managed_coverage, thin_test_band |
| `zcc-integer-binary-emit-chunk51` | 155 | thin_managed_coverage |
| `zcc-compare-rel-chunk55` | 161 | thin_managed_coverage, thin_test_band |
| `zcc-unary-not-bitwise-chunk56` | 53 | thin_managed_coverage |
| `zcc-function-call-math-chunk69` | 200 | thin_managed_coverage |
| `v-colortables-chunk81` | 91 | thin_test_band |
| `r-vanillatrans-chunk82` | 91 | thin_test_band |
| `zcc-member-function-call-resolve-chunk84` | 160 | thin_test_band |
| `file-wad-chunk95` | 252 | thin_test_band |
| `matrix-chunk96` | 399 | thin_test_band |
| `tgatexture-chunk98` | 154 | thin_test_band |
| `pcxtexture-chunk100-complete` | 168 | thin_test_band |
| `file-zip-chunk103` | 93 | missing_audit_padding_review |
| `imgztexture-chunk105` | 90 | thin_test_band |
| `pngtexture-chunk106` | 207 | thin_test_band |
| `ddstexture-chunk107` | 253 | thin_test_band |
| `qoitexture-chunk110` | 145 | thin_test_band |
| `pngtexture-chunk111-complete` | 164 | thin_test_band |
| `ddstexture-chunk112-dxt35` | 192 | thin_test_band |
| `rawpagetexture-chunk116` | 93 | thin_test_band |
| `cmdlib-complete-chunk122` | 212 | thin_test_band |
| `m-argv-complete-chunk123` | 129 | thin_test_band |
| `configfile-complete-chunk125` | 172 | thin_test_band |
| `gameconfig-bootstrap-chunk126` | 113 | thin_test_band |
| `gameconfig-autoload-chunk129` | 163 | thin_test_band |
| `gameconfig-migrations-chunk133` | 195 | thin_test_band |
| `gameconfig-gamesetup-chunk135` | 106 | thin_test_band |
| `gameconfig-archive-chunk142` | 95 | thin_test_band |
| `i-time-complete-chunk143` | 173 | thin_test_band |
| `utf8-mixed-misc-chunk144` | 137 | thin_test_band |
| `gameconfig-paths-chunk146` | 80 | thin_test_band |
| `stringtable-locale-chunk149` | 87 | thin_test_band |
| `stringtable-load-chunk152` | 105 | thin_test_band |
| `stringtable-langid-chunk154` | 257 | thin_managed_coverage, thin_test_band |
| `namedef-static-chunk155` | 274 | thin_test_band |
| `stringtable-csv-chunk159` | 86 | thin_test_band |
| `c-bind-queue-chunk166` | 123 | thin_test_band |
| `c-cvars-float-string-chunk167` | 141 | thin_test_band |
| `c-bind-readlump-chunk168` | 60 | thin_test_band |
| `intermission-base-keys-chunk170` | 129 | thin_test_band |
| `intermission-fader-wiper-scroller-chunk171` | 110 | thin_test_band |
| `intermission-textscreen-factory-chunk172` | 154 | thin_test_band |
| `linked-sectors-move-chunk174` | 149 | thin_test_band |
| `intermission-endsequence-chunk177` | 86 | thin_test_band |
| `intermission-startfinale-chunk178` | 72 | thin_test_band |
| `terrain-parse-outer-chunk181` | 96 | thin_test_band |
| `terrain-generic-parse-chunk186` | 154 | thin_test_band |
| `decallib-scale-parse-chunk188` | 91 | thin_test_band |
| `animdef-header-chunk190` | 123 | thin_test_band |
| `animdef-pic-range-chunk193` | 131 | thin_test_band |
| `animdef-warp-switch-chunk194` | 189 | thin_test_band |
| `sndinfo-ambient-filter-chunk195` | 101 | thin_test_band |
| `decallib-bst-lookup-chunk196` | 131 | thin_test_band |
| `animdef-camera-door-chunk198` | 154 | thin_test_band |
| `animdef-fire-ticker-chunk199` | 146 | thin_test_band |
| `anim-switch-runtime-chunk203` | 180 | thin_managed_coverage, thin_test_band |
| `sndinfo-player-registry-chunk204` | 180 | thin_managed_coverage |
| `gameinfo-teaminfo-chunk206` | 203 | thin_test_band |
| `keyconf-chunk207` | 239 | missing_audit_padding_review |
| `sprite-frame-install-chunk208` | 196 | missing_audit_padding_review, thin_test_band |
| … | … | (truncated; see full reaudit log) |
