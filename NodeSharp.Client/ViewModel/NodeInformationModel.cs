using CommunityToolkit.Mvvm.ComponentModel;
using Facet;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.Client.ViewModel;

[Facet(typeof(NodeInformation))]
public partial class NodeInformationModel : ObservableObject
{
    // [ObservableProperty] private string typeId;
    // [ObservableProperty] private string activateOnStart;
    // [ObservableProperty] private string isEnabled;
}