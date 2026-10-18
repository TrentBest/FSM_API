# FSM_API LLM — repository and CI contract

This is the coordination contract for the LLM maintaining FSM_API. Treat it as the repository owner's direction; do not infer permission to publish from a green build.

## Branch policy ("Sith policy")

- Maintain only `master` and `development` as long-lived branches.
- Do not create feature, fix, experiment, release, or other persistent branches. Work directly on `development` for ongoing work; promote tested changes to `master` using the owner's intended release process.
- If legacy branches exist, do not mass-delete them blindly. Report the list to the owner and remove them only when the owner has authorized cleanup / confirmed they are no longer needed.
- Keep changes scoped to FSM_API. Coordinate cross-repository findings through `*LLM.md` notes rather than editing consumers' source.

## CI and dependency convention

- Development/testing in dependent repositories must consume the latest FSM_API source from the `development` branch, using the repository-source / ProjectReference convention those repositories already use. Do not make development verification depend solely on the currently published NuGet version.
- Published/release builds must consume the published `TheSingularityWorkshop.FSM_API` NuGet package at the version deliberately released from `master`; do not publish experimental development bits merely because the workflow is green.
- Keep `master` and `development` CI checks. Preserve the explicit boolean publish gate and trusted publishing. A build, test, pack, or artifact upload is not permission to publish.
- Before changing CI, inspect the standard already adopted by the dependent repositories and match its behavior rather than inventing a new convention. Validate both repository-source development builds and package-based release builds.
- FSM_API currently has no reason to add Unity types or other host-specific dependencies to its core.

## Current investigation: Renderer disposal failure

See [RendererLLM.md](RendererLLM.md) for the full report and links.

The Renderer source-integration workflow reported an `ArgumentOutOfRangeException` from `List<FSMHandle>.Remove` inside `FSM_API.Internal.DestroyInstance`, reached during `RendererComputationMachine.Dispose()`. This is a reproduction target, not proof of an FSM_API bug.

Important existing contract evidence: FSM_API's README describes operations as main-thread-only with deferred mutation handling and says structural modifications are deferred until after the tick. Before concluding that this is a race, inspect the exact call path and contract. In particular:

1. Determine whether Renderer invokes `DestroyInstance` on the main/tick-owning thread and whether it can overlap with ticking or another structural mutation.
2. Inspect all instance-registry mutation paths and the relationship among bucket membership, handle validity, and equality/index lookup.
3. Reproduce with a focused test; distinguish consumer misuse from a broken internal invariant.
4. If a defect is confirmed, fix it in FSM_API and add a regression test. If Renderer violates the documented contract, report the specific violation and coordinate a consumer-side correction.
5. Do not suppress the exception or change public API behavior without evidence.

## Completion evidence required

For each change, report commit SHA, exact test/workflow run, and outcome. Do not say a fix is verified until the regression test and relevant downstream source-integration workflow pass. Do not claim GPU-rendered pixels or package publication without direct evidence.
