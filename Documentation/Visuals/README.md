# FSM_API living visuals

These visuals are generated from semantic GUI trees by **GUI.SVG**, the SVG interoperability adapter in [TheSingularityWorkshop.GUI](https://github.com/TrentBest/TheSingularityWorkshop.GUI). FSM_API owns the behavior model; GUI Core owns the semantic scene description; GUI.SVG manifests that description as SVG. The SVG is a documentation artifact, not a second runtime or an authoritative execution clock.

## Planned visual set

| Artifact | What it teaches | Source of truth |
| --- | --- | --- |
| `fsm-api-ownership.svg` | Host, caller-owned context, definition, handle, and API responsibilities | FSM_API ownership contract |
| `fsm-api-lifecycle.svg` | Enter, Update, transition, Exit, next-tick Enter, suspension, and explicit destruction | Runtime lifecycle tests and tick contract |
| `fsm-api-representations.svg` | Separate string/integer registries and explicit Transfer boundary | Public API contract and facade |
| `fsm-api-tick-boundary.svg` | Tick snapshot, queued replacement, lifecycle finalization, and boundary commit | Tick-boundary regression tests |

## Generation contract

The generator must use `GuiBuilder` from `TheSingularityWorkshop.GUI.Core` and `SvgGuiRenderer` from `TheSingularityWorkshop.GUI.SVG`. It must not hand-author SVG XML or infer behavior from the artwork. Every label and edge must correspond to a documented API or tested runtime rule.

The GUI repository's current SVG adapter guide and runnable gallery are the implementation reference:

- [GUI.SVG adapter contract](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/docs/SVG_ADAPTER.md)
- [Semantic-tree-to-SVG use case](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/docs/USE_CASES/01-SEMANTIC-TREE-TO-SVG.md)
- [Runnable SVG gallery generator](https://github.com/TrentBest/TheSingularityWorkshop.GUI/blob/development/samples/SvgGallery/Program.cs)

The generated assets should be reviewed as images as well as XML. XML validity alone cannot detect clipped labels, overlapping arrows, unreadable contrast, or misleading spatial relationships.

## Important boundaries

- Declarative SVG animation is viewer-interpreted. It is not an FSM_API update loop.
- GUI.SVG serializes supplied references; it is not a URL sanitizer. Use controlled local documentation scenes and do not treat XML escaping as SVG security sanitization.
- General scene geometry and Visualizer frames belong to Visualize. GUI.SVG is used here because these diagrams are semantic documentation interfaces.
- Regenerate an image whenever the API contract, lifecycle ordering, identity boundary, or relevant test changes. Review the source and generated artifact together.
