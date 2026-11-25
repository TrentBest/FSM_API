using TheSingularityWorkshop.Workshop.Gui;
using TheSingularityWorkshop.Workshop.Gui.Svg;

/// <summary>
/// Animated, explanatory storyboards for the FSM_API documentation.
/// The base SVG attributes describe the completed state; SMIL animates the
/// teaching sequence when the viewing environment supports it.
/// </summary>
public static class VisualSequences
{
    public static void WriteAll(string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);
        Write(destinationDirectory, "fsm-api-add-state.svg", BuildAddState().Build(), 1280, 720);
        Write(destinationDirectory, "fsm-api-state-transition.svg", BuildTransition().Build(), 1280, 720);
        Write(destinationDirectory, "fsm-api-tick-atomic-mutation.svg", BuildTickMutation().Build(), 1280, 720);
    }

    private static void Write(string destinationDirectory, string name, GuiNode scene, int width, int height)
    {
        var svg = new SvgGuiRenderer().Render(scene, width, height);
        File.WriteAllText(Path.Combine(destinationDirectory, name), svg);
        Console.WriteLine($"Generated {name}");
    }

    private static GuiBuilder Frame(string id, string title, string description)
    {
        var scene = GuiBuilder.Create("svg-scene", id)
            .Text(title)
            .Property("description", description);
        scene.Child("svg-rect", "background", n => n
            .Property("width", "100").Property("height", "56.25")
            .Property("fill", "#101d2b"));
        scene.Child("svg-text", "title", n => n.Text(title)
            .Property("x", "5").Property("y", "7")
            .Property("fill", "#eef6ff").Property("font-size", "3.2")
            .Property("font-weight", "700"));
        return scene;
    }

    private static GuiBuilder BuildAddState()
    {
        var scene = Frame("fsm-api-add-state",
            "Adding a state: preview becomes part of the definition",
            "S0 is already defined. A proposed S1 appears as a dashed ghost, moves into its final position, resolves into a solid state, and gains a transition edge. The completed state is the static fallback.");

        scene.Child("svg-rect", "machine", n => n
            .Property("x", "9").Property("y", "13").Property("width", "82").Property("height", "34")
            .Property("rx", "2").Property("fill", "#172b3c")
            .Property("stroke", "#426478").Property("stroke-width", "0.45"));
        scene.Child("svg-circle", "s0", n => n
            .Property("cx", "31").Property("cy", "29").Property("r", "7")
            .Property("fill", "#174b49").Property("stroke", "#7ee0bd").Property("stroke-width", "0.8"));
        scene.Child("svg-text", "s0-label", n => n.Text("S0")
            .Property("x", "28.5").Property("y", "30").Property("fill", "#ffffff")
            .Property("font-size", "3").Property("font-weight", "700"));
        scene.Child("svg-line", "new-edge", n => n
            .Property("x1", "38").Property("y1", "29").Property("x2", "62").Property("y2", "29")
            .Property("stroke", "#ffcf86").Property("stroke-width", "0.7")
            .Property("marker-end", "url(#arrowhead)"));
        scene.Child("svg-defs", "definitions", defs => defs
            .Child("svg-marker", "arrowhead", marker => marker
                .Property("refX", "5").Property("refY", "5").Property("markerWidth", "4")
                .Property("markerHeight", "4").Property("orient", "auto")
                .Child("svg-polygon", "arrowhead-shape", polygon => polygon
                    .Property("points", "0,0 10,5 0,10").Property("fill", "#ffcf86"))));
        scene.Child("svg-circle", "s1", n => n
            .Property("cx", "69").Property("cy", "29").Property("r", "7")
            .Property("fill", "#25385a").Property("stroke", "#a5a9f5")
            .Property("stroke-width", "0.8").Property("stroke-dasharray", "0 0")
            .Child("svg-animate", "resolve-outline", a => a
                .Property("attributeName", "stroke-dasharray")
                .Property("from", "7 4").Property("to", "0 0")
                .Property("dur", "1.2s").Property("begin", "0s").Property("fill", "freeze"))
            .Child("svg-animate", "resolve-opacity", a => a
                .Property("attributeName", "opacity").Property("from", "0.35").Property("to", "1")
                .Property("dur", "1.2s").Property("begin", "0s").Property("fill", "freeze"))
            .Child("svg-animate-transform", "settle-position", a => a
                .Property("attributeName", "transform").Property("type", "translate")
                .Property("from", "0 8").Property("to", "0 0")
                .Property("dur", "1.2s").Property("begin", "0s").Property("fill", "freeze")));
        scene.Child("svg-text", "s1-label", n => n.Text("S1")
            .Property("x", "66.5").Property("y", "30").Property("fill", "#ffffff")
            .Property("font-size", "3").Property("font-weight", "700"));
        scene.Child("svg-text", "sequence-caption", n => n.Text("Existing state retained  •  proposed state resolves  •  connection becomes part of the definition")
            .Property("x", "12").Property("y", "42").Property("fill", "#d6e7f4")
            .Property("font-size", "2.25"));
        scene.Child("svg-text", "fallback-note", n => n.Text("Static fallback: both states and their completed relationship remain visible.")
            .Property("x", "12").Property("y", "52").Property("fill", "#9fb5c7")
            .Property("font-size", "2.0"));
        return scene;
    }

    private static GuiBuilder BuildTransition()
    {
        var scene = Frame("fsm-api-state-transition",
            "Executing a transition: lifecycle order matters",
            "The active marker highlights S0, follows the accepted transition edge, and arrives at S1. Exit belongs to the state being left; destination Enter is shown as the next eligible update.");

        scene.Child("svg-circle", "s0", n => n
            .Property("cx", "27").Property("cy", "28").Property("r", "8")
            .Property("fill", "#174b49").Property("stroke", "#7ee0bd").Property("stroke-width", "0.8"));
        scene.Child("svg-text", "s0-label", n => n.Text("S0")
            .Property("x", "24.5").Property("y", "29").Property("fill", "#ffffff").Property("font-size", "3").Property("font-weight", "700"));
        scene.Child("svg-circle", "s1", n => n
            .Property("cx", "73").Property("cy", "28").Property("r", "8")
            .Property("fill", "#25385a").Property("stroke", "#a5a9f5").Property("stroke-width", "0.8"));
        scene.Child("svg-text", "s1-label", n => n.Text("S1")
            .Property("x", "70.5").Property("y", "29").Property("fill", "#ffffff").Property("font-size", "3").Property("font-weight", "700"));
        scene.Child("svg-line", "transition-edge", n => n
            .Property("x1", "35").Property("y1", "28").Property("x2", "65").Property("y2", "28")
            .Property("stroke", "#ffcf86").Property("stroke-width", "0.8"));
        scene.Child("svg-circle", "active-marker", n => n
            .Property("cx", "27").Property("cy", "28").Property("r", "2")
            .Property("fill", "#ffcf86").Property("stroke", "#fff1ce").Property("stroke-width", "0.35")
            .Child("svg-animate", "traverse-edge", a => a
                .Property("attributeName", "cx").Property("values", "27;73")
                .Property("keyTimes", "0;1").Property("dur", "3s")
                .Property("begin", "0s").Property("fill", "freeze")));
        scene.Child("svg-text", "exit-step", n => n.Text("1  EXIT S0")
            .Property("x", "15").Property("y", "43").Property("fill", "#ffcf86").Property("font-size", "2.5").Property("font-weight", "700"));
        scene.Child("svg-text", "identity-step", n => n.Text("2  Current-state identity changes")
            .Property("x", "39").Property("y", "43").Property("fill", "#d6e7f4").Property("font-size", "2.25"));
        scene.Child("svg-text", "enter-step", n => n.Text("3  ENTER S1 on the next eligible update")
            .Property("x", "15").Property("y", "50").Property("fill", "#7ee0bd").Property("font-size", "2.5").Property("font-weight", "700"));
        return scene;
    }

    private static GuiBuilder BuildTickMutation()
    {
        var scene = Frame("fsm-api-tick-atomic-mutation",
            "Definition replacement waits for the tick boundary",
            "A callback requests a replacement while the tick is executing. The request is queued; all selected handles finish against the stable definition view; the replacement commits during finalization.");

        var steps = new[]
        {
            (x: 7, title: "1  SNAPSHOT", detail: "Eligible handles", fill: "#25385a", stroke: "#a5a9f5"),
            (x: 30, title: "2  EXECUTE", detail: "Stable definition", fill: "#174b49", stroke: "#7ee0bd"),
            (x: 53, title: "3  QUEUE", detail: "Replacement requested", fill: "#493b2d", stroke: "#ffcf86"),
            (x: 76, title: "4  COMMIT", detail: "Tick boundary", fill: "#25385a", stroke: "#a5a9f5")
        };
        foreach (var step in steps)
        {
            scene.Child("svg-rect", $"step-{step.x}", n => n
                .Property("x", step.x.ToString()).Property("y", "20").Property("width", "17")
                .Property("height", "15").Property("rx", "1").Property("fill", step.fill)
                .Property("stroke", step.stroke).Property("stroke-width", "0.6"));
            scene.Child("svg-text", $"step-title-{step.x}", n => n.Text(step.title)
                .Property("x", (step.x + 1.2).ToString()).Property("y", "26")
                .Property("fill", "#ffffff").Property("font-size", "2.1").Property("font-weight", "700"));
            scene.Child("svg-text", $"step-detail-{step.x}", n => n.Text(step.detail)
                .Property("x", (step.x + 1.2).ToString()).Property("y", "31")
                .Property("fill", "#d6e7f4").Property("font-size", "1.75"));
        }
        scene.Child("svg-line", "progress-line", n => n
            .Property("x1", "24").Property("y1", "27.5").Property("x2", "76").Property("y2", "27.5")
            .Property("stroke", "#d4e2ee").Property("stroke-width", "0.45"));
        scene.Child("svg-circle", "progress-marker", n => n
            .Property("cx", "15.5").Property("cy", "17").Property("r", "1.8")
            .Property("fill", "#ffcf86")
            .Child("svg-animate", "advance", a => a
                .Property("attributeName", "cx").Property("values", "15.5;38.5;61.5;84.5")
                .Property("keyTimes", "0;0.33;0.66;1").Property("dur", "4s")
                .Property("begin", "0s").Property("fill", "freeze")));
        scene.Child("svg-text", "invariant", n => n.Text("Invariant: no sibling handle observes a half-applied replacement.")
            .Property("x", "9").Property("y", "46").Property("fill", "#eef6ff")
            .Property("font-size", "2.5").Property("font-weight", "700"));
        scene.Child("svg-text", "finalization", n => n.Text("Finalization processes pending lifecycle work before queued registrations.")
            .Property("x", "9").Property("y", "52").Property("fill", "#9fb5c7").Property("font-size", "2.0"));
        return scene.Build();
    }
}
