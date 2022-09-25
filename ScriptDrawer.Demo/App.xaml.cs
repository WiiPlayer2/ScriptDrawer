using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ScriptDrawer.Demo;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private readonly IHost host;

    public App()
    {
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        host = new HostBuilder()
            .ConfigureDefaults(args)
            .ConfigureServices(ConfigureServices)
            .Build();
    }

    private void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();
        services.Configure<Config>(ctx.Configuration);
        services.AddScriptDrawer();
    }

    #region Startup & Exit

    private async void App_OnStartup(object sender, StartupEventArgs e)
    {
        await host.StartAsync();

        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        var mainViewModel = host.Services.GetRequiredService<MainViewModel>();

        mainWindow.DataContext = mainViewModel;
        mainWindow.Show();
    }

    private async void App_OnExit(object sender, ExitEventArgs e)
    {
        using (host)
        {
            await host.StopAsync(TimeSpan.FromSeconds(5));
        }
    }

    #endregion
}
