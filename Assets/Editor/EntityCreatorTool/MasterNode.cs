using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

public class MasterNode : Node
{
    public EntityComponentsHolderSO Config{get; private set;}

    private Label _statsLabel;

    public MasterNode(EntityComponentsHolderSO config)
    {
        Config = config;
        title = "Entity Components Container";
        AddToClassList("master-node");

        var port = CreatePort(Direction.Input, Port.Capacity.Multi, typeof(ScriptableObject));
        port.portName = "Components";
        inputContainer.Add(port);

        //Add stats display area
        var statsContainer = new VisualElement();
        statsContainer.style.paddingTop = 10;
        statsContainer.style.paddingBottom = 10;
        statsContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        _statsLabel = new Label("POWER:0 | SPEED:0 | DURABILITY:0 | COUNT:0");
        _statsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _statsLabel.style.color = Color.white;
        UpdateStatsDisplay(0, 0, 0, 0);
        
        statsContainer.Add(_statsLabel);
        extensionContainer.Add(statsContainer);

        var inspector = new InspectorElement(config);
        extensionContainer.Add(inspector);
        RefreshExpandedState();
        RefreshPorts();
        expanded = true;
    }
    // stats updating method
    public void UpdateStatsDisplay(int power, int speed, int durability, int count)
    {
        string colorPwr = "#f97c7c";
        string colorSpd = "#83ec83";
        string colorDur = "#e6e085";
        string colorCnt = "#7cedd2";
        string gray = "#888888";
        _statsLabel.text = 
           $"<color={colorPwr}>POWER:</color> <b><color={colorPwr}>{power}</b></color> <color={gray}>|</color> " +
           $"<color={colorSpd}>SPEED:</color> <color={colorSpd}><b>{speed}</b></color> <color={gray}>|</color> " +
           $"<color={colorDur}>DURABILITY:</color> <color={colorDur}><b>{durability}</b></color> <color={gray}>|</color> " +
           $"<color={colorCnt}>COUNT:</color> <color={colorCnt}><b>{count}</b></color>";
    }
    // port creation
    private Port CreatePort(Direction direction, Port.Capacity capacity, Type type)
    {
        var listener = new MyEdgeConnectorListener();
        var port = CleanPort.Create<Edge>(Orientation.Horizontal, direction, capacity, type, listener);

        return port;
    }

}
public class CleanPort : Port
{
    protected CleanPort(Orientation orientation, Direction direction, Capacity capacity, Type type) 
        : base(orientation, direction, capacity, type) { }

    public static CleanPort Create<TEdge>(Orientation orientation, Direction direction, Capacity capacity, Type type, IEdgeConnectorListener listener) 
        where TEdge : Edge, new()
    {
        var port = new CleanPort(orientation, direction, capacity, type);
        port.m_EdgeConnector = new EdgeConnector<TEdge>(listener);
        port.AddManipulator(port.m_EdgeConnector);
        return port;
    }
}

public class MyEdgeConnectorListener : IEdgeConnectorListener
{
    public void OnDropOutsidePort(Edge edge, Vector2 position) { }
    public void OnDrop(GraphView graphView, Edge edge)
    {
        if (edge.input == null || edge.output == null) return;

        graphView.AddElement(edge);
        edge.input.Connect(edge);
        edge.output.Connect(edge);
        var change = new GraphViewChange
        {
            edgesToCreate = new List<Edge> { edge }
        };
    
        graphView.graphViewChanged?.Invoke(change);
    }
}
