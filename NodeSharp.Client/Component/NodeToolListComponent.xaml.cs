using NodeSharp.Client.Services;
using NodeSharp.Client.ViewModel;
using NodeSharp.NodeEngine.Model;

namespace NodeSharp.Client.Component;

public partial class NodeToolListComponent : Microsoft.Maui.Controls.ContentView
{
    private readonly NodeToolListComponentModel viewModel;
    private readonly DiagramViewModel diagramViewModel;

    public NodeToolListComponent()
    {
        viewModel = AppService.GetRequiredService<NodeToolListComponentModel>();
        diagramViewModel = AppService.GetRequiredService<DiagramViewModel>();

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
    
    
    private void OnSaveTapped(object sender, EventArgs e) { /* Save logic */ }
    private void OnSaveAsTapped(object sender, EventArgs e) { /* Save As logic */ }

    private async void OnLoadTapped(object sender, EventArgs e)
    {
        await diagramViewModel.Init();
    }
    private void OnNewTapped(object sender, EventArgs e) { /* New logic */ }

    private void OnQuitTapped(object sender, EventArgs e)
    {
        Application.Current?.Quit();
    }


}