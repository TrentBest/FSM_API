# 07\. Robust Error Handling: Cascading Degradation System

FSM_API provides error reporting and configurable degradation thresholds to help applications contain and diagnose failures in scheduled FSM execution. These mechanisms improve resilience, but they are not a blanket guarantee that every exception is caught or that application state remains unaffected.

An exception may indicate a failure in user-provided state logic or transition evaluation. FSM_API reports scheduled instance-update failures and can shut down repeatedly failing instances; callers must still handle exceptions from direct API calls and consider the effects of partially completed application logic.

-----

## 📚 Table of Contents

[00. Introduction to FSM_API](00_Introduction.md)

[01. Core Concepts: Your Guide to FSM_API](01_Core_Concepts.md)

[03. Getting Started with C# ](03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](05_Context_Implementation.md)

[06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](06_FSM_Modifier_Deep_Dive.md)

[07. Robust Error Handling: Cascading Degradation System](07_Error_Handling.md)

[08. Performance Tips & Best Practices](08_Performance_Tips.md)

[09. Common Use Cases & Examples](09_Common_Use_Cases.md)

[10. FSM_API for Non-Coders: A Big Picture Overview](10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](11_FAQ.md)


-----

🧠 How It Works: The Four-Level System

The system operates on multiple levels to contain and report errors:

1. Scheduled Update Exception Handling

During the scheduled instance update, `FSMHandle.Update` catches exceptions thrown by `Definition.Step` (including state actions and transition evaluation) and reports them through the instance-error system. This does not mean every public API operation swallows exceptions: for example, `FSMHandle.TransitionTo` reports a transition failure and then rethrows it to the caller. Handle direct API calls according to their documented exception behavior.

2. Error Reporting

When an internal error occurs within the API, such as an exception in user code or an attempt to use an unregistered FSM, it triggers the FSM_API.Error.OnInternalApiError event. This event is your primary mechanism for receiving detailed error reports.

You can subscribe to this event to get real-time diagnostic notifications about non-FSM-specific internal errors, warnings, or significant API events. This allows you to integrate FSM errors with your application's logging or telemetry systems.

3. Instance Degradation (FUBAR'd Instances)

If an FSMHandle instance reports errors, the API increments a per-handle count in an internal dictionary. The implementation does not reset this count after a successful update; it remains until explicitly reset or the handle's count entry is removed during teardown. Therefore, the threshold is a cumulative count since the last reset—not necessarily a run of consecutive failures. When the threshold is reached, shutdown is enqueued for deferred processing.

    Instance Error Threshold: The FSM_API.Error.InstanceErrorThreshold property sets the number of reported instance errors required to enqueue shutdown. The count is cumulative until reset or cleanup. The default value is 5.

4. Definition Degradation (FUBAR'd Definitions)

If a fundamental flaw exists in an FSM Definition (the blueprint), it can cause repeated errors across multiple instances. The system tracks these occurrences. When a definition's error count reaches a certain threshold, the entire definition and all its remaining active instances are scheduled for complete destruction.

    Definition Error Threshold: The FSM_API.Error.DefinitionErrorThreshold property sets the number of times an FSM definition can cause an instance to be shut down due to errors before the entire definition is scheduled for destruction. The default value is 3.

The `IsValid` property within `IStateContext` is an execution gate. If `context.IsValid` returns false, the scheduler skips that handle while it remains registered; restoring validity allows processing to resume. Explicitly unregister or destroy the instance when it should be removed.

⚙️ Configuration and Control

As a developer, you have control over how sensitive the system is and can manually manage error counts.

Adjusting Thresholds

You can modify the default thresholds to match the needs of your application. For example, in a critical system, you might set the InstanceErrorThreshold to 1 to immediately shut down a failing instance.
```csharp
// To change the InstanceErrorThreshold
FSM_API.Error.InstanceErrorThreshold = 2; // Enqueue shutdown when 2 errors have been counted

// To change the DefinitionErrorThreshold
FSM_API.Error.DefinitionErrorThreshold = 1; // Destroy the definition after just one instance fails
```

Manual Reset

The API provides methods to manually reset error counts for specific instances or definitions. This is useful for debugging and testing.

    FSM_API.Error.ResetInstanceErrorCount(FSMHandle handle): Removes the accumulated error count for a specific FSM instance; use this explicitly if your policy requires a fresh count after recovery.

    FSM_API.Error.ResetDefinitionErrorCount(string fsmDefinitionName): Resets the error count for a specific FSM definition.

    FSM_API.Error.Reset(): Clears all accumulated error counts for both FSM instances and definitions, providing a fresh state for the system.

✅ Benefits for a Non-Coder

Even if you're not a coder, this system provides powerful benefits for your projects:

    Failure Containment: Scheduled instance-update failures are reported and repeatedly failing instances can be shut down, reducing the chance that one faulty FSM continues disrupting the system. This does not guarantee that the wider application cannot fail.

    Automatic Degradation: When configured error thresholds are reached, failing instances or definitions can be scheduled for shutdown/removal. This is lifecycle management, not a guarantee against every resource leak.

    Clear Diagnostics: FSM_API generates clear, structured error reports that your developers can use to quickly identify and fix issues. You can think of it as an automatic logbook for FSM failures.
    
-----

[➡️ Continue to: 08. Performance Tips & Best Practices](08_Performance_Tips.md)