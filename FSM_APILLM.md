# FSM_API LLM contract

Follow the repository owner's branch and CI conventions. Maintain only master and development as long-lived branches; do not create persistent feature branches. Do not delete existing branches without explicit authorization. Keep changes scoped to FSM_API and coordinate consumer findings through *LLM.md handoffs.

Development verification in downstream repositories should use FSM_API source from development via the established ProjectReference convention. Release builds should use the intentionally published NuGet package. Preserve the explicit boolean publish gate and trusted publishing; green CI is not permission to publish.

Investigate the Renderer DestroyInstance disposal failure with a focused reproduction. Determine whether the caller violates the documented main-thread/deferred-mutation contract or whether an internal instance-registry invariant is broken. Inspect every registry mutation path, add a regression test if a defect is confirmed, and report exact commit and workflow evidence. Do not suppress the exception or change public API behavior without evidence.

## 2026-10-18: Registry concurrency contract narrowed

The public README describes the supported execution model as **main-thread operations with deferred mutation handling**. The API source confirms that Interaction.MoveInstance queues its structural change with QueueDeferredModification, but Interaction.DestroyInstance calls Internal.DestroyInstance synchronously. The internal destroy path immediately mutates bucket.Instances with List.Remove; creation and moves also mutate the registry lists.

Therefore, “deferred mutation handling” must not be read as a guarantee that every public structural operation is thread-safe or automatically deferred. The current docs do not establish that direct destruction may race safely with ticking or other registry mutations. Renderer’s successful current-source run (115/115 tests against FSM_API 69949dc499c39f28d5b85d554d3f6006c7e9286d) means no runtime patch is justified yet.

Next discriminating step: validate Renderer lifecycle calls against the documented main-thread-only contract, then run its source-integration suite in serial and normal parallel modes if that contract check does not explain the earlier failure. Treat a serial/parallel difference as evidence to investigate, not as proof by itself. Only add registry synchronization or a regression test after reproducing the defect and defining the intended concurrency contract.


## LLM API usage reference

For instructional onboarding—when to use FSM_API, verified usage patterns, performance trade-offs, and source/test verification—start with [FSM_API_LLM_USAGE.md](FSM_API_LLM_USAGE.md). This usage manual complements rather than replaces this file's maintenance, branch, CI, release, and evidence contract.


## Package-aware release branch semantics — mandatory

Branch meaning depends on whether this package has actually been published, not merely on the existence or name of a branch.

- **Never published:** `development` is the authoritative source of truth. `master` is pre-release scaffolding, not a released-package baseline, and may be identical to `development`. Before the first release, the owner may replace/reset the pre-release `master` tree to the verified, approved `development` candidate. Do not preserve artificial pre-release history as though it represented a NuGet release.
- **Previously published:** `master` must represent the exact source commit corresponding to the latest successfully published NuGet package. `development` remains the active forward-moving source. Do not move `master` merely to make it match newer development work.
- **First release transition:** after the exact candidate has passed its release gates and the owner explicitly authorizes publication, publish that approved candidate. Establish `master` at the exact released source commit. Afterward, development may advance independently.
- **Classification evidence:** verify publication history from the authoritative NuGet feed and release/publish evidence. Do not infer that a package was published from a project file, version string, tag, successful pack job, skipped publish job, or branch name.
- **Branch count:** the permanent target remains exactly `master` and `development`. For an unreleased package, equality is normal and desirable; for a released package, divergence is normal after subsequent development.
- **Authorization boundary:** this policy describes the intended release model; it is not blanket permission to publish, force-push, replace a branch, merge, delete branches, or deploy. Obtain explicit owner authorization for the specific operation. Never publish NuGet packages without explicit authorization.

When auditing, record the package's verified publication state as `never-published` or `published`, the latest published version and source SHA when applicable, both branch SHAs, and the evidence used to classify it. If publication state cannot be verified, mark it `unknown` and preserve both refs until clarified.
