using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSingularityWorkshop.FSM_API
{
    public static partial class FSM_API
    {
        /// <summary>
        /// Provides a centralized entry point for defining and creating instances of
        /// Finite State Machines (FSMs) within the API.
        /// </summary>
        /// <remarks>
        /// This static class serves as a factory for <see cref="FSMBuilder"/> objects
        /// (used to construct FSM definitions) and for instantiating live FSMs from
        /// those definitions. All operations related to the initial setup and creation
        /// of FSMs go through the methods provided here.
        /// </remarks>
        public static class Create
        {
            /// <summary>
            /// Initiates the definition process for a new Finite State Machine (FSM),
            /// or provides an <see cref="FSMBuilder"/> to modify an existing FSM's definition.
            /// </summary>
            /// <remarks>
            /// Use this method to either begin designing a brand new FSM from scratch
            /// or to retrieve a builder for an FSM that has already been defined.
            /// The <see cref="FSMBuilder"/> returned is your primary tool for adding states,
            /// transitions, and configuring event handlers.
            /// <para>
            /// If an FSM with the specified <paramref name="fsmName"/> and <paramref name="processingGroup"/>
            /// already exists, the builder will be pre-populated with its current definition,
            /// allowing for seamless modification.
            /// </para>
            /// </remarks>
            /// <param name="fsmName">
            /// A unique, descriptive name for this FSM definition. This name is used to
            /// reference the FSM definition when creating instances or modifying it later.
            /// Defaults to "UnNamedFSM".
            /// </param>
            /// <param name="processRate">
            /// The default processing rate for instances of this FSM definition.
            /// <list type="bullet">
            /// <item><term>0 (default)</term><description>Event-driven: FSM instances will only process when explicitly triggered by an event.</description></item>
            /// <item><term>&gt; 0</term><description>Tick-based: FSM instances will process at a fixed interval (e.g., in milliseconds), in addition to event triggers.</description></item>
            /// <item><term>-1</term><description>Manual: FSM instances require explicit calls to their process method (e.g., FSMHandle.Process()) and are not automatically ticked or purely event-driven.</description></item>
            /// </list>
            /// Values less than -1 will be silently coerced to 0 (event-driven).
            /// </param>
            /// <param name="processingGroup">
            /// The name of the processing group this FSM definition belongs to.
            /// FSMs within the same processing group can be managed and processed collectively.
            /// Defaults to "Update".
            /// </param>
            /// <returns>
            /// An <see cref="FSMBuilder"/> object, which acts as a fluent API for
            /// defining the FSM's states, transitions, and actions.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null, empty, or consists only of white-space characters.
            /// </exception>
            public static FSMBuilder CreateFiniteStateMachine(
            string fsmName = "UnNamedFSM",
                int processRate = 0, string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }
                if (processRate < -1)
                {
                    Error.InvokeInternalApiError($"Invalid processRate '{processRate}' for FSM '{fsmName}'. Setting to 0 (event-driven).", null);
                    processRate = 0;
                }

                var buckets = FSM_API.Internal.GetOrCreateBucketProcessingGroup(processingGroup);
                if (buckets.TryGetValue(fsmName, out var existingBucket))
                {
                   
                    return new FSMBuilder(existingBucket.Definition != null ? existingBucket.Definition : FSM_API.Internal.GetDefaultFSM());
                }

                return new FSMBuilder(fsmName, processRate, processingGroup);
            }

            /// <summary>
            /// Creates a live instance of a registered FSM definition, binding it to a specific context object.
            /// This allows the FSM to manage the state and behaviors of that context dynamically at runtime.
            /// </summary>
            /// <remarks>
            /// After defining an FSM structure using <see cref="CreateFiniteStateMachine(string,int,string)"/>,
            /// use this method to create active, runnable instances. Each instance operates independently,
            /// maintaining its own current state and executing actions based on its bound context object.
            /// <para>
            /// The <paramref name="ctx"/> object serves as the 'owner' of the FSM's state. When the FSM
            /// transitions, its associated actions and guards typically operate on or retrieve data from this
            /// context object.
            /// </para>
            /// </remarks>
            /// <param name="fsmName">
            /// The unique name of the FSM definition to instantiate. This must correspond to an FSM
            /// previously defined via <see cref="CreateFiniteStateMachine(string,int,string)"/>.
            /// </param>
            /// <param name="ctx">
            /// The object that will serve as the FSM's context. This object **must** implement
            /// the <see cref="IStateContext"/> interface, which provides a common interface for FSM
            /// operations regardless of the specific context type.
            /// </param>
            /// <param name="processingGroup">
            /// The name of the processing group where the FSM definition is registered.
            /// This must match the group specified during the FSM's definition.
            /// Defaults to "Update".
            /// </param>
            /// <returns>
            /// An <see cref="FSMHandle"/> that provides control over the newly created FSM instance,
            /// allowing you to trigger events, query its current state, and manage its lifecycle.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null, empty, or consists only of white-space characters.
            /// </exception>
            /// <exception cref="ArgumentNullException">
            /// Thrown if the provided <paramref name="ctx"/> (context object) is <c>null</c>.
            /// </exception>
            /// <exception cref="KeyNotFoundException">
            /// Thrown if the FSM definition specified by <paramref name="fsmName"/> and <paramref name="processingGroup"/>
            /// cannot be found. Ensure the FSM has been defined prior to attempting instantiation.
            /// </exception>
            public static FSMHandle CreateInstance(
                string fsmName,
                IStateContext ctx, string processingGroup = "Update")
            {
                if (string.IsNullOrWhiteSpace(fsmName))
                {
                    throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
                }
                if (ctx == null)
                {
                    throw new ArgumentNullException(nameof(ctx), "State context (ctx) cannot be null when creating an FSM instance.");
                }
                if (string.IsNullOrWhiteSpace(processingGroup))
                {
                    throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
                }

                if (!Internal.GetBuckets().TryGetValue(processingGroup, out var categoryBuckets) || !categoryBuckets.TryGetValue(fsmName, out var bucket))
                {
                    throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in processing group '{processingGroup}'. Cannot create instance.");
                }
                if (bucket.Definition == null)
                {
                    bucket.Definition = Internal.GetDefaultFSM();
                }
                var handle = new FSMHandle(bucket.Definition, ctx);
                bucket.Instances.Add(handle);
                return handle;
            }

            /// <summary>
            /// Ensures an FSM processing group exists within the API's internal management system.
            /// FSM instances registered under this group will be processed when the corresponding
            /// <c>Tick</c> method (e.g., <see cref="FSM_API.Interaction.Update(string)"/>)
            /// is called for that specific group.
            /// </summary>
            /// <remarks>
            /// Calling this method is generally optional, as processing groups are automatically created
            /// when an FSM definition or instance is first created within that group. However,
            /// you might use this method to pre-initialize a group or to explicitly confirm its existence.
            /// </remarks>
            /// <param name="processingGroup">
            /// The unique name for the processing group to create or ensure exists.
            /// Group names are case-sensitive.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown if <paramref name="processingGroup"/> is null, empty, or consists only of white-space characters.
            /// </exception>
            public static void CreateProcessingGroup(string processingGroup)
            {

                Internal.GetOrCreateBucketProcessingGroup(processingGroup);
            }
        }
    }
}