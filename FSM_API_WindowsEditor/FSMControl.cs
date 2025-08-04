using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TheSingularityWorkshop.FSM_API;

namespace FSM_API_WindowsEditor
{
    public partial class FSMControl : UserControl
    {
        private FSM_EditorFiniteStateMachine _fsmDefinition;

        public FSMControl()
        {
            InitializeComponent();
            AddStateButton.Click += AddStateButton_Click;
            RemoveStateButton.Click += RemoveStateButton_Click;
            AddTransitionButton.Click += AddTransitionButton_Click;
            RemoveTransitionButton.Click += RemoveTransitionButton_Click;

            FSM_NameTextBox.TextChanged += (s, e) => _fsmDefinition.FSM_Name = FSM_NameTextBox.Text;
            ProcessingGroupomboBox.SelectedIndexChanged += (s, e) => _fsmDefinition.ProcessingGroup = ProcessingGroupomboBox.SelectedItem as FSM_EditorProcessingGroup;
            

            // Initialize sample process groups for demonstration
            InitializeProcessGroups();
        }

        private void InitializeProcessGroups()
        {
            // This would likely come from a shared manager in a real app
            var processGroups = new List<FSM_EditorProcessingGroup>
            {
                new FSM_EditorProcessingGroup { ProcessingGroup = "UI", ProcessingGroupColor = System.Drawing.Color.LightBlue },
                new FSM_EditorProcessingGroup { ProcessingGroup = "AI", ProcessingGroupColor = System.Drawing.Color.LightGreen },
                new FSM_EditorProcessingGroup { ProcessingGroup = "Physics", ProcessingGroupColor = System.Drawing.Color.LightSalmon }
            };
            ProcessingGroupomboBox.DataSource = processGroups;
            ProcessingGroupomboBox.DisplayMember = "Name"; // Display the Name property
            ProcessingGroupomboBox.ValueMember = "Name";   // Use Name as the internal value
        }


        public FSM_EditorFiniteStateMachine CurrentFSM
        {
            get => _fsmDefinition;
            set
            {
                _fsmDefinition = value;
                if (_fsmDefinition != null)
                {
                    // Update main FSM properties
                    FSM_NameTextBox.Text = _fsmDefinition.FSM_Name;
                    ProcessingGroupomboBox.SelectedItem = _fsmDefinition.ProcessingGroup;
                    // Set ProcessingRateComboBox based on _fsmDefinition.ProcessingRate

                    // Clear existing controls
                    StateFlowLayoutPanel.Controls.Clear();
                    TransitionFlowLayoutPanel.Controls.Clear();

                    // Populate states
                    foreach (var state in _fsmDefinition.States)
                    {
                        AddStateControl(state);
                    }
                    // Update transitions after all states are added so combo boxes can find them
                    foreach (var transition in _fsmDefinition.Transitions)
                    {
                        AddTransitionControl(transition);
                    }

                    UpdateTransitionComboBoxes(); // Ensure all transition controls have updated state lists
                }
            }
        }

        private void AddStateButton_Click(object sender, EventArgs e)
        {
            var newState = new FSM_EditorState { StateName = $"New State {(_fsmDefinition.States.Count + 1)}" };
            _fsmDefinition.States.Add(newState);
            AddStateControl(newState);
            UpdateTransitionComboBoxes(); // States list changed, update transition dropdowns
        }

        private void AddStateControl(FSM_EditorState state)
        {
            var stateControl = new FSM_StateControl();
            stateControl.StateName = state.StateName;
            stateControl.OnEnterBehavior = state.OnEnterMethodName;
            stateControl.OnUpdateBehavior = state.OnUpdateMethodName;
            stateControl.OnExitBehavior = state.OnExitMethodName;

            // When the state control's properties change, update the model
            stateControl.GetStateNameTextBox().TextChanged += (s, e) => state.StateName = stateControl.StateName;
            stateControl.GetOnEnterNameTextBox().TextChanged += (s, e) => state.OnEnterMethodName = stateControl.OnEnterBehavior;
            stateControl.GetOnUpdateTextBox().TextChanged += (s, e) => state.OnUpdateMethodName = stateControl.OnUpdateBehavior;
            stateControl.GetOnExitTextBox().TextChanged += (s, e) => state.OnExitMethodName = stateControl.OnExitBehavior;

            StateFlowLayoutPanel.Controls.Add(stateControl);
        }

        private void RemoveStateButton_Click(object sender, EventArgs e)
        {
            if (StateFlowLayoutPanel.Controls.Count > 0)
            {
                var selectedStateControl = StateFlowLayoutPanel.Controls.OfType<FSM_StateControl>()
                                            .FirstOrDefault(c => c.ContainsFocus || IsChildControlFocused(c)); // Or have a selection mechanism

                if (selectedStateControl != null)
                {
                    // Find the corresponding FSMState object and remove it
                    var stateToRemove = _fsmDefinition.States.FirstOrDefault(s => s.StateName == selectedStateControl.StateName);
                    if (stateToRemove != null)
                    {
                        // Remove any transitions involving this state
                        _fsmDefinition.Transitions.RemoveAll(t => t.FromState == stateToRemove.StateName || t.ToState == stateToRemove.StateName);
                        _fsmDefinition.States.Remove(stateToRemove);
                        StateFlowLayoutPanel.Controls.Remove(selectedStateControl);
                        selectedStateControl.Dispose(); // Clean up control

                        // Re-populate transitions to reflect changes
                        TransitionFlowLayoutPanel.Controls.Clear();
                        foreach (var transition in _fsmDefinition.Transitions)
                        {
                            AddTransitionControl(transition);
                        }
                        UpdateTransitionComboBoxes(); // States list changed, update transition dropdowns
                    }
                }
            }
        }

        // Helper to check if any child control of a UserControl has focus
        private bool IsChildControlFocused(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child.Focused) return true;
                if (child.HasChildren && IsChildControlFocused(child)) return true;
            }
            return false;
        }

        private void AddTransitionButton_Click(object sender, EventArgs e)
        {
            var newTransition = new FSM_EditorTransition
            {
                FromState = _fsmDefinition.States.FirstOrDefault().ToString(), // Default to first state
                ToState = _fsmDefinition.States.FirstOrDefault().ToString(),   // Default to first state
                ConditionMethodName = "NewCondition"
            };
            _fsmDefinition.Transitions.Add(newTransition);
            AddTransitionControl(newTransition);
        }

        private void AddTransitionControl(FSM_EditorTransition transition)
        {
            var transitionControl = new TransitionUserControl();
            transitionControl.CurrentTransition = transition;
            transitionControl.SetAvailableStates(_fsmDefinition.States);

            // When the transition control's properties change, update the model
            transitionControl.GetFromComboBox().SelectedIndexChanged += (s, e) => transition.FromState = transitionControl.FromState.StateName;
            transitionControl.GetToComboBox().SelectedIndexChanged += (s, e) => transition.ToState = transitionControl.ToState.StateName;
            transitionControl.GetConditionTextBox().TextChanged += (s, e) => transition.ConditionMethodName = transitionControl.Condition;

            TransitionFlowLayoutPanel.Controls.Add(transitionControl);
        }

        private void RemoveTransitionButton_Click(object sender, EventArgs e)
        {
            if (TransitionFlowLayoutPanel.Controls.Count > 0)
            {
                // This requires a selection mechanism, for now, removes last one
                var selectedTransitionControl = TransitionFlowLayoutPanel.Controls.OfType<TransitionUserControl>()
                                                .FirstOrDefault(c => c.ContainsFocus || IsChildControlFocused(c)); // Or your selection

                if (selectedTransitionControl != null)
                {
                    // Find the corresponding FSMTransition object and remove it
                    var transitionToRemove = selectedTransitionControl.CurrentTransition;
                    if (transitionToRemove != null)
                    {
                        _fsmDefinition.Transitions.Remove(transitionToRemove);
                        TransitionFlowLayoutPanel.Controls.Remove(selectedTransitionControl);
                        selectedTransitionControl.Dispose();
                    }
                }
            }
        }

        // Call this whenever the list of states changes to refresh dropdowns in transitions
        private void UpdateTransitionComboBoxes()
        {
            foreach (TransitionUserControl transitionControl in TransitionFlowLayoutPanel.Controls)
            {
                transitionControl.SetAvailableStates(_fsmDefinition.States);
            }
        }
    }
}
