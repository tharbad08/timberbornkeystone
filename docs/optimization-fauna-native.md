# Native fauna hot-path caching (Keystone 1.3.3 / Timberborn 1.1)

## Problem
`FaunaSpawnDrainer.VisitCluster` runs whenever queued fauna clusters are drained. Previously, every visit re-enumerated Class E recipes for the selected biome and allocated fresh level-bucket dictionaries and lists. Every existing fauna entry also looked up its blueprint by linearly scanning `FlourishCatalog.AllClassE`. These are repeated operations while a queue is active, not one-time startup work.

## Change
- Group recipes by level once per biome in the existing drainer (lazy, only after `FlourishCatalog.IsLoaded`).
- Build a first-match blueprint-name index lazily after catalog PostLoad.
- Preserve exact-match (ordinal) blueprint comparison, the **first registered duplicate** rule, recipe ordering, per-cluster deficit calculation, camera/frustum checks, callbacks, and queue behavior.
- Keep `MaxSpawnsPerFrame = 2` and `MaxVisitsPerFrame = 6`. No deferred/dropped fauna spawns or change to population convergence.
- Caches live only inside the world-scoped drainer. No new static/global caches and no Harmony patches.

## Deliberately excluded
The previous PR reduced `MaxSpawnsPerFrame` to 1, changing fauna replenishment throughput. That is pacing, not a proven CPU-work reduction; this replacement retains the original limit.
`ChunkBiomeTicker` and `ChunkClusterTicker` already maintain topology-aware cached schedules and parallel/rolling sweeps; `EcologyFieldUpdater` already caches schedules and pools its worker arrays. No speculative changes are made to them without a new measured bottleneck. Fauna template prewarming is also omitted until its actual startup lifecycle and benefit are verified.

## Validation (pending exact-source build and game tests)
1. Build against real Timberborn 1.1 and Keystone dependencies; do not use invented API stubs.
2. Measure `Fauna.Drain` frame time, allocation count, `Fauna.Drain.Visits`, and `Fauna.Drain.Spawns` on the same colony at x1/x3; record baseline and sample size.
3. Verify recipes and first-match duplicates produce the same assigned level, category and spawn weights.
4. Verify startup, game reload, queue catch-up, fish/deer/cattle populations, and camera-blocked regions.
5. Disable external TB Optimizer Keystone patches while testing to avoid double optimization.
6. Check crash logs and test with both fresh and old colonies.

**Status:** Source inspection and the changes above only. No measured speedup is claimed and the exact branch has not been compiled or run in-game here.
