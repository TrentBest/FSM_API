# FSM_API visual storytelling contract

The purpose of an FSM_API visual is to make a runtime rule understandable before the reader has mastered its vocabulary. A diagram that merely repeats the prose is not enough. A storyboard must show a meaningful change over time.

## Teaching sequence

Every animated explanation follows this order:

1. **Initial condition** — show the actual state of the machine before the operation.
2. **Trigger** — identify the API operation, callback, guard, or host update that causes the change.
3. **Intermediate condition** — make pending, proposed, active, or deferred work visually distinct.
4. **Result** — resolve to the completed state that the contract guarantees.
5. **Invariant** — state what did not change, what was preserved, or what was deliberately deferred.

The visual must teach one concept at a time. If a scene requires multiple unrelated explanations, split it into separate storyboards.

## Shared visual vocabulary

| Visual treatment | Meaning |
| --- | --- |
| Solid state node | State already present in the definition |
| Dashed outline / ghost node | Proposed or pending state, not yet committed |
| Highlighted state | State whose callback or behavior is currently being illustrated |
| Moving marker | Explanatory indication of execution or traversal |
| Highlighted edge | Transition selected for the illustrated sequence |
| Boundary divider | Tick or lifecycle consistency boundary |
| Preserved identity marker | A handle or context that survives the operation |
| Muted / suspended treatment | Present but currently ineligible to execute |

Color reinforces meaning; it must never be the only carrier of meaning. Labels, shape, line style, and accessible descriptions must preserve the distinction for readers who cannot perceive color.

## First three storyboards

### Add a state

- Start with the existing FSM and S0 in its initial condition.
- Introduce S1 as a dashed ghost node.
- Move the proposed node into its final position.
- Resolve its dashed outline into a solid outline.
- Reveal the new relationship to the existing machine.
- Keep S0 and all existing identities intact.

### Execute a transition

- Highlight S0 as the active state.
- Show the relevant guard being evaluated and accepted.
- Traverse the selected transition edge.
- Mark Exit on the state being left.
- Change the current-state identity.
- Show Enter on the destination as the next eligible update, not as part of the transition step.

### Replace a definition during a tick

- Show the eligible handle snapshot.
- Keep the current definition visually stable while callbacks execute.
- Represent a callback-originated replacement as queued / pending.
- Finish the selected work against the stable definition view.
- Cross the tick boundary and commit the queued replacement.
- Make the invariant explicit: no sibling handle observes a half-applied replacement.

## Implementation and evidence

Storyboards are built as semantic GUI trees and rendered by GUI.SVG. Do not hand-author SVG XML in the generator. Prefer declarative SVG animation nodes for sequencing; the static SVG attributes must still present a meaningful completed state when animation is unsupported or disabled.

The storyboard is an explanation of a contract, not a measurement of the running program. It must not imply live telemetry, actual elapsed runtime, concurrency, or callback execution unless a separate trace-driven feature is implemented and clearly identified.

For every storyboard, document:

- the public API or runtime operation being illustrated;
- the source contract and relevant regression tests;
- the initial and final states;
- the invariant that the reader should take away;
- the static fallback behavior;
- any viewer-dependent animation behavior.

Animation must not invent a transition, lifecycle callback, identity change, or timing guarantee. If the source/tests do not establish a behavior, the storyboard must not teach it as fact.

## Review checklist

- [ ] The first frame represents a valid initial condition.
- [ ] Proposed and committed entities are visually distinct.
- [ ] Motion corresponds to a real documented operation.
- [ ] Existing state and handle identity are not accidentally implied to change.
- [ ] Lifecycle order and tick boundaries are accurate.
- [ ] The final static rendering remains understandable without animation.
- [ ] Text remains legible and unclipped at normal documentation scale.
- [ ] Accessible title and description explain the scene.
- [ ] The SVG is regenerated from semantic source and reviewed as an image.
- [ ] The relevant API contract and regression tests are linked nearby.
