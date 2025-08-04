using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSM_API_WindowsEditor
{
    public partial class FSM_StateControl : UserControl
    {
        public FSM_StateControl()
        {
            InitializeComponent();
            // Link text box events to update properties
            StateNameTextBox.TextChanged += (s, e) => StateName = StateNameTextBox.Text;
            OnEnterTextBox.TextChanged += (s, e) => OnEnterBehavior = OnEnterTextBox.Text;
            OnUpdateTextBox.TextChanged += (s, e) => OnUpdateBehavior = OnUpdateTextBox.Text;
            OnExitTextBox.TextChanged += (s, e) => OnExitBehavior = OnExitTextBox.Text;
        }

        public TextBox GetStateNameTextBox()
        {
            return StateNameTextBox;
        }

        public TextBox GetOnEnterNameTextBox()
        {
            return OnEnterTextBox;
        }

        public TextBox GetOnUpdateTextBox()
        {
            return OnUpdateTextBox;
        }

        public TextBox GetOnExitTextBox()
        {
            return OnExitTextBox;
        }

        public string StateName
        {
            get => StateNameTextBox.Text;
            set => StateNameTextBox.Text = value;
        }

        public string OnEnterBehavior
        {
            get => OnEnterTextBox.Text;
            set => OnEnterTextBox.Text = value;
        }

        public string OnUpdateBehavior
        {
            get => OnUpdateTextBox.Text;
            set => OnUpdateTextBox.Text = value;
        }

        public string OnExitBehavior
        {
            get => OnExitTextBox.Text;
            set => OnExitTextBox.Text = value;
        }
    }
}
