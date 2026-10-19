![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# FSM_API 2.0.0 — release readiness and remaining work

**Purpose:** one source-controlled checklist for taking FSM_API from the published 1.x line to the first 2.0 package with integer-backed execution. This is a release gate, not a promise that every item is already implemented.

> **Current status (2026-10-19, active integration):** the integer-backed runtime, broad integer test suite, and deterministic compiler bridge are present on `development`. The latest documentation commits are validated by [workflow #556](https://github.com/TrentBest/FSM_API/actions/runs/39194276841) and [workflow #557](https://github.com/TrentBest/FSM_API/actions/runs/39194317880): build/test/package and documentation jobs passed; coverage and package artifacts uploaded; NuGet publishing skipped. This is integration evidence, not release readiness: trace-based parity, benchmark evidence, storage/identity validation, branch reconciliation, package validation, and release remain open. Readiness remains approximately **40%**. The historical 400/400 result applies only to the original feature-branch commit.

## Release rule

- Keep the existing string-oriented API available for readable authoring, compatibility, diagnostics, and existing consumers.
- Add integer-backed execution as a deliberate runtime representation; do not claim a speedup or universal O(1) behavior until equivalent workloads have been measured.
- Do not add a Warehouse dependency to the general FSM_API package as part of this release. Compact integer indexing and a future Warehouse-backed distribution are separate decisions.
- The current published package remains the 1.x line. **Do not publish 2.0.0 until every release-blocking gate below is checked and the release is explicitly authorized.**
- Keep documentation, tests, code, package metadata, and the actual default/release branch in agreement.

## Priority legend

- **P0 — release blocker:** must be completed before 2.0.0 can ship.
- **P1 — required readiness:** must be resolved or explicitly excluded with documented rationale before release.
- **P2 — follow-up:** valuable but must not be confused with the minimum 2.0 contract.

## 0. Repository hygiene — Operation Sith

### P0 — establish the two-branch policy

- [ ] Inventory every remote branch, its latest commit, open PRs, unique commits, and unique files.
- [ ] Preserve useful work from historical and feature branches before deleting any branch; branch deletion must not be the mechanism used to decide what work matters.
- [ ] Treat `master` as the release branch and `development` as the only integration branch.
- [ ] Consolidate the useful integer-runtime work from `feature/fsm-api-2.0` into a reviewable path targeting `development`; update or replace the existing draft 2.0 PR rather than losing its tests and history.
- [ ] Reconcile the branch histories before declaring the repository clean. As of the 2026-10-19 comparison, `development` and `master` have diverged (development is 261 commits ahead and 5 behind); do not blindly merge either direction.
- [ ] After preservation and verification, remove all remote branches except `master` and `development`. Confirm the final remote branch list contains exactly those two.
- [ ] Ensure CI validates pushes and pull requests to `development` and `master`, while package publishing can occur only through the explicit release path on `master`.
- [ ] Audit stale PRs and historical links before closing or retargeting them; preserve the meaningful technical record in this checklist or in the resulting PR.

**Safety note:** branches currently visible include old experiments as well as `feature/fsm-api-2.0`, which contains the integer runtime and tests. Do not delete any of them until their useful work has been preserved and verified.

## 1. Recover a clean 1.x baseline and release boundary

### P0

- [ ] Inspect the published 1.x contract and compare it against both `master` and `development`; identify intentional behavior changes versus accidental drift.
- [ ] Resolve the `IStateContext.IsValid` lifecycle discrepancy across source, tests, README, guides, and release metadata. The intended development behavior is that invalidity suspends scheduled processing without automatically unregistering the handle; permanent removal is explicit. Decide and test the contract before merging it to `master`.
- [ ] Review all public API changes accumulated since the last published package: builder behavior, fallback-state behavior, handle lifecycle, processing groups, deferred mutation, errors, and context validity.
- [ ] Establish a tagged/commit-pinned baseline of current 1.x behavior and tests so parity comparisons have a reliable reference.
- [ ] Confirm target frameworks, nullable/implicit-using choices, package assets, license, icon, README, release notes, and XML documentation are accurate. Current project metadata advertises multiple legacy targets; verify that every target is still buildable and intentionally supported for 2.0.0.
- [ ] Ensure no unrelated refactor or ecosystem integration is silently bundled into the integer-backed release.

## 2. Define the 2.0 integer contract

### P0

- [ ] Document the two representations and their boundary: human-readable names for authoring/diagnostics, compact integer identifiers for execution.
- [ ] Specify identity scope separately for FSM definitions, states, processing groups, transitions, and instances/handles. Define whether IDs are stable across builds, persisted data, and process restarts.
- [ ] Define duplicate-name, unknown-name, missing-ID, invalid-ID, overflow, and ID-collision behavior.
- [ ] Decide who allocates IDs and when. Runtime hot paths must not fall back to repeated string hashing/lookups merely to make the integer API appear complete.
- [ ] Define how IDs map back to names for diagnostics, debugging, traces, and living documentation.
- [ ] Specify whether public callers can supply IDs or whether a deterministic compiler/registry owns allocation. Prevent accidental dependence on `.GetHashCode()` as a persistent or cross-process identity.
- [ ] Define the supported compatibility contract: preserve existing string APIs where possible; explicitly document any intentional breaking changes.
- [ ] Document thread-safety and ownership boundaries. Deferred mutation is not a blanket guarantee that arbitrary contexts or registry operations can be accessed concurrently without coordination.

## 3. Integrate the integer runtime without losing work

### P0

- [ ] Review the implementation in `feature/fsm-api-2.0/FSM_API/IntegerBacked` and its associated tests against current `development`, not only against the older feature-branch base.
- [x] Port the integer runtime and its existing test suite into `development` without replacing the established string-backed implementation. Integration validation is still pending.
- [ ] Resolve namespace/type naming, project inclusion, public API surface, and package-target compatibility.
- [ ] Preserve the already added regression coverage for self-removal during ticking and for newly created handles not running until the next tick.
- [ ] Review collection mutation, handle registration, disposal, reset, state/definition destruction, and deferred-operation ordering in both implementations.
- [ ] Keep engine-specific types out of the core package and avoid introducing dependencies upward into FSM_COS or application layers.
- [ ] Run the full repository build and tests after each integration tranche; record the exact commit and run link in this document.

## 4. Complete the name-to-ID bridge

### P0

- [x] Implement a deterministic compiler bridge in `FSM_API/IntegerBacked/FSMCompiler.cs`. It assigns dense IDs by ordinal state-name order, preserves actions/conditions, and returns reverse diagnostic mappings. Validate this implementation and close any parity gaps before marking the bridge gate complete.
- [ ] Ensure string-authored definitions can be compiled into integer-backed definitions without semantic loss.
- [x] Provide immutable forward and reverse state-name mappings in `FSMCompilationResult`; add runtime/diagnostic examples and verify reconstruction/versioning separately.
- [ ] Complete compiler edge-case coverage: duplicate/replaced authored names, unknown source and destination states, missing initial states, any-state transitions, invalid definitions, and mapping reconstruction. Focused tests now cover unknown source/destination states, missing initial state, any-state conversion, deterministic dense IDs, and read-only forward/reverse mappings; they still require execution on current `development`.
- [ ] Specify serialization/versioning behavior for IDs. Never assume an implementation-generated ID is a durable save-game or wire-protocol identity unless the contract guarantees it.
- [ ] Verify the integer execution path can run without name lookup in the tick/transition hot path.
- [ ] Add examples demonstrating authoring once, compiling, inspecting, and executing.

## 5. Prove behavior parity

### P0

- [ ] Create a traceable parity matrix mapping each supported string-backed behavior to an integer-backed test.
- [ ] Cover initial state selection and entry semantics, including empty definitions and invalid/missing initial states.
- [ ] Cover update ordering, state entry/update/exit callbacks, transition guard ordering, regular and any-state transitions, and transition selection when several guards are true. **Parity gate:** source inspection and existing tests are not enough to establish equivalent transition semantics. Add equivalent trace-based tests for Any-State precedence, regular-transition fallback, invalid targets, and lifecycle timing; resolve any observed divergence or document an intentional difference.
- [ ] Cover timers, process rates, processing groups, manual/event-driven ticking, and tick counters.
- [ ] Cover handle identity, context association, current state, entered-state flags, reset, pause/resume where supported, destruction, unregistration, and definition removal.
- [ ] Cover invalid-context suspension/resumption and explicit permanent removal consistently with the agreed contract.
- [ ] Cover mutation from callbacks: add/remove handles, add/remove states and transitions, destroy definitions, deferred operations, self-removal, removal of adjacent handles, and creation during a tick.
- [ ] Cover error thresholds, exceptions in guards/actions, cleanup behavior, and diagnostic output without asserting a blanket exception-containment guarantee.
- [ ] Cover null/invalid arguments, duplicate registrations, missing groups/definitions/states, and boundary values.
- [ ] Use equivalent inputs and compare observable traces/results, not only whether both implementations return without throwing.
- [ ] Require all parity gaps to be fixed or explicitly listed as intentional API differences in the public migration guide.

## 6. Validate integer identity and storage

### P0

- [ ] Decide whether runtime IDs are compact and dense, sparse but bounded, or caller-assigned; document the chosen invariant.
- [ ] Measure the memory and resize costs of the current array-backed storage for small, typical, large, and sparse-ID workloads.
- [ ] Test capacity growth, maximum ID, invalid indices, ID reuse, removal, and registration failure.
- [ ] Avoid claiming the whole runtime is O(1) based on indexed lookup alone. State exactly which operations have constant-time lookup and which involve growth, scans, callbacks, or allocation.
- [ ] Measure whether ID allocation and registration are outside the steady-state hot path.
- [ ] Keep future Warehouse/DataShelves/alive-dirty-bit integration as a separate architecture gate unless explicitly scoped into this package version.

## 7. Performance evidence

### P0

- [ ] Establish a reproducible benchmark project or documented executable benchmark path that builds from the repository.
- [ ] Compare string-backed and integer-backed implementations using equivalent workloads and correctness checks.
- [ ] Include state count, transition count, handle count, processing-group count, update cadence, runtime/framework, hardware, warm-up, iteration count, and build configuration.
- [ ] Measure definition build/registration, instance creation/destruction, lookup, steady-state ticking, transition evaluation, processing-group dispatch, mutation, allocations, and memory footprint separately.
- [ ] Include small and large workloads; include realistic dense-ID and sparse-ID cases if sparse IDs are supported.
- [ ] Re-run the known string-backed baselines (including the prior approximately 305 ns / 360 B single-group Update observation and the larger 50-group case) under controlled, documented conditions before comparing them to integer-backed numbers. Treat historical numbers as observations, not guarantees.
- [ ] Publish results, methodology, caveats, and raw output in versioned documentation. Do not promise a speedup unless the measurements support it.

## 8. Tests, CI, static checks, and package validation

### P0

- [ ] Run clean Release builds and the complete test suite across every supported target framework or document a justified target-specific limitation.
- [ ] Ensure warnings and errors are resolved; review obsolete APIs, XML documentation warnings, nullable assumptions, and analyzer findings.
- [ ] Validate documentation links and generate the DocFX site/artifact.
- [ ] Verify test coverage includes every changed coverable line and that the integer suite is included in normal CI, not only a manually selected job.
- [ ] Verify build artifacts contain the expected assemblies, XML docs, README, icon, and required documentation.
- [ ] Inspect the packed `.nupkg` contents and package metadata before any publication.
- [ ] Confirm publish workflow has a single deliberate release gate; ordinary development pushes and PR builds must never publish.
- [ ] Confirm trusted publishing/credentials and release permissions match the other Workshop packages without enabling accidental releases.
- [ ] Record CI run URLs and outcomes here. A green build on the isolated feature branch is evidence for that commit only, not proof that the merged release candidate is green.

## 9. Documentation and living proof

### P0

- [ ] Align README, NuGet readme, quick start, API reference, design notes, FAQ, XML comments, migration guide, and release notes with the implementation that will ship.
- [ ] Add a concise 1.x-to-2.0 migration guide with old/new examples and explicit compatibility notes.
- [ ] Add a real, executable example that demonstrates name-based authoring, deterministic integer compilation, runtime execution, and readable state diagnostics.
- [ ] Keep diagrams tied to tests/source. Clearly label conceptual animation as illustrative; only call a visual “live” if it is actually driven by the implementation or generated test/benchmark data.
- [ ] Update the integer-backing progress map and visual coverage inventory as gates change; do not use unsupported percentage-complete claims.
- [ ] Document what is intentionally deferred after 2.0.0, including any Warehouse-specific distribution or O(1) population architecture.

## 10. Downstream integration and ecosystem check

### P1

- [ ] Verify representative consumers can continue using the string-backed API unchanged.
- [ ] Add an integration test or sample proving both authoring paths can coexist without ambiguity.
- [ ] Audit dependent packages and repositories for assumptions about string identifiers, handle types, state names, package version, and target frameworks.
- [ ] Update dependencies only after the candidate package contract is stable; do not force unrelated dependent-package updates before the core contract is known.
- [ ] Confirm FSM_API remains a low-level, framework-agnostic behavior substrate and that composition/arbitration responsibilities remain in higher layers such as FSM_COS.

## 11. Release candidate and publication

### P0

- [ ] Merge only reviewed, tested work from `development` to `master` through the agreed release path.
- [ ] Confirm the two branches are synchronized according to policy and no feature branches are required to reconstruct the release.
- [ ] Select and verify the final semantic version (`2.0.0` unless release policy is deliberately changed); confirm package metadata and release notes agree.
- [ ] Build and inspect the exact candidate package from the exact release commit.
- [ ] Run final clean CI on the release commit and verify the publish job is gated as intended.
- [ ] Review the final diff against the 1.x baseline and confirm every public breaking change is documented.
- [ ] Obtain explicit release authorization before publishing to NuGet.
- [ ] After publication, verify the package page, package contents, version, README, symbols/XML docs as applicable, and a clean consumer install/build.
- [ ] Tag the release commit, record the package URL and CI evidence, and update this checklist with the actual release outcome.

## 12. Explicitly out of scope unless promoted

### P2

- [ ] Warehouse-backed O(1) distribution, DataShelves/page alignment, alive/dirty-bit tracking, or other alternate storage backends.
- [ ] Broader hierarchical runtime restructuring/arbitration features that belong in a separately validated design.
- [ ] New host-engine integrations or application-specific features.
- [ ] Speculative API expansion not required for integer identity, parity, diagnostics, or measured performance.

## Current evidence and working references

- [Integer-backing progress map](Design/FSM_API/Integer_Backing_Progress.md)
- [Visual coverage inventory](Design/FSM_API/Visual_Coverage.md)
- [Integer-backed implementation branch](https://github.com/TrentBest/FSM_API/tree/feature/fsm-api-2.0)
- [Draft FSM_API 2.0 PR #18](https://github.com/TrentBest/FSM_API/pull/18)
- [Current development branch](https://github.com/TrentBest/FSM_API/tree/development)
- [Release branch](https://github.com/TrentBest/FSM_API/tree/master)

### Checklist maintenance checkpoint (2026-10-18)

- Re-read the authoritative checklist and the integer-backing progress map on `development` before continuing implementation.
- No release gate was promoted based on source-file presence alone. Current-development build/test/package and documentation CI is now recorded as green (runs #504 and #547 below); parity, storage, benchmarks, branch reconciliation, package validation, and release remain open.
- Keep the checklist as the source of truth; update both documents whenever implementation or validation evidence changes, and record the exact commit plus workflow URL before closing a validation gate.

### Follow-up audit (2026-10-18, after locating this checklist)

- [x] Located and read this authoritative readiness document and the linked integer-backing progress map on `development`.
- [x] Corrected the workflow's NuGet publish condition on `development` so the explicit boolean publish gate can only publish from `master`, the designated release branch. Change: [`592cead`](https://github.com/TrentBest/FSM_API/commit/592ceada0b51ad9615e68e2c5d707c2949d1b729). This is a safety correction, **not** evidence that the workflow has passed CI; validate it in the next development workflow run.
- [x] Current-`development` CI for the integer runtime/compiler integration passed on commit [`e6c2638`](https://github.com/TrentBest/FSM_API/commit/e6c2638935f2e478ab9807dcc9cfca12ca733f8a): [workflow run #504](https://github.com/TrentBest/FSM_API/actions/runs/39178589802) completed successfully on 2026-10-18. The build/test/package job passed 401 tests (0 failed, 0 skipped); the documentation job passed; the NuGet publish job was skipped. This validates that commit only, not 2.0 release readiness.
- [ ] Resolve the documented Any-State versus regular-transition ordering discrepancy with trace-based parity tests before declaring behavior parity.
- [ ] Reconcile the current branch history carefully. Latest comparison snapshot reports `development` 217 commits ahead and 5 behind `master`; preserve unique commits before any history-changing action or branch deletion.
### Initial audit findings (2026-10-18)

- [x] Located an existing integer-backed implementation and broad tests on `feature/fsm-api-2.0`; latest recorded feature-branch CI reported 400/400 passing.
- [x] Located existing progress documentation describing the missing compiler/bridge, incomplete parity, storage-density, benchmark, and integration gates.
- [ ] Reconcile `master` and `development` histories: the branch comparison reports 169 commits ahead and 5 behind for development at audit time.
- [ ] Consolidate and validate feature work on current `development` before any branch deletion.
- [ ] Re-check every checklist status against source, tests, and CI before marking release-ready.

---

<p align="center">
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em>
</p>


### Live branch and PR audit update (2026-10-18, latest connector inspection)

The 2026-10-19 live comparison supersedes the earlier count above: `development` is **257 commits ahead and 5 behind `master`** (merge base `97956eb9b24f4f2d34bba9ccdc3bd7766e9f74aa`). The histories remain divergent; no merge or deletion is justified from counts alone.

Remote branch inventory returned 16 branches:
- Protected intended branches: `master`, `development`.
- Historical/feature branches to preserve and audit before any deletion: `AddingUnitTests`, `HireMeFSMDemo`, `RefactoringToHashBacking`, `Splitting-FSM_API-class-into-multiple-partial-classes`, `TargetingStarcraft2DotNet`, `TickCountingSystem`, `WindowsFormsEditor`, `docs/benchmarking`, `docs/readme-standard`, `feature/fsm-api-2.0-next`, `feature/fsm-api-2.0-registry`, `feature/fsm-api-2.0-work`, `feature/fsm-api-2.0`, `feature/integer-backed-runtime`.

Initial comparison against `development` reports these unique commit/file deltas (counts are comparison evidence, not proof that every change is or is not preserved):
- `AddingUnitTests`: 2 commits / 5 changed files.
- `HireMeFSMDemo`: 1 / 1.
- `RefactoringToHashBacking`: 2 / 7.
- `Splitting-FSM_API-class-into-multiple-partial-classes`: 0 / 0 (524 commits behind).
- `TargetingStarcraft2DotNet`: 0 / 0 (491 behind).
- `TickCountingSystem`: 0 / 0 (360 behind).
- `WindowsFormsEditor`: 2 / 16; contains a standalone Windows Forms editor project and is not assumed to belong in the core 2.0 package.
- `docs/benchmarking`: 3 / 3, including `Documentation/Benchmarking.md` and README changes.
- `docs/readme-standard`: 4 / 3, including the readiness checklist and README changes.
- `feature/fsm-api-2.0-next`, `feature/fsm-api-2.0-registry`, and `feature/fsm-api-2.0-work`: each 24 / 64.
- `feature/fsm-api-2.0`: 93 / 90; this is the source of the original integer runtime and draft PR #18.
- `feature/integer-backed-runtime`: 1 / 2.

Open PR audit:
- [#18 — FSM_API 2.0 integer-backed runtime](https://github.com/TrentBest/FSM_API/pull/18) remains **open and draft**, with base `development`, head `feature/fsm-api-2.0`, head commit `dd96646a3718923bb87a48af695a0c5b6819db8e`, 93 commits and 90 changed files. Its feature-branch 400/400 run is not current-`development` integration proof.
- [#20 — benchmark documentation](https://github.com/TrentBest/FSM_API/pull/20) and [#22 — README alignment](https://github.com/TrentBest/FSM_API/pull/22) also appear in the open-PR search and need their bases/heads reviewed before any closure or retargeting.

**Still open:** this is a first-pass inventory, not a completed Operation Sith gate. Exact current head SHAs for every branch, open-PR linkage for each unique branch, unique file content comparison against `development`, and preservation of any still-useful material must be completed before branch deletion. No branch has been deleted, no PR has been merged/closed/retargeted, and `master` has not been changed.


### Current development CI verification (2026-10-18)

- [x] Confirmed [workflow run #504](https://github.com/TrentBest/FSM_API/actions/runs/39178589802) for commit [`e6c2638`](https://github.com/TrentBest/FSM_API/commit/e6c2638935f2e478ab9807dcc9cfca12ca733f8a) completed with overall conclusion **success**.
- [x] `build_test_and_package`: success; 401 tests passed, 0 failed, 0 skipped. The test-fixture correction assigns `InitialState = "Missing"` after `AddState`, because adding the first state establishes the default initial state.
- [x] `build_documentation`: success.
- [x] `publish_nuget`: skipped; no package was published.
- [ ] Add trace-based parity coverage for string-backed and integer-backed transition/lifecycle ordering. In particular, decide and test the intended contract when Any-State and state-specific transitions both match; existing runtimes differ here.
- [ ] Continue PR and branch-history reconciliation. This successful run validates the recorded development commit only and does not authorize merge, branch deletion, or release.


### PR #18 initial code-review findings (2026-10-18)

The first pass over [PR #18](https://github.com/TrentBest/FSM_API/pull/18) found changes that must be resolved before considering it merge-ready:

- The PR changes the package version from `1.0.13` to `1.0.26` while presenting itself as the integer-backed 2.0 work. This is not a 2.0.0 release candidate and needs an intentional versioning decision.
- It renames the public static facade from `FSM_API` to `FsmApi` across many files, which appears to be a broad source-compatibility break unrelated to the narrow integer-runtime boundary. Confirm the migration intent and compatibility policy before accepting it.
- It adds `Context_ID` to public `IContext`, and the integer path relies on integer IDs. This changes the public interface contract and needs a documented migration story; do not treat it as an implementation-only detail.
- The PR modifies the publish workflow with an explicit `&& false` and changes CI triggers. Keep publishing disabled unless release authorization is given, but reconcile its workflow changes against the deliberate master-only/manual-boolean policy already recorded here.
- The PR's 90-file footprint includes many legacy API/test edits alongside the new `IntegerBacked` implementation. Review must establish which edits are required for integer backing and which are unrelated refactoring before merge.

These are review blockers/questions, not a claim that every change is necessarily incorrect. PR #18 remains open and draft; no merge, retarget, or closure was performed. The successful current-development run #504 does not validate this PR's head commit.


### Publication and branch comparison refresh (2026-10-18)

- [x] Refreshed the live `master...development` comparison: `development` is **217 commits ahead and 5 behind** `master`; merge base remains `97956eb9b24f4f2d34bba9ccdc3bd7766e9f74aa`. This supersedes the prior 214-ahead snapshot above.
- [x] Re-read the workflow files on both branches. On `development`, NuGet publishing requires a manual dispatch, explicit `inputs.publish == true`, and `github.ref == 'refs/heads/master'`. On `master`, the publish job remains explicitly disabled by an additional `&& false`; this is a safety state, not a release-ready publishing configuration. No package was published.
- [ ] Before any release, reconcile the workflow policy deliberately: preserve the explicit boolean confirmation and master-only release boundary, then validate the resulting workflow in a real run. Do not remove the master-side disabled gate without explicit release authorization.
- [ ] PR #18 (integer-backed runtime), PR #20 (benchmark documentation), and PR #22 (README alignment) are still open in the latest search; #18 and #20 are drafts. Review each PR's exact base/head and changed-file overlap before merge, retarget, or closure. No PR was merged or closed during this audit.
- [x] Current-`development` CI verified for commit `e6c2638935f2e478ab9807dcc9cfca12ca733f8a`: [run #504](https://github.com/TrentBest/FSM_API/actions/runs/39178589802) completed successfully; build/test/package and documentation jobs passed, with 401 tests passing and publish skipped. This does not certify a release commit.


### Runtime transition source review (2026-10-18)

- [x] Compared the current string-backed `FSM.Step` with `IntegerBacked.FSM.Step`.
- [x] Confirmed a concrete semantic mismatch: the integer runtime evaluates the current state's `Update`, then takes the **first matching valid Any-State transition** and returns before checking regular transitions. The string runtime evaluates `Update`, then continues through all Any-State transitions without returning; it can call the old state's `Exit` multiple times, overwrite the handle's state more than once, and then evaluate a regular transition from the original state and overwrite the result again.
- [ ] Do not patch this solely to make source look alike. Add trace-based tests that characterize callback order and transition selection on both paths; define the compatibility contract and expected single-step semantics, then align the implementation to that contract.
- [ ] Extend parity tests beyond this transition case to include initial entry, manual condition evaluation, forced transitions, exception behavior, and handle lifecycle. The current `IntegerParityTests` has focused integer-only assertions but is not yet a cross-runtime trace parity suite.
- [ ] Review the existing string `FSMHandle.Update` / scheduler lifecycle before designing the cross-runtime harness, because the string runtime's `Step` updates the handle and defers entry handling through the handle's entered-state flag, whereas integer `Step` directly calls `Enter`/ `Exit`.

This is a source-level finding only; no runtime behavior was changed in this review. Preserve string API compatibility unless a deliberate breaking change is explicitly approved.


### Any-State transition fix and regression coverage (2026-10-19)

- [x] Updated the string-backed `FSM.Step` to assign the matching Any-State target to `next` and return immediately after the first matching valid Any-State transition. This preserves Any-State precedence, prevents subsequent Any-State guards from firing, prevents regular-transition overwrite, and lets `FSMHandle.Update` retain the selected target.
- [x] Added `Tests/FSM_API_AnyStateTransition_Tests.cs` to assert first-match behavior, Any-State precedence over a matching regular transition, resulting handle state, and callback trace.
- [x] Investigated the first regression-test CI failure: the test used `Transition(FSM.AnyStateIdentifier, ...)`, which routes through the regular-transition builder API. The supported explicit API is `AnyTransition(to, condition)`; the test now uses that API in commit `a4b016f`.
- [x] The corrected regression test and runtime fix passed the build/test/package job in [CI run #516](https://github.com/TrentBest/FSM_API/actions/runs/39179228239) for commit `f0f78b0`; documentation validation was still in progress at checklist update time, and publishing was skipped. Earlier failures were caused by the test using the regular-transition builder API instead of `AnyTransition`. 
- [x] Added no-match fallback coverage: when the Any-State guard returns false, the regular transition fires and the expected guard/exit trace is asserted. This is included in commit `f0f78b0` and its build/test/package job passed in run #516.
- [ ] Extend coverage to invalid Any-State targets and lifecycle entry timing; inspect any test failures before further changes.
- [ ] Review whether direct `FSM.Step` invocation without a registered handle should have a defined transition-result contract; the existing implementation couples target resolution to the handle lookup.
### Deeper transition-result finding (2026-10-19)

- [x] Re-read the complete string-backed `FSM.Step` and its caller `FSMHandle.Update` rather than relying only on the transition loop.
- [x] Reviewed the Any-State transition path and its caller. Do not treat the suspected overwrite/continued-loop behavior as a confirmed discrepancy without reproducing it against the current development source. Use trace-based tests through the real handle update path to establish final state and callback ordering.
- [x] Confirmed the integer-backed `EvaluateConditions` documentation explicitly states Any-State first, then regular transitions, at most one transition per evaluation; the integer `Step` also returns after its first valid matching Any-State transition.
- [x] Added a trace-based characterization for multiple matching Any-State guards and a simultaneously matching regular transition: the first valid Any-State target wins, later Any-State and regular guards are not evaluated, and the old state exits once. The corrected regression test passed the build/test/package job in run #516.
- [x] Added a no-match characterization proving a false Any-State guard falls through to the regular transition, with callback trace assertions.
- [ ] Add a separate Any-State-only match with no regular match, invalid Any-State target coverage, and lifecycle entry-timing coverage. Keep the contract decision explicit before declaring full parity.
- [x] Regression coverage exercises the real handle update path and asserts the final `CurrentState`, so it covers the caller's state assignment as well as the transition trace.

### CI and PR follow-up review (2026-10-18)

- [x] [Workflow run #506](https://github.com/TrentBest/FSM_API/actions/runs/39178743981) completed successfully for checklist commit `c02989b`: build/test/package succeeded, documentation generation/link validation succeeded, and NuGet publishing was skipped. This run did not include the subsequent checklist-only commit `6d772d9`; the next push will trigger its own validation.
- [x] First-pass file review of [PR #20](https://github.com/TrentBest/FSM_API/pull/20): adds benchmark documentation and README material, but its only workflow change appends another `&& false` to the publish condition. The documentation should be evaluated against the companion benchmark repo's executable workload, raw output, environment, and dates; the PR remains draft and is not merged.
- [x] First-pass file review of [PR #22](https://github.com/TrentBest/FSM_API/pull/22): enables CI for both `master` and `development`, which matches the desired branch trigger direction, and proposes a README rewrite. It also adds an older, short version of this readiness checklist (38 lines) that conflicts with the authoritative expanded checklist now on `development`. Do not merge that stale checklist as-is; selectively review the README and workflow edits against current files.
- [ ] Complete detailed benchmark-claim verification against the companion benchmark repository and confirm the reported values are reproducible before adopting any performance claims.
- [ ] Reconcile PR #22's README/workflow value without replacing this authoritative readiness record; PR #20 remains subject to benchmark evidence review and a single, deliberate publish gate.


### Any-State regression follow-up (2026-10-19)

- [x] Verified the corrected test commit `f0f78b0035043fef6754c7943e1113452a38a2bf` passed the `build_test_and_package` job in [CI run #516](https://github.com/TrentBest/FSM_API/actions/runs/39179228239). At the time of this update, `build_documentation` was still in progress; `publish_nuget` was skipped.
- [x] Confirmed [run #514](https://github.com/TrentBest/FSM_API/actions/runs/39179139688), validating the prior checklist commit, completed successfully: build/test and documentation passed, publishing skipped.
- [ ] Recheck run #516 after the documentation job completes; only then mark the full workflow green.
- [ ] Continue remaining transition parity work: an Any-State-only match without any regular transition, invalid-target handling, and lifecycle entry timing. Existing regression tests establish first-match precedence and false-guard fallback, not complete string/integer parity.


### Any-State-only regression coverage (2026-10-19)

- [x] Added a focused regression test for an Any-State guard that matches when no regular transitions are defined. It asserts the handle ends the update in the selected target state and checks the exact update/guard/exit trace.
- [ ] Validate test commit [`79a71c6`](https://github.com/TrentBest/FSM_API/commit/79a71c6942f7d30d7c5843ebde5047dba6a5dbe7) in development CI before treating the new test as passing.
- [ ] Continue with invalid Any-State target behavior and lifecycle entry timing; do not infer full string/integer parity from this test alone.
- [x] Confirmed checklist commit [`0e392a1`](https://github.com/TrentBest/FSM_API/commit/0e392a14018e13c95f65218c679d5cdf675dad01) completed [CI run #517](https://github.com/TrentBest/FSM_API/actions/runs/39179333904) successfully: build/test/package and documentation passed; NuGet publishing was skipped.


### Invalid Any-State target regression (2026-10-19)

- [x] Added a regression test proving a missing Any-State target is skipped before its guard executes, and a subsequent valid Any-State transition can still match.
- [ ] Validate test commit [`e56633e`](https://github.com/TrentBest/FSM_API/commit/e56633e4184a27dd47ac3c79406bcd7396167fa8) in development CI.
- [x] For Any-State-only test commit [`79a71c6`](https://github.com/TrentBest/FSM_API/commit/79a71c6942f7d30d7c5843ebde5047dba6a5dbe7), the build/test/package job succeeded in [run #518](https://github.com/TrentBest/FSM_API/actions/runs/39179560009); documentation was still running at the time of this note, so the overall workflow is not yet marked green.
- [ ] Inspect lifecycle entry timing in implementation and tests before changing the behavior or claiming parity.

### Scheduled Any-State transition parity (2026-10-19)

- [x] Added scheduler-level regression coverage in commit [`46c68fa`](https://github.com/TrentBest/FSM_API/commit/46c68faf8ddb1933da31c9dd5637a494c2608265). CI run [#522](https://github.com/TrentBest/FSM_API/actions/runs/39179828861) reproduced the defect: 405 tests passed and the new scheduled-update test failed because the handle remained in state `A` instead of transitioning to `B`.
- [x] Fixed scheduled transition matching in commit [`10dec78`](https://github.com/TrentBest/FSM_API/commit/10dec7831dacf9e4a1e1b51db183a653ab4b5fee): scheduled ticks now match the Any-State sentinel as well as the current state, retain Any-State-first ordering from `GetAllTransitions()`, and skip missing targets before invoking guards.
- [x] Confirmed the sentinel-only attempt did not cover the builder's legacy marker (run [#523](https://github.com/TrentBest/FSM_API/actions/runs/39179884700)); the marker follow-up passed build/test/package in run [#525](https://github.com/TrentBest/FSM_API/actions/runs/39180033916). Documentation validation was still queued at the time of this update.
- [ ] Continue lifecycle parity review, including consistency between `FSM.Step` documentation and its actual execution order; do not claim full parity from the scheduler regression alone.

- [x] Follow-up inspection found that `FSMBuilder.AnyTransition` stores the legacy source marker `"*"`, while the direct FSM API also defines `FSM.AnyStateIdentifier`. The first scheduler fix recognized only the latter; CI run [#523](https://github.com/TrentBest/FSM_API/actions/runs/39179884700) confirmed the scheduled regression still failed.
- [x] Updated commit [`d32e5aa`](https://github.com/TrentBest/FSM_API/commit/d32e5aa3fbeb70652ed89a13317732dfdba4e772) to recognize both markers; build/test/package passed in run [#525](https://github.com/TrentBest/FSM_API/actions/runs/39180033916), and the full workflow passed in run [#526](https://github.com/TrentBest/FSM_API/actions/runs/39180044443). NuGet publishing was skipped.

### Manual handle entry lifecycle (2026-10-19)

- [x] Added regression coverage in commit [`045c35e`](https://github.com/TrentBest/FSM_API/commit/045c35e4ecf43ef37134226b90cc87df6e843fc2) for the documented contract that a manual first `FSMHandle.Update()` enters the initial state before updating it.
- [x] Updated [`FSMHandle.Update`](https://github.com/TrentBest/FSM_API/commit/eed09b4bbb863acf53c7ecf342ff4ca1a9cd1d36) to run the current state's entry action once before the first manual step, and synchronized the entered flag/state for `TransitionTo` and `ResetFSMInstance` so explicit transitions do not cause a duplicate entry on the next manual update.
- [x] Confirmed the new manual lifecycle regression reproduces the defect in run [#528](https://github.com/TrentBest/FSM_API/actions/runs/39180137337): 406 tests passed and the new test failed because `HasEnteredCurrentState` remained false. The fix is in commit [`eed09b4`](https://github.com/TrentBest/FSM_API/commit/eed09b4bbb863acf53c7ecf342ff4ca1a9cd1d36); its build/test/package job passed in run [#529](https://github.com/TrentBest/FSM_API/actions/runs/39180160637), with documentation validation still in progress.
- [x] Corrected `FSM.Step` XML remarks in commit [`37a80cc`](https://github.com/TrentBest/FSM_API/commit/37a80cc19621fd3d6a0332d240c1fa8c7b6bb0c9) to describe the implemented order (state Update, Any-State transitions, then regular transitions) and clarify that entry is managed by the instance lifecycle, not by `Step` itself. Documentation CI remains pending.
- [x] Updated the `FSMHandle.Update` XML remarks in commit [`a28096b`](https://github.com/TrentBest/FSM_API/commit/a28096b71f566b0f3bce9faaae9b95a48429c4fb) to document first-entry behavior and clarify that transition targets enter on the next update, while explicit forced transitions enter immediately. Validate documentation generation in CI.

### Follow-through checkpoint (2026-10-19, UTC)

- [x] Rechecked the manual-first-entry regression workflow [#529](https://github.com/TrentBest/FSM_API/actions/runs/39180160637): both `build_test_and_package` and `build_documentation` completed successfully; `publish_nuget` was skipped. This supersedes the earlier note that documentation validation was still in progress for this run.
- [x] Confirmed the preceding reproducer [#528](https://github.com/TrentBest/FSM_API/actions/runs/39180137337) failed in `Build and Test for .NET 8.0`, demonstrating the missing first-entry lifecycle behavior before the fix. The failure is retained as regression evidence, not counted as a passing run.
- [ ] Do not infer that the latest `development` head is fully green from #529: the run validated the specific fix commit `eed09b4`; newer commits require their own CI evidence. The latest inspected commit is [`cab0a89`](https://github.com/TrentBest/FSM_API/commit/cab0a8947043a0f7d6c8d2c3508d30587a789a09), whose available combined status showed Codecov patch status only, not a complete Actions run.
- [x] Refreshed the live `master...development` comparison on 2026-10-19: `development` is **251 commits ahead and 5 behind** `master`. This is a newer snapshot than the 217-ahead count recorded above; the branches remain divergent, so no merge or branch deletion is implied.
- [ ] Next technical gate remains a trace-driven lifecycle/parity audit, including entry timing after scheduled and manual transitions, reset/forced-transition semantics, invalid targets, and cross-runtime equivalence. Keep the Renderer `DestroyInstance` report on its separate reproduction track: the LLM handoff documents that the public contract does not promise arbitrary concurrent structural mutations, and no runtime synchronization patch should be made without a focused reproducer.
- [ ] Reconfirm the latest head's full CI and update this checklist with exact commit/run evidence before promoting any further release gate. No package was published; publication remains explicitly unauthorized.

### Manual condition evaluation parity follow-up (2026-10-19, UTC)

- [x] Source review identified a separate manual-evaluation gap: `FSMHandle.EvaluateConditions()` filtered transitions to `From == CurrentState`, excluding Any-State transitions despite the API documentation defining them as global and `GetAllTransitions()` returning them first.
- [x] Opened draft PR [#23](https://github.com/TrentBest/FSM_API/pull/23) on isolated branch `fix/manual-any-state-evaluation`. The proposed fix includes canonical and legacy Any-State markers, skips missing targets before invoking guards, and synchronizes `CurrentState` / `HasEnteredCurrentState` after a successful forced transition.
- [x] Added a trace-based regression test for manually evaluating an Any-State transition without regular transitions.
- [x] Initial PR #23 validation completed in [workflow run #538](https://github.com/TrentBest/FSM_API/actions/runs/39181325759): build/test/package and documentation passed; NuGet publishing was skipped. Subsequent regression-test coverage was added and validated separately in run #541.
- [ ] Review the broader manual-evaluation contract after CI: regular transition target safety, exception behavior, and parity with the scheduled path still require explicit characterization.
- [ ] This PR is not merged and does not change `master`; no package was published. Release readiness remains below 100%.


- [x] Rechecked PR #23 workflow [#538](https://github.com/TrentBest/FSM_API/actions/runs/39181325759): `build_test_and_package` and `build_documentation` both completed successfully; `publish_nuget` was skipped as intended. This validates the PR head's current build/test/package and documentation jobs.
- [x] Audited the manual-evaluation patch against `FSM.ForceTransition`: forced transitions invoke Exit and then Enter immediately, matching the explicit-transition contract; the patch skips missing targets before evaluating their guards and sets `HasEnteredCurrentState` after a successful forced transition.
- [x] Extended focused manual-evaluation tests for Any-State precedence over a true regular transition, invalid-target skip before guard invocation, and no-match behavior. The expanded test commit is [`afc21af`](https://github.com/TrentBest/FSM_API/commit/afc21af3f0c1cf23b492dcfe330ad62c87df98a7); all CI jobs are now complete in [run #541](https://github.com/TrentBest/FSM_API/actions/runs/39183015409): build/test/package and documentation passed, publish skipped.
- [ ] Review manual evaluation's exception/reporting behavior and regular-transition safety before declaring transition-path parity complete. Do not merge PR #23 or publish a package without explicit authorization.


### Expanded manual-evaluation verification (2026-10-19, UTC)

- [x] Verified test commit [`afc21af`](https://github.com/TrentBest/FSM_API/commit/afc21af3f0c1cf23b492dcfe330ad62c87df98a7) with [CI run #541](https://github.com/TrentBest/FSM_API/actions/runs/39183015409): `build_test_and_package` succeeded, `build_documentation` succeeded, and `publish_nuget` was skipped.
- [x] Confirmed the checklist's development-branch blob before this update was `61e9cca2eb7b103f27768c63b835081ff313a462`; this update records the complete PR-branch verification evidence, but does not claim the PR is merged or that development itself includes the fix.
- [ ] Characterize manual guard exception handling and regular-transition target safety, then decide whether those behaviors should match the scheduled path or be documented as distinct.
- [ ] Recheck the latest development-head CI independently. PR-branch CI is not proof that development or the release candidate is green.
- [ ] Keep PR #23 unmerged and publishing unauthorized until the remaining review and explicit release approval are complete.


### Guard-exception follow-up (2026-10-19, UTC)

- [x] Fixed the regression-test compile error by importing `System` in `Tests/FSM_API_AnyStateTransition_Tests.cs` (the test uses `InvalidOperationException`). Fix commit: [`1a40ed5`](https://github.com/TrentBest/FSM_API/commit/1a40ed512b5072a7b5434df4e7b3fb74e2296dc3).
- [x] Verified corrected PR head in [workflow run #543](https://github.com/TrentBest/FSM_API/actions/runs/39184170040): `build_test_and_package` and `build_documentation` succeeded; `publish_nuget` was skipped.
- [x] Added a regression test confirming a throwing Any-State guard is handled without aborting evaluation and does not prevent a subsequent eligible regular guard from transitioning. The test is present on the PR head validated by run #543.
- [ ] Complete the manual-evaluation audit for regular-transition target safety and any concurrency edge cases; verify latest `development`-head CI independently.
- [ ] PR #23 remains unmerged; PR CI is not proof that `development` itself contains the fix. No package publication is authorized by this checklist update.


### Live integration follow-up (2026-10-19)

- [x] Reconfirmed the integer-backed runtime is already present on `development`, including `FSM_API/IntegerBacked`, `Tests/IntegerBacked`, and the deterministic `FSMCompiler.cs` bridge. The source of the original implementation remains [`feature/fsm-api-2.0`](https://github.com/TrentBest/FSM_API/tree/feature/fsm-api-2.0).
- [x] Verified current-development workflow [#547](https://github.com/TrentBest/FSM_API/actions/runs/39185150851) on commit [`02da084`](https://github.com/TrentBest/FSM_API/commit/02da0840dd32889cc7a53250d293fcdfe80641c4) completed successfully: build/test/package and documentation jobs passed; coverage and artifacts uploaded; NuGet publish skipped.
- [x] Corrected the stale CI status in the [integer-backing progress map](Design/FSM_API/Integer_Backing_Progress.md) to distinguish current-development CI from historical feature-branch validation.
- [ ] Reconcile PR [#18](https://github.com/TrentBest/FSM_API/pull/18) and its branch history. The original feature branch compares as 93 commits ahead and 305 behind `development` in the latest comparison; do not merge it wholesale. The runtime core files compared so far have identical blob SHAs on both branches, so inspect the remaining unique commits/files for work not represented in `development`.
- [ ] Complete trace-based string/integer parity, storage/identity validation, reproducible benchmark evidence, multi-target/package review, and final release-candidate validation.
- [ ] Operation Sith remains open: preserve and classify all useful branch/PR work before any branch deletion. No merge, branch deletion, `master` modification, or package publication was performed in this audit.
