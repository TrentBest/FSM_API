using System.Xml.Linq;

namespace FSM_API_WindowsEditor
{
    public partial class FSM_MainEditor : Form
    {
        public static class EditorSettings
        {

        }

        public FSM_MainEditor()
        {
            InitializeComponent();
            foreach (var menuDropDown in FSM_MenuStrip.Items)
            {

            }
        }
    }

    public class FSM_Application
    {
        public List<FSM_Workspace> Workspaces { get; set; } = new List<FSM_Workspace>();
        public List<FSM_ExtraWorkspaceLink> ExtraWorkspaceLinks { get; set; } = new List<FSM_ExtraWorkspaceLink>();
    
        public FSM_Application(string appPath)
        {
            LoadApplicationManifest(appPath);
        }

        private void LoadApplicationManifest(string appPath)
        {
            
        }
    }

    public class FSM_Workspace
    {
        public string WorkspaceName { get; set; }
        public Dictionary<FSM_EditorFiniteStateMachine, FSM_WorkspacePosition> Members { get; set; } = new Dictionary<FSM_EditorFiniteStateMachine, FSM_WorkspacePosition>();
        public List<FSM_WorkspaceLink> WorkspaceLinks { get; set; } = new List<FSM_WorkspaceLink>();

        public void AddFiniteStateMachineToWorkspace(FSM_EditorFiniteStateMachine fsm, FSM_WorkspacePosition position)
        {
            Members.Add(fsm, position);
        }

        public void RemoveFiniteStateMachineFromWorkspace(string fsmName)
        {
            var fsm = Members.FirstOrDefault(s=>s.Key.FSM_Name == fsmName);
            if(fsm.Key != null)
            {
                Members.Remove(fsm.Key);
            }
        }
    }

  
    public class FSM_WorkspaceLink
    {
        public string ParentFSM_Name { get; set; }
        public string ChildFSM_Name { get; set; }
        public override string ToString()
        {
            return $"{ParentFSM_Name}=>{ChildFSM_Name}";
        }
    }

    public class FSM_ExtraWorkspaceLink
    {
        FSM_WorkspaceLink FSM_WorkspaceLink { get; set; }
        FSM_Workspace LinkedWorkspace { get; set; }
        public override string ToString()
        {
            return $"{LinkedWorkspace.WorkspaceName}.{FSM_WorkspaceLink}";
        }
    }

    public class FSM_WorkspacePosition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int SortOrder { get; set; }
        public override string ToString()
        {
            return $"({X},{Y}):{SortOrder}";
        }
    }


    public class FSM_EditorProcessingGroup
    {
        public string ProcessingGroup { get; set; }
        public Color ProcessingGroupColor { get; set; }
        public override string ToString() => ProcessingGroup;
    }

    public class FSM_EditorState
    {
        public string StateName { get; set; }
        public string OnEnterMethodName { get; set; }
        public string OnUpdateMethodName { get; set; }
        public string OnExitMethodName { get; set; }

        public override string ToString() => StateName;

    }

    public class FSM_EditorTransition
    {
        public string FromState { get; set; }
        public string ToState { get; set; }
        public string ConditionMethodName { get; set; }
        public override string ToString()
        {
            return $"[{FromState}]=>[{ToState}]:  {ConditionMethodName}";
        }
    }

    public class FSM_EditorFiniteStateMachine
    {
        public string FSM_Name { get; set; }
        public int ProcessingRate { get; set; }
        public FSM_EditorProcessingGroup ProcessingGroup { get; set; }
        public List<FSM_EditorState> States { get; } = new List<FSM_EditorState>();
        public List<FSM_EditorTransition> Transitions { get; } = new List<FSM_EditorTransition>();
    }
}
