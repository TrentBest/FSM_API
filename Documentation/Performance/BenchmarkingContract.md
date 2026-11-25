![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# FSM_API Performance Evidence and Benchmark Contract

**Owner:** The Singularity Workshop  
**Status (2026-11-25):** 2.0 paired benchmark smoke lane exists; calibrated measurements and performance gates are not yet active.  
**Related implementation:** [FSM_API_Benchmark](https://github.com/TrentBest/FSM_API_Benchmark)  
**Benchmark coverage catalog:** [stable IDs and coverage status](https://github.com/TrentBest/FSM_API_Benchmark/blob/benchmark-ledger-foundation/Documentation/BenchmarkCatalog.md)  
**Regression policy:** [staged comparison and gate policy](https://github.com/TrentBest/FSM_API_Benchmark/blob/benchmark-ledger-foundation/Documentation/RegressionPolicy.md)

## Purpose

FSM_API is core technology. Performance changes must be measured against reproducible evidence, not intuition or a single noisy timing. This document separates the existing large-simulation stress-test narrative from controlled operation-level microbenchmarks and establishes the evidence contract for future changes.

## Two different kinds of evidence

### Large-simulation stress test

[`FSM_API/InitialBenchmark.md`](../../FSM_API/InitialBenchmark.md) records a historical predator/prey simulation with high agent counts and time-sliced updates. It is useful as a record of the workload and historical observations, but its published table does not provide enough environment, source revision, run identity, or raw result provenance to serve as a strict per-commit regression gate without further verification.

Do not compare these FPS or memory-delta values directly with BenchmarkDotNet nanosecond/op or allocated-bytes/op values. They answer different questions and use different workload boundaries.

### Operation-level microbenchmarks

The [FSM_API_Benchmark repository](https://github.com/TrentBest/FSM_API_Benchmark) now has two distinct evidence tracks:

- The current `master` / `development` baseline project targets `.NET 8` and references the published FSM_API `1.0.13` package. It contains the original string-backed single-FSM and multi-processing-group benchmarks. Two September 2026 `.diagsession` files are retained as diagnostic artifacts; they are not normalized BenchmarkDotNet result sets.
- Open draft [PR #1](https://github.com/TrentBest/FSM_API_Benchmark/pull/1) adds a broader `.NET 10` 1.x benchmark laboratory and a separate source-linked FSM_API 2.0 comparison project. The 2.0 project currently pairs public string/integer update and successful definition lookup scenarios. It references a separately checked-out FSM_API source tree, not the published 1.0.13 package.

The latest benchmark PR validation, [run #17](https://github.com/TrentBest/FSM_API_Benchmark/actions/runs/44984466828), built both projects, generated the reflection inventory, and passed both BenchmarkDotNet Dry smoke tests. Its calibrated-measurement job was skipped. Therefore it establishes build and fixture invocation, **not performance numbers**. The workflow currently checks out FSM_API `development`, not PR #42's exact candidate commit.

Open draft [PR #5](https://github.com/TrentBest/FSM_API_Benchmark/pull/5) adds a stable semantic benchmark catalog and staged regression policy. These PRs remain unmerged; no calibrated 2.0 measurements or active regression gates are claimed. The paired suite is an initial foundation, not complete API coverage. The historical multi-group fixture has an additional validity caveat: its StateA callback increments a counter and transitions to StateB after 1,000 updates, while StateB has no update callback. A long BenchmarkDotNet warm-up/measurement can therefore change the work being timed. The historical single-FSM fixture alternates successful transitions each tick. Keep the published figures as reported historical observations only; do not use them as a calibrated scaling curve or no-transition baseline.

## Stable benchmark identity

Every measured operation receives a semantic ID of the form:

`FSMAPI.<Area>.<Operation>.<Scenario>`

Examples:

- `FSMAPI.Execution.Update.SingleFsmTick`
- `FSMAPI.Execution.Update.ManyGroups`
- `FSMAPI.Builder.BuildDefinition.Empty`
- `FSMAPI.Lookup.State.ById.Hit`
- `FSMAPI.Security.InvalidInput.Reject`

The ID stays stable across implementation refactors. Workload parameters, commit SHA, package version, environment, and run identity are metadata. If the semantics genuinely change, record an explicit migration instead of silently joining incompatible history.

## Evidence required for each run

- FSM_API package/version and exact source commit where known.
- Benchmark-suite commit and stable benchmark ID.
- Runtime/SDK, OS, architecture, CPU, configuration, and relevant execution context.
- Exact workload dimensions and fixture/setup behavior.
- Distribution and uncertainty data, throughput where meaningful, allocation and GC data where meaningful.
- Compatible baseline run, absolute and percentage delta, and report/gate decision.
- Immutable raw artifact and normalized CSV/Markdown report; reruns must have distinct identities.

Unknown provenance must be recorded as unknown. Never fill it with guesses.

## Regression handling

The initial mode is report-only. Characterize same-environment variation before activating failure gates. Once calibrated, a credible, material slowdown beyond an operation-specific budget should block the performance check unless an explicit review documents a justified correctness or security tradeoff.

A security or correctness requirement must not be removed to improve a score. Benchmark valid, invalid, boundary, and adversarial paths as applicable; document the cost and rationale, then optimize without weakening the requirement.

Use multiple workload sizes to detect suspicious growth-rate changes. Scaling measurements are evidence about observed growth, not proof of a formal asymptotic complexity guarantee.

## Sith / release discipline

- Preserve the published 1.0.13 package; this work does not authorize a NuGet publish.
- Keep `master` and `development` as the two enduring branches.
- Reconcile and ledger unique branch work before any retirement. The user performs surplus-branch deletion; this document is not permission to delete.
- Do not merge benchmark or documentation changes merely because they exist; review diffs, run relevant validation, and keep release publication explicitly gated.

The current branch reconciliation record is [Documentation/BRANCH_RECONCILIATION.md](../BRANCH_RECONCILIATION.md). The benchmark implementation work is tracked by [FSM_API issue #24](https://github.com/TrentBest/FSM_API/issues/24) and [FSM_API_Benchmark issue #4](https://github.com/TrentBest/FSM_API_Benchmark/issues/4).
