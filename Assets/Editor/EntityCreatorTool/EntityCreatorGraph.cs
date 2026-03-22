using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;


public class EntityCreatorGraph : GraphView
{
    public EntityCreatorGraph() { }
    public System.Action<ISelectable> OnNodeSelectionChanged;

    public void Init(EditorWindow window)
    {
        var oldGrid = this.Q<GridBackground>();
        if (oldGrid != null) oldGrid.RemoveFromHierarchy();

        var grid = new GridBackground();
        grid.name = "Grid";
        Insert(0, grid);
        grid.StretchToParentSize();
  
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ContentZoomer());

        graphViewChanged = OnGraphViewChanged;

        AddSearchWindow(window);
    }
    
    public override void AddToSelection(ISelectable selectable)
    {
        base.AddToSelection(selectable);
        OnNodeSelectionChanged?.Invoke(selectable);
    }

    public override void RemoveFromSelection(ISelectable selectable)
    { 
        base.RemoveFromSelection(selectable);
        OnNodeSelectionChanged?.Invoke(null);
    }


    public void AddSearchWindow(EditorWindow window)
    {
        var searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        searchWindow.Init(this, window);

        nodeCreationRequest = context => 
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach(port => {
            if (startPort != port && startPort.node != port.node)
                compatiblePorts.Add(port);
        });
        return compatiblePorts;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
            {
                var masterNode = edge.input.node as MasterNode;
                var componentNode = edge.output.node as ComponentNode;
                if (masterNode == null || componentNode == null)
                    Debug.Log("Master node or component node is null");

                if (masterNode != null && componentNode != null)
                {
                    if(!IsComponentAlreadyExists(masterNode, componentNode))
                    {
                        masterNode.Config.Modules.Add(componentNode.Config);
                        Debug.Log($"Component: '{componentNode.Config.Name()}' was added in component list, size is: {masterNode.Config.Modules.Count}");
                    }
                }
            }
        }
        if (change.elementsToRemove != null)
        {
            foreach (var element in change.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    var masterNode = edge.input.node as MasterNode;
                    var compNode = edge.output.node as ComponentNode;

                    if (masterNode != null && compNode != null)
                    {
                        masterNode.Config.Modules.Remove(compNode.Config);
                        EditorUtility.SetDirty(masterNode.Config);
                        Debug.Log($"Removed connection to: {compNode.Config.Name()}");
                    }
                }
            }
        }
        RecalculateMasterStats();
        return change;
    }
    private bool IsComponentAlreadyExists(MasterNode masterNode, ComponentNode componentNode)
    {
        if(masterNode.Config.Modules.Count > 0)
        {
            string name = componentNode.Config.Name();
            foreach (var conf in masterNode.Config.Modules)
            {
                if(conf.Name() == name)
                {
                    Debug.Log($"The '{name}' already exists");
                    return true;
                }
            }
            return false;
        }
        return false;
    }
    public void RecalculateMasterStats()
    {
        var masterNode = nodes.ToList().OfType<MasterNode>().FirstOrDefault();
        if (masterNode == null) return;

        int power = 0;
        float speed = 0f;
        int durability = 0;
        int count = masterNode.Config.Modules.Count;

        foreach (var comp in masterNode.Config.Modules)
        {
            switch (comp)
            {
                case MoveConfigSO moveConfigSO:
                    speed += moveConfigSO.Speed;
                break;

                case DamageableConfigSO damageableConfigSO:
                    durability += damageableConfigSO.Hp;
                    durability += damageableConfigSO.Armor * 10;
                break;

                default:
                    Debug.Log($"Component {comp.name} has no calculation logic defined.");
                break;
            }
        }

        masterNode.UpdateStatsDisplay(power, (int)speed, durability, count);
    }
}
