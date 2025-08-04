namespace FSM_API_WindowsEditor
{
    partial class FSM_StateControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            StateNameTextBox = new TextBox();
            OnEnterTextBox = new TextBox();
            OnUpdateTextBox = new TextBox();
            OnExitTextBox = new TextBox();
            SuspendLayout();
            // 
            // StateNameTextBox
            // 
            StateNameTextBox.Location = new Point(3, 3);
            StateNameTextBox.Name = "StateNameTextBox";
            StateNameTextBox.PlaceholderText = "State Name";
            StateNameTextBox.Size = new Size(169, 27);
            StateNameTextBox.TabIndex = 0;
            // 
            // OnEnterTextBox
            // 
            OnEnterTextBox.Location = new Point(178, 3);
            OnEnterTextBox.Name = "OnEnterTextBox";
            OnEnterTextBox.PlaceholderText = "OnEnter:";
            OnEnterTextBox.Size = new Size(125, 27);
            OnEnterTextBox.TabIndex = 1;
            // 
            // OnUpdateTextBox
            // 
            OnUpdateTextBox.Location = new Point(309, 3);
            OnUpdateTextBox.Name = "OnUpdateTextBox";
            OnUpdateTextBox.PlaceholderText = "OnUpdate:";
            OnUpdateTextBox.Size = new Size(125, 27);
            OnUpdateTextBox.TabIndex = 2;
            // 
            // OnExitTextBox
            // 
            OnExitTextBox.Location = new Point(440, 3);
            OnExitTextBox.Name = "OnExitTextBox";
            OnExitTextBox.PlaceholderText = "OnExit:";
            OnExitTextBox.Size = new Size(125, 27);
            OnExitTextBox.TabIndex = 3;
            // 
            // FSM_StateControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(OnExitTextBox);
            Controls.Add(OnUpdateTextBox);
            Controls.Add(OnEnterTextBox);
            Controls.Add(StateNameTextBox);
            Name = "FSM_StateControl";
            Size = new Size(568, 35);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox StateNameTextBox;
        private TextBox OnEnterTextBox;
        private TextBox OnUpdateTextBox;
        private TextBox OnExitTextBox;
    }
}
