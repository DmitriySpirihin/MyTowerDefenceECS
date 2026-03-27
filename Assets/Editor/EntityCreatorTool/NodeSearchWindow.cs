using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private EntityCreatorGraph _entityCreatorGraph;
    private EditorWindow _editorWindow;

    public void Init(EntityCreatorGraph entityCreatorGraph, EditorWindow editorWindow)
    {
        _entityCreatorGraph = entityCreatorGraph;
        _editorWindow = editorWindow;
    }
    
    //The list of configs
    //We can add new configs SO here
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Add Component"), 0),
            new SearchTreeGroupEntry(new GUIContent("Transform and moovement"), 1),
            new SearchTreeEntry(new GUIContent("Scale")) { level = 2, userData = typeof(ScaleConfigSO) },
            new SearchTreeEntry(new GUIContent("Move")) { level = 2, userData = typeof(MoveConfigSO) },
            new SearchTreeEntry(new GUIContent("Rotate")) { level = 2, userData = typeof(RotateConfigSO) },

            new SearchTreeGroupEntry(new GUIContent("Initial"), 1),
            new SearchTreeEntry(new GUIContent("Damageable")) { level = 2, userData = typeof(DamageableConfigSO) },

            new SearchTreeGroupEntry(new GUIContent("Rendering"), 1),
            new SearchTreeEntry(new GUIContent("Base color")) { level = 2, userData = typeof(BaseColorConfigSO) },
            new SearchTreeEntry(new GUIContent("Tint")) { level = 2, userData = typeof(TintConfigSO) },
            new SearchTreeEntry(new GUIContent("Shape deformation noise")) { level = 2, userData = typeof(NoiseConfigSO) },
            new SearchTreeEntry(new GUIContent("Shape displacement")) { level = 2, userData = typeof(DisplaceConfigSO) },
            
        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        // Getting entry type
        var type = (Type)entry.userData;
        
        // Creating SO instance
        var config = CreateInstance(type);
        config.name = type.Name;

        // Creating graph node
        var node = new ComponentNode(config as BaseEntityConfigSO, () => _entityCreatorGraph.RecalculateMasterStats());

        
        var mousePos = _editorWindow.rootVisualElement.ChangeCoordinatesTo(_editorWindow.rootVisualElement.parent, context.screenMousePosition - _editorWindow.position.position);
        var graphMousePos = _entityCreatorGraph.contentViewContainer.WorldToLocal(mousePos);
        
        node.SetPosition(new Rect(graphMousePos, Vector2.zero));
        _entityCreatorGraph.AddElement(node);
        
        return true;
    }
}
