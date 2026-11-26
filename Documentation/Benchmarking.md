![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# FSM_API Benchmarking

Performance claims in FSM_API are backed by an executable benchmark project rather than by intuition.

## Where is the benchmark?

The companion project is:

**[TrentBest/FSM_API_Benchmark](https://github.com/TrentBest/FSM_API_Benchmark)**

The production package stays focused on state-machine behavior. The benchmark repository contains the experiment, BenchmarkDotNet configuration, workload parameters, and recorded evidence.

## What are we measuring?

The current benchmark suite asks two related questions.

### 1. What does one real update cost?

`FSM_StringPerformanceBaseline` builds a two-state FSM with two transitions and measures:

~~~csharp
FsmApi.Interaction.Update(ProcessingGroup);
~~~

This is intentionally a real API operation rather than an isolated dictionary or string benchmark.

### 2. How does update cost scale across groups?

`FSM_StringGroupIterationBenchmark` creates named processing groups and measures updating 1, 10, and 50 of them.

The benchmark uses BenchmarkDotNet's `[Params]` to vary the workload:

~~~csharp
[Params(1, 10, 50)]
public int ActiveProcessGroups { get; set; }
~~~

The operation then updates the requested groups by name.

This is useful because the current API uses string identifiers at this boundary. The benchmark gives us an observed baseline before any future lookup optimization.

## Recorded result

The September 2026 benchmark run produced:

| Active process groups | Mean | Allocated |
|---:|---:|---:|
| 1 | **305.1 ns** | **360 B** |
| 10 | **3,115.7 ns** | **3,600 B** |
| 50 | **15,736.6 ns** | **18,000 B** |

A source audit found that this historical group-iteration fixture is **not stationary** across a long BenchmarkDotNet run: each group's StateA callback increments a counter, and a transition moves it to StateB after the counter exceeds 1,000; StateB has no update callback. Depending on warm-up and measurement duration, the benchmark can therefore change from callback work to mostly empty scheduler work during one run. The original source also does not prime or verify a fixed steady-state callback count.

For that reason, do not interpret the table as a clean scaling curve or as a stable per-group marginal cost. Retain the figures as historical reported observations only. The single-FSM fixture is a different workload: it alternates between two states through successful transitions, so its 305.1 ns / 360 B observation is not a no-transition baseline either. The raw `.diagsession` files do not replace normalized BenchmarkDotNet result exports and full machine/source provenance.

A corrected benchmark should hold callback and transition behavior constant for every measured invocation, prime the runtime before measurement, and verify callback counts outside the timed region. The source-linked 2.0 suite now follows that setup discipline for its initial no-transition scheduler comparison.

## How to interpret the numbers

### Mean

`Mean` is the average measured time for one benchmark operation.

For example:

~~~text
305.1 ns
~~~

is approximately 305 billionths of a second.

### Allocated

`Allocated` is managed memory allocated during one benchmark operation.

The allocation results matter because an operation can be fast while still creating garbage-collection pressure.

The recorded 50-group operation reports 18,000 B in the historical experiment. Because the fixture can change its callback workload during a long run, treat that allocation value as a historical observation too—not a reliable per-frame cost or a regression target.

## Why the benchmark is useful

The benchmark gives future optimization work something concrete to preserve or improve.

For example, if the implementation later changes from string-backed lookup toward an integer-backed registry, we do not have to argue from:

> "integer lookup should be faster."

We can measure:

~~~text
current implementation
        │
        ▼
305.1 ns / 360 B
        │
   implementation
       change
        │
        ▼
new benchmark
        │
        ▼
compare
~~~

That is the useful role of the benchmark: **it turns an optimization hypothesis into an experiment.**

## How to recognize the benchmark

When reading the benchmark source, look for:

- `[Benchmark]` — the method BenchmarkDotNet measures.
- `[Params]` — the workload dimension being varied.
- `[GlobalSetup]` — the state prepared before measurement.
- `[MemoryDiagnoser]` — allocation measurements.
- `[CPUUsageDiagnoser]` — CPU diagnostics.

The benchmark source is the authoritative description of what the result means.

## Reproducing it

The repository currently has two distinct benchmark tracks, and their evidence must not be conflated.

### Released 1.x package baseline

The original benchmark project on the repository's `master` / `development` branches targets `.NET 8` and references the published `TheSingularityWorkshop.FSM_API` package `1.0.13`. It contains two original string-backed entry points: a single-FSM update and updates across 1, 10, or 50 named processing groups. The recorded September 2026 values above are historical observations for that package/workload. The repository's two checked-in `.diagsession` files are diagnostic artifacts, not normalized BenchmarkDotNet CSV/JSON evidence. Keep the numbers as historical context, not as a calibrated regression baseline.

### FSM_API 2.0 paired representation track

The open, draft [benchmark-laboratory PR #1](https://github.com/TrentBest/FSM_API_Benchmark/pull/1) adds a broader .NET 10 suite and a separate source-linked `FSM_Benchmark_2_0` project. The latter references the checked-out FSM_API source project rather than substituting the published 1.0.13 package. It currently pairs:

- `FSMAPI.Execution.Update.NoTransitions`: string-backed and integer-backed public update calls, parameterized by 1/10/50 definitions × 1/10/50 instances per definition.
- `FSMAPI.Lookup.Definition.ById.Hit`: successful string-backed and integer-backed definition-presence lookups with 1/10/50 definitions.

The execution fixture uses the same context type, callback, process rate, state count, and number of update calls; it primes both runtimes and verifies callback counts before measurement. These are useful initial comparisons, not complete coverage.

One important confound is already identified in the source: string-backed public `Interaction.Update(string)` wraps the tick with `Stopwatch.StartNew()`, stops it, and checks a warning threshold; integer-backed public `Interaction.Update(int)` forwards directly to `Runtime.Update`. The public-update pair therefore measures the cost of the current public entry points, including this instrumentation asymmetry. It cannot isolate the benefit of integer identity by itself. A follow-up comparison must measure equivalent scheduler-core paths separately from the string wrapper.

The lookup pair also compares representation-specific key shapes: string name plus processing-group name versus globally unique integer FSM ID. It is a public identity-presence comparison, not a pure string-equality-versus-integer-equality microbenchmark.

The benchmark branch also adds broader 1.x operation benchmarks, an API reflection inventory, and a method-level coverage ledger.

[Validation run #17](https://github.com/TrentBest/FSM_API_Benchmark/actions/runs/44984466828) succeeded on benchmark commit `7a09fb760af425f20c914d013787b1bed86a9eeb`: both benchmark projects built, the public API inventory was generated, and the legacy and 2.0 BenchmarkDotNet Dry smoke tests passed. The calibrated benchmark job was **skipped**. This proves build/setup/discovery/invocation only; it provides **no timing result and no measured integer speedup**.

Important source boundary: the smoke workflow checks out FSM_API's `development` branch. It has not yet benchmarked PR #42's exact integer-move candidate commit. The workflow now accepts an `fsm_api_ref` input during manual dispatch, so the calibrated run can be pinned to a branch, tag, or exact source SHA. Before accepting a 2.0 result, pin and record the exact FSM_API source SHA, run both representations in the same process/environment, preserve raw BenchmarkDotNet Markdown/CSV/JSON and provenance, and validate equivalent observable behavior outside the timed region.

The benchmark PR #5 adds a stable semantic-ID catalog and staged report-only regression policy; it is also open and draft. Neither PR is merged, and no performance regression gate is active.

Run:

~~~bash
dotnet run -c Release
~~~

Use Release configuration for performance measurement.

Benchmark numbers are environment-sensitive. CPU, .NET runtime, operating system, thermal state, background processes, and benchmark configuration can all affect the result.

## Why this is not a normal CI test

Correctness tests ask whether behavior is right.

Benchmarks ask how expensive behavior is on a particular machine under a particular workload.

A benchmark result is therefore best treated as a dated experimental observation. The benchmark project is kept executable so future changes can be measured again.

## The evidence chain

A reader should be able to follow:

~~~text
FSM_API README
     │
     ▼
Benchmarking guide
     │
     ▼
FSM_API_Benchmark repository
     │
     ▼
benchmark class
     │
     ▼
[Benchmark] method
     │
     ▼
BenchmarkDotNet result
     │
     ▼
documented interpretation
~~~

That is deliberately more useful than printing a performance number without telling the reader where it came from.

## Historical baseline

These 1.x package measurements were recorded during the September 2026 FSM_API performance work. The separate 2.0 source-linked track is still in smoke-test validation; no 2.0 timing has been collected or accepted.

They establish a baseline for the implementation and workload that existed at that time. They should be rerun after changes to:

- processing-group lookup;
- FSM registration;
- update traversal;
- allocation behavior;
- state transition execution;
- runtime identifier representation.

**Measure first. Change one thing. Measure again.**


## Reproducibility and regression policy

See the [benchmark evidence contract](Performance/BenchmarkingContract.md) for stable semantic IDs, required run provenance, immutable result history, and staged regression-gate policy. The benchmark suite and its coverage catalog live in the companion [FSM_API_Benchmark repository](https://github.com/TrentBest/FSM_API_Benchmark).

**Release note:** the historical numbers above describe the September 2026 workload and FSM_API 1.0.13. They are not evidence of FSM_API 2.0 performance. The existence of paired benchmark methods is not itself a measured improvement. Rerun equivalent workloads against the exact candidate commit before making a 2.0 performance claim.
