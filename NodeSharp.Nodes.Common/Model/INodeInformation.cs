using Microsoft.Maui.Controls;

namespace NodeSharp.Nodes.Common.Model;

public interface INodeInformation
{
    string TypeId { get; set; }
    string RuntimeType { get; set; }
    bool ActivateOnStart { get; set; }
    bool IsEnabled { get; set; }
    string? Information { get; set; }
    string? Symbol { get; set; }
    int NumberOfInputs { get; set; }
    int NumberOfOutputs { get; set; }
    bool HasInformationText { get; }
    
    ContentView? NodeConfigurePopup { get; }
}