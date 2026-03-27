using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Reflection;


public class EntityCreatorGraph : GraphView
{
    public System.Action<ISelectable> OnNodeSelectionChanged;
    private bool _isRefreshing;

    public EntityCreatorGraph() { }

    public void Init(EditorWindow window)
    {
        Insert(0, new GridBackground());

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ContentZoomer());

        graphViewChanged = OnGraphViewChanged;
        
        SetupDragAndDrop();
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

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node &&
            endPort.portType == startPort.portType).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (_isRefreshing) return change;
        // Adding module logic
        if (change.edgesToCreate != null)
        {
            foreach (var edge in change.edgesToCreate)
            {
                if (edge.input.node is MasterNode master && edge.output.node is ComponentNode comp)
                {
                    var masterConfig = master.Config;
                    var moduleConfig = comp.Config;

                    //Checking asset is already exists or not
                    if (AssetDatabase.Contains(masterConfig))
                    {
                        if (!AssetDatabase.Contains(moduleConfig))
                        {
                            AssetDatabase.AddObjectToAsset(moduleConfig, masterConfig);
                            AssetDatabase.SaveAssets();
                        }
                    }

                     //Adding to the list if not present
                    if (!masterConfig.Modules.Contains(moduleConfig))
                    {
                        masterConfig.Modules.Add(moduleConfig);
                        EditorUtility.SetDirty(masterConfig);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
        // Remove module logic
        if (change.elementsToRemove != null)
        {
            foreach (var element in change.elementsToRemove)
            {
                if (element is Edge edge && edge.input?.node is MasterNode master && edge.output?.node is ComponentNode comp)
                {
                    if (master.Config != null && master.Config.Modules.Contains(comp.Config))
                    {
                        master.Config.Modules.Remove(comp.Config);
                        EditorUtility.SetDirty(master.Config);
                    };
                }
            }
        }
        // Setting new Rect positions to configs
        if (change.movedElements != null)
        {
            foreach (var node in change.movedElements.OfType<ComponentNode>())
            {
                node.Config.nodePositionInEditor = node.GetPosition().position;
                EditorUtility.SetDirty(node.Config);
            }
        }

        RecalculateMasterStats();
        return change;
    }

    public void LoadFromMasterConfig(EntityComponentsHolderSO config)
    {
        _isRefreshing = true;
        
        DeleteElements(graphElements.ToList());

        var masterNode = new MasterNode(config);
        masterNode.SetPosition(new Rect(100, 100, 200, 150));
        AddElement(masterNode);
        
        var masterInput = masterNode.inputContainer.Q<Port>();

        foreach (var conf in config.Modules)
        {
            if (conf == null) continue;
            
            var componentNode = new ComponentNode(conf, RecalculateMasterStats);
            componentNode.SetPosition(new Rect(conf.nodePositionInEditor, Vector2.zero));
            AddElement(componentNode);

            var compOutput = componentNode.outputContainer.Q<Port>();
            if (masterInput != null && compOutput != null)
                LinkNodes(masterInput, compOutput);
        }

        _isRefreshing = false; // UNLOCK
        RecalculateMasterStats();
    }

    private void LinkNodes(Port input, Port output)
    {
        var edge = new Edge { input = input, output = output };
        edge.input.Connect(edge);
        edge.output.Connect(edge);
        AddElement(edge);
    }

    public void RecalculateMasterStats()
    {
        var masterNode = nodes.OfType<MasterNode>().FirstOrDefault();
        if (masterNode == null) return;

        int power = 0, durability = 0;
        float speed = 0f;

        foreach (var comp in masterNode.Config.Modules)
        {
            if (comp is MoveConfigSO move) speed += move.Speed;
            else if (comp is DamageableConfigSO dmg) durability += dmg.Hp + (dmg.Armor * 10);
        }

        masterNode.UpdateStatsDisplay(power, (int)speed, durability, masterNode.Config.Modules.Count);
    }

    private void SetupDragAndDrop()
    {
        RegisterCallback<DragUpdatedEvent>(ev => {
            if (DragAndDrop.objectReferences.FirstOrDefault() is EntityComponentsHolderSO)
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        });

        RegisterCallback<DragPerformEvent>(ev => {
            if (DragAndDrop.objectReferences.FirstOrDefault() is EntityComponentsHolderSO config) {
                LoadFromMasterConfig(config);
                DragAndDrop.AcceptDrag();
            }
        });
    }

    public void AddSearchWindow(EditorWindow window)
    {
        var searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        searchWindow.Init(this, window);
        nodeCreationRequest = ctx => SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), searchWindow);
    }
}
