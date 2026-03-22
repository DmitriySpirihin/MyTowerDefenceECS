using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using System;

public class ComponentNode : Node
{
    public BaseEntityConfigSO Config{get; private set;}
    public System.Action OnDataChanged;

    public ComponentNode(BaseEntityConfigSO config, System.Action onDataChanged)
    {
        Config = config;

        title = "Entity's Component";
        AddToClassList("component-node");

        var port = CreatePort(Direction.Output, Port.Capacity.Single, typeof(ScriptableObject));
        port.portName = "Out";
        inputContainer.Add(port);

        var inspector = new InspectorElement(config);
        extensionContainer.Add(inspector);

        inspector.RegisterCallback<SerializedPropertyChangeEvent>(evt => 
        {
            OnDataChanged?.Invoke();
        });

        // adding a random color each new conponent node
        Color randomColor = Color.HSVToRGB(UnityEngine.Random.value, 0.6f, 0.3f);
        var nodeBorder = this.Q("node-border");
        if (nodeBorder != null)
            nodeBorder.style.backgroundColor = randomColor;
        port.portColor = randomColor;
        
        

        RefreshExpandedState();
        RefreshPorts();
    }

    private Port CreatePort(Direction direction, Port.Capacity capacity, Type type)
    {
        var listener = new MyEdgeConnectorListener(); 
        var port = CleanPort.Create<Edge>(Orientation.Horizontal, direction, capacity, type, listener);
        return port;
    }
}