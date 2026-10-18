![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# FSM_API visual coverage map

This page is the working inventory for making the documentation explain behavior visually—not just decorate prose. It records which important concepts already have a visual, what kind of claim the visual makes, and where the next useful gap appears.

**Interpretation rule:** unless a visual explicitly says otherwise, authored SVG animation is a conceptual teaching model, not a live connection to an executing FSM_API instance. Source code and tests remain authoritative for exact runtime behavior.

## Current coverage

| Concept | Visual | What it teaches | Evidence / boundary |
|---|---|---|---|
| Recognize state-driven behavior in everyday life | [Everyday state model](../../User%20Guide/Theory/Visuals/Everyday_State_Model.svg) | A door moves through named phases under triggers. | Conceptual example; not a physical door controller or safety design. |
| Reusable definition, live instance, application context | [FSM_API architecture](../../User%20Guide/Visuals/FSM_API_Architecture.svg) | One blueprint can be instantiated repeatedly; each instance has its own current state and interacts with application-owned context. | Conceptual animation; see the core concepts and executable examples. |
| State lifecycle and transition timing | [State lifecycle](../../User%20Guide/Visuals/State_Lifecycle.svg) | Entry, update, condition evaluation, exit, and next-tick entry. | Must remain aligned with the lifecycle implementation and tests. |
| Guard versus action | [Guard and action](../../User%20Guide/Visuals/Guard_vs_Action.svg) | A transition condition returns a decision; state actions perform effects. | Conditions should not be used for side effects. |
| Moore versus Mealy output models | [Moore and Mealy](../../User%20Guide/Theory/Visuals/Mealy_vs_Moore.svg) | State-associated versus transition-associated output. | Theory comparison; FSM_API has no dedicated transition-output callback. |
| Combining behavior dimensions | [Behavior separation](../../User%20Guide/Theory/Visuals/Behavioral_Separation.svg) | A flat machine can multiply combinations; independent machines can separate concerns. | Separate handles do not imply parallel threads or automatic coordination. |
| Choosing an approach | [Behavior-model decision guide](../../User%20Guide/Theory/Visuals/Choosing_A_Behavior_Model.svg) | When to consider an FSM, composed FSMs, controllers, behavior trees, or planners. | Heuristic, not a strict taxonomy; approaches can be combined. |
| Tick boundaries and registry ownership | [Registry tick boundary](../../User%20Guide/Theory/Visuals/Registry_Tick_Boundary.svg) | Why mutation timing and tick ownership matter. | Conceptual animation; check implementation before inferring edge-case guarantees. |
| Handle validity | [Handle validity](../../User%20Guide/Visuals/FSMHandle_Validity.svg) | Invalid context suspends updates without itself unregistering the handle. | Must agree with the lifecycle docs and tests. |
| Handle lifecycle | [Handle lifecycle](../../User%20Guide/Visuals/FSMHandle_Lifecycle.svg) | Instance/handle lifecycle at a conceptual level. | Keep explicit destruction distinct from temporary invalidity. |
| Integer-backed implementation progress | [Integer backing roadmap](Visuals/Integer_Backing_Roadmap.svg) · [Progress and acceptance criteria](Integer_Backing_Progress.md) | What exists on the isolated feature branch and which integration gates remain. | Roadmap is not a percentage or a claim that the feature is released. |
| API versus integrations | [API vs. integrations](../../User%20Guide/Visuals/API_vs_Integrations.png) | Core responsibilities versus host-specific adapters. | Keep engine-specific types out of core. |
| State context and application data | [Context relationship](../../User%20Guide/Visuals/FSM_Context_Relationship.png) | The machine uses application-owned context rather than owning domain data. | Context validity affects whether an instance is eligible to update. |
| Simple two-state example | [Light switch FSM](../../User%20Guide/Visuals/Light_Switch_FSM.svg) | States, conditions, and a small transition cycle. | Teaching example, not runtime telemetry. |
| Traffic-light example | [Traffic-light FSM](../../User%20Guide/Theory/Visuals/TrafficLightFSM.svg) | A familiar cycle with named states and transition timing. | Timing assumptions should be stated separately from the state diagram. |

## Next candidates to evaluate

These are candidates, not promises to create graphics for every row. A visual is worth adding only if it explains something better than prose or executable examples.

1. **Processing groups and process rates:** show host-driven ticks, named group selection, per-definition rates, and manual stepping without suggesting that the library owns a host loop.
2. **Runtime modification and deferred work:** distinguish immediate operations from queued registry changes and show the relevant tick boundary.
3. **Error handling and thresholds:** visualize cumulative instance-error counts, threshold callbacks, destruction, and the limits of cleanup guarantees.
4. **Disposal and ownership:** show what explicit destruction/unregistration does versus invalid-context suspension.
5. **Performance and integer backing:** show current string-backed lookup versus the isolated integer runtime only where measurements and implementation details support the comparison.
6. **A source-backed, runnable walkthrough:** prefer an executable example or test-backed view over another diagram when it can prove the same concept more directly.

## Visual acceptance checklist

Before calling a concept covered, verify:

- The title, alternative text, labels, and surrounding prose make the intended concept clear.
- The relative path resolves and DocFX includes the visual in the generated artifact.
- Any animation is explicitly described as conceptual unless it is genuinely driven by runtime data.
- Reduced-motion preferences are respected for animated SVGs where applicable.
- The visual matches the source, API contract, and tests; edge cases and guarantees are not invented.
- The visual adds explanatory value and does not simply duplicate another diagram.
- Related table-of-contents links and next/previous links point to the intended documents.

---

<p align="center">
  <a href="https://github.com/TrentBest/WebPage/tree/development/TheSingularityWorkshop/wwwroot/Images" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex converging on its central eye" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
