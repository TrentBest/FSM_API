using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// Provides a highly intuitive and fluent API for defining the structure and behavior of a Finite State Machine (FSM).
    /// </summary>
    /// <remarks>
    /// The <see cref="FSMBuilder"/> serves as the cornerstone for crafting your FSM definitions.
    /// It embodies a **primary core user-friendly approach**, allowing for the very easy and intuitive
    /// expression of complex FSM logic. Its design enables a **non-coder to actually derive the definitions
    /// and scaffold the state methods**, making powerful state machine design accessible to a wider audience.
    /// <para>
    /// FSM definitions are typically established during an application's initialization phase. It's a common pattern
    /// to first check if an FSM by a specific name already <see cref="FSM_API.Interaction.Exists(string,string)"/>; if not, you
    /// then proceed to define it using this builder. Once an FSM is defined, it exists as an "Idea" or a
    /// blueprint. Nothing happens until you explicitly create instances of it by obtaining <see cref="FSMHandle"/>s
    /// for specific contexts (e.g., your game entities).
    /// </para>
    /// <para>
    /// Through its fluent API, you can incrementally add states, define precise transitions between them,
    /// set the FSM's initial starting state, and configure crucial runtime parameters. The method chaining
    /// results in highly readable and self-documenting FSM blueprints, clearly articulating "what" your
    /// system does at each stage. Runtime definition or modification of FSMs is also perfectly supported.
    /// </para>
    /// <para>
    /// Once fully configured, the <see cref="BuildDefinition"/> method finalizes this blueprint and
    /// registers it with the <see cref="FSM_API"/> system. This registration makes your FSM ready to be
    /// instantiated into live, operational FSM instances that drive your application's logic.
    /// When an <see cref="FSMHandle"/> is created from a defined FSM, the <see cref="State(string, Action{IStateContext}, Action{IStateContext}, Action{IStateContext})"/>'s
    /// `onEnter` method for its initial state is immediately invoked.
    /// </para>
    /// <para>
    /// Instances of this builder are typically obtained through the convenient factory methods
    /// provided by <see cref="FSM_API.Create"/>, such as
    /// <see cref="FSM_API.Create.CreateFiniteStateMachine(string, int, string)"/> for new definitions,
    /// or by initializing with an existing <see cref="FSM"/> definition using <see cref="FSMBuilder.FSMBuilder(FSM)"/>
    /// to alter existing ones.
    /// For FSMs that are no longer needed, use <see cref="FSM_API.Interaction.DestroyFiniteStateMachine(string, string)"/> to remove the definition
    /// and any associated handles. Individual handles can be <see cref="FSM_API.Interaction.Unregister(FSMHandle)"/>ed.
    /// </para>
    /// <example>
    /// A simple FSM definition using the <see cref="FSMBuilder"/> to model player states:
    /// <code><![CDATA[
    /// // Define a custom context for player-specific data that states and transitions can access.
    /// // This is how state-specific logic can interact with the broader game world/object.
    /// public class PlayerContext : IStateContext
    /// {
    ///     public string PlayerName { get; set; } = "Hero";
    ///     public bool IsMoving { get; set; } = false;
    ///     public bool HasJumped { get; set; } = false;
    /// }
    ///
    /// // Example usage: Define a "PlayerFSM" that updates 60 times per second
    /// // and belongs to the "GameLoop" processing group.
    /// FSM_API.Create.CreateFiniteStateMachine("PlayerFSM", 60, "GameLoop")
    ///     // Define the "Idle" state and its behaviors
    ///     .State("Idle",
    ///         onEnter: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} is now Idle."),
    ///         onUpdate: (ctx) => { /* Logic to check for player input to move or jump */ },
    ///         onExit: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} stopped being Idle."))
    ///     // Define the "Moving" state
    ///     .State("Moving",
    ///         onEnter: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} started Moving."),
    ///         onUpdate: (ctx) => { /* Handle continuous movement, check if player stops or jumps */ },
    ///         onExit: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} stopped Moving."))
    ///     // Define the "Jumping" state
    ///     .State("Jumping",
    ///         onEnter: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} is Jumping!"),
    ///         onUpdate: (ctx) => { /* Apply jump physics, check for landing */ },
    ///         onExit: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} landed."))
    ///     // Set the starting state for any new FSM instance based on this blueprint
    ///     .WithInitialState("Idle")
    ///     // Define transitions based on conditions evaluated against the PlayerContext
    ///     .Transition("Idle", "Moving", (ctx) => ((PlayerContext)ctx).IsMoving)
    ///     .Transition("Moving", "Idle", (ctx) => !((PlayerContext)ctx).IsMoving)
    ///     .Transition("Moving", "Jumping", (ctx) => ((PlayerContext)ctx).HasJumped)
    ///     .Transition("Idle", "Jumping", (ctx) => ((PlayerContext)ctx).HasJumped)
    ///     // An 'Any-State' transition: from *any* current state, transition to "Idle"
    ///     // if the player has jumped AND is no longer moving (e.g., they landed and stopped)
    ///     .Transition(FSM.AnyStateIdentifier, "Idle", (ctx) => ((PlayerContext)ctx).HasJumped && !((PlayerContext)ctx).IsMoving)
    ///     // Finalize the definition and register it with the FSM system
    ///     .ModifyDefinition();
    /// ]]></code>
    /// </example>
    /// </remarks>
    public class FSMBuilder
    {

        private string _fsmName = "UnNamedFSM";
        private int _processRate;
        private readonly List<FSMState> _states = new();
        private readonly List<FSMTransition> _transitions = new();
        private string _initialState;
        private string _processGroup = "Update";

        /// <summary>
        /// Initializes a new <see cref="FSMBuilder"/> instance for creating a fresh FSM definition.
        /// </summary>
        /// <remarks>
        /// This constructor is primarily intended for internal API use and is invoked by
        /// methods like <see cref="FSM_API.Create.CreateFiniteStateMachine(string, int, string)"/>.
        /// Users typically do not call this constructor directly.
        /// </remarks>
        /// <param name="fsmName">The unique name for the FSM definition. This name will be used to
        /// register and retrieve the FSM within the <see cref="FSM_API"/> system.</param>
        /// <param name="processRate">The default processing rate for instances of this FSM.
        /// Refer to <see cref="FSM.ProcessRate"/> for details on valid values.</param>
        /// <param name="processingGroup">The update category this FSM belongs to, influencing
        /// when and how its instances are ticked. Refer to <see cref="FSM.ProcessingGroup"/> for details.</param>
        public FSMBuilder(string fsmName, int processRate = 0, string processingGroup = "Update")
        {
            _fsmName = fsmName;
            _processRate = processRate;
            _processGroup = processingGroup;
            _initialState = string.Empty; // Will be set explicitly or default to first state during Build
        }

        /// <summary>
        /// Initializes an <see cref="FSMBuilder"/> for an already existing FSM definition,
        /// allowing it to be modified or extended.
        /// </summary>
        /// <remarks>
        /// This constructor is intended for internal API use, typically called by
        /// methods that allow modification of existing FSMs (e.g., in a future
        /// `FSM_API.Interaction.ModifyFiniteStateMachine` if one were to be added).
        /// When using this constructor, the builder is pre-populated with the existing
        /// states and transitions of the provided <paramref name="fsm"/> definition,
        /// allowing for seamless modification via the fluent API.
        /// Note: The name of the FSM will default to a modified version of the original
        /// (e.g., "OriginalName2") to prevent accidental overwriting if the user intends
        /// to create a new FSM based on an existing one without explicitly renaming it.
        /// </remarks>
        /// <param name="fsm">The existing <see cref="FSM"/> definition to load and potentially modify.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="fsm"/> is <c>null</c>.</exception>
        public FSMBuilder(FSM fsm)
        {
            if (fsm == null)
            {
                throw new ArgumentNullException(nameof(fsm), "Cannot initialize FSMBuilder with a null FSM definition.");
            }

            _fsmName = $"{fsm.Name}2"; // Default new name to allow safe modification or cloning

            _processRate = fsm.ProcessRate;
            _initialState = fsm.InitialState;
            _processGroup = fsm.ProcessingGroup;

            // Load existing states and transitions into the builder for modification
            foreach (var state in fsm.GetAllStates())
            {
                _states.Add(state);
            }
            // Note: FSM.GetAllTransitions combines regular and Any-State.
            // When adding back, FSM.AddTransition handles replacement.
            foreach (var transition in fsm.GetAllTransitions())
            {
                _transitions.Add(transition);
            }
        }

        /// <summary>
        /// Adds or updates a state definition within the FSM. This is a core method for
        /// defining the FSM's behavior.
        /// </summary>
        /// <remarks>
        /// Each state is uniquely identified by its <paramref name="name"/>. If a state
        /// with the same name is added multiple times, an <see cref="ArgumentException"/>
        /// will be thrown. The `onEnter`, `onUpdate`, and `onExit` actions define the
        /// specific behavior of the FSM when it enters, stays in, and leaves this state, respectively.
        /// These actions provide the hooks for implementing your FSM's specific logic, and they
        /// receive an <see cref="IStateContext"/> instance, which is how you pass and access
        /// per-FSM-instance data.
        /// </remarks>
        /// <param name="name">The unique name of the state (e.g., "Idle", "Attacking", "Loading").</param>
        /// <param name="onEnter">An <see cref="Action{IStateContext}"/> invoked when the FSM
        /// transitions *into* this state. This is where you typically initialize state-specific behaviors.</param>
        /// <param name="onUpdate">An <see cref="Action{IStateContext}"/> invoked every time
        /// the FSM instance is processed while *in* this state. This is for continuous, per-tick logic.</param>
        /// <param name="onExit">An <see cref="Action{IStateContext}"/> invoked when the FSM
        /// transitions *out of* this state. This is where you typically clean up state-specific resources or stop behaviors.</param>
        /// <returns>The current <see cref="FSMBuilder"/> instance, allowing for fluent chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="name"/> is null, empty,
        /// whitespace, or if a state with the same name already exists in this builder.</exception>
        public FSMBuilder State(string name, Action<IStateContext> onEnter, Action<IStateContext> onUpdate, Action<IStateContext> onExit)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("State name cannot be null or empty.", nameof(name));
            }
            if (_states.Any(s => s.Name == name))
            {
                throw new ArgumentException($"State with name '{name}' already exists in this FSM definition. State names must be unique.", nameof(name));
            }
            // No null checks for actions here, as they can legitimately be null (no-op)
            // The FSMState constructor handles this gracefully if it's designed to.

            _states.Add(new FSMState(name, onEnter, onUpdate, onExit));
            return this;
        }

        /// <summary>
        /// Sets the automatic processing rate for instances of this FSM definition.
        /// </summary>
        /// <remarks>
        /// This controls how frequently the FSM's <see cref="FSM.Step"/> method will be
        /// automatically called when its <see cref="_processGroup"/> is updated.
        /// Refer to <see cref="FSM.ProcessRate"/> for a detailed explanation of valid values
        /// (-1, 0, or positive integers).
        /// </remarks>
        /// <param name="rate">The desired processing rate.</param>
        /// <returns>The current <see cref="FSMBuilder"/> instance, allowing for fluent chaining.</returns>
        public FSMBuilder WithProcessRate(int rate)
        {
            _processRate = rate;
            return this;
        }

        /// <summary>
        /// Assigns a unique name to this FSM definition.
        /// </summary>
        /// <remarks>
        /// This name is crucial for registering and later retrieving the FSM blueprint
        /// from the <see cref="FSM_API"/> system. It should be unique across all FSM definitions.
        /// If this builder was initialized to modify an existing FSM (e.g., via <see cref="FSMBuilder.FSMBuilder(FSM)"/>),
        /// calling `WithName` allows you to explicitly rename the FSM.
        /// </remarks>
        /// <param name="name">The unique name for the FSM definition.</param>
        /// <returns>The current <see cref="FSMBuilder"/> instance, allowing for fluent chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="name"/> is null, empty, or whitespace.</exception>
        public FSMBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(name));
            }
            _fsmName = name;
            return this;
        }

        /// <summary>
        /// Specifies the initial state for new instances of this FSM definition.
        /// </summary>
        /// <remarks>
        /// When an <see cref="FSMHandle"/> is created from this definition, it will
        /// immediately enter the state specified by <paramref name="name"/>.
        /// It is crucial that the state specified here has been previously defined
        /// using the <see cref="State"/> method. If not explicitly set, the first
        /// state added to the builder will be automatically chosen as the initial state
        /// during <see cref="BuildDefinition"/>. Therefore, the order in which states
        /// are added using <see cref="State"/> can matter if no explicit initial state is set.
        /// </remarks>
        /// <param name="name">The name of the state that will be the initial state.</param>
        /// <returns>The current <see cref="FSMBuilder"/> instance, allowing for fluent chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="name"/> is null, empty, or whitespace.</exception>
        public FSMBuilder WithInitialState(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Initial state name cannot be null or empty.", nameof(name));
            }
            _initialState = name;
            return this;
        }

        /// <summary>
        /// Adds a transition rule between two specific states.
        /// </summary>
        /// <remarks>
        /// A regular transition defines that the FSM can move from the <paramref name="from"/> state
        /// to the <paramref name="to"/> state only if the <paramref name="condition"/>
        /// evaluates to <c>true</c> while the FSM is currently in the <paramref name="from"/> state.
        /// <para>
        /// **Important for Definition Order:** While the <paramref name="to"/> state does not
        /// strictly need to be defined (via <see cref="State"/>) before a transition is added,
        /// it is **critical** that the <paramref name="from"/> state *is* defined in the builder
        /// chain *before* its transitions are declared. If a transition refers to a 'from' state
        /// that does not exist by the time <see cref="BuildDefinition"/> is called, that specific
        /// transition may be ignored or cause an error depending on the underlying FSM implementation.
        /// </para>
        /// For global transitions that can fire from *any* state, use <see cref="FSM.AnyStateIdentifier"/>
        /// as the <paramref name="from"/> parameter.
        /// </remarks>
        /// <param name="from">The name of the source state from which the transition can occur. Use <see cref="FSM.AnyStateIdentifier"/> for global (any-state) transitions.</param>
        /// <param name="to">The name of the target state to which the FSM will transition.</param>
        /// <param name="condition">A <see cref="Func{T, TResult}"/> delegate that takes an <see cref="IStateContext"/>
        /// and returns <c>true</c> if the transition should occur, <c>false</c> otherwise.</param>
        /// <returns>The current <see cref="FSMBuilder"/> instance, allowing for fluent chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="from"/> or <paramref name="to"/>
        /// state names are null, empty, or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="condition"/> function is <c>null</c>.</exception>
        public FSMBuilder Transition(string from, string to, Func<IStateContext, bool> condition)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                throw new ArgumentException("'From' state name cannot be null or empty.", nameof(@from));
            }
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("'To' state name cannot be null or empty.", nameof(to));
            }
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition), "Transition condition cannot be null.");
            }

            // Note: The FSM class handles logic for replacing duplicate transitions (e.g., if a new condition is set for an existing from->to pair)
            _transitions.Add(new FSMTransition(@from, to, condition));
            return this;
        }

        /// <summary>
        /// Assigns this FSM definition to a specific processing group (update category).
        /// </summary>
        /// <remarks>
        /// This property allows you to organize FSMs into logical groups, giving you granular
        /// control over when and how they are updated. For example, you might have "PhysicsUpdate"
        /// or "AnimationUpdate" groups. Updates for these groups are typically triggered
        /// manually by calling <see cref="FSM_API.Interaction.Update(string)"/>.
        /// <para>
        /// A powerful feature of this system is that **each processing group can maintain its own
        /// unique copy of the same FSM definition**. If an <see cref="FSMHandle"/> (and its associated
        /// context) is migrated to a different processing group, the system will seamlessly
        /// create a dedicated copy of the FSM definition within that new group, ensuring isolated
        /// and optimized processing without affecting other groups.
        /// </para>
        /// This feature also enables complex hierarchical FSM systems, where one FSM's state
        /// logic might trigger the update of another FSM's processing group.
        /// </remarks>
        /// <param name="category">The name of the processing group (e.g., "Update", "GameLogic", "AI").</param>
        /// <returns>The current <see cref="FSMBuilder"/> instance, allowing for fluent chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="category"/> is null, empty, or whitespace.</exception>
        public FSMBuilder WithUpdateCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(category));
            }
            _processGroup = category;
            return this;
        }

        /// <summary>
        /// Finalizes the FSM definition and registers it with the <see cref="FSM_API"/> system.
        /// </summary>
        /// <remarks>
        /// This is the terminal step in defining an FSM. Calling this method performs
        /// crucial validations (e.g., ensuring at least one state exists, and the initial state is valid)
        /// and then constructs the immutable <see cref="FSM"/> object based on the
        /// builder's configuration. The resulting <see cref="FSM"/> blueprint is then
        /// registered with the global <see cref="FSM_API.Internal.Register(string, FSM, int, string)"/>
        /// system, making it available for instantiation into live <see cref="FSMHandle"/> objects.
        /// <para>
        /// If an FSM definition with the same name already exists within its specified <see cref="WithUpdateCategory(string)"/>,
        /// this operation will overwrite (update) that existing definition.
        /// </para>
        /// After <see cref="BuildDefinition"/> is called, the builder's internal state
        /// is cleared, making it ready to define a new FSM or be re-used. The FSM definition, once built,
        /// can be managed and potentially <see cref="FSM_API.Interaction.DestroyFiniteStateMachine(string, string)"/>ed if no longer needed.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no states have been defined using <see cref="State(string, Action{IStateContext}, Action{IStateContext}, Action{IStateContext})"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the specified initial state does not exist among the defined states.
        /// </exception>
        public void BuildDefinition()
        {
            // --- Validation before building ---
            if (_states.Count == 0)
            {
                throw new InvalidOperationException($"FSM '{_fsmName}' cannot be built: No states have been defined. Use .State() to add states.");
            }

            string finalInitialState;
            if (string.IsNullOrWhiteSpace(_initialState))
            {
                // If no initial state explicitly set, use the first added state (order matters here!)
                finalInitialState = _states[0].Name;
            }
            else
            {
                // Validate that the specified initial state actually exists
                if (!_states.Any(s => s.Name == _initialState))
                {
                    throw new ArgumentException($"Initial state '{_initialState}' specified for FSM '{_fsmName}' does not exist. Ensure you add it with .State() before building.", nameof(_initialState));
                }
                finalInitialState = _initialState;
            }



            // --- Build the FSM ---
            var machine = new FSM
            {
                Name = _fsmName,
                ProcessRate = _processRate,
                InitialState = finalInitialState,
                ProcessingGroup = _processGroup
            };

            foreach (var s in _states)
            {
                machine.AddState(s);
            }
            foreach (var t in _transitions)
            {
                // AddTransition method in FSM handles both regular and AnyState transitions based on 'from'
                // and is responsible for validating if 'from' states exist or if 'to' states need to exist.
                machine.AddTransition(t.From, t.To, t.Condition);
            }

            // Register with FSM_API. This handles new registration or updating an existing one.
            FSM_API.Internal.Register(
                _fsmName,
                machine,
                _processRate,
                _processGroup);

            // Clear builder state to prevent accidental reuse of internal lists for a new build operation.
            _states.Clear();
            _transitions.Clear();
            _initialState = string.Empty;
            _processRate = 0;
            _processGroup = "Update";
        }

        /// <summary>
        /// Removes a state from the FSM definition being built by this builder.
        /// </summary>
        /// <remarks>
        /// This method removes the specified state from the internal list of states managed by the builder.
        /// When <see cref="BuildDefinition"/> is called, the FSM will be constructed without this state.
        /// <para>
        /// If you are modifying an existing FSM definition at runtime and need to handle
        /// active instances currently in the state being removed, you should use
        /// <see cref="FSM_API.Interaction.RemoveStateFromFSM(string, string, string)"/>,
        /// which wraps this builder method and manages instance transitions.
        /// </para>
        /// If the state does not exist in the builder's current definition, no action is taken.
        /// </remarks>
        /// <param name="stateName">The name of the state to remove.</param>
        /// <returns>The current <see cref="FSMBuilder"/> instance for fluent chaining.</returns>
        public FSMBuilder Without(string stateName)
        {
            var state = _states.FirstOrDefault(s => s.Name == stateName);
            if (state != null)
            {
                var handles = FSM_API.Internal.GetBucket(_fsmName, _processGroup);
                var handlesInState = handles.Instances.Where(s => s.CurrentState == stateName);
                foreach (var handleInState in handlesInState)
                {
                    handleInState.TransitionTo(_initialState);
                }
                _states.Remove(state);
            }
            return this;
        }

        /// <summary>
        /// Removes a specific transition from the FSM definition being built by this builder.
        /// </summary>
        /// <remarks>
        /// This method removes the specified transition from the internal list of transitions managed by the builder.
        /// When <see cref="BuildDefinition"/> is called, the FSM will be constructed without this transition.
        /// <para>
        /// This is useful for dynamically adjusting FSM behavior by removing paths. If the transition
        /// does not exist in the builder's current definition, no action is taken.
        /// </para>
        /// If you are modifying an existing FSM definition at runtime and wish to remove a transition,
        /// you should typically use <see cref="FSM_API.Interaction.RemoveTransition(string,string,string,string)"/>,
        /// which wraps this builder method.
        /// </remarks>
        /// <param name="fromState">The name of the state from which the transition originates.
        /// This can be <see cref="FSM.AnyStateIdentifier"/> for 'Any State' transitions.</param>
        /// <param name="toState">The name of the state to which the transition leads.</param>
        /// <returns>The current <see cref="FSMBuilder"/> instance for fluent chaining.</returns>
        public FSMBuilder WithoutTransition(string fromState, string toState)
        {
            var transition = _transitions.FirstOrDefault(s => s.From == fromState && s.To == toState);
            if (transition != null)
            {
                _transitions.Remove(transition);
            }
            return this;
        }
    }
}