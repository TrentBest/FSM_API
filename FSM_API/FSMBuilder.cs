using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSingularityWorkshop.FSM_API
{
    /// <summary>
    /// This is your **primary tool for creating and setting up new FSMs (Finite State Machines)**. 🛠️
    /// Think of it as a special "construction kit" that lets you easily build complex FSM blueprints step-by-step.
    /// </summary>
    /// <remarks>
    /// The <see cref="FSMBuilder"/> is designed to be very **user-friendly**, allowing you to define
    /// how your FSM works in a clear, flowing way (this is called a "fluent API").
    /// Even if you're not a seasoned coder, you can use this builder to:
    /// <list type="bullet">
    ///     <item>Define each **state** the FSM can be in (like "Idle" or "Running").</item>
    ///     <item>Set up the **rules (transitions)** for moving between these states.</item>
    ///     <item>Choose the FSM's **starting state**.</item>
    ///     <item>And decide **how often** the FSM should check its rules.</item>
    /// </list>
    /// <para>
    /// You typically use this builder once during your application's setup to create the FSM's blueprint.
    /// After the blueprint is built, you then create **instances** of that FSM using an <see cref="FSMHandle"/>
    /// to actually make things happen in your game or application.
    /// </para>
    /// <para>
    /// The methods in this builder are designed to be "chained" together, making your FSM definitions
    /// very readable, almost like telling a story of how your FSM behaves.
    /// Once you're done setting everything up, you call <see cref="BuildDefinition"/> to save your
    /// FSM blueprint and make it available for use.
    /// </para>
    /// <example>
    /// Here's a simple example of building an FSM for a player character, showing how readable it can be:
    /// <code><![CDATA[
    /// // First, define a "context" (a data bag) for your player FSM.
    /// // This is where you'll store specific player information like if they are moving or jumping.
    /// public class PlayerContext : IStateContext
    /// {
    ///     public string PlayerName { get; set; } = "Hero";
    ///     public bool IsMoving { get; set; } = false;
    ///     public bool HasJumped { get; set; } = false;
    /// }
    ///
    /// // Now, use the FSMBuilder to create your "PlayerFSM" blueprint.
    /// // We want it to update 60 times per second and be part of the "GameLoop" group.
    /// FSM_API.Create.CreateFiniteStateMachine("PlayerFSM", 60, "GameLoop")
    ///     // Define the "Idle" state: what happens when entering, during updates, and when exiting.
    ///     .State("Idle",
    ///         onEnter: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} is now Idle."),
    ///         onUpdate: (ctx) => { /* Check for player input to move or jump */ },
    ///         onExit: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} stopped being Idle."))
    ///     // Define the "Moving" state.
    ///     .State("Moving",
    ///         onEnter: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} started Moving."),
    ///         onUpdate: (ctx) => { /* Handle continuous movement, check if player stops or jumps */ },
    ///         onExit: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} stopped Moving."))
    ///     // Define the "Jumping" state.
    ///     .State("Jumping",
    ///         onEnter: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} is Jumping!"),
    ///         onUpdate: (ctx) => { /* Apply jump physics, check for landing */ },
    ///         onExit: (ctx) => Console.WriteLine($"{((PlayerContext)ctx).PlayerName} landed."))
    ///     // Tell the FSM that "Idle" is the very first state a new instance will start in.
    ///     .WithInitialState("Idle")
    ///     // Set up the rules (transitions) for how the FSM moves between states.
    ///     // These conditions check the player's data in the PlayerContext.
    ///     .Transition("Idle", "Moving", (ctx) => ((PlayerContext)ctx).IsMoving)
    ///     .Transition("Moving", "Idle", (ctx) => !((PlayerContext)ctx).IsMoving)
    ///     .Transition("Moving", "Jumping", (ctx) => ((PlayerContext)ctx).HasJumped)
    ///     .Transition("Idle", "Jumping", (ctx) => ((PlayerContext)ctx).HasJumped)
    ///     // This is an "Any-State" transition: from *any* current state, if the player
    ///     // has jumped AND is no longer moving, transition back to "Idle" (e.g., they landed).
    ///     .Transition(FSM.AnyStateIdentifier, "Idle", (ctx) => ((PlayerContext)ctx).HasJumped && !((PlayerContext)ctx).IsMoving)
    ///     // Finish building and save this FSM blueprint so you can create live instances from it.
    ///     .BuildDefinition();
    /// ]]></code>
    /// </example>
    /// </remarks>
    public class FSMBuilder
    {

        private string _fsmName = "UnNamedFSM";
        private int _processRate;
        private readonly List<FSMState> _states = new List<FSMState>();
        private readonly List<FSMTransition> _transitions = new List<FSMTransition>();
        private string _initialState;
        private string _processGroup = "Update";
        private readonly List<FSMTransition> _anyTransitions = new List<FSMTransition>();

        /// <summary>
        /// Starts a new <see cref="FSMBuilder"/> to help you create a brand-new FSM blueprint.
        /// </summary>
        /// <remarks>
        /// You usually won't call this directly. Instead, you'll use a helper method like
        /// <see cref="FSM_API.Create.CreateFiniteStateMachine(string, int, string)"/>, which then uses this behind the scenes.
        /// </remarks>
        /// <param name="fsmName">The unique name for your new FSM blueprint (e.g., "PlayerMovementFSM").</param>
        /// <param name="processRate">How often instances of this FSM should automatically update. See <see cref="FSM.ProcessRate"/> for details.</param>
        /// <param name="processingGroup">A name for a group this FSM belongs to, which helps organize updates. See <see cref="FSM.ProcessingGroup"/> for details.</param>
        public FSMBuilder(string fsmName, int processRate = 0, string processingGroup = "Update")
        {
            _fsmName = fsmName;
            _processRate = processRate;
            _processGroup = processingGroup;
            _initialState = string.Empty; // Will be set explicitly or default to first state during Build
        }

        /// <summary>
        /// Starts an <see cref="FSMBuilder"/> with an **existing FSM blueprint** loaded into it.
        /// This lets you make changes or add new features to an FSM that's already defined.
        /// </summary>
        /// <remarks>
        /// This is for advanced use when you need to modify an FSM blueprint after it's been created.
        /// The builder will be pre-filled with all the states and transitions from the <paramref name="fsm"/>
        /// you provide, so you can continue building on it.
        /// </remarks>
        /// <param name="fsm">The existing FSM blueprint you want to load and modify.</param>
        /// <exception cref="ArgumentNullException">Happens if you try to load a `null` FSM blueprint.</exception>
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
        /// Adds or updates a **"state"** for your FSM blueprint. This is where you define what happens
        /// when your FSM is in a particular condition or mode.
        /// </summary>
        /// <remarks>
        /// Each state needs a unique <paramref name="name"/>. If you try to add a state with the same
        /// name twice, you'll get an error.
        /// <para>
        /// You also define three key actions for each state:
        /// <list type="bullet">
        ///     <item><term><paramref name="onEnter"/>:</term><description>What happens **when the FSM first enters** this state.</description></item>
        ///     <item><term><paramref name="onUpdate"/>:</term><description>What happens **repeatedly while the FSM is in** this state.</description></item>
        ///     <item><term><paramref name="onExit"/>:</term><description>What happens **just before the FSM leaves** this state.</description></item>
        /// </list>
        /// </para>
        /// These actions receive a `context` (your <see cref="IStateContext"/> data bag)
        /// which lets them interact with the specific object or system that this FSM instance belongs to.
        /// You can provide `null` for any action if nothing specific needs to happen at that point.
        /// </remarks>
        /// <param name="name">The unique name for this state (e.g., "Idle", "Attacking").</param>
        /// <param name="onEnter">An optional action to run when entering this state.</param>
        /// <param name="onUpdate">An optional action to run repeatedly while in this state.</param>
        /// <param name="onExit">An optional action to run when exiting this state.</param>
        /// <returns>The builder itself, so you can chain more commands.</returns>
        /// <exception cref="ArgumentException">Happens if the state `name` is empty or a state with that name already exists.</exception>
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
        /// Sets how often instances of this FSM blueprint should automatically check and update themselves.
        /// </summary>
        /// <remarks>
        /// This controls the **"Process Rate"**. See <see cref="FSM.ProcessRate"/> for a detailed explanation:
        /// <list type="bullet">
        ///     <item><term><c>-1</c></term><description>Update **every single time** the FSM's group is processed.</description></item>
        ///     <item><term><c>0</c></term><description>Update **only when you manually tell it to** (no automatic updates).</description></item>
        ///     <item><term><c>&gt;0</c></term><description>Update **every Nth time** the FSM's group is processed (e.g., 5 means every 5th update).</description></item>
        /// </list>
        /// </remarks>
        /// <param name="rate">The desired update rate.</param>
        /// <returns>The builder itself, for chaining.</returns>
        public FSMBuilder WithProcessRate(int rate)
        {
            _processRate = rate;
            return this;
        }

        /// <summary>
        /// Sets a new **name** for this FSM blueprint.
        /// </summary>
        /// <remarks>
        /// This is useful if you used the constructor that loads an existing FSM (<see cref="FSMBuilder(FSM)"/>)
        /// and want to give it a proper new name, or simply rename an FSM you're building.
        /// </remarks>
        /// <param name="name">The new unique name for the FSM blueprint.</param>
        /// <returns>The builder itself, for chaining.</returns>
        /// <exception cref="ArgumentException">Happens if the <paramref name="name"/> is empty.</exception>
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
        /// Sets the **starting state** for any new FSM instance created from this blueprint.
        /// </summary>
        /// <remarks>
        /// When an FSM instance is first created, it will immediately enter this state.
        /// Make sure you've already defined this state using the <see cref="State(string, Action{IStateContext}, Action{IStateContext}, Action{IStateContext})"/> method.
        /// </remarks>
        /// <param name="stateName">The name of the state that new FSM instances should start in.</param>
        /// <returns>The builder itself, for chaining.</returns>
        /// <exception cref="ArgumentException">Happens if the <paramref name="stateName"/> is empty.</exception>
        public FSMBuilder WithInitialState(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                throw new ArgumentException("Initial state name cannot be null or empty.", nameof(stateName));
            }
            _initialState = stateName;
            return this;
        }

        /// <summary>
        /// Defines a **rule (transition)** for your FSM to move from one specific state to another.
        /// </summary>
        /// <remarks>
        /// This transition rule will only be considered when the FSM is currently in the `from` state.
        /// If you define multiple transitions between the exact same `from` and `to` states,
        /// the last one you define will replace any earlier ones.
        /// </remarks>
        /// <param name="from">The name of the state where this transition starts.</param>
        /// <param name="to">The name of the state where this transition leads to.</param>
        /// <param name="condition">A function (rule) that must return `true` for this transition to happen.
        /// This function receives the <see cref="IStateContext"/> (your FSM's data bag).</param>
        /// <returns>The builder itself, for chaining.</returns>
        /// <exception cref="ArgumentException">Happens if `from` or `to` state names are empty, or if the `condition` is `null`.</exception>
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


        public FSMBuilder AnyTransition(string to, Func<IStateContext, bool> condition)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("'To' state name cannot be null or empty.", nameof(to));
            }
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition), "Transition condition cannot be null.");
            }
            _anyTransitions.Add(new FSMTransition("*", to, condition));

            return this;
        }


        /// <summary>
        /// Finalizes your FSM blueprint and **registers it** with the FSM system. ✅
        /// </summary>
        /// <remarks>
        /// After calling this, your FSM blueprint is complete and saved. You can then start
        /// creating live instances of it using <see cref="FSM_API.Create.CreateInstance(string, IStateContext, string)"/>.
        /// <para>
        /// This method checks for common mistakes like:
        /// <list type="bullet">
        ///     <item>If the FSM `Name` is empty.</item>
        ///     <item>If no states have been added.</item>
        ///     <item>If the `InitialState` you set actually exists.</item>
        /// </list>
        /// If you're building on an existing FSM blueprint (using <see cref="FSMBuilder(FSM)"/>),
        /// this method will update the existing definition or create a new one if the name changed.
        /// </para>
        /// </remarks>
        /// <returns>The completed <see cref="FSM"/> blueprint.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the FSM has no name, no states, or the initial state doesn't exist.</exception>
        public void BuildDefinition()
        {
            // --- Validation before building ---
            //if (_states.Count == 0)
            //{
            //    throw new InvalidOperationException($"FSM '{_fsmName}' cannot be built: No states have been defined. Use .State() to add states.");
            //}

            string finalInitialState = string.Empty;
            if (string.IsNullOrWhiteSpace(_initialState))
            {
                // If no initial state explicitly set, use the first added state (order matters here!)
                if (_states.Count > 0)
                    finalInitialState = _states[0].Name;
                else
                    finalInitialState = "Undefined";
            }
            else
            {
                //// Validate that the specified initial state actually exists
                //if (!_states.Any(s => s.Name == _initialState))
                //{
                //    throw new ArgumentException($"Initial state '{_initialState}' specified for FSM '{_fsmName}' does not exist. Ensure you add it with .State() before building.", nameof(_initialState));
                //}
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
            foreach (var anyT in _anyTransitions)
            {
                machine.AddAnyStateTransition(anyT);
            }
            foreach (var t in _transitions)
            {
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
        /// Sets the **"processing group"** name for this FSM blueprint.
        /// </summary>
        /// <remarks>
        /// This helps organize your FSMs into categories. You can then tell the FSM system
        /// to update all FSMs in a specific group at once.
        /// See <see cref="FSM.ProcessingGroup"/> for more context.
        /// </remarks>
        /// <param name="groupName">The name of the processing group (e.g., "GameLoop", "UIA", "PhysicsUpdate").</param>
        /// <returns>The builder itself, for chaining.</returns>
        /// <exception cref="ArgumentException">Happens if the `groupName` is empty.</exception>
        public FSMBuilder WithProcessingGroup(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                throw new ArgumentException("Processing group name cannot be null or empty.", nameof(groupName));
            }
            _processGroup = groupName;
            return this;
        }
    }
}