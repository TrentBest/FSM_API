using System;
using NUnit.Framework;
using TheSingularityWorkshop.FSM_API;
using TheSingularityWorkshop.FSM_API.IntegerBacked;
using StringFsm = TheSingularityWorkshop.FSM_API.FSM;
using StringFsmState = TheSingularityWorkshop.FSM_API.FSMState;

namespace TheSingularityWorkshop.FSM_API.Tests.IntegerBacked
{
    [TestFixture]
    public sealed class FSMCompiler_Tests
    {
        private sealed class Context : IStateContext
        {
            public string Name { get; set; } = "FSMCompiler_Tests";
            public bool IsValid { get; set; } = true;
        }

        [Test]
        public void Compile_AssignsDenseDeterministicIDs_AndPreservesInitialState()
        {
            var source = new StringFsm { Name = "CompilerFixture" };
            source.AddState(new StringFsmState("Zulu", null, null, null));
            source.AddState(new StringFsmState("Alpha", null, null, null));

            var first = FSMCompiler.Compile(source, 42, 7);
            var second = FSMCompiler.Compile(source, 42, 7);

            Assert.That(first.StateNameToID["Alpha"], Is.EqualTo(0));
            Assert.That(first.StateNameToID["Zulu"], Is.EqualTo(1));
            Assert.That(first.StateNameToID["Alpha"], Is.EqualTo(second.StateNameToID["Alpha"]));
            Assert.That(first.StateIDToName[1], Is.EqualTo("Zulu"));
            Assert.That(first.Definition.InitialStateID, Is.EqualTo(1));
            Assert.That(first.Definition.ProcessRate, Is.EqualTo(0));
            Assert.That(first.Definition.ProcessingGroupID, Is.EqualTo(7));
        }

        [Test]
        public void Compile_PreservesStateActionsAndTransitionConditions()
        {
            var entered = 0;
            var updated = 0;
            var exited = 0;
            var source = new StringFsm { Name = "ActionFixture" };
            source.AddState(new StringFsmState("Start", _ => entered++, _ => updated++, _ => exited++));
            source.AddState(new StringFsmState("End", null, null, null));
            source.AddTransition("Start", "End", _ => true);

            var compiled = FSMCompiler.Compile(source, 43);
            var handle = new FSMHandleInt(compiled.Definition, new Context());
            handle.Initialize();

            Assert.That(entered, Is.EqualTo(1));
            handle.Update();

            Assert.That(updated, Is.EqualTo(1));
            Assert.That(exited, Is.EqualTo(1));
            Assert.That(handle.CurrentStateID, Is.EqualTo(compiled.StateNameToID["End"]));
        }

        [Test]
        public void Compile_PreservesAnyStateTransitions()
        {
            var source = new StringFsm { Name = "AnyStateFixture" };
            source.AddState(new StringFsmState("Start", null, null, null));
            source.AddState(new StringFsmState("Finish", null, null, null));
            source.AddAnyStateTransition("Finish", _ => true);

            var compiled = FSMCompiler.Compile(source, 44);
            var next = compiled.Definition.Step(
                compiled.StateNameToID["Start"],
                new Context());

            Assert.That(next, Is.EqualTo(compiled.StateNameToID["Finish"]));
        }

        [Test]
        public void Compile_RejectsTransitionsToUndefinedStates()
        {
            var source = new StringFsm { Name = "InvalidTransitionFixture" };
            source.AddState(new StringFsmState("Present", null, null, null));
            source.AddTransition("Present", "Missing", _ => true);

            Assert.Throws<InvalidOperationException>(() => FSMCompiler.Compile(source, 46));
        }


        [Test]
        public void Compile_RejectsTransitionsFromUndefinedStates()
        {
            var source = new StringFsm { Name = "InvalidTransitionSourceFixture" };
            source.AddState(new StringFsmState("Present", null, null, null));
            source.AddTransition("Missing", "Present", _ => true);

            Assert.Throws<InvalidOperationException>(() => FSMCompiler.Compile(source, 47));
        }

        [Test]
        public void Compile_ReturnsReadOnlyStateMappings()
        {
            var source = new StringFsm { Name = "ReadOnlyMappingFixture" };
            source.AddState(new StringFsmState("Only", null, null, null));
            var compiled = FSMCompiler.Compile(source, 48);

            Assert.That(compiled.StateNameToID, Is.InstanceOf<System.Collections.ObjectModel.ReadOnlyDictionary<string, int>>());
            Assert.That(compiled.StateIDToName, Is.InstanceOf<System.Collections.ObjectModel.ReadOnlyDictionary<int, string>>());
            Assert.Throws<NotSupportedException>(() =>
                ((System.Collections.Generic.IDictionary<string, int>)compiled.StateNameToID).Add("Injected", 1));
            Assert.Throws<NotSupportedException>(() =>
                ((System.Collections.Generic.IDictionary<int, string>)compiled.StateIDToName).Add(1, "Injected"));
        }

        [Test]
        public void Compile_RejectsUndefinedInitialState()
        {
            var source = new StringFsm { Name = "InvalidFixture" };
            source.AddState(new StringFsmState("Present", null, null, null));
            source.InitialState = "Missing";

            Assert.Throws<InvalidOperationException>(() => FSMCompiler.Compile(source, 45));
        }

        [Test]
        public void Decompile_RestoresNamedDefinitionUsingExplicitMapping()
        {
            var updated = 0;
            var source = FSM_API.Create.CreateFiniteStateMachine("Authored", 3, "Authoring")
                .State("Start", onUpdate: _ => updated++)
                .State("Finish")
                .WithInitialState("Start")
                .Transition("Start", "Finish", _ => true)
                .BuildDefinition();

            var compiled = FSMCompiler.Compile(source, 49, 7);
            var restored = FSMCompiler.Decompile(
                compiled.Definition,
                "Restored",
                "Execution",
                compiled.StateIDToName);

            Assert.That(restored.Name, Is.EqualTo("Restored"));
            Assert.That(restored.ProcessingGroup, Is.EqualTo("Execution"));
            Assert.That(restored.ProcessRate, Is.EqualTo(3));
            Assert.That(restored.InitialState, Is.EqualTo("Start"));
            Assert.That(restored.GetAllStates().Count, Is.EqualTo(2));
            Assert.That(restored.GetAllTransitions().Count, Is.EqualTo(1));

            var restoredStart = new FSMHandle(restored, new Context());
            restoredStart.Update("Execution");
            Assert.That(updated, Is.EqualTo(1));
        }

        [Test]
        public void Decompile_RejectsMissingStateNameMapping()
        {
            var definition = new FSMBuilderInt(50).State(0).BuildDefinition();

            Assert.Throws<InvalidOperationException>(() => FSMCompiler.Decompile(
                definition,
                "MissingMap",
                "Execution",
                new System.Collections.Generic.Dictionary<int, string>()));
        }
    }
}
