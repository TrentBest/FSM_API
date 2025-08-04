namespace FSM_API_WindowsEditor
{
    partial class TransitionUserControl
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
            FromComboBox = new ComboBox();
            ToComboBox = new ComboBox();
            ConditionTextBox = new TextBox();
            SuspendLayout();
            // 
            // FromComboBox
            // 
            FromComboBox.FormattingEnabled = true;
            FromComboBox.Location = new Point(3, 3);
            FromComboBox.Name = "FromComboBox";
            FromComboBox.Size = new Size(151, 28);
            FromComboBox.TabIndex = 0;
            FromComboBox.Text = "From";
            // 
            // ToComboBox
            // 
            ToComboBox.FormattingEnabled = true;
            ToComboBox.Location = new Point(160, 3);
            ToComboBox.Name = "ToComboBox";
            ToComboBox.Size = new Size(151, 28);
            ToComboBox.TabIndex = 1;
            ToComboBox.Text = "To";
            // 
            // ConditionTextBox
            // 
            ConditionTextBox.Location = new Point(317, 4);
            ConditionTextBox.Name = "ConditionTextBox";
            ConditionTextBox.PlaceholderText = "Name of Condition Method";
            ConditionTextBox.Size = new Size(243, 27);
            ConditionTextBox.TabIndex = 2;
            // 
            // TransitionUserControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(ConditionTextBox);
            Controls.Add(ToComboBox);
            Controls.Add(FromComboBox);
            Name = "TransitionUserControl";
            Size = new Size(563, 38);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox FromComboBox;
        private ComboBox ToComboBox;
        private TextBox ConditionTextBox;
    }
}
