 ```csharp
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
 ```