using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeSharp.Nodes.Common;

namespace NodeSharp.Nodes.Inject.ViewModel;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class NodeBodyViewModel : ObservableObject, INodeBodyViewModel
{
    [ObservableProperty]
    private BaseNode node;

    public NodeBodyViewModel(BaseNode node)
    {
        Node = node;
    }
}