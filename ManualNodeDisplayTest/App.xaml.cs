using Microsoft.Extensions.DependencyInjection;

namespace ManualNodeDisplayTest;

public partial class App : Application
{
    public App()
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
            "Ngo9BigBOggjGyl/VkR+XU9Ff1RDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS3tSdEdnWX9ccHBXRWRcVU91XQ==");

        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell()
        {
            WidthRequest = 1600,
            HeightRequest = 800,
        });
    }
}