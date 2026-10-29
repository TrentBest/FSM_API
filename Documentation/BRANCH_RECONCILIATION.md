# FSM_API Branch Reconciliation Ledger

**Purpose:** Reconcile pre-Sith-policy branches deliberately before reducing the repository to the two enduring branches, `master` and `development`.

**Policy:** Preserve unique value before retirement. This ledger is an audit record, not permission to merge or delete branches. Trent retains responsibility for manual deletion once the ledger explicitly marks the repository ready.

**Audit baseline:** comparisons below use `development` as the base branch. Counts are GitHub compare results observed during the 2026-10-25 audit. A branch can be far behind and still contain valuable unique commits. Do not merge an entire divergent branch merely to preserve a subset.

## Branch inventory

| Branch | Compare with development | Unique content observed | Reconciliation disposition |
|---|---:|---|---|
| `master` | Protected end-state branch | Current published/release baseline | Keep as enduring branch; no changes made by this audit. |
| `development` | Baseline | Current integration target | Keep as enduring branch; audit and reconcile here. |
| `feature/fsm-api-2.0` | 93 ahead / 368 behind; 90 changed files | Broad 2.0 implementation; string-runtime changes, IntegerBacked implementation and tests | **Primary implementation source to review**, not a wholesale merge candidate. Reconcile selectively against development and sibling feature branches. |
| `feature/fsm-api-2.0-work` | 24 ahead / 368 behind; 64 changed files | Integer-backed types/runtime and tests; shared merge base with next/registry variants | Pairwise compare metadata reported one commit of divergence in each direction, but direct file fetch now confirms the workflow file has the exact same blob SHA (`730cd4dd444dae2a6de89f2f53af37b9499cdcc5`) on all three branches. No workflow-content difference exists among these refs. The shared implementation/test footprint is therefore effectively duplicated; confirm exact head ancestry/commit metadata before choosing a canonical ref. |
| `feature/fsm-api-2.0-next` | 24 ahead / 368 behind; 64 changed files | Same reported implementation/test change set as work/registry variants | Direct fetch confirms the workflow content is identical to the other two variants (blob `730cd4dd444dae2a6de89f2f53af37b9499cdcc5`). Exact head/ancestry verification remains before selecting a canonical feature branch. |
| `feature/fsm-api-2.0-registry` | 24 ahead / 368 behind; 64 changed files | Same reported implementation/test change set as work/next variants | Direct fetch confirms the workflow content is identical to the other two variants (blob `730cd4dd444dae2a6de89f2f53af37b9499cdcc5`). The branch name alone does not prove a distinct registry implementation; exact head/ancestry verification remains. |
| `feature/integer-backed-runtime` | 1 ahead / 366 behind; 2 changed files relative to development; 3 commits relative to feature/fsm-api-2.0 | Adds `FSM_API/IntegerBacked/FSMRegistry.cs` and `Tests/IncrementalVersionTests.cs`; also changes project version | **Must be reviewed explicitly.** The registry is an older dictionary-backed design, distinct from the direct-indexed runtime. Search history shows a later commit, `a30c8ff8`, titled “fix: remove incomplete integer-backed registry scaffold”; therefore preserve the design/history as evidence, but do not merge the registry blindly. The incremental test is a version-provenance marker, not behavioral coverage. |
| `RefactoringToHashBacking` | 2 ahead / 368 behind; 7 changed files | Hash-oriented changes in FSM, handle, state, transition, interaction, and context | Review as historical design evidence. Compare its behavior and tests with the 2.0 canonical-ID design; preserve any useful collision-safe lookup insight, but do not merge wholesale. |
| `fix/manual-any-state-evaluation` | 15 ahead / 70 behind; 3 changed files | Changes FSM/handle and any-state transition tests | **Behavioral fix candidate.** Inspect commit diffs and tests against current 2.0 behavior; port only if still applicable and covered by tests. |
| `docs/readme-standard` | 4 ahead / 322 behind; 3 changed files | README, `Documentation/FSM_API_2.0.0_READINESS.md`, workflow | **Documentation preservation candidate.** Readiness tracker is especially relevant; compare its claims with current source before adopting. |
| `docs/benchmarking` | 3 ahead / 361 behind; 3 changed files | Benchmark documentation and README/workflow edits | Preserve benchmark methodology and measured results only if reproducible and still representative. |
| `AddingUnitTests` | 2 ahead / 472 behind; 5 changed files | Additional/changed tests and project configuration | Inspect exact test additions and coverage before retirement; port any non-duplicate tests. |
| `WindowsFormsEditor` | 2 ahead / 565 behind; 16 changed files | Separate Windows Forms editor project and UI source/resources | Historical editor appears distinct from core runtime. Decide whether to archive/document its status or preserve a reference; do not pull UI dependencies into the core package. |
| `HireMeFSMDemo` | 1 ahead / 577 behind; workflow-only diff | Workflow change | Review whether the workflow change has any current value; otherwise likely retirement candidate after evidence check. |
| `Splitting-FSM_API-class-into-multiple-partial-classes` | 0 ahead / 632 behind; no unique commits | No commits unique relative to development | Candidate for retirement after final head/ancestry confirmation. |
| `TargetingStarcraft2DotNet` | 0 ahead / 599 behind; no unique commits | No commits unique relative to development | Candidate for retirement after final head/ancestry confirmation. |
| `TickCountingSystem` | 0 ahead / 468 behind; no unique commits | No commits unique relative to development | Candidate for retirement after final head/ancestry confirmation. |

## Integer integration findings

The integer integration branch must not be conflated with the current IntegerBacked runtime:

- `feature/fsm-api-2.0` contains a direct-indexed `FSMRuntime` and a separate `IntegerBacked.FsmApi` facade.
- `feature/integer-backed-runtime` contains an older `FSMRegistry` keyed by dictionaries and a different handle type. It rejects duplicate definitions rather than replacing them, returns read-only views of instances, and has a static registry lifetime. These are different lifecycle and concurrency semantics, not just alternative storage syntax.
- The registry code has no visible synchronization around its static dictionaries/lists or handle counter. Its `Clear()` resets the handle ID counter, which merits stale-handle/ID-reuse review if this design is ever revived.
- The incremental-version test asserts a constant string against itself. It records a claimed version marker but does not verify the package's actual assembly/package version or runtime behavior.
- Search history includes commit `8a4cda4d` (“Add integer-backed FSM registry”) and later `a30c8ff8` (“fix: remove incomplete integer-backed registry scaffold”). Treat the old registry as a historical design candidate, not an automatically approved implementation.

## Required gates before manual branch deletion

- [ ] Confirm exact head SHAs and ancestry for `feature/fsm-api-2.0-work`, `feature/fsm-api-2.0-next`, and `feature/fsm-api-2.0-registry`. Their workflow file content is identical by blob SHA, so the remaining decision is commit ancestry/canonical-reference choice, not workflow content.
- [ ] Inspect and reconcile all 93 unique commits / 90-file change set from `feature/fsm-api-2.0` against current `development`; document what is ported, superseded, or intentionally excluded.
- [ ] Inspect any-state fix branch and port relevant behavioral fixes/tests if still valid.
- [ ] Inspect test-only, benchmark, README/readiness, and Windows Forms branches for unique useful artifacts.
- [ ] Resolve the integer registry versus direct-indexed runtime design explicitly; test sparse IDs, memory growth, thread safety, lifecycle, handle-ID wrap/reuse, and string/integer API parity.
- [ ] Confirm core remains dependency-free and engine-agnostic.
- [ ] Run the relevant build/test/CI checks after any port; do not infer success from branch comparison alone.
- [ ] Update this ledger with commit SHAs, exact file/test dispositions, CI evidence, and a clear **READY FOR MANUAL BRANCH DELETION** status only after all gates are satisfied.

## Follow-up audit notes (2026-10-25)

- Direct fetch of `.github/workflows/dotnet.yml` from `feature/fsm-api-2.0-work`, `feature/fsm-api-2.0-next`, and `feature/fsm-api-2.0-registry` returned the same blob SHA, `730cd4dd444dae2a6de89f2f53af37b9499cdcc5`. The earlier statement that the branches had workflow differences is corrected: their file contents are identical; commit-level ancestry still needs exact verification.
- Re-ran compares against current `development`: `fix/manual-any-state-evaluation` is 15 ahead / 72 behind and changes `FSM_API/FSM.cs`, `FSM_API/FSMHandle.cs`, and `Tests/FSM_API_AnyStateTransition_Tests.cs` (230 added test lines). Current `development` already has the first six test methods in that file; the branch's genuinely additional tests are `EvaluateConditions_IncludesAnyStateTransitions_AndUpdatesHandle`, `EvaluateConditions_AnyStateMatchPreemptsTrueRegularTransition`, `EvaluateConditions_InvalidAnyStateTargetIsSkippedBeforeGuard`, `EvaluateConditions_NoMatchingTransitionLeavesStateUnchanged`, `EvaluateConditions_SkipsTargetRemovedByGuard`, `EvaluateConditions_DoesNotRecordTargetRemovedByExitCallback`, `Step_DoesNotRecordTargetRemovedByExitCallback`, and `EvaluateConditions_ContinuesAfterThrowingAnyStateGuard`. These are valuable candidate regressions. Inspect each against current `FSMHandle`/`FSM` behavior and port tests first where they represent intended contracts; the old source implementation is divergent and must not be copied wholesale.
- `AddingUnitTests` is 2 ahead / 474 behind and changes interaction/internal API, project configuration, and `FSM_API_FSMState_Tests.cs` (104 added lines). Its state tests include invalid-name validation, callback setter invocation/null handling, and `ToString` formatting; current `development` already tests constructor validation and direct Enter/Update/Exit actions, so port only distinct assertions after checking current API names and intended public surface.
- `docs/benchmarking` is 3 ahead / 363 behind and adds `Documentation/Benchmarking.md` (181 lines) plus README/workflow edits. The document links the executable `FSM_API_Benchmark` companion repository and dates the 1/10/50-group results (305.1 ns / 360 B; 3,115.7 ns / 3,600 B; 15,736.6 ns / 18,000 B) to September 2026 against FSM_API 1.0.13. Preserve its evidence-chain/methodology, but rerun before presenting these as representative of 2.0.
- `RefactoringToHashBacking` is 2 ahead / 370 behind and edits six runtime source files plus workflow. It includes both string dictionaries and integer-ID collections; this is evidence of an incomplete hybrid approach, not proof of collision-safe identity. Do not merge wholesale.
- `WindowsFormsEditor` adds a standalone `net8.0-windows` WinForms project referencing released FSM_API 1.0.5. Keep it outside the dependency-free core; decide whether its source is worth preserving as a separate sample/archive artifact before branch retirement.

## Current status

**Not ready for manual branch deletion.** This is the first inventory pass; several branches contain unreviewed unique changes, and the three 24-commit 2.0 variants still need exact-head verification. No branches have been merged or deleted by this audit. No package has been published.


## Follow-up audit notes (2026-10-25, readiness document and integer-branch history)

- Read `Documentation/FSM_API_2.0.0_READINESS.md` from `docs/readme-standard` (blob `906e21c7d937d1639c2774aec7658b7a2c3d7318`). The checklist is appropriately cautious: it does not promise a public selector/API shape or performance before implementation and tests verify them; it calls for parity across lifecycle, transitions, processing groups, deferred mutation, handles, identity boundaries, benchmarks, downstream FSM_COS review, clean consumer restore, metadata/migration notes, and an explicit owner approval gate for NuGet publication. Preserve this checklist as the release-gate source, then reconcile it into `development` once the actual 2.0 public API is decided. No gate is marked complete by the existence of this document.
- Recompared `feature/fsm-api-2.0-work`, `feature/fsm-api-2.0-next`, and `feature/fsm-api-2.0-registry` against the current `development` head `e1230bb3cc893da2bf0f2b4a467c7b39a159b7dc`: each reports 24 ahead / 372 behind, merge base `1c91094f1541ec3c3d15bc92aefc9223def224b1`, and the same 64-file footprint. All three have the same workflow blob, but compare results alone do not prove identical heads or ancestry. Keep the exact-head check open; do not nominate a canonical branch based only on the names or matching file lists.
- Integer branch version discrepancy: `feature/integer-backed-runtime/FSM_API/FSM_API.csproj` declares package version `1.0.15`, whereas the historical release baseline in the ledger is FSM_API 1.0.13. Treat this as branch-local metadata, not proof that 1.0.15 was published. The branch's only commit unique to current development adds the version/project-file change and a 46-line `IncrementalVersionTests.cs`; its test does not establish package provenance or runtime correctness.
- Historical registry commits are now source-verified: `8a4cda4d2f534b7b664f2a0aa90a198b0fbcca4a` added a static dictionary registry with unsynchronized collections and a resetting handle counter; `a30c8ff8ddf061c694f470dfa9b86077032b5d1a` later removed that exact scaffold. This confirms that the older registry was explicitly withdrawn, so preserve its design rationale/history but do not resurrect it without a new design decision.
- Readiness document's central direction is compatible with the current reconciliation: do not freeze downstream FSM_COS integration while integer backing is finished, but keep FSM_API 2.0 release readiness gated on a verified backing contract and behavioral parity.

## Current status

**Not ready for manual branch deletion.** The readiness checklist has been reviewed but not yet reconciled into the active development docs; exact head ancestry for the three sibling 2.0 branches remains unverified; the 2.0 implementation and its tests remain unreviewed as a whole. No branches have been merged or deleted by this audit. No package has been published.


- Exact sibling head commits retrieved: `feature/fsm-api-2.0-work` = `ba2dd5d26a91c928ce8b2fdcadc72dfc9829935f`; `feature/fsm-api-2.0-next` = `1dd4449c265595a45c6b4190907a2bc8c38721d7`; `feature/fsm-api-2.0-registry` = `7d731c747435c41e596e16ef9dfb80609e4564d0`. Pairwise comparisons give the same merge base `d45c7aae8c595cb470683701df12205824bceb4f`, one commit ahead/behind each way, and report only `.github/workflows/dotnet.yml` as changed. The three heads each use the same commit message, “ci: keep FSM_API NuGet publication disabled,” and the workflow blob is identical across branches. This strongly suggests equivalent CI edits committed independently, not three separate implementation deltas. Final tree/ancestry verification and canonical-reference selection remain open; do not delete a branch yet.


## Follow-up audit notes (2026-10-25, integer-backed 2.0 source review)

- Recompared the three sibling CI branches against one another. feature/fsm-api-2.0-work vs feature/fsm-api-2.0-next and feature/fsm-api-2.0-work vs feature/fsm-api-2.0-registry each report a shared merge base d45c7aae8c595cb470683701df12205824bceb4f, one commit unique on each side, and only .github/workflows/dotnet.yml changed. This corroborates that their branch-specific difference is the separately committed workflow edit; keep the branches until the repo owner chooses the canonical reference and the exact tree/ancestry check is complete.
- Compared current development with feature/fsm-api-2.0: current compare reports 93 commits ahead / 374 behind and a large, coherent implementation/test footprint. This is not a merge recommendation: the feature branch contains broad legacy-runtime edits plus a separate integer-backed subsystem, so reconcile contracts and tests selectively.
- Read the branch's FSM_API/IntegerBacked/FsmApi.cs, FSMRuntime.cs, and FSMBuilderInt.cs directly. The integer facade currently lives in TheSingularityWorkshop.FSM_API.IntegerBacked.FsmApi and exposes a separate static runtime; it does not itself preserve the legacy API entry point as the facade over one shared runtime. The builder rejects negative FSM/state/group identities, rejects duplicate state IDs, and refuses to build an empty definition, which differs from the legacy safe empty-builder behavior documented for compatibility. These are public-contract decisions, not just implementation details, and need explicit parity tests/API review before adoption.
- The runtime stores definitions and process counters in arrays indexed by FSM_ID, expands capacity to FSM_ID + 1, and scans the full definition-array length during Update/UpdateAll. Thus a sparse or very large ID can cause disproportionate allocation and iteration; this does not meet the stated preference for an internal sparse-ID mapping. Avoid hash-only identity, but use a collision-safe mapping/index strategy internally if sparse IDs are required.
- The reviewed runtime's Register, CreateInstance, RemoveInstance, GetInstances, Unregister, and update loops access mutable arrays/lists without visible synchronization. CreateInstance uses _nextHandleID++; the shown source does not establish wraparound safety or stale-handle protection. This is a source-review risk list, not a verified runtime-test failure; verify the complete runtime and concurrency semantics before treating the branch's tests as sufficient.
- The integer builder starts _initialStateID at FSM.AnyStateIdentifier; the reviewed BuildDefinition() only validates the initial ID when it differs from that sentinel. Confirm that sentinel semantics and default-initial-state behavior are intentional and match the legacy builder before porting.
- Next critical-path work: establish the public API and compatibility contract first; write parity/edge/concurrency tests against that contract; then adapt the runtime representation for sparse IDs, thread safety, and handle lifecycle; only then benchmark and update readiness evidence. Do not merge the integer subsystem wholesale and do not publish NuGet without explicit owner approval.

## Current status

**Not ready for manual branch deletion.** Source inspection has identified concrete API parity, sparse-ID, synchronization, and handle-lifecycle questions that must be resolved before the main 2.0 implementation can be considered reconciled. Current readiness remains **approximately 50% (provisional)**: branch inventory and major design risks are documented, but the public API contract, selective implementation reconciliation, behavioral/concurrency tests, 2.0 benchmarks, downstream compatibility, consumer restore, and final release gates remain incomplete. No branches have been merged or deleted by this audit. No package has been published.


## Follow-up audit notes (2026-10-25, any-state regression-test harvest)

- Re-fetched `Tests/FSM_API_AnyStateTransition_Tests.cs` from current `development` and `fix/manual-any-state-evaluation`. Current development already contains six related tests, but the branch has eight additional cases that are not present by test name:
  1. `EvaluateConditions_IncludesAnyStateTransitions_AndUpdatesHandle`
  2. `EvaluateConditions_AnyStateMatchPreemptsTrueRegularTransition`
  3. `EvaluateConditions_InvalidAnyStateTargetIsSkippedBeforeGuard`
  4. `EvaluateConditions_NoMatchingTransitionLeavesStateUnchanged`
  5. `EvaluateConditions_SkipsTargetRemovedByGuard`
  6. `EvaluateConditions_DoesNotRecordTargetRemovedByExitCallback`
  7. `Step_DoesNotRecordTargetRemovedByExitCallback`
  8. `EvaluateConditions_ContinuesAfterThrowingAnyStateGuard`
- The most valuable gaps are mutation during evaluation (a guard removes a proposed target; an exit callback removes a target after the guard succeeds) and exception resilience when an any-state guard throws. The branch uses direct `FSM`/`FSMHandle` construction for some cases, so adapt test setup to current public/internal API rather than copying the old file verbatim.
- The candidate tests have not yet been ported or run against current development. They are not evidence that current development fails; they are an evidence-backed regression-test harvest list. Next action should be to add/adapt the cases on development, run the relevant test suite/CI, then inspect any failing expectations before deciding whether a minimal source fix is warranted.
- Latest compare at this audit: `development` head `5b9a0864623e7a49e1c933e350e20c5ac193d4d4`; `fix/manual-any-state-evaluation` is 15 commits ahead / 77 behind, with three changed files (`FSM.cs`, `FSMHandle.cs`, and the test file). No branch was merged or deleted.

## Current status

**Not ready for manual branch deletion.** The any-state branch contains a concrete list of candidate regression tests, but none of the eight additional cases has been ported and verified on current development yet. Overall 2.0 readiness remains **approximately 50% (provisional)**. No branches have been merged or deleted by this audit. No package has been published.

## Follow-up audit notes (2026-10-25, regression tests committed)

- Added three focused tests to current development in commit [46fe6ec](https://github.com/TrentBest/FSM_API/commit/46fe6ec7f330a71528214f8e9582491ce5ec4de8):
  - `EvaluateConditions_SkipsAnyStateTargetRemovedByGuard`
  - `EvaluateConditions_DoesNotCommitTargetRemovedByExitCallback`
  - `EvaluateConditions_ContinuesAfterThrowingAnyStateGuard`
- These exercise target removal during a guard, target removal during the outgoing state's exit callback, and continuation after a throwing Any-State guard. They are tests under verification, not yet confirmed passing.
- GitHub Actions .NET workflow run [40164015816](https://github.com/TrentBest/FSM_API/actions/runs/40164015816) was queued for commit `46fe6ec7f330a71528214f8e9582491ce5ec4de8` when checked. Wait for its result and inspect any failures before changing production code. No local test run was performed.
- Readiness remains **approximately 50% (provisional)**; this narrow test addition does not resolve the 2.0 API contract, integer runtime architecture, concurrency, benchmarks, consumer compatibility, or release gates. Still **not ready for manual branch deletion**. No branches merged/deleted; no NuGet package published.

## Follow-up audit notes (2026-10-25, CI failure and callback-mutation fix)

- CI run [40164015816](https://github.com/TrentBest/FSM_API/actions/runs/40164015816) for test commit `46fe6ec` completed with **3 failed / 424 passed**. The three failures were the newly added Any-State tests. Inspection showed two problems: those tests called `EvaluateConditions()`, which filters transitions by the current state's `From` value and therefore does not evaluate the Any-State sentinel; and the runtime's `FSM.Step` could commit a target that a guard or Exit callback had removed.
- Fixed `FSM_API/FSM.cs` in [9d442da](https://github.com/TrentBest/FSM_API/commit/9d442da217783e0a1100f953c4dcbe87013aebdb): re-check the target after user guard code and after the current state's Exit callback, and do not commit a target removed by either callback. When Exit removes the target, preserve the current state identity and clear the handle's entered-state flag.
- Adapted the three tests to exercise Any-State behavior through `FSMHandle.Update()` and match deferred-entry lifecycle semantics in [8dfaf76](https://github.com/TrentBest/FSM_API/commit/8dfaf76d37695e8174bc61f6f61ccf9e76a82e1b). The follow-up CI run is expected to validate these changes; no passing result claimed yet.
- The initial failed run did not pack a package or publish NuGet; the publish job was skipped. No branches merged/deleted. Readiness remains **approximately 50% (provisional)** and the repo is **not ready for manual branch deletion** until follow-up CI and broader 2.0 reconciliation are complete.


## Follow-up audit notes (2026-10-25, verified Any-State regression fix)

- [x] Verified the latest development commit `b3227f5fe7e793d31c49a859ab4877a8e7f913d4` in [workflow #636](https://github.com/TrentBest/FSM_API/actions/runs/40166555251): build/test/package and documentation succeeded; the test suite passed **427/427**; NuGet publishing was skipped.
- [x] The validated change set includes transition-list snapshots during evaluation, passes the owning `FSMHandle` into transition evaluation so direct handles receive state-entry updates, and corrects the throwing-guard test fixture.
- [x] Updated `Documentation/FSM_API_2.0.0_READINESS.md` to replace its stale 2026-10-19 status with the current CI evidence and retain the provisional **50%** readiness estimate.
- [ ] Continue the full 2.0 source/API reconciliation, sparse-ID and concurrency/lifecycle design, behavior parity, repeatable benchmark comparison, downstream compatibility, and release-gate review.

**Current status:** CI for the latest targeted regression fix is green, but the repository is **not ready for manual branch deletion**. The broader branch-preservation gates remain open. No branches have been merged or deleted, and no NuGet package has been published.


## Current audit refresh (2026-10-29)

This section supersedes the old branch-count snapshot above for the **current inventory**. Historical ahead/behind counts and dated findings remain useful audit history but must not be treated as current values.

GitHub's live branch search returned exactly these 18 refs on 2026-10-29:

```text
AddingUnitTests
HireMeFSMDemo
RefactoringToHashBacking
Splitting-FSM_API-class-into-multiple-partial-classes
TargetingStarcraft2DotNet
TickCountingSystem
WindowsFormsEditor
benchmark-contract-docs
development
docs/benchmarking
docs/readme-standard
feature/fsm-api-2.0
feature/fsm-api-2.0-next
feature/fsm-api-2.0-registry
feature/fsm-api-2.0-work
feature/integer-backed-runtime
fix/manual-any-state-evaluation
master
```

The canonical set is `master` + `development`; the other 16 refs remain under audit. No branch was deleted, no stale PR was merged, and `master` was not changed.

### Refreshed comparisons and new evidence

| Branch | Current compare against `development` | Required disposition work |
|---|---|---|
| `RefactoringToHashBacking` | 2 ahead / 469 behind; six runtime source files plus workflow changed | Reconcile runtime semantics and tests; not safe to retire yet. |
| `AddingUnitTests` | 2 ahead / 573 behind; interaction/internal API, project files, and FSM state tests changed | Extract and port distinct useful test coverage; not safe to retire yet. |
| `fix/manual-any-state-evaluation` | 15 ahead / 171 behind; FSM, handle, and Any-State tests changed | Correctness review and selective integration are still required; PR #23 is draft. |
| `benchmark-contract-docs` | 1 ahead / 84 behind; unique `Documentation/Performance/BenchmarkingContract.md` | Reconcile and preserve useful benchmark contract content; ensure links resolve. |
| `docs/benchmarking` | 3 ahead / 463 behind; benchmark guide, README, workflow | Reconcile user-facing guidance; do not copy the branch's workflow wholesale. |
| `docs/readme-standard` | 4 ahead / 424 behind; README, readiness, workflow | Compare onboarding and release claims to current code and the Workshop documentation standard. |

Comparison counts may change as `development` advances; each must be refreshed again before final disposition. The two owner-approved candidates (`HireMeFSMDemo`, `WindowsFormsEditor`) remain candidates only until the same commit/reference/PR audit is complete. Branches previously observed with zero commits ahead also need current-tip and reference checks.

### Latest CI finding

- [Run 40956077990](https://github.com/TrentBest/FSM_API/actions/runs/40956077990) failed during test compilation: `Tests/IntegerBacked/FSMCompiler_Tests.cs` indexed an `IReadOnlyCollection<FSMTransition>` at lines 180–181.
- The test was corrected to select the single transition through `System.Linq.Enumerable.Single(...)` in commit [`da04cc8`](https://github.com/TrentBest/FSM_API/commit/da04cc89dbaaa30fa734f81a39b164c90b1997db).
- Follow-up run [40961786751](https://github.com/TrentBest/FSM_API/actions/runs/40961786751) was in progress at the time of this refresh. Do not mark the compile failure resolved until that run (or a newer run on the current `development` head) completes successfully and the relevant test job is green.

### Sith / CiAi coordination

The FSM_API workflow contains a live `sith_branch_policy` job for an explicitly requested publish. It queries the live GitHub branch inventory and requires exactly `development master`, and requires `refs/heads/master`; the publish job requires that gate to succeed. The current extra branches therefore block publication. Build/test/package checks may run without satisfying the publish gate, and passing the gate does not authorize a release.

CiAi already tracks the reusable fail-closed policy and branch audit in [issue #22](https://github.com/TrentBest/TheSingularityWorkshop.CiAi/issues/22) and [issue #23](https://github.com/TrentBest/TheSingularityWorkshop.CiAi/issues/23). Keep this repository's live gate and CiAi's reusable evidence/authorization work coordinated; do not create a parallel policy engine or weaken either check.

### Current disposition

**Audit remains IN PROGRESS. No branch is being declared ready for deletion in this refresh.** Final owner-facing deletion recommendations must be delivered once, after all unique code/tests/docs/workflow behavior is reconciled, CI evidence is current, and all PR/reference checks are complete.
