using System.Text;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Inject.Component;

namespace NodeSharp.Nodes.Inject;

public class NodeInformation : INodeInformation
{
    private NodeInformation(string typeId, string runtimeType, bool activateOnStart, bool isEnabled, string information, string symbol,
        int numberOfInputs, int numberOfOutputs, ContentView nodeConfigurePopup)
    {
        TypeId = typeId;
        RuntimeType = runtimeType;
        ActivateOnStart = activateOnStart;
        IsEnabled = isEnabled;
        Information = information;
        Symbol = symbol;
        NumberOfInputs = numberOfInputs;
        NumberOfOutputs = numberOfOutputs;
        NodeConfigurePopup = nodeConfigurePopup;
    }


    public NodeInformation() : this("NodeInject", "NodeInject", true, true,
        "cybersecurity, where attackers inject malicious code into applications", "\ue713", 0, 1, new InjectConfigurePopupComponent())
    {
    }

    public string TypeId { get; set; }
    public string RuntimeType { get; set; }
    public bool ActivateOnStart { get; set; }
    public bool IsEnabled { get; set; }
    public string? Information { get; set; }
    public string? Symbol { get; set; }

    public int NumberOfInputs { get; set; }

    public int NumberOfOutputs { get; set; }

    public bool HasInformationText => !string.IsNullOrWhiteSpace(Information);

    public ContentView NodeConfigurePopup { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{nameof(TypeId)}: {TypeId}");
        sb.AppendLine($"{nameof(ActivateOnStart)}: {ActivateOnStart}");
        sb.AppendLine($"{nameof(IsEnabled)}: {IsEnabled}");
        sb.AppendLine($"{nameof(Information)}: {Information}");
        sb.AppendLine($"{nameof(Symbol)}: {Symbol}");
        sb.AppendLine($"{nameof(HasInformationText)}: {HasInformationText}");
        return sb.ToString();
    }
}