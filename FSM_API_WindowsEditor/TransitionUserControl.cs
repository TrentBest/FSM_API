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
    public partial class TransitionUserControl : UserControl
    {
        public TransitionUserControl()
        {
            InitializeComponent();
            // Link text box and combobox events to update properties
            FromComboBox.SelectedIndexChanged += (s, e) => FromState = FromComboBox.SelectedItem as FSM_EditorState;
            ToComboBox.SelectedIndexChanged += (s, e) => ToState = ToComboBox.SelectedItem as FSM_EditorState;
            ConditionTextBox.TextChanged += (s, e) => Condition = ConditionTextBox.Text;
        }

        // Property to hold the current transition data
        private FSM_EditorTransition _currentTransition;
        public FSM_EditorTransition CurrentTransition
        {
            get => _currentTransition;
            set
            {
                _currentTransition = value;
                if (_currentTransition != null)
                {
                    // Update UI from model
                    FromComboBox.SelectedItem = _currentTransition.FromState;
                    ToComboBox.SelectedItem = _currentTransition.ToState;
                    ConditionTextBox.Text = _currentTransition.ConditionMethodName;
                }
            }
        }

        // Expose individual properties for convenience, though CurrentTransition is primary
        public FSM_EditorState FromState
        {
            get => FromComboBox.SelectedItem as FSM_EditorState;
            set => FromComboBox.SelectedItem = value;
        }
        
        public FSM_EditorState ToState
        {
            get => ToComboBox.SelectedItem as FSM_EditorState;
            set => ToComboBox.SelectedItem = value;
        }

        public string Condition
        {
            get => ConditionTextBox.Text;
            set => ConditionTextBox.Text = value;
        }

        public ComboBox GetFromComboBox()
        {
            return FromComboBox;
        }

        public ComboBox GetToComboBox()
        {
            return ToComboBox;
        }

        public TextBox GetConditionTextBox()
        {
            return ConditionTextBox;
        }

        // Method to populate the state dropdowns
        public void SetAvailableStates(IEnumerable<FSM_EditorState> states)
        {
            FromComboBox.DataSource = new List<FSM_EditorState>(states); // Use a copy to avoid modification issues
            ToComboBox.DataSource = new List<FSM_EditorState>(states);   // from the same source collection
        }
    }
}
