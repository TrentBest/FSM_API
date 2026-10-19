![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# FSM_API 2.0.0 — release readiness and remaining work

**Purpose:** one source-controlled checklist for taking FSM_API from the published 1.x line to the first 2.0 package with integer-backed execution. This is a release gate, not a promise that every item is already implemented.

> **Current status (2026-10-18, active integration):** the integer-backed runtime and deterministic compiler bridge are present on `development`. Current-development CI run [#504](https://github.com/TrentBest/FSM_API/actions/runs/39178589802) passed 401 tests and the documentation build for commit `e6c2638`; publishing was skipped. A newer checklist-only commit is being validated by run [#506](https://github.com/TrentBest/FSM_API/actions/runs/39178743981). This is integration evidence, not release readiness: trace-based parity, benchmark evidence, branch reconciliation, package validation, and release remain open. The historical 400/400 result applies only to the original feature-branch commit.

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
- [ ] Reconcile the branch histories before declaring the repository clean. As of this audit, `development` and `master` have diverged (development is 169 commits ahead and 5 behind at the comparison snapshot); do not blindly merge either direction.
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
- [ ] Cover update ordering, state entry/update/exit callbacks, transition guard ordering, regular and any-state transitions, and transition selection when several guards are true. **Known parity risk from source review:** both implementations run the current state's update before evaluating Any-State guards, but the string implementation continues through its regular-transition loop after an Any-State transition changes the handle, while the integer implementation takes the first Any-State transition and returns. Add a trace-based comparison and either align behavior or document an intentional difference.
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
- No release gate was promoted based on source-file presence alone. Current-branch build/test evidence is still absent from the recorded evidence, so compiler validation, parity, storage, benchmarks, branch reconciliation, package validation, and release remain open.
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

The current live comparison supersedes the earlier count above: `development` is **217 commits ahead and 5 behind `master`** (merge base `97956eb9b24f4f2d34bba9ccdc3bd7766e9f74aa`). The histories remain divergent; no merge or deletion is justified from counts alone.

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
- [ ] Await CI for the runtime fix and regression test; no local build/test execution was available in this connector pass.
- [ ] Extend coverage to no-match behavior, invalid Any-State targets, and lifecycle entry timing; inspect any test failures before further changes.
- [ ] Review whether direct `FSM.Step` invocation without a registered handle should have a defined transition-result contract; the existing implementation couples target resolution to the handle lookup.

### Deeper transition-result finding (2026-10-19)

- [x] Re-read the complete string-backed `FSM.Step` and its caller `FSMHandle.Update` rather than relying only on the transition loop.
- [x] Confirmed the Any-State discrepancy has a second consequence: the string-backed Any-State branch mutates the registered handle's `CurrentState` and `HasEnteredCurrentState`, but does not assign `next` or return. `FSMHandle.Update` then unconditionally assigns `CurrentState = nextState`, where `nextState` is still the original state unless a regular transition also matches. Thus an Any-State-only match may be overwritten by the caller; if a regular transition also matches, that transition may overwrite the Any-State target after a second Exit.
- [x] Confirmed the integer-backed `EvaluateConditions` documentation explicitly states Any-State first, then regular transitions, at most one transition per evaluation; the integer `Step` also returns after its first valid matching Any-State transition.
- [ ] Add characterization tests for: one Any-State match with no regular match; multiple matching Any-State guards; Any-State and regular guards both matching; and exact Update/guard/Exit/Enter trace. Keep the contract decision explicit before implementation changes.
- [ ] Test through the real string `FSMHandle.Update` path as well as direct blueprint evaluation, because the caller's unconditional state assignment is part of the observed behavior.

### CI and PR follow-up review (2026-10-18)

- [x] [Workflow run #506](https://github.com/TrentBest/FSM_API/actions/runs/39178743981) completed successfully for checklist commit `c02989b`: build/test/package succeeded, documentation generation/link validation succeeded, and NuGet publishing was skipped. This run did not include the subsequent checklist-only commit `6d772d9`; the next push will trigger its own validation.
- [x] First-pass file review of [PR #20](https://github.com/TrentBest/FSM_API/pull/20): adds benchmark documentation and README material, but its only workflow change appends another `&& false` to the publish condition. The documentation should be evaluated against the companion benchmark repo's executable workload, raw output, environment, and dates; the PR remains draft and is not merged.
- [x] First-pass file review of [PR #22](https://github.com/TrentBest/FSM_API/pull/22): enables CI for both `master` and `development`, which matches the desired branch trigger direction, and proposes a README rewrite. It also adds an older, short version of this readiness checklist (38 lines) that conflicts with the authoritative expanded checklist now on `development`. Do not merge that stale checklist as-is; selectively review the README and workflow edits against current files.
- [ ] Complete detailed benchmark-claim verification against the companion benchmark repository and confirm the reported values are reproducible before adopting any performance claims.
- [ ] Reconcile PR #22's README/workflow value without replacing this authoritative readiness record; PR #20 remains subject to benchmark evidence review and a single, deliberate publish gate.
