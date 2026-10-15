# Native Keystone fauna optimizations (Timberborn 1.1 / Keystone 1.3.3)

## Origin and scope

Port **all three** Keystone-specific TB Optimizer features into Keystone's own source. Unlike an external Harmony adapter, these modifications use existing Keystone classes, game services, and the native startup lifecycle. No new mod dependency or Harmony patch is introduced.

### 1. One actual fauna spawn per frame

`FaunaSpawnDrainer.MaxSpawnsPerFrame` changes **2 → 1**. The original drainer handles the queue itself: a successful spawn re-enqueues the cluster, and unfilled deficits stay available for later visits. No fabricated "successful" return and no second queue are necessary. **Behavior change:** catch-up rate is intentionally reduced (at most 60 vs 120 spawns/sec at 60 fps); this limits costly same-frame allocations, it does not reduce the total work to spawn the same population.

### 2. Cache Class E recipe lookups

- Cache the first recipe for each blueprint name after `FlourishCatalog.PostLoad`, preserving the previous first-match behavior and ordinal comparisons.
- Cache `ClassEForBiome` level buckets in the world-scoped drainer so repeated cluster visits reuse recipe lists instead of allocating new dictionaries and scanning the same immutable catalog each visit.
- Before the catalog reports `IsLoaded`, keep the previous lookup behavior and do not freeze empty lists.

### 3. Prewarm three fauna templates

The new `KeystoneFaunaTemplatePrewarmer` receives the game's `TemplateCollectionService` and `TemplateInstantiator` through Bindito. After `KeystoneStartupWarmup.PostLoad` completes normal ecology initialization, it finds `KeystoneCow`, `KeystoneBull`, and `KeystoneDeer` in the loaded template collection and calls the game's `CacheInstance(blueprint)`. This uses the **vanilla template cache** (no parallel cache), records timings in verbose logs, and catches exceptions so a failed optional prewarm does not break the colony. Timing differs slightly from TB Optimizer's `ShowPrimaryUI` patch: it occurs during Keystone's completed PostLoad warmup rather than at UI display. Verify first-frame and loading-time behavior in the game.

## Preserved invariants

- `MaxVisitsPerFrame = 6`, camera/frustum gating, cluster iteration, live registry counts, and spawn re-enqueue semantics
- No global/static world cache and no Harmony hook ownership conflicts inside Keystone
- All recipe weights, category settings, levels, persistence, and entity cleanup paths untouched

## Validation and evidence

- [x] Audited TB Optimizer's three Keystone files and Keystone upstream's preexisting implementations.
- [x] Kept the three contributions in one separately reviewable native-performance branch, independent of MoM compatibility.
- [ ] Compile **this exact commit** against Timberborn 1.1 and actual required mod assemblies.
- [ ] Run same-save x1/x3 before/after benchmarks: `Fauna.Drain` duration, allocs, count, peak frame times, real x3 throughput, no silent skipped work.
- [ ] Verify first-duplicate recipe lookup, new/loaded worlds, fish/cattle/deer, camera-blocked clusters, retry/queue carryover.
- [ ] Measure and inspect startup prewarming in `Player.log` for all three animals; check no type-load failure.
- [ ] Disable matching TB Optimizer external Keystone features during testing to avoid duplicate prewarm and pacing layers.

**Status:** source prepared, but uncompiled and without in-game regression measurements. No measured performance improvement is claimed.
