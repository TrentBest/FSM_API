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
| `feature/fsm-api-2.0-work` | 24 ahead / 368 behind; 64 changed files | Integer-backed types/runtime and tests; shared merge base with next/registry variants | Pairwise comparisons show each of the three variants differs from the others by one commit in each direction, with `.github/workflows/dotnet.yml` the only changed file. The implementation/test change set appears shared; review workflow differences, then retain one canonical feature branch only until reconciliation is complete. |
| `feature/fsm-api-2.0-next` | 24 ahead / 368 behind; 64 changed files | Same reported implementation/test change set as work/registry variants | Pairwise comparison differs only in `.github/workflows/dotnet.yml`; workflow differences still need review before selecting the canonical feature branch. |
| `feature/fsm-api-2.0-registry` | 24 ahead / 368 behind; 64 changed files | Same reported implementation/test change set as work/next variants | Pairwise comparison differs only in `.github/workflows/dotnet.yml`; the branch name alone does not prove a distinct registry implementation. Review workflow differences before retirement. |
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

- [ ] Review the pairwise workflow-only differences among `feature/fsm-api-2.0-work`, `feature/fsm-api-2.0-next`, and `feature/fsm-api-2.0-registry`; choose one canonical feature branch only after confirming workflow intent.
- [ ] Inspect and reconcile all 93 unique commits / 90-file change set from `feature/fsm-api-2.0` against current `development`; document what is ported, superseded, or intentionally excluded.
- [ ] Inspect any-state fix branch and port relevant behavioral fixes/tests if still valid.
- [ ] Inspect test-only, benchmark, README/readiness, and Windows Forms branches for unique useful artifacts.
- [ ] Resolve the integer registry versus direct-indexed runtime design explicitly; test sparse IDs, memory growth, thread safety, lifecycle, handle-ID wrap/reuse, and string/integer API parity.
- [ ] Confirm core remains dependency-free and engine-agnostic.
- [ ] Run the relevant build/test/CI checks after any port; do not infer success from branch comparison alone.
- [ ] Update this ledger with commit SHAs, exact file/test dispositions, CI evidence, and a clear **READY FOR MANUAL BRANCH DELETION** status only after all gates are satisfied.

## Current status

**Not ready for manual branch deletion.** This is the first inventory pass; several branches contain unreviewed unique changes, and the three 24-commit 2.0 variants still need exact-head verification. No branches have been merged or deleted by this audit. No package has been published.
