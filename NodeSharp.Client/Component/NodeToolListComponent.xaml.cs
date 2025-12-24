namespace NodeSharp.Client.Component;

public partial class NodeToolListComponent : Microsoft.Maui.Controls.ContentView
{
    public NodeToolListComponent()
    {
        InitializeComponent();
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

    //private void OnThemeSwitchClicked(object sender, EventArgs e)
    //{
    //    var app = Application.Current as App;

    //    // Check current theme
    //    bool isDark = app.Resources.MergedDictionaries.OfType<DarkTheme>().Any();

    //    // Toggle
    //    app.ApplyTheme(isDark ? AppTheme.Light : AppTheme.Dark);

    //    var currentTheme = Application.Current.RequestedTheme;

    //    if (currentTheme == AppTheme.Dark)
    //    {
    //        // Dark mode is active
    //    }
    //    else
    //    {
    //        // Light mode is active
    //    }

    //}

}