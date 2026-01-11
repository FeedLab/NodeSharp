using System.Text;

namespace NodeSharp.Nodes.Common.Model;

public class NodeInformation(string typeId, string runtimeType, bool activateOnStart, bool isEnabled, string information, string symbol, int numberOfInputs, int numberOfOutputs) : INodeInformation
{
    public string TypeId { get; set; } = typeId;

    public string RuntimeType { get; set; } = runtimeType;
    public bool ActivateOnStart { get; set; } = activateOnStart;
    public bool IsEnabled { get; set; } = isEnabled;
    public string? Information { get; set; } = information;
    public string? Symbol { get; set; } = symbol;

    public int NumberOfInputs { get; set; } = numberOfInputs;
    
    public int NumberOfOutputs { get; set; } = numberOfOutputs;
    
    public bool HasInformationText => !string.IsNullOrWhiteSpace(Information);

    public ContentView? NodeConfigurePopup { get; }
    
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