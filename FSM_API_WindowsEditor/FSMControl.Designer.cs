namespace FSM_API_WindowsEditor
{
    partial class FSMControl
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
            FSM_NameTextBox = new TextBox();
            splitContainer1 = new SplitContainer();
            ProcessingGroupomboBox = new ComboBox();
            ProcessingRateComboBox = new ComboBox();
            StateSplitContainer = new SplitContainer();
            flowLayoutPanel1 = new FlowLayoutPanel();
            AddStateButton = new Button();
            RemoveStateButton = new Button();
            StateFlowLayoutPanel = new FlowLayoutPanel();
            TransitionsSplitContainer = new SplitContainer();
            TransitionsFlowLayoutPanel = new FlowLayoutPanel();
            AddTransitionButton = new Button();
            RemoveTransitionButton = new Button();
            TransitionFlowLayoutPanel = new FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)StateSplitContainer).BeginInit();
            StateSplitContainer.Panel1.SuspendLayout();
            StateSplitContainer.Panel2.SuspendLayout();
            StateSplitContainer.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TransitionsSplitContainer).BeginInit();
            TransitionsSplitContainer.Panel1.SuspendLayout();
            TransitionsSplitContainer.Panel2.SuspendLayout();
            TransitionsSplitContainer.SuspendLayout();
            TransitionsFlowLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // FSM_NameTextBox
            // 
            FSM_NameTextBox.Location = new Point(3, 3);
            FSM_NameTextBox.Name = "FSM_NameTextBox";
            FSM_NameTextBox.PlaceholderText = "FSM Name";
            FSM_NameTextBox.Size = new Size(125, 27);
            FSM_NameTextBox.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.Location = new Point(3, 36);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(StateSplitContainer);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(TransitionsSplitContainer);
            splitContainer1.Size = new Size(679, 400);
            splitContainer1.SplitterDistance = 198;
            splitContainer1.TabIndex = 1;
            // 
            // ProcessingGroupomboBox
            // 
            ProcessingGroupomboBox.FormattingEnabled = true;
            ProcessingGroupomboBox.Location = new Point(155, 5);
            ProcessingGroupomboBox.Name = "ProcessingGroupomboBox";
            ProcessingGroupomboBox.Size = new Size(199, 28);
            ProcessingGroupomboBox.TabIndex = 2;
            ProcessingGroupomboBox.Text = "Processing Group";
            // 
            // ProcessingRateComboBox
            // 
            ProcessingRateComboBox.FormattingEnabled = true;
            ProcessingRateComboBox.Location = new Point(360, 5);
            ProcessingRateComboBox.Name = "ProcessingRateComboBox";
            ProcessingRateComboBox.Size = new Size(139, 28);
            ProcessingRateComboBox.TabIndex = 3;
            ProcessingRateComboBox.Text = "Processing Rate";
            // 
            // StateSplitContainer
            // 
            StateSplitContainer.Dock = DockStyle.Fill;
            StateSplitContainer.Location = new Point(0, 0);
            StateSplitContainer.Name = "StateSplitContainer";
            // 
            // StateSplitContainer.Panel1
            // 
            StateSplitContainer.Panel1.Controls.Add(flowLayoutPanel1);
            // 
            // StateSplitContainer.Panel2
            // 
            StateSplitContainer.Panel2.Controls.Add(StateFlowLayoutPanel);
            StateSplitContainer.Size = new Size(679, 198);
            StateSplitContainer.SplitterDistance = 75;
            StateSplitContainer.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(AddStateButton);
            flowLayoutPanel1.Controls.Add(RemoveStateButton);
            flowLayoutPanel1.Location = new Point(0, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(72, 192);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // AddStateButton
            // 
            AddStateButton.Location = new Point(3, 3);
            AddStateButton.Name = "AddStateButton";
            AddStateButton.Size = new Size(69, 29);
            AddStateButton.TabIndex = 0;
            AddStateButton.Text = "+";
            AddStateButton.UseVisualStyleBackColor = true;
            // 
            // RemoveStateButton
            // 
            RemoveStateButton.Location = new Point(3, 38);
            RemoveStateButton.Name = "RemoveStateButton";
            RemoveStateButton.Size = new Size(69, 29);
            RemoveStateButton.TabIndex = 1;
            RemoveStateButton.Text = "-";
            RemoveStateButton.UseVisualStyleBackColor = true;
            // 
            // StateFlowLayoutPanel
            // 
            StateFlowLayoutPanel.Location = new Point(-1, 3);
            StateFlowLayoutPanel.Name = "StateFlowLayoutPanel";
            StateFlowLayoutPanel.Size = new Size(601, 195);
            StateFlowLayoutPanel.TabIndex = 0;
            // 
            // TransitionsSplitContainer
            // 
            TransitionsSplitContainer.Dock = DockStyle.Fill;
            TransitionsSplitContainer.Location = new Point(0, 0);
            TransitionsSplitContainer.Name = "TransitionsSplitContainer";
            // 
            // TransitionsSplitContainer.Panel1
            // 
            TransitionsSplitContainer.Panel1.Controls.Add(TransitionsFlowLayoutPanel);
            // 
            // TransitionsSplitContainer.Panel2
            // 
            TransitionsSplitContainer.Panel2.Controls.Add(TransitionFlowLayoutPanel);
            TransitionsSplitContainer.Size = new Size(679, 198);
            TransitionsSplitContainer.SplitterDistance = 71;
            TransitionsSplitContainer.TabIndex = 0;
            // 
            // TransitionsFlowLayoutPanel
            // 
            TransitionsFlowLayoutPanel.Controls.Add(AddTransitionButton);
            TransitionsFlowLayoutPanel.Controls.Add(RemoveTransitionButton);
            TransitionsFlowLayoutPanel.Location = new Point(-3, -1);
            TransitionsFlowLayoutPanel.Name = "TransitionsFlowLayoutPanel";
            TransitionsFlowLayoutPanel.Size = new Size(75, 202);
            TransitionsFlowLayoutPanel.TabIndex = 0;
            // 
            // AddTransitionButton
            // 
            AddTransitionButton.Location = new Point(3, 3);
            AddTransitionButton.Name = "AddTransitionButton";
            AddTransitionButton.Size = new Size(68, 29);
            AddTransitionButton.TabIndex = 0;
            AddTransitionButton.Text = "+";
            AddTransitionButton.UseVisualStyleBackColor = true;
            // 
            // RemoveTransitionButton
            // 
            RemoveTransitionButton.Location = new Point(3, 38);
            RemoveTransitionButton.Name = "RemoveTransitionButton";
            RemoveTransitionButton.Size = new Size(68, 31);
            RemoveTransitionButton.TabIndex = 1;
            RemoveTransitionButton.Text = "-";
            RemoveTransitionButton.UseVisualStyleBackColor = true;
            // 
            // TransitionFlowLayoutPanel
            // 
            TransitionFlowLayoutPanel.Location = new Point(3, 3);
            TransitionFlowLayoutPanel.Name = "TransitionFlowLayoutPanel";
            TransitionFlowLayoutPanel.Size = new Size(604, 198);
            TransitionFlowLayoutPanel.TabIndex = 0;
            // 
            // FSMControl
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(ProcessingRateComboBox);
            Controls.Add(ProcessingGroupomboBox);
            Controls.Add(splitContainer1);
            Controls.Add(FSM_NameTextBox);
            Name = "FSMControl";
            Size = new Size(685, 439);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            StateSplitContainer.Panel1.ResumeLayout(false);
            StateSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)StateSplitContainer).EndInit();
            StateSplitContainer.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            TransitionsSplitContainer.Panel1.ResumeLayout(false);
            TransitionsSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)TransitionsSplitContainer).EndInit();
            TransitionsSplitContainer.ResumeLayout(false);
            TransitionsFlowLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox FSM_NameTextBox;
        private SplitContainer splitContainer1;
        private SplitContainer StateSplitContainer;
        private ComboBox ProcessingGroupomboBox;
        private ComboBox ProcessingRateComboBox;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button AddStateButton;
        private Button RemoveStateButton;
        private FlowLayoutPanel StateFlowLayoutPanel;
        private SplitContainer TransitionsSplitContainer;
        private FlowLayoutPanel TransitionsFlowLayoutPanel;
        private Button AddTransitionButton;
        private Button RemoveTransitionButton;
        private FlowLayoutPanel TransitionFlowLayoutPanel;
    }
}
