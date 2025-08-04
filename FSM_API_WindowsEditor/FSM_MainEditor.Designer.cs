namespace FSM_API_WindowsEditor
{
    partial class FSM_MainEditor
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            FSM_MenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            createNewWorkspaceToolStripMenuItem = new ToolStripMenuItem();
            openWorkspaceToolStripMenuItem = new ToolStripMenuItem();
            quitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            gitToolStripMenuItem = new ToolStripMenuItem();
            FSM_StatusStrip = new StatusStrip();
            WorkspaceFlowLayoutPanel = new FlowLayoutPanel();
            FSM_MenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // FSM_MenuStrip
            // 
            FSM_MenuStrip.ImageScalingSize = new Size(20, 20);
            FSM_MenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, gitToolStripMenuItem });
            FSM_MenuStrip.Location = new Point(0, 0);
            FSM_MenuStrip.Name = "FSM_MenuStrip";
            FSM_MenuStrip.Size = new Size(800, 28);
            FSM_MenuStrip.TabIndex = 1;
            FSM_MenuStrip.Text = "FSM_Menu";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { createNewWorkspaceToolStripMenuItem, openWorkspaceToolStripMenuItem, quitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // createNewWorkspaceToolStripMenuItem
            // 
            createNewWorkspaceToolStripMenuItem.Name = "createNewWorkspaceToolStripMenuItem";
            createNewWorkspaceToolStripMenuItem.Size = new Size(245, 26);
            createNewWorkspaceToolStripMenuItem.Text = "Create New Workspace";
            // 
            // openWorkspaceToolStripMenuItem
            // 
            openWorkspaceToolStripMenuItem.Name = "openWorkspaceToolStripMenuItem";
            openWorkspaceToolStripMenuItem.Size = new Size(245, 26);
            openWorkspaceToolStripMenuItem.Text = "Open Workspace";
            // 
            // quitToolStripMenuItem
            // 
            quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            quitToolStripMenuItem.Size = new Size(245, 26);
            quitToolStripMenuItem.Text = "Quit";
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(49, 24);
            editToolStripMenuItem.Text = "Edit";
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(55, 24);
            viewToolStripMenuItem.Text = "View";
            // 
            // gitToolStripMenuItem
            // 
            gitToolStripMenuItem.Name = "gitToolStripMenuItem";
            gitToolStripMenuItem.Size = new Size(42, 24);
            gitToolStripMenuItem.Text = "Git";
            // 
            // FSM_StatusStrip
            // 
            FSM_StatusStrip.ImageScalingSize = new Size(20, 20);
            FSM_StatusStrip.Location = new Point(0, 426);
            FSM_StatusStrip.Name = "FSM_StatusStrip";
            FSM_StatusStrip.Size = new Size(800, 24);
            FSM_StatusStrip.TabIndex = 2;
            FSM_StatusStrip.Text = "I've got nothing";
            // 
            // WorkspaceFlowLayoutPanel
            // 
            WorkspaceFlowLayoutPanel.Location = new Point(0, 31);
            WorkspaceFlowLayoutPanel.Name = "WorkspaceFlowLayoutPanel";
            WorkspaceFlowLayoutPanel.Size = new Size(800, 394);
            WorkspaceFlowLayoutPanel.TabIndex = 3;
            // 
            // FSM_MainEditor
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(WorkspaceFlowLayoutPanel);
            Controls.Add(FSM_StatusStrip);
            Controls.Add(FSM_MenuStrip);
            MainMenuStrip = FSM_MenuStrip;
            Name = "FSM_MainEditor";
            Text = "TheSingularityWorkshop - FSM Editor";
            FSM_MenuStrip.ResumeLayout(false);
            FSM_MenuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private MenuStrip FSM_MenuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem createNewWorkspaceToolStripMenuItem;
        private ToolStripMenuItem openWorkspaceToolStripMenuItem;
        private ToolStripMenuItem quitToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem gitToolStripMenuItem;
        private StatusStrip FSM_StatusStrip;
        private FlowLayoutPanel WorkspaceFlowLayoutPanel;
    }
}
