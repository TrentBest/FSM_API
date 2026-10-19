![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Integer-backed FSM implementation — progress map

**Release checklist:** [FSM_API 2.0.0 Readiness — comprehensive remaining work](../../FSM_API_2.0.0_READINESS.md). This is the authoritative release-gate checklist, including Operation Sith branch cleanup, parity, benchmark, CI, documentation, and publication gates.


**Purpose:** make the remaining work visible so the integer-backed design can be resumed without reconstructing the whole vision from scattered commits.

> **Status in one sentence (2026-10-19):** the integer-backed runtime, broad tests, and deterministic `FSMCompiler` bridge are present on `development`; current-development CI is green, including run [#547](https://github.com/TrentBest/FSM_API/actions/runs/39185150851) at `02da084`. That run passed build/test/package and documentation jobs, uploaded coverage and artifacts, and skipped NuGet publishing. This proves the recorded development snapshot builds; it does not close behavior parity, benchmark evidence, storage validation, branch reconciliation, or release gates.

<p align="center">
  <a href="Visuals/Integer_Backing_Roadmap.svg">
    <img src="Visuals/Integer_Backing_Roadmap.svg" alt="Integer-backed runtime and tests are integrated into development and current-development CI is green; full parity, benchmark evidence, storage validation, branch reconciliation, and release remain explicit gates." width="100%" />
  </a>
</p>

## Current snapshot

- **Source of the original implementation:** [`feature/fsm-api-2.0`](https://github.com/TrentBest/FSM_API/tree/feature/fsm-api-2.0)
- **Current integration target:** [`development`](https://github.com/TrentBest/FSM_API/tree/development)
- **Integration snapshot:** the integer runtime and associated tests are present under [`FSM_API/IntegerBacked`](https://github.com/TrentBest/FSM_API/tree/development/FSM_API/IntegerBacked) and [`Tests/IntegerBacked`](https://github.com/TrentBest/FSM_API/tree/development/Tests/IntegerBacked). The deterministic string-to-integer compiler is in `FSM_API/IntegerBacked/FSMCompiler.cs`.
- **Latest feature-branch validation:** [run 39102742225](https://github.com/TrentBest/FSM_API/actions/runs/39102742225), commit [`dd96646`](https://github.com/TrentBest/FSM_API/commit/dd96646a3718923bb87a48af695a0c5b6819db8e), 2026-10-18. **400 passed, 0 failed**. The NuGet publish job was skipped; no package was published.
- **Historical regression:** [run 34809515652](https://github.com/TrentBest/FSM_API/actions/runs/34809515652) on 2026-09-14 had 398 passed and 2 failed out of 400. Those two update-loop tests now pass after the feature-branch fix described below.
- **Current-development CI:** [run #547](https://github.com/TrentBest/FSM_API/actions/runs/39185150851) passed on `development` at `02da0840dd32889cc7a53250d293fcdfe80641c4`. The build/test/package job, .NET 8.0 test step, Codecov upload, package artifact upload, local documentation-link validation, DocFX generation, and documentation artifact upload all succeeded; NuGet publishing was skipped. The checklist records run #504 as 401 passed / 0 failed / 0 skipped for the integer integration snapshot. CI green is necessary, not a substitute for trace parity and performance evidence.
- **Release readiness: approximately 40%.** This is a cautious estimate, not a weighted claim that code presence equals completion. Trace-based parity, reproducible benchmarks, storage/identity validation, branch reconciliation, package review, and release-candidate validation remain open.

## Work already present on the feature branch

| Area | Evidence | Status |
|---|---|---|
| Integer state IDs and state storage | [`FSM.cs`](https://github.com/TrentBest/FSM_API/blob/feature/fsm-api-2.0/FSM_API/IntegerBacked/FSM.cs), [`FSMState.cs`](https://github.com/TrentBest/FSM_API/blob/feature/fsm-api-2.0/FSM_API/IntegerBacked/FSMState.cs) | Implemented |
| Integer FSM IDs and definition storage | [`FSMRuntime.cs`](https://github.com/TrentBest/FSM_API/blob/feature/fsm-api-2.0/FSM_API/IntegerBacked/FSMRuntime.cs) | Implemented |
| Runtime-facing API | [`FsmApi.cs`](https://github.com/TrentBest/FSM_API/blob/feature/fsm-api-2.0/FSM_API/IntegerBacked/FsmApi.cs), [`FSMHandleInt.cs`](https://github.com/TrentBest/FSM_API/blob/feature/fsm-api-2.0/FSM_API/IntegerBacked/FSMHandleInt.cs) | Implemented |
| Fluent builder, timers, transitions | [`FSMBuilderInt.cs`](https://github.com/TrentBest/FSM_API/blob/feature/fsm-api-2.0/FSM_API/IntegerBacked/FSMBuilderInt.cs), [`FSMTimersInt.cs`](https://github.com/TrentBest/FSM_API/blob/feature/fsm-api-2.0/FSM_API/IntegerBacked/FSMTimersInt.cs), [`FSMTransition.cs`](https://github.com/TrentBest/FSM_API/blob/feature/fsm-api-2.0/FSM_API/IntegerBacked/FSMTransition.cs) | Implemented; parity still to prove |
| Contract, lifecycle, edge, stress, and selected parity tests | [integer test suite](https://github.com/TrentBest/FSM_API/tree/feature/fsm-api-2.0/Tests/IntegerBacked) | Broad coverage exists; full feature-branch suite passes; complete parity remains open |

## Remaining gates — in dependency order

### 1. Runtime mutation invariants — **resolved on the feature branch; regression tests pass**

The fix is isolated on `feature/fsm-api-2.0`: [`FSMHandleInt.IsRegistered`](https://github.com/TrentBest/FSM_API/commit/4aa64deb10420b924b578fdc8134aa53bbd2d42a) tracks runtime membership, and [`FSMRuntime` tick snapshots](https://github.com/TrentBest/FSM_API/commit/0ff4b93d65f889c9fcbb32b4d5b19d3997573f85) prevent list mutation from skipping live handles or running newly created handles in the same tick. Removing the currently executing handle no longer shifts the next handle out of the iteration, and handles created by callbacks are not part of the current tick's snapshot.

The regression tests are `FSMRuntime_Stress_Tests.Update_SelfRemovalMustNotSkipTheNextHandle` and `FSMRuntime_Stress_Tests.Update_NewHandlesMustNotRunUntilTheNextTick`. Both pass in [the 400/400 feature-branch run](https://github.com/TrentBest/FSM_API/actions/runs/39102742225). Keep this behavior protected by those tests.

### 2. Complete string-backed ↔ integer-backed translation — **initial bridge implemented; validation pending**

The initial `FSMCompiler` assigns dense deterministic IDs by ordinal state-name order, carries state actions and transition conditions into the integer representation, and returns immutable forward/reverse state-name mappings. Focused tests cover deterministic IDs, action/guard transfer, Any-State conversion, missing initial state, unknown transition source/destination, and mapping immutability. Current-development CI has passed (see run #504 in the release checklist and current head run #547); serialization/reconstruction and broader invalid-definition coverage remain open.

Acceptance criteria:
- deterministic mapping from authored state/FSM names to integer IDs;
- explicit reverse mapping for diagnostics and visualization;
- duplicate, unknown, and missing identifiers fail clearly;
- the mapping can be serialized or reconstructed consistently where required;
- runtime execution does not need to look up state names.

### 3. Prove behavioral parity — **partially covered; trace-based comparison still required**

The existing `IntegerParityTests.cs` checks selected cases, not the complete public behavior surface. The existing tests do not yet establish full string/integer transition parity. Do not treat the suspected continued-loop/overwrite behavior as confirmed without reproducing it against current development. Add equivalent trace-based tests through the real handle update path for Any-State precedence, regular fallback, invalid targets, callback ordering, and final state; resolve observed differences or document intentional differences. Then extend the matrix across state entry/update/exit, guard ordering, forced transitions, invalid contexts, lifecycle, timers, processing rates/groups, error paths, and runtime mutation.

### 4. Verify ID density and storage costs — **open design/validation gate**

The current integer branch uses resizable arrays indexed by IDs. This gives direct indexed access after registration, but capacity grows to accommodate the largest ID. Sparse or externally assigned IDs can therefore waste memory; registration-time resizing is also not a constant-time operation. The string-to-integer boundary should normally assign compact, deterministic runtime IDs, or document and test an explicit sparse-ID policy.

Do not describe the whole system as O(1) merely because an indexed lookup is O(1). Measure registration, resizing, update, transition evaluation, allocations, and realistic dense/sparse cases separately.

### 5. Establish benchmark evidence — **open**

Use reproducible, equivalent workloads for string-backed and integer-backed implementations. Record runtime/framework, number of states/transitions/instances, tick rate, warm-up, elapsed time, allocations, and correctness. Include both small workloads and larger workloads; report measured results rather than promising a speedup in advance.

### 6. Integrate deliberately — **port completed; validation and review in progress**

The integer runtime, its existing integer test suite, and the initial compiler bridge have been ported into `development`. The current-development build/test/package and documentation jobs have a recorded green result. The next gate is behavior-parity review and trace-based regression testing, then:
- run the full repository build, tests, package build, and documentation build;
- document which API surface is string-backed versus integer-backed;
- update examples and API docs; preserve the existing string API;
- review public API/versioning compatibility before any release.

## Architectural boundary to preserve

The string API remains useful for readable authoring and rapid prototyping. The integer API is a distinct execution representation, not a mandate to remove strings everywhere.

**Keep the Warehouse-backed/O(1) distribution decision separate.** The ordinary public `FSM_API` package must not silently acquire a Warehouse dependency just because this integer-backed surface is developed. Indexed integer storage, a string-to-integer compiler, and a Warehouse-backed specialized package are related but separate deliverables with separate acceptance criteria.

## Resume checklist

- [x] Fix the two mutation test failures and verify the full 400-test suite passes on the feature branch.
- [x] Record current-development build/test/package and documentation CI for the integrated runtime/compiler (run [#504](https://github.com/TrentBest/FSM_API/actions/runs/39178589802), 401 passed; latest development-head workflow [#547](https://github.com/TrentBest/FSM_API/actions/runs/39185150851) also passed).
- [ ] Complete the string/integer behavior parity matrix; resolve the newly documented Any-State/regular-transition ordering discrepancy.
- [ ] Specify compact-ID and sparse-ID behavior; test memory and resizing.
- [ ] Publish reproducible benchmark results.
- [x] Port the integer runtime and existing integer tests into `development`; current-development CI is green. The original feature branch still requires history/content reconciliation and is not a merge-ready substitute for the integrated state.
- [ ] Run full CI and review package/versioning before release.

Update this page whenever a gate changes status. Link the exact commit and successful workflow run; do not mark a gate complete from code presence alone.

---

<p align="center">
  <a href="https://github.com/TrentBest/TheSingularityWorkshop" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex converging on its central eye" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
