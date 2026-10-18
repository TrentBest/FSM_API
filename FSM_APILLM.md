# FSM_API LLM contract

Follow the repository owner's branch and CI conventions. Maintain only master and development as long-lived branches; do not create persistent feature branches. Do not delete existing branches without explicit authorization. Keep changes scoped to FSM_API and coordinate consumer findings through *LLM.md handoffs.

Development verification in downstream repositories should use FSM_API source from development via the established ProjectReference convention. Release builds should use the intentionally published NuGet package. Preserve the explicit boolean publish gate and trusted publishing; green CI is not permission to publish.

Investigate the Renderer DestroyInstance disposal failure with a focused reproduction. Determine whether the caller violates the documented main-thread/deferred-mutation contract or whether an internal instance-registry invariant is broken. Inspect every registry mutation path, add a regression test if a defect is confirmed, and report exact commit and workflow evidence. Do not suppress the exception or change public API behavior without evidence.