using System;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor.Docking.Extensions;
using Photino.Blazor.Docking.Sample.Pages;
using Photino.Blazor.Docking.Sample.Services;
using Photino.Blazor.Docking.Sample.Shared;
using Index = Photino.Blazor.Docking.Sample.Pages.Index;

namespace Photino.Blazor.Docking.Sample;

class Program
{
    private static void InitializeServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddScoped<TestService>();

        // to register singleton service (single between OS windows) -
        // use service instance manually created and statically stored:
        // services.AddSingleton(_testService);
    }

    [STAThread]
    static void Main(string[] args)
    {
        var appBuilder = PhotinoBlazorAppBuilder.CreateDefault(args);

        InitializeServices(appBuilder.Services);

        appBuilder.Services.AddPhotinoBlazorDocking(
            InitializeServices,
            [
                new DockPanelConfig(typeof(Index), "index", "Index page"),
                new DockPanelConfig(typeof(Counter), "counter", "Counter Page"),
                new DockPanelConfig(typeof(FetchData), "fetchData", "Fetch data page"),
                new DockPanelConfig(typeof(TestPage1), "testPage1", "Test page #1"),
                new DockPanelConfig(typeof(TestPage2), "testPage2", "Test page #2"),
                new DockPanelConfig(typeof(TestFloatPanel), "testFloatPanel", "Test float panel"),
            ]
            //, typeof(DemoWrapper)
        );

        // register root component and selector
        appBuilder.RootComponents.Add<App>("app");

        var app = appBuilder.Build();

        // customize window
        app.MainWindow
            .SetSize(1500, 1000)
            .SetIconFile("favicon.ico")
            .SetTitle("Docking Demo");

        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            app.MainWindow.ShowMessage("Fatal exception", error.ExceptionObject.ToString());
        };

        app.Run();
    }
}
