using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeSharp.Nodes.Common.ViewModels;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class DefaultBoxNodeBodyViewModel : ObservableObject
{
    [ObservableProperty]
    private BaseNode node;

    /// <inheritdoc/>
    public DefaultBoxNodeBodyViewModel(BaseNode node)
    {
        Node = node;
    }
}