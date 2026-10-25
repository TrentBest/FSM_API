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
