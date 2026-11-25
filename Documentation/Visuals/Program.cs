using TheSingularityWorkshop.Workshop.Gui;
using TheSingularityWorkshop.Workshop.Gui.Svg;

var outputDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Documentation/Visuals"));
Directory.CreateDirectory(outputDirectory);

Write("fsm-api-ownership.svg", BuildOwnership(), 1280, 720);
Write("fsm-api-lifecycle.svg", BuildLifecycle(), 1280, 720);
Write("fsm-api-representations.svg", BuildRepresentations(), 1280, 720);
Write("fsm-api-tick-boundary.svg", BuildTickBoundary(), 1280, 720);

void Write(string name, GuiNode scene, int width, int height)
{
    var svg = new SvgGuiRenderer().Render(scene, width, height);
    File.WriteAllText(Path.Combine(outputDirectory, name), svg);
    Console.WriteLine($"Generated {name}");
}

static GuiNode BuildOwnership()
{
    var scene = GuiBuilder.Create("svg-scene", "fsm-api-ownership")
        .Text("FSM_API ownership boundaries")
        .Property("description", "The host owns the clock and domain data. FSM_API owns definitions and handles. A handle retains the caller-owned context reference.");

    scene.Child("svg-rect", "host", n => n.Property("x","4").Property("y","5").Property("width","92").Property("height","12").Property("rx","1").Property("fill","#19344a").Property("stroke","#69b9d0"));
    scene.Child("svg-text", "host-label", n => n.Text("APPLICATION / HOST — owns data, services, and update loop").Property("x","8").Property("y","12").Property("fill","#f4f8fc").Property("font-size","2.5"));
    scene.Child("svg-rect", "definition", n => n.Property("x","8").Property("y","25").Property("width","36").Property("height","19").Property("rx","1").Property("fill","#25385a").Property("stroke","#a5a9f5"));
    scene.Child("svg-text", "definition-label", n => n.Text("FSM DEFINITION").Property("x","11").Property("y","31").Property("fill","#ffffff").Property("font-size","2.7").Property("font-weight","700"));
    scene.Child("svg-text", "definition-detail", n => n.Text("States · guards · callbacks").Property("x","11").Property("y","37").Property("fill","#d8e4f2").Property("font-size","2.1"));
    scene.Child("svg-rect", "handle", n => n.Property("x","56").Property("y","25").Property("width","36").Property("height","19").Property("rx","1").Property("fill","#174b49").Property("stroke","#7ee0bd"));
    scene.Child("svg-text", "handle-label", n => n.Text("LIVE FSM HANDLE").Property("x","59").Property("y","31").Property("fill","#ffffff").Property("font-size","2.7").Property("font-weight","700"));
    scene.Child("svg-text", "handle-detail", n => n.Text("State · lifecycle · identity").Property("x","59").Property("y","37").Property("fill","#d8f4eb").Property("font-size","2.1"));
    scene.Child("svg-line", "definition-handle", n => n.Property("x1","44").Property("y1","34").Property("x2","56").Property("y2","34").Property("stroke","#d4e2ee").Property("stroke-width","0.45"));
    scene.Child("svg-rect", "context", n => n.Property("x","56").Property("y","54").Property("width","36").Property("height","15").Property("rx","1").Property("fill","#493b2d").Property("stroke","#ffcf86"));
    scene.Child("svg-text", "context-label", n => n.Text("CALLER-OWNED CONTEXT").Property("x","59").Property("y","60").Property("fill","#ffffff").Property("font-size","2.35").Property("font-weight","700"));
    scene.Child("svg-text", "context-detail", n => n.Text("Same object reference").Property("x","59").Property("y","65").Property("fill","#f8e7c8").Property("font-size","2.1"));
    scene.Child("svg-line", "handle-context", n => n.Property("x1","74").Property("y1","44").Property("x2","74").Property("y2","54").Property("stroke","#ffcf86").Property("stroke-width","0.5"));
    scene.Child("svg-text", "host-note", n => n.Text("Host calls Update when ready").Property("x","8").Property("y","57").Property("fill","#d8e4f2").Property("font-size","2.5"));
    scene.Child("svg-text", "boundary-note", n => n.Text("FSM_API does not own the application's clock or copy its domain object.").Property("x","8").Property("y","77").Property("fill","#eef6ff").Property("font-size","2.5"));
    return scene.Build();
}

static GuiNode BuildLifecycle()
{
    var scene = GuiBuilder.Create("svg-scene", "fsm-api-lifecycle")
        .Text("FSM_API handle lifecycle")
        .Property("description", "A valid handle enters on its first eligible update, updates, evaluates transitions, exits, and enters the destination on a later update. Invalid context suspends execution without destruction.");
    scene.Child("svg-rect","created",n=>n.Property("x","5").Property("y","12").Property("width","16").Property("height","10").Property("rx","1").Property("fill","#25385a").Property("stroke","#a5a9f5"));
    scene.Child("svg-text","created-t",n=>n.Text("Created").Property("x","8").Property("y","18").Property("fill","#ffffff").Property("font-size","2.5"));
    scene.Child("svg-rect","entered",n=>n.Property("x","29").Property("y","12").Property("width","18").Property("height","10").Property("rx","1").Property("fill","#174b49").Property("stroke","#7ee0bd"));
    scene.Child("svg-text","entered-t",n=>n.Text("Enter").Property("x","34").Property("y","18").Property("fill","#ffffff").Property("font-size","2.5"));
    scene.Child("svg-rect","update",n=>n.Property("x","55").Property("y","12").Property("width","18").Property("height","10").Property("rx","1").Property("fill","#174b49").Property("stroke","#7ee0bd"));
    scene.Child("svg-text","update-t",n=>n.Text("Update").Property("x","60").Property("y","18").Property("fill","#ffffff").Property("font-size","2.5"));
    scene.Child("svg-rect","transition",n=>n.Property("x","81").Property("y","12").Property("width","15").Property("height","10").Property("rx","1").Property("fill","#493b2d").Property("stroke","#ffcf86"));
    scene.Child("svg-text","transition-t",n=>n.Text("Guard").Property("x","84").Property("y","18").Property("fill","#ffffff").Property("font-size","2.5"));
    scene.Child("svg-line","a",n=>n.Property("x1","21").Property("y1","17").Property("x2","29").Property("y2","17").Property("stroke","#d4e2ee").Property("stroke-width","0.5"));
    scene.Child("svg-line","b",n=>n.Property("x1","47").Property("y1","17").Property("x2","55").Property("y2","17").Property("stroke","#d4e2ee").Property("stroke-width","0.5"));
    scene.Child("svg-line","c",n=>n.Property("x1","73").Property("y1","17").Property("x2","81").Property("y2","17").Property("stroke","#d4e2ee").Property("stroke-width","0.5"));
    scene.Child("svg-rect","exit",n=>n.Property("x","81").Property("y","36").Property("width","15").Property("height","10").Property("rx","1").Property("fill","#493b2d").Property("stroke","#ffcf86"));
    scene.Child("svg-text","exit-t",n=>n.Text("Exit").Property("x","85").Property("y","42").Property("fill","#ffffff").Property("font-size","2.5"));
    scene.Child("svg-rect","next-enter",n=>n.Property("x","55").Property("y","36").Property("width","18").Property("height","10").Property("rx","1").Property("fill","#174b49").Property("stroke","#7ee0bd"));
    scene.Child("svg-text","next-enter-t",n=>n.Text("Next Enter").Property("x","57").Property("y","42").Property("fill","#ffffff").Property("font-size","2.2"));
    scene.Child("svg-line","d",n=>n.Property("x1","88").Property("y1","22").Property("x2","88").Property("y2","36").Property("stroke","#ffcf86").Property("stroke-width","0.5"));
    scene.Child("svg-line","e",n=>n.Property("x1","81").Property("y1","41").Property("x2","73").Property("y2","41").Property("stroke","#d4e2ee").Property("stroke-width","0.5"));
    scene.Child("svg-text","timing",n=>n.Text("The destination Enter runs on the next eligible update — not in the transition step.").Property("x","8").Property("y","57").Property("fill","#eef6ff").Property("font-size","2.7"));
    scene.Child("svg-rect","suspend",n=>n.Property("x","8").Property("y","68").Property("width","36").Property("height","10").Property("rx","1").Property("fill","#493b2d").Property("stroke","#ffcf86"));
    scene.Child("svg-text","suspend-t",n=>n.Text("IsValid = false → suspended").Property("x","11").Property("y","74").Property("fill","#ffffff").Property("font-size","2.2"));
    scene.Child("svg-rect","destroy",n=>n.Property("x","56").Property("y","68").Property("width","36").Property("height","10").Property("rx","1").Property("fill","#4b2732").Property("stroke","#ff8f9b"));
    scene.Child("svg-text","destroy-t",n=>n.Text("Explicit destroy → removed").Property("x","59").Property("y","74").Property("fill","#ffffff").Property("font-size","2.2"));
    return scene.Build();
}

static GuiNode BuildRepresentations()
{
    var scene = GuiBuilder.Create("svg-scene", "fsm-api-representations")
        .Text("Two representations, one behavioral contract")
        .Property("description", "String-backed and integer-backed runtimes have independent registries and identity spaces. Explicit Transfer converts definitions between them.");
    scene.Child("svg-rect","string",n=>n.Property("x","5").Property("y","12").Property("width","34").Property("height","30").Property("rx","1").Property("fill","#25385a").Property("stroke","#a5a9f5"));
    scene.Child("svg-text","string-title",n=>n.Text("STRING-BACKED").Property("x","9").Property("y","20").Property("fill","#ffffff").Property("font-size","3").Property("font-weight","700"));
    scene.Child("svg-text","string-id",n=>n.Text("Names · named groups").Property("x","9").Property("y","28").Property("fill","#d8e4f2").Property("font-size","2.4"));
    scene.Child("svg-text","string-handle",n=>n.Text("FSMHandle").Property("x","9").Property("y","36").Property("fill","#d8e4f2").Property("font-size","2.4"));
    scene.Child("svg-rect","integer",n=>n.Property("x","61").Property("y","12").Property("width","34").Property("height","30").Property("rx","1").Property("fill","#174b49").Property("stroke","#7ee0bd"));
    scene.Child("svg-text","integer-title",n=>n.Text("INTEGER-BACKED").Property("x","65").Property("y","20").Property("fill","#ffffff").Property("font-size","3").Property("font-weight","700"));
    scene.Child("svg-text","integer-id",n=>n.Text("IDs · integer groups").Property("x","65").Property("y","28").Property("fill","#d8f4eb").Property("font-size","2.4"));
    scene.Child("svg-text","integer-handle",n=>n.Text("FSMHandleInt").Property("x","65").Property("y","36").Property("fill","#d8f4eb").Property("font-size","2.4"));
    scene.Child("svg-rect","transfer",n=>n.Property("x","43").Property("y","52").Property("width","14").Property("height","10").Property("rx","1").Property("fill","#493b2d").Property("stroke","#ffcf86"));
    scene.Child("svg-text","transfer-t",n=>n.Text("Transfer").Property("x","45").Property("y","58").Property("fill","#ffffff").Property("font-size","2.2"));
    scene.Child("svg-line","left",n=>n.Property("x1","22").Property("y1","42").Property("x2","48").Property("y2","52").Property("stroke","#ffcf86").Property("stroke-width","0.5"));
    scene.Child("svg-line","right",n=>n.Property("x1","78").Property("y1","42").Property("x2","52").Property("y2","52").Property("stroke","#ffcf86").Property("stroke-width","0.5"));
    scene.Child("svg-text","shared",n=>n.Text("Same observable contract: lifecycle · transitions · scheduling · errors").Property("x","8").Property("y","77").Property("fill","#eef6ff").Property("font-size","2.4"));
    return scene.Build();
}

static GuiNode BuildTickBoundary()
{
    var scene = GuiBuilder.Create("svg-scene", "fsm-api-tick-boundary")
        .Text("A tick is a consistency boundary")
        .Property("description", "The runtime snapshots eligible work, defers callback-originated definition replacement, processes lifecycle changes, then commits the last queued replacement at the end of the tick.");
    scene.Child("svg-rect","snapshot",n=>n.Property("x","5").Property("y","12").Property("width","19").Property("height","16").Property("rx","1").Property("fill","#25385a").Property("stroke","#a5a9f5"));
    scene.Child("svg-text","snapshot-t",n=>n.Text("1. SNAPSHOT").Property("x","8").Property("y","19").Property("fill","#ffffff").Property("font-size","2.4").Property("font-weight","700"));
    scene.Child("svg-text","snapshot-d",n=>n.Text("Eligible handles").Property("x","8").Property("y","24").Property("fill","#d8e4f2").Property("font-size","2.1"));
    scene.Child("svg-rect","execute",n=>n.Property("x","29").Property("y","12").Property("width","19").Property("height","16").Property("rx","1").Property("fill","#174b49").Property("stroke","#7ee0bd"));
    scene.Child("svg-text","execute-t",n=>n.Text("2. EXECUTE").Property("x","32").Property("y","19").Property("fill","#ffffff").Property("font-size","2.4").Property("font-weight","700"));
    scene.Child("svg-text","execute-d",n=>n.Text("Stable definition").Property("x","32").Property("y","24").Property("fill","#d8f4eb").Property("font-size","2.1"));
    scene.Child("svg-rect","queue",n=>n.Property("x","53").Property("y","12").Property("width","19").Property("height","16").Property("rx","1").Property("fill","#493b2d").Property("stroke","#ffcf86"));
    scene.Child("svg-text","queue-t",n=>n.Text("3. QUEUE").Property("x","56").Property("y","19").Property("fill","#ffffff").Property("font-size","2.4").Property("font-weight","700"));
    scene.Child("svg-text","queue-d",n=>n.Text("Last replace wins").Property("x","56").Property("y","24").Property("fill","#f8e7c8").Property("font-size","2.1"));
    scene.Child("svg-rect","commit",n=>n.Property("x","77").Property("y","12").Property("width","18").Property("height","16").Property("rx","1").Property("fill","#25385a").Property("stroke","#a5a9f5"));
    scene.Child("svg-text","commit-t",n=>n.Text("4. COMMIT").Property("x","80").Property("y","19").Property("fill","#ffffff").Property("font-size","2.4").Property("font-weight","700"));
    scene.Child("svg-text","commit-d",n=>n.Text("At tick end").Property("x","80").Property("y","24").Property("fill","#d8e4f2").Property("font-size","2.1"));
    scene.Child("svg-line","ab",n=>n.Property("x1","24").Property("y1","20").Property("x2","29").Property("y2","20").Property("stroke","#d4e2ee").Property("stroke-width","0.5"));
    scene.Child("svg-line","bc",n=>n.Property("x1","48").Property("y1","20").Property("x2","53").Property("y2","20").Property("stroke","#d4e2ee").Property("stroke-width","0.5"));
    scene.Child("svg-line","cd",n=>n.Property("x1","72").Property("y1","20").Property("x2","77").Property("y2","20").Property("stroke","#d4e2ee").Property("stroke-width","0.5"));
    scene.Child("svg-text","note",n=>n.Text("Callbacks may request replacement, but no sibling handle sees it mid-tick.").Property("x","8").Property("y","48").Property("fill","#eef6ff").Property("font-size","2.8"));
    scene.Child("svg-text","finalize",n=>n.Text("Finalization order: pending lifecycle work → queued registrations.").Property("x","8").Property("y","58").Property("fill","#d8e4f2").Property("font-size","2.5"));
    scene.Child("svg-text","reentry",n=>n.Text("Nested Update / UpdateAll / handle updates are rejected.").Property("x","8").Property("y","68").Property("fill","#ffcf86").Property("font-size","2.5"));
    return scene.Build();
}
