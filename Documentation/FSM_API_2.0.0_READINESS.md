# FSM_API 2.0.0 — Development Readiness

**Last reviewed:** 2026-11-28  
**Branch:** `development`

## Public API migration

**Status: Implemented on development**

- The original string-backed public implementation is now `TheSingularityWorkshop.FSM_API.FsmApi`.
- Its partial implementation and timer surface are consolidated under `FsmApi`.
- The concise `FSM_API.FsmApi` namespace facade forwards to that authoritative implementation.
- `Create`, `Interaction`, `Error`, `Internal`, and explicit `Transfer` are exposed through the established namespace; the concise namespace exposes `Create`, `Interact`, `Transfer`, and `Error`.
- Integer-backed execution remains a separate runtime with its own registry and identity model.
- The repository name, NuGet package ID, and established namespaces have not been renamed.

## Validation

The latest verified full development validation is [.NET workflow run #992](https://github.com/TrentBest/FSM_API/actions/runs/43697312085), on exact commit `4298f7eb002129b09c2e1bc4e1defd0f0f6c7d35`.

- `build_test_and_package`: completed successfully.
  - Dependency restore: passed.
  - .NET 10 build and test step: passed (**489 passed, 0 failed, 0 skipped**).
  - NuGet package artifact generation: passed.
  - DLL and NuGet artifact uploads: passed.
- `build_documentation`: completed successfully.
  - Local documentation link validation: passed.
  - DocFX generation: passed.
  - Documentation artifact upload: passed.
- `publish_nuget`: skipped; no publication was performed.
- `sith_branch_policy`: skipped on this ordinary development validation; this run is not evidence of Sith compliance.
- The commit's Codecov patch status is successful.

This records workflow/job outcomes and the exact test totals reported by the successful .NET 10 test step.

## Remaining 2.0 release gates

1. Complete string/integer behavioral parity across lifecycle, transitions, invalidation, mutation, exceptions, and destruction.
2. Complete and verify error-policy and threshold parity across both runtimes.
3. Specify deterministic identity, reverse mapping, persistence, and versioning contracts.
4. Produce reproducible performance benchmarks with equivalent workloads and historical comparisons.
5. Finish implementation-linked Workshop documentation, API reference, examples, and visual explanations.
6. Review all evidence and obtain explicit owner authorization before release or NuGet publication.

## Branch and publication gate

The live branch inventory checked on 2026-11-27 contains **23 branches**: `master`, `development`, and 21 additional branches. The latest evidence refresh commit (`b79bd725dae58c65b19eee9d31b237605657aac0`) records `branch_count: 23` and `status: NON-COMPLIANT`; the live branch listing independently returns the same 23 branches. The canonical two-branch policy is therefore confirmed non-compliant. No surplus branch has been deleted as part of this review; branch deletion remains owner-controlled.

The .NET 10 validation run above skipped its embedded Sith policy job, so it is not evidence of branch compliance. Publication must remain blocked until a fresh live inventory proves that exactly `master` and `development` exist and the explicit owner authorization requirement is independently met.

## Release boundary

All migration work is isolated to `development`. No merge to `master`, package publication, deployment, or release is authorized by this readiness report.


## Review update — 2026-11-26

### Framework compatibility policy

The multi-target strategy is intentional: preserve the widest practical compatibility reach rather than dropping older target frameworks solely because they are out of mainstream support. The current library project targets:

`net10.0;net6.0;netcoreapp3.1;netstandard2.1;netstandard2.0;net47`

The current CI workflow runs a solution build with the .NET 10 SDK, which builds the library's configured target frameworks, and separately runs the NUnit suite only for `net10.0`. Therefore the evidence model must remain explicit:

- **Compile compatibility:** the CI build must succeed for every target framework listed by the library project.
- **Behavioral verification:** the current automated test suite runs on .NET 10 only; it does not establish runtime behavior on every historical target.
- **Consumer support statement:** retain the older targets as compatibility targets, but describe their validation level accurately. Do not imply Microsoft servicing or security support for end-of-life runtimes.
- **Release gate:** preserve the six-target matrix unless a specific compatibility blocker is found and the owner approves changing the reach.

### API contract and diagnostics review

The public API contract now distinguishes the error-policy implementation more precisely:

- String and integer runtimes maintain representation-specific instance and definition counters.
- Both runtimes use the same configured instance and definition threshold values.
- Integer callback failures report through the shared internal diagnostic event.
- The concise facade exposes integer counter copies and per-instance/per-definition reset operations, but no global integer-counter reset, separate integer threshold configuration, or public integer-specific error-injection method. The string counter getters return live mutable backing dictionaries, whereas integer getters return copies; `Reset()` clears only string-backed counters.
- Full threshold and exception-path behavioral parity remains a release gate.

The concise facade's threshold properties and diagnostic event now have XML summaries, and the legacy string-backed `InvokeInstanceError` method now has complete parameter documentation.

### Remaining evidence gaps found in this review

1. **Framework test coverage:** all six target frameworks are included in the library build matrix, but runtime tests currently execute only on `net10.0`. Decide whether compile-only validation is the intended compatibility promise or whether selected historical runtime test lanes are practical.
2. **Package version boundary:** the development project still declares package version `1.0.13` while the development documentation describes the 2.0.0 API. This is acceptable as long as development artifacts are not published, but the 2.0 release candidate must explicitly set and verify its intended package version and release notes before any publish workflow is authorized.
3. **Error parity:** equivalent input/trace tests must cover shared thresholds, callback exceptions, pending removal, definition unregister, resets, and interactions between invalidation and thrown exceptions. Cleanup scheduling differs between runtimes, as do counter getter ownership and global reset scope; these are explicit contract differences to resolve or preserve deliberately.
4. **Public API traceability:** continue mapping every public member to its XML documentation, tests, conceptual explanation, and examples. This review has improved the Error surface but is not a completed whole-library inventory.
5. **Visual evidence:** living SVGs and theory chapters must be reconciled to the exact runtime implementation and tests before being treated as release evidence.\n6. **API traceability:** an initial source-backed inventory is now recorded in [API_TRACEABILITY_2.0.0.md](API_TRACEABILITY_2.0.0.md). It maps the concise facade to implementation, known test areas, theory, examples, and visuals, while explicitly recording incomplete parity and verification gaps. It is not a parity sign-off.

Documentation-only changes made during this review are on `development`; they do not change runtime behavior, package version, branch inventory, or publication authorization.
\n\n## Review update — 2026-11-27\n\nThe live Sith-policy evidence was refreshed and confirms 23 branches / `NON-COMPLIANT`; this is consistent with the direct branch listing. The 2.0 API traceability inventory was added on `development`. The inventory found that the concise facade should not be described as offering complete string/integer feature parity: its actual exposed members and the independent integer surface must be described explicitly. No runtime behavior, package version, target framework, branch, merge, publication, or deployment was changed by this documentation pass.\n

## Review update — 2026-11-28

### Transfer and diagnostic contract findings

- The concise Transfer facade is fully located: `FSM_API/FsmApi.cs` delegates to `IntegerBacked.FSMCompiler.Compile` and `Decompile`. Conversion preserves definition behavior through shared callbacks/conditions and explicit state mappings; it does not transfer live instances.
- Existing compiler tests cover deterministic dense IDs, initial-state preservation, action and regular/Any-State transition preservation, invalid source/target/initial states, read-only mapping objects, reverse conversion, and round-trip behavior. These are existing tests, not newly added tests.
- The string-backed `GetErrorCounts()` and `GetDefinitionErrorCounts()` methods return their live mutable backing dictionaries. Integer-backed getters return copies. The concise `Reset()` clears only string-backed counters; integer reset methods are per-handle/per-definition.
- The runtimes share threshold settings but schedule threshold cleanup differently: string-backed cleanup uses the deferred-modification queue; integer-backed cleanup processes pending lifecycle changes at the end of its update call. These differences remain explicit parity/contract questions, not assumed equivalence.

### Current verification and branch evidence

- Latest completed full .NET workflow observed during this review: [run #1125](https://github.com/TrentBest/FSM_API/actions/runs/46087202082), exact commit `14be41e594e3d2cd2c6db7d23b84a1832c373db6`. Solution build/package and documentation jobs succeeded; .NET 10 tests reported **489 passed, 0 failed, 0 skipped**. The build emitted 282 warnings and 0 errors. NuGet publication and the embedded Sith gate were skipped.
- Newer documentation commits are being validated separately. The latest observed .NET run is [run #1130](https://github.com/TrentBest/FSM_API/actions/runs/46087374098), currently in progress at the time of this update; do not treat the earlier successful run as validation of the latest commit.
- The latest direct live branch inventory contains **24 branches**, including `development`, `master`, and 22 additional branches. The exact-two-branch Sith policy remains **NON-COMPLIANT**. No branch was deleted; surplus-branch cleanup remains owner-controlled.
- The Pages workflow is push-triggered and uses the CiAi deployment workflow. Its latest observed run is [run #71](https://github.com/TrentBest/FSM_API/actions/runs/46087374455), in progress. I did not manually dispatch a deployment; this automation is triggered by development pushes.
- Package version remains `1.0.13`; all six library target frameworks remain unchanged. No runtime implementation, package publication, merge to `master`, or branch deletion was performed.

The detailed source/test map is maintained in [API_TRACEABILITY_2.0.0.md](API_TRACEABILITY_2.0.0.md). These findings improve documentation accuracy but do not constitute 2.0 parity sign-off.
