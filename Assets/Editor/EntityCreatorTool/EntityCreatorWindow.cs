using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEditor.UIElements;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine;

public class EntityCreatorWindow : EditorWindow
{
    [MenuItem("Window/Entity Creator Graph")]
    public static void Open() => GetWindow<EntityCreatorWindow>("Entity Creator");

    private EntityComponentsHolderSO config;
    private VisualElement _inspectorContainer;

    void CreateGUI()
    {
        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        splitView.style.flexGrow = 1; 
        rootVisualElement.Add(splitView);

        var _graphView = new EntityCreatorGraph();
        _graphView.Init(this);
        _graphView.style.flexGrow = 1; 

        _inspectorContainer = new VisualElement();
        _inspectorContainer.style.backgroundColor = new Color(0.08f, 0.1f, 0.13f);
        _inspectorContainer.style.paddingLeft = 10;
        _inspectorContainer.style.paddingRight = 10;

        splitView.Add(_graphView);
        splitView.Add(_inspectorContainer);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/EntityCreatorTool/EntityEditorStyles.uss");
        if (styleSheet != null) 
            rootVisualElement.styleSheets.Add(styleSheet);

        var toolbar = new Toolbar();
        var addBtn = new Button(() =>
        {
            // Checking if there is already masterNode
            var masterNode = _graphView.nodes.ToList().OfType<MasterNode>().FirstOrDefault();
            if (masterNode != null) return;
            config = CreateInstance<EntityComponentsHolderSO>();
            var node = new MasterNode(config);
            _graphView.AddElement(node);
        }){text = "Add master container for components"};
        
        // save asset button
        var saveBtn = new Button(() =>
        {
            if(config == null) return;

            string path = EditorUtility.SaveFilePanelInProject("Save master config", "NewEntityConfig", "asset", "Select save location");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                Console.WriteLine($"Saved to {path}");
            }

        }){text = "Save asset"};
        
        _graphView.AddSearchWindow(this);

        toolbar.Add(addBtn);
        toolbar.Add(saveBtn);
        rootVisualElement.Add(toolbar);

        _graphView.OnNodeSelectionChanged = (selectable) => {
        _inspectorContainer.Clear();

        if (selectable is Node node)
        {
            var header = new Label(node.title.ToUpper());
            header.style.paddingBottom = 10;
            _inspectorContainer.Add(header);

            ScriptableObject so = null;
            if (node is ComponentNode comp) so = comp.Config;
            else if (node is MasterNode master) so = master.Config;

            if (so != null)
            {
                var inspector = new InspectorElement(so);
                _inspectorContainer.Add(inspector);
            
                inspector.RegisterCallback<SerializedPropertyChangeEvent>(evt => {
                    _graphView.RecalculateMasterStats();
                });
            }
        } };
    }

    
}
