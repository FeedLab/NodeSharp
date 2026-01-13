using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeSharp.Nodes.Common.ViewModels;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0034:Direct field reference to [ObservableProperty] backing field")]
public partial class LastOutputMessageTooltipViewModel: ObservableObject, IQueryAttributable
{
    [ObservableProperty] private BaseNode? baseNode;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        BaseNode = (BaseNode)query[nameof(BaseNode)];
    }
}