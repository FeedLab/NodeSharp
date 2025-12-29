using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeSharp.Client.ViewModel;

namespace NodeSharp.Client.Component;

public partial class BoxAnchorRightComponent : ContentView
{
    public BoxAnchorRightComponent()
    {
        InitializeComponent();
        
        CanvasRightAnchorArea.BindingContextChanged += OnBindingContextChanged;
    }
    
    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (BindingContext is BoxNode boxNode)
        {
            // UpdateInputAnchors(boxNode);
            // UpdateOutputAnchors(boxNode);

            boxNode.PropertyChanged += (s, args) =>
            {
                Debug.WriteLine("BoxNode changed:");
                // if (args.PropertyName == nameof(BoxNode.Height))
                // {
                //     UpdateInputAnchors(boxNode);
                //     UpdateOutputAnchors(boxNode);
                // }
            };
        }
    }
}