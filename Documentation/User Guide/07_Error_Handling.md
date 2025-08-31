# 07\. Robust Error Handling: Cascading Degradation System

The FSM_API is designed with a Robust Error Handling system, also known as the Cascading Degradation System, to ensure your application remains stable and resilient. This system intelligently handles exceptions and prevents them from crashing your entire application.

In complex state machine logic, an unhandled exception in one FSM could crash the entire application, corrupt the game state, or create hidden bugs. The Cascading Degradation System mitigates these risks by isolating failures and maintaining overall system stability.

-----

## 📚 Table of Contents

[00. Introduction to FSM_API](User%20Guide/00_Introduction.md)

[01. Core Concepts: Your Guide to FSM_API](User%20Guide/01_Core_Concepts.md)

[03. Getting Started with C# (Non-Unity)](User%20Guide/03_Getting_Started_CSharp.md)

[04. FSMBuilder Deep Dive: Building Your FSMs](User%20Guide/04_FSM_Builder_Deep_Dive.md)

[05. Understanding and Implementing Your Context (IStateContext)](User%20Guide/05_Context_Implementation.md)

[06. FSMModifier Deep Dive: Modifying Your FSMs at Runtime](User%20Guide/06_FSM_Modifier_Deep_Dive.md)

[07. Robust Error Handling: Cascading Degradation System](User%20Guide/07_Error_Handling.md)

[08. Performance Tips & Best Practices](User%20Guide/08_Performance_Tips.md)

[09. Common Use Cases & Examples](User%20Guide/09_Common_Use_Cases.md)

[10. FSM_API for Non-Coders: A Big Picture Overview](User%20Guide/10_Non_Coder_Overview.md)

[11. Frequently Asked Questions (FAQ)](User%20Guide/11_FAQ.md)


-----

🧠 How It Works: The Four-Level System

The system operates on multiple levels to contain and report errors:

1. Catching All Exceptions

All FSM_API operations that execute user-provided code, such as onEnter, onUpdate, onExit actions, and transition conditions, are wrapped in a try-catch block. This ensures that no exception goes unhandled within the FSM's execution cycle.

2. Error Reporting

When an internal error occurs within the API, such as an exception in user code or an attempt to use an unregistered FSM, it triggers the FSM_API.Error.OnInternalApiError event. This event is your primary mechanism for receiving detailed error reports.

You can subscribe to this event to get real-time diagnostic notifications about non-FSM-specific internal errors, warnings, or significant API events. This allows you to integrate FSM errors with your application's logging or telemetry systems.

3. Instance Degradation (FUBAR'd Instances)

If an FSMHandle instance repeatedly throws exceptions, the system marks it as unstable. The API tracks these errors in an internal dictionary. If an FSM instance reaches a configurable limit of consecutive errors, it will be automatically unregistered from the system to prevent further issues.

    Instance Error Threshold: The FSM_API.Error.InstanceErrorThreshold property sets the maximum number of consecutive errors an FSM instance can encounter before it is automatically shut down. The default value is 5.

4. Definition Degradation (FUBAR'd Definitions)

If a fundamental flaw exists in an FSM Definition (the blueprint), it can cause repeated errors across multiple instances. The system tracks these occurrences. When a definition's error count reaches a certain threshold, the entire definition and all its remaining active instances are scheduled for complete destruction.

    Definition Error Threshold: The FSM_API.Error.DefinitionErrorThreshold property sets the number of times an FSM definition can cause an instance to be shut down due to errors before the entire definition is scheduled for destruction. The default value is 3.

The IsValid property within your IStateContext is another crucial part of error handling. If context.IsValid returns false, the FSM_API knows the underlying object is no longer viable and automatically unregisters the FSMHandle.

⚙️ Configuration and Control

As a developer, you have control over how sensitive the system is and can manually manage error counts.

Adjusting Thresholds

You can modify the default thresholds to match the needs of your application. For example, in a critical system, you might set the InstanceErrorThreshold to 1 to immediately shut down a failing instance.
```csharp
// To change the InstanceErrorThreshold
FSM_API.Error.InstanceErrorThreshold = 2; // Shut down after just 2 consecutive errors

// To change the DefinitionErrorThreshold
FSM_API.Error.DefinitionErrorThreshold = 1; // Destroy the definition after just one instance fails
```

Manual Reset

The API provides methods to manually reset error counts for specific instances or definitions. This is useful for debugging and testing.

    FSM_API.Error.ResetInstanceErrorCount(FSMHandle handle): Resets the error count for a specific FSM instance.

    FSM_API.Error.ResetDefinitionErrorCount(string fsmDefinitionName): Resets the error count for a specific FSM definition.

    FSM_API.Error.Reset(): Clears all accumulated error counts for both FSM instances and definitions, providing a fresh state for the system.

✅ Benefits for a Non-Coder

Even if you're not a coder, this system provides powerful benefits for your projects:

    Application Stability: It prevents bugs in one part of the system from crashing your entire application, ensuring a reliable user experience.

    Automatic Cleanup: The system automatically cleans up problematic FSMs, preventing them from consuming resources or creating memory leaks.

    Clear Diagnostics: FSM_API generates clear, structured error reports that your developers can use to quickly identify and fix issues. You can think of it as an automatic logbook for FSM failures.
    
-----

[➡️ Continue to: 08. Performance Tips & Best Practices](08_Performance_Tips.md)