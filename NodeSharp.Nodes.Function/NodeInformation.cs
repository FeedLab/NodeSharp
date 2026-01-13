using System.Text;
using NodeSharp.Nodes.Common.Model;
using NodeSharp.Nodes.Function.Component;

namespace NodeSharp.Nodes.Function;

public class NodeInformation : INodeInformation
{
    private NodeInformation(string typeId, string runtimeType, bool activateOnStart, bool isEnabled, string information, string symbol,
        int numberOfInputs, int numberOfOutputs)
    {
        TypeId = typeId;
        RuntimeType = runtimeType;
        ActivateOnStart = activateOnStart;
        IsEnabled = isEnabled;
        Information = information;
        Symbol = symbol;
        NumberOfInputs = numberOfInputs;
        NumberOfOutputs = numberOfOutputs;
    }


    public NodeInformation() : this("Function", "NodeFunction", true, true,
        "cybersecurity, where attackers inject malicious code into applications", "\ue710", 0, 1)
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