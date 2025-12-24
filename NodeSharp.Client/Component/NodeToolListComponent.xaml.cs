using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.Client.Component;

public partial class NodeToolListComponent : Microsoft.Maui.Controls.ContentView
{
    private readonly NodeToolListComponentModel viewModel;

    public NodeToolListComponent()
    {
        viewModel = AppService.GetRequiredService<NodeToolListComponentModel>();

        InitializeComponent();
        
        this.BindingContext = viewModel;
    }

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string),
        typeof(NodeToolListComponent), string.Empty, propertyChanged: OnTitleChanged);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (NodeToolListComponent)bindable;
        // control.TitleLabel.Text = (string)newValue;
    }

    private void OnSearchClicked(object sender, EventArgs e)
    {
        var app = Application.Current as App;
        if (app != null)
        {
            app.ToggleTheme();
        }
    }
}