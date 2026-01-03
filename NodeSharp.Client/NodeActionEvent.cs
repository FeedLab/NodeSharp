namespace NodeSharp.Client;

public enum NodeActionEventType
{
    Add,
    Delete,
    Reset
}

public class NodeActionEvent
{
    public NodeActionEventType ActionEventType { get; set; } = NodeActionEventType.Add;
}