using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeSharp.Nodes.Common.ViewModels;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class BoxNodeStatusTextViewModel : ObservableObject
{
    [ObservableProperty]
    private BaseNode node;

    /// <param name="node"></param>
    /// <inheritdoc/>
    public BoxNodeStatusTextViewModel(BaseNode node)
    {
        this.Node = node;
    }
}