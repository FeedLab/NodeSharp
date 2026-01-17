using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeSharp.Nodes.KS0212.ViewModel
{
    public partial class Ks0212ConfigurePopupViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] private NodeKs0212? selectedNode;
        
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            SelectedNode = (NodeKs0212)query[nameof(NodeKs0212)];

            if (SelectedNode is not null)
            {
            }
        }
    }
}
