# Summary

**Branches:** Stable, non-optimized baseline in `main`; optimized variant in `version/opt-1`.

**Goal:** If there is internet, simulate “remote” download; otherwise run the level via a fallback chain of **cache → embedded Resources**.

**Approach:** Kept the architecture minimal; no extra design patterns. The only external dependency is **UniTask**.

**UX:** Added a **+250 ms** controlled delay to smooth the demo flow and reduce perceived loading flicker.

**Data size optimization:** Shortened JSON field names; stored the **board mask in hex**. If `levelId` is missing, it’s **auto-generated**.

**Build-time move:** For local simulation, required files are moved to **StreamingAssets** at build time via an **AssetPostprocessor**.

---

# Versioning

* **`main`**: Reference baseline (no additional optimizations).
* **`version/opt-1`**: JSON shortening + board hex mask + build-time move + online/offline chain + **+250 ms** UX delay.

---

# Workflow

* **Try remote:** If internet is available, fetch JSON from the simulated “remote” with `UnityWebRequest`.
* **Write/read cache:** On success, write to `Application.persistentDataPath`; on subsequent launches, **do not re-download**.
* **Resources fallback:** If there’s no internet and no cache, start with the embedded copy in **Resources**.
* **Loop:** When the level is cleared, return to the main screen and prepare the next level.
  This flow complies with the case requirements. **Additionally, in the optimized version, manifest entries for played levels are removed.**

---

# Data Format & Optimization

**Rationale:** Based on the case statement “Files within ProjectRoot/Levels and Resources can be modified in any way or optimized to improve memory usage and overall performance,” several JSON optimizations were applied.

**Short JSON fields:**
`L` (level), `Li` (levelId), `D` (difficulty), `G` (gridSize), `Mh` (mask-hex)

**Board → Hex:** For square grids like 8×8, generate a bitmask and store as a **hex string**; parsing restores it **in a single pass** without loss.

**`levelId` policy:** If `Li` is missing, assign a deterministic ID as `level_{L}_updated`. This allows removing `levelId` from JSON entirely if desired, yielding extra savings.

**Note 1:** `D` can be reduced to a single letter (e.g., `e/h`), but it’s kept as a string for now to avoid extra parse complexity; can be a next step.
**Note 2:** As an alternative, data could be grouped in packs of 25 for batch download/distribution; not applied to avoid drifting too far from case requirements.

**Example (optimized):**

```json
{ "L": 12, "Li": "level_12_updated", "D": "hard", "G": 8, "Mh": "F3CC_0A7E_..." }
```

---

# Dependencies

* **Cysharp/UniTask**: single package to simplify async flow.

---

# Build & File Organization

**Source layout:** During development, test data lives under `ProjectRoot/Levels` and `Assets/Resources/...`.
Per the case statement, these contents were optimized accordingly.

**AssetPostprocessor rule:**
During build, specific folders under **Resources** are moved into **StreamingAssets** (required for local download simulation).

**Excluded:** `Assets/Resources/resources_levels_1_500_legacy` is **not** included in the build; only the necessary sets are moved.

**Integrity:** Use atomic writes and avoid re-downloading if content already exists (as required by the case).

---

# Test Scenarios (Smoke)

* **Online:** Load a level with network on → relaunch; it **shouldn’t re-download**, should come from cache.
* **Offline (cache exists):** Turn off internet; previously downloaded level plays from **cache**.
* **Offline (no cache):** With no internet and no cache, **Resources fallback** kicks in.
* **Visual flow:** No flicker on the loading screen; **+250 ms** simulation active.
* **Data integrity:** JSON → mask-hex → board round-trip should yield identical results across runs.

---

# Performance Notes

* **Low-end mobile target:** Short JSON keys, single-pass parse, no unnecessary allocations (reduced GC pressure).
* **Network minimization:** Cache-once prevents redundant requests (aligned with the case goals).

---

# Manifest-Based Download Tracking

Downloaded files are tracked via a **manifest**. Instead of per-file “exists?” checks, a single control point (the manifest) is used—both by preference and for the following extensibility reasons:

* **Consistency:** Files belonging to the same release are grouped under a single **contentVersion**; eliminates “mixed version” risk.
* **Fewer network calls:** When level-based versioning is applied and the CDN also serves the manifest, you do **1 manifest GET + downloads for the diff**, instead of N separate HEAD/GET calls. This reduces RTT and radio wake-ups on mobile.
* **Differential updates:** With version- or date-based tracking, diff against the previous manifest and download **only changed files**.
* **Rollback & pinning:** The client can be pinned to a specific manifest version; easy rollback if needed.
* **Storage management:** The manifest can drive **prune** strategies by file size and last access time (e.g., removing records for played levels).
* **Local access:** Instead of allocating per-level existence checks locally, required presence/metadata is obtained **once** from the manifest.
