# FSM API: Performance Benchmarks & Optimization Analysis

This document presents the baseline stress test results for the FSM API. These tests were conducted to establish the current performance limits and identify key areas for architectural optimization—specifically the transition from string-based lookups to integer-based hashing.

## Stress Test Methodology

To accurately gauge the performance overhead of the FSM API, we did not use lightweight, "happy path" agents. Instead, we deployed a complex Predator-Prey simulation involving two distinct agent types with heavy per-frame logic and significant memory allocation pressure.

The simulation, **"The Quick Brown Fox & The Lazy Dog,"** forces the API to handle high-frequency state changes and expensive transition logic.

### The Agents
1.  **The Quick Brown Fox (Prey):**
    * **Behaviors:** Patrols (`Walking`), reacts to proximity (`Jumping`), and reacts to collisions (`Fleeing`).
    * **Stress Factor:** Uses iterative spatial checks against neighbor lists (`VisibleAgents`) to trigger jumps.
    * **Allocations:** Uses LINQ expressions (e.g., `CollidedAgents.Any(s => s is LazySleepingDog)`) in transition conditions, deliberately creating Garbage Collection pressure to test API stability under load.

2.  **The Lazy Sleeping Dog (Predator):**
    * **Behaviors:** Dormant (`Sleeping`), reactive (`Awake`), aggressive (`Chasing`), and interactive (`Mangling`).
    * **Stress Factor:** Transitions require analyzing lists of nearby agents every tick.
    * **Heavy Logic:** The `Chasing` state modifies the agent's speed and position every frame, forcing the FSM context to mutate data constantly.

### The "Heavy" Workload
The test does **not** use optimized caching or integer IDs. It intentionally leans on the most expensive parts of the current C# architecture to prove robustness:
* **String-Based Lookups:** Every single state transition (e.g., `"Walking"` to `"Fleeing"`) forces the API to perform string equality checks and dictionary lookups.
* **Unoptimized LINQ:** Transition conditions utilize `Func<IStateContext, bool>` delegates containing LINQ queries, generating allocation noise (garbage) that the system must manage.
* **High Concurrency:** At peak load, over **240,000** of these complex agents are evaluating their logic simultaneously.

---

## Baseline Stress Test Results

The following data represents the "Stress Test" project results using the Baseline API. The **Interval** column represents the simulation update frequency (e.g., time-slicing where 1 = every frame, 3 = every 3rd frame), which allows for higher agent counts by distributing logic updates.

| Interval | Agents | FPS | Logic Ops/Frame | Mem Delta (MB) |
| :--- | :--- | :--- | :--- | :--- |
| 1 | 50000 | 58.51 | 150,000 | 8 |
| 1 | 55000 | 79.11 | 165,000 | 17 |
| 1 | 60000 | 70.40 | 180,000 | 4 |
| 1 | 65000 | 64.63 | 195,000 | 3 |
| 1 | 70000 | 61.76 | 210,000 | 6 |
| 1 | 75000 | 57.70 | 225,000 | 7 |
| 1 | 80000 | 55.02 | 240,000 | 6 |
| 1 | 85000 | 51.85 | 255,000 | 9 |
| 1 | 90000 | 50.64 | 270,000 | 10 |
| 1 | 95000 | 47.69 | 285,000 | 9 |
| 1 | 100000 | 42.53 | 300,000 | 11 |
| 1 | 105000 | 43.05 | 315,000 | 14 |
| 1 | 110000 | 41.00 | 330,000 | 15 |
| 1 | 115000 | 38.14 | 345,000 | 13 |
| 1 | 120000 | 37.27 | 360,000 | 16 |
| 1 | 125000 | 35.23 | 375,000 | 18 |
| 1 | **130000** | **34.33** | 390,000 | 20 |
| 1 | **135000** | **32.62** | 405,000 | 17 |
| 1 | **140000** | **32.02** | 420,000 | 21 |
| 1 | **145000** | **30.45** | 435,000 | 23 |
| 1 | **150000** | **30.25** | 450,000 | 25 |
| 1 | **155000** | **28.80** | 465,000 | 23 |
| 2 | 80000 | 82.51 | 120,000 | 55 |
| 2 | 85000 | 72.08 | 131,750 | 4 |
| 2 | 90000 | 68.77 | 135,000 | 24 |
| 2 | 95000 | 65.19 | 147,250 | 23 |
| 2 | 100000 | 62.11 | 150,000 | 25 |
| 2 | 105000 | 59.77 | 162,750 | 27 |
| 2 | 110000 | 57.16 | 165,000 | 29 |
| 2 | 115000 | 52.92 | 178,250 | 31 |
| 2 | 120000 | 52.30 | 180,000 | 33 |
| 2 | 125000 | 49.76 | 193,750 | 36 |
| 2 | 130000 | 47.03 | 195,000 | 37 |
| 2 | 135000 | 45.80 | 209,250 | 39 |
| 2 | 140000 | 44.25 | 210,000 | 42 |
| 2 | 145000 | 42.85 | 224,750 | 40 |
| 2 | 150000 | 42.21 | 225,000 | 41 |
| 2 | 155000 | 39.79 | 240,250 | 45 |
| 2 | 160000 | 39.48 | 240,000 | 45 |
| 2 | 165000 | 38.21 | 255,750 | 48 |
| 2 | 170000 | 36.79 | 255,000 | 49 |
| 2 | 175000 | 35.32 | 271,250 | 53 |
| 2 | **180000** | **34.79** | 270,000 | 54 |
| 2 | **185000** | **34.40** | 286,750 | 57 |
| 2 | **190000** | **32.94** | 285,000 | 57 |
| 2 | **195000** | **32.11** | 302,250 | 57 |
| 2 | **200000** | **30.13** | 300,000 | 62 |
| 2 | **205000** | **30.43** | 317,750 | 62 |
| 2 | **210000** | **30.18** | 315,000 | 66 |
| 2 | **215000** | **28.83** | 333,250 | 66 |
| 3 | 120000 | 63.76 | 120,000 | 75 |
| 3 | 130000 | 55.69 | 130,000 | 25 |
| 3 | 140000 | 49.44 | 147,000 | 26 |
| 3 | 150000 | 47.48 | 150,000 | 26 |
| 3 | 160000 | 45.01 | 160,000 | 32 |
| 3 | 170000 | 40.94 | 178,500 | 32 |
| 3 | 180000 | 38.82 | 180,000 | 36 |
| 3 | 190000 | 37.41 | 190,000 | 37 |
| 3 | 200000 | 35.61 | 210,000 | 43 |
| 3 | **210000** | **33.34** | 210,000 | 45 |
| 3 | **220000** | **32.21** | 220,000 | 48 |
| 3 | **230000** | **31.14** | 241,500 | 50 |
| 3 | **240000** | **29.48** | 240,000 | 50 |

### Analysis of Baseline Metrics

The table demonstrates the API's current capability to scale linearly with time-slicing optimizations.
* **Interval 1 (Every Frame):** Maintains **>30 FPS** up to approximately **150,000 agents**.
* **Interval 3 (Every 3rd Frame):** Successfully pushes the envelope to **240,000 agents** while holding **~30 FPS**.

While these numbers confirm the API is production-ready for high-density simulations, the **Logic Ops/Frame** vs. **Mem Delta** correlation reveals a clear optimization opportunity in how state identifiers are handled.

### Roadmap: String vs. Hash Backend

We have identified that `string` comparisons for state keys are a primary contributor to the "Logic Ops" overhead seen in the table above.

**Anticipated Impact of Switching to Hashing:**
1.  **Reduced CPU Cycles:** Changing from String Equality (O(N)) to Integer/Hash Equality (O(1)) will drastically lower the cost of every state transition check.
2.  **Memory Stability:** Hashing eliminates the need for temporary string allocations during lookups, which we expect will flatten the "Mem Delta" curve significantly, reducing Garbage Collection pressure.
3.  **Throughput Increase:** By removing the string overhead, we anticipate the **Logic Ops/Frame** capacity to increase by an order of magnitude, potentially allowing **Interval 1** to match or exceed the current performance of **Interval 3**.