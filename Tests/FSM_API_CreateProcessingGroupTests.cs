//using Xunit;
using System; // Needed for ArgumentException
using TheSingularityWorkshop.FSM_API; // Your actual namespace

namespace TheSingularityWorkshop.FSM_API.Tests
{
    public class FSM_API_CreateProcessingGroupTests
    {
        //// Test Case 1: Valid group name should create the group without throwing an exception.
        //[Fact]
        //public void CreateProcessingGroup_WithValidName_ShouldCreateGroup()
        //{
        //    // Arrange
        //    string groupName = "TestGroup1";

        //    // Act & Assert
        //    // We use Assert.DoesNotThrow to ensure the method call completes without an exception.
        //    // For a static method that creates something, a successful call is often the primary assertion
        //    // unless there's a public way to verify the group's existence (e.g., FSM_API.GetProcessingGroup or a collection).
        //    Assert.DoesNotThrow(() => FSM_API.CreateProcessingGroup(groupName));

        //    // Optional: If FSM_API has a method like 'HasProcessingGroup(string name)' or 'GetProcessingGroup(string name)'
        //    // you would add an assertion here to verify its existence.
        //    // Example: Assert.True(FSM_API.HasProcessingGroup(groupName));
        //    // Or: Assert.NotNull(FSM_API.GetProcessingGroup(groupName));
        //}

        //// Test Case 2: Null group name should throw an ArgumentException.
        //[Fact]
        //public void CreateProcessingGroup_WithNullName_ShouldThrowArgumentException()
        //{
        //    // Arrange
        //    string groupName = null;

        //    // Act & Assert
        //    var exception = Assert.Throws<ArgumentException>(() => FSM_API.CreateProcessingGroup(groupName));

        //    // Optional: Assert on the exception message if you want to be very specific.
        //    // Assert.Contains("processingGroup cannot be null or empty", exception.Message);
        //}

        //// Test Case 3: Empty group name should throw an ArgumentException.
        //[Fact]
        //public void CreateProcessingGroup_WithEmptyName_ShouldThrowArgumentException()
        //{
        //    // Arrange
        //    string groupName = string.Empty;

        //    // Act & Assert
        //    var exception = Assert.Throws<ArgumentException>(() => FSM_API.CreateProcessingGroup(groupName));

        //    // Optional: Assert on the exception message if you want to be very specific.
        //    // Assert.Contains("processingGroup cannot be null or empty", exception.Message);
        //}

        //// Test Case 4: Duplicate group name (if allowed by your API)
        //// If your API allows creating the same group multiple times (e.g., it's idempotent)
        //// then this test would assert that no exception is thrown.
        //// If your API should throw an exception for duplicates, then assert that specific exception.
        //[Fact]
        //public void CreateProcessingGroup_WithDuplicateName_ShouldNotThrowException()
        //{
        //    // Arrange
        //    string groupName = "DuplicateGroup";
        //    FSM_API.CreateProcessingGroup(groupName); // Create it once

        //    // Act & Assert
        //    // Assert.DoesNotThrow is used here assuming your API is idempotent for group creation.
        //    Assert.DoesNotThrow(() => FSM_API.CreateProcessingGroup(groupName));
        //}
    }
}