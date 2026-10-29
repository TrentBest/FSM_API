![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# FSM_API Theory — A Learning Path

This index is the entry point for learning the reasoning behind finite-state machines (FSMs), when to use them, and what FSM_API's runtime boundaries mean in practice. It complements the [user guide](../00_Introduction.md), [getting-started guide](../03_Getting_Started_CSharp.md), API reference, source, and tests; it does not replace them.

> **Evidence labels:** The conceptual diagrams and examples in these chapters are teaching models unless a chapter explicitly identifies a named source revision or executed test. A conceptual diagram is not a runtime trace. For claims about FSM_API behavior, follow the implementation and tests linked from the relevant chapter.

## Learning path

The sequence moves from a general model to design choices, then to the practical runtime constraints specific to this library. Each chapter is a standalone document and may be read independently.

1. **[What are finite state machines?](01_What_Are_FSMs.md)** — Build intuition from everyday systems, define states and transitions, and distinguish a model from the real system it describes.
2. **[Types of FSMs](02_Types_Of_FSMs.md)** — Compare Mealy and Moore output models, then examine hierarchical and concurrent patterns. Pay attention to the difference between a general FSM concept and a feature natively exposed by FSM_API.
3. **[Advantages and disadvantages](03_Advantages_And_Disadvantages.md)** — Understand the trade-offs: explicit state, transition complexity, state explosion, and the cost of representing continuous or highly combinatorial behavior discretely.
4. **[When to use FSMs](04_When_To_Use_FSMs.md)** — Decide whether the problem has meaningful discrete modes, constrained transitions, and behavior that benefits from explicit lifecycle.
5. **[FSM limitations and alternatives](05_FSM_Limitations_And_Alternatives.md)** — Compare FSMs with alternatives and combinations; choose based on the shape of the problem rather than the popularity of a pattern.
6. **[Tick boundaries, deferred work, and registry ownership](06_Tick_Boundaries_And_Registry_Ownership.md)** — Connect the abstract model to FSM_API's actual execution boundary. In particular, a deferred-mutation queue is not a global lock, and immediate registry operations must not race with ticking or other registry writers.

## From theory to implementation

After the learning path, use these implementation-facing resources:

- [Core concepts](../01_Core_Concepts.md) — the library's principal types and how they fit together.
- [Getting started with C#](../03_Getting_Started_CSharp.md) — a worked integration path for a host-controlled update loop.
- [FSMBuilder deep dive](../04_FSM_Builder_Deep_Dive.md) — defining states and transitions.
- [Context implementation](../05_Context_Implementation.md) — separating application data from the state-machine definition.
- [Runtime modification](../06_FSM_Modifier_Deep_Dive.md) — changing definitions and instances.
- [Error handling](../07_Error_Handling.md) — lifecycle and failure-related behavior.
- [Performance tips](../08_Performance_Tips.md) — practical performance considerations; use named benchmarks for measured claims.
- [Non-coder overview](../10_Non_Coder_Overview.md) — a less code-centric introduction.

## Evidence and limits

FSM theory gives a way to reason about a model; it does not automatically prove the implementation correct. When evaluating a claim:

1. Identify the source contract or code path.
2. State the behavior observed under specific conditions.
3. Form a proposition precise enough to test.
4. Link repeatable tests, traces, or benchmark evidence.
5. Record counterexamples, assumptions, and unanswered questions.

Do not treat asymptotic complexity, benchmark timing, formal proof, conceptual diagrams, and anecdotes as interchangeable evidence. A performance result must name its workload, environment, and revision. A lifecycle claim should be checked against source and tests, not inferred from a picture.

## Scope and ownership

This repository owns general finite-state-machine theory and FSM_API-specific behavior. Higher-level Workshop projects should explain their own domain and link here for general FSM concepts rather than duplicating this material. Engine-specific integrations belong in their integration repositories; FSM_API itself remains framework-agnostic.

---

<p align="center"><em>The Singularity Workshop — If a picture is worth a thousand words, what is a living picture worth?</em></p>
