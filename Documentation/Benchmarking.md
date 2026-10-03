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
FSM_API.Interaction.Update(ProcessingGroup);
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

The measured workload scales approximately linearly in this range.

That does **not** mean FSM_API has a universal linear complexity guarantee for every operation. It means this particular experiment showed approximately linear growth when the number of explicitly updated processing groups increased from 1 to 50.

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

The recorded 50-group operation allocates 18,000 B in the benchmark workload. That is evidence about this experiment—not a statement that every application frame using 50 groups will allocate exactly 18 KB.

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

The benchmark project targets .NET 8 and references FSM_API 1.0.13.

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

These measurements were recorded during the September 2026 FSM_API performance work.

They establish a baseline for the implementation and workload that existed at that time. They should be rerun after changes to:

- processing-group lookup;
- FSM registration;
- update traversal;
- allocation behavior;
- state transition execution;
- runtime identifier representation.

**Measure first. Change one thing. Measure again.**
