using Syncfusion.Maui.Themes;

namespace NodeSharp.Client;

public partial class App : Application
{
    public App()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjGyl/VkR+XU9Ff1RDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS3tSdEdnWX9ccHBXRWRcVU91XQ==");
        
        InitializeComponent();

    //    ApplyTheme(AppTheme.Light);
        
        //(Application.Current as App)?.ToggleTheme();

        // Apply initial theme
        // if (Current != null)
        // {
        //     ApplyTheme(Current.RequestedTheme);
        // }
        // else
        // {
        //     ApplyTheme(AppTheme.Light);
        // }

        // Subscribe to theme change event
        //Current.RequestedThemeChanged += (s, e) => { ApplyTheme(e.RequestedTheme); };

        // if (Current != null)
        // {
        //     Current.RequestedThemeChanged += (s, e) =>
        //     {
        //         var themeDictionary = Resources.MergedDictionaries
        //             .OfType<SyncfusionThemeResourceDictionary>()
        //             .FirstOrDefault();
        //
        //         var visualTheme = e.RequestedTheme == AppTheme.Dark
        //             ? SfVisuals.MaterialDark
        //             : SfVisuals.MaterialLight;
        //
        //         Resources.MergedDictionaries.Add(new SyncfusionThemeResourceDictionary() { VisualTheme = visualTheme });
        //     };
        // }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell())
        {
            Width = 1600,
            Height = 900
        };

        return window;
    }

    public void ApplyThemxe(AppTheme theme)
    {
        // Try to find the existing Syncfusion theme dictionary
        var themeDictionary = Resources.MergedDictionaries
            .OfType<SyncfusionThemeResourceDictionary>()
            .FirstOrDefault();

        if (themeDictionary != null)
        {
            // Just update the VisualTheme property
            themeDictionary.VisualTheme =
                theme == AppTheme.Dark ? SfVisuals.MaterialDark : SfVisuals.MaterialLight;

            // Re‑insert to force refresh (optional but recommended)
            Resources.MergedDictionaries.Remove(themeDictionary);
            Resources.MergedDictionaries.Add(themeDictionary);
        }
        else
        {
            // If no Syncfusion dictionary exists yet, add one
            Resources.MergedDictionaries.Add(
                new SyncfusionThemeResourceDictionary()
                {
                    VisualTheme = theme == AppTheme.Dark ? SfVisuals.MaterialDark : SfVisuals.MaterialLight
                });
        }
    }

    //public void ToggleTheme()
    //{
    //    var app = Application.Current;

    //    var themeDictionary = app.Resources.MergedDictionaries
    //        .OfType<SyncfusionThemeResourceDictionary>()
    //        .FirstOrDefault();

    //    if (themeDictionary != null)
    //    {
    //        themeDictionary.VisualTheme =
    //            themeDictionary.VisualTheme == SfVisuals.MaterialDark
    //            ? SfVisuals.MaterialLight
    //            : SfVisuals.MaterialDark;

    //        // Force re-apply so controls update
    //        app.Resources.MergedDictionaries.Remove(themeDictionary);
    //        app.Resources.MergedDictionaries.Add(themeDictionary);
    //    }
    //}

    public void ToggleTheme()
    {
        if (Application.Current == null)
        {
            return;
        }
        
        ICollection<ResourceDictionary> mergedDictionaries = Application.Current.Resources.MergedDictionaries;

        var theme = mergedDictionaries?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
        
        if (theme != null)
        {
            if (theme.VisualTheme is SfVisuals.MaterialDark)
            {
                theme.VisualTheme = SfVisuals.MaterialLight;
                Application.Current.UserAppTheme = AppTheme.Light;
            }
            else
            {
                theme.VisualTheme = SfVisuals.MaterialDark;
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
        }
    }

    public void ApplyTheme(AppTheme requestedTheme)
    {
        var application = Application.Current;
        
        if (application?.Resources.MergedDictionaries == null)
        {
            return;
        }
        
        ICollection<ResourceDictionary> mergedDictionaries = application.Resources.MergedDictionaries;

        if (mergedDictionaries != null)
        {
            var visualTheme = mergedDictionaries.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();

            if (requestedTheme is AppTheme.Dark)
            {
                if (visualTheme != null)
                {
                    visualTheme.VisualTheme = SfVisuals.MaterialDark;
                    application.UserAppTheme = AppTheme.Dark;
                }
                else
                {
                    throw new Exception("Unable to find Syncfusion theme dictionary.");
                }
            }
            else
            {
                if (visualTheme != null)
                {
                    visualTheme.VisualTheme = SfVisuals.MaterialLight;
                    application.UserAppTheme = AppTheme.Dark;
                }
                else
                {
                    throw new Exception("Unable to find Syncfusion theme dictionary.");
                }
            }
        }
    }
}