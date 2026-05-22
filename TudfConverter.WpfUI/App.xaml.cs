using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TudfConverter.Application;
using TudfConverter.Application.Configuration;
using TudfConverter.Infrastructure;
using TudfConverter.WpfUI.ViewModels;
using TudfConverter.WpfUI.Views;

namespace TudfConverter.WpfUI;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (s, ev) =>
        {
            Log.Fatal(ev.Exception, "Unhandled UI dispatcher exception occurred.");
            MessageBox.Show($"A critical error occurred: {ev.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ev.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, ev) =>
        {
            Log.Fatal(ev.Exception, "Unobserved background task exception occurred.");
            ev.SetObserved();
        };

        AppDomain.CurrentDomain.UnhandledException += (s, ev) =>
        {
            if (ev.ExceptionObject is Exception ex)
            {
                Log.Fatal(ex, "AppDomain unhandled exception occurred.");
            }
        };

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            })
            .ConfigureServices((context, services) =>
            {
                var settings = context.Configuration.GetSection("TudfSettings").Get<TudfAppSettings>() ?? new TudfAppSettings();
                services.AddSingleton(settings);

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(context.Configuration)
                    .CreateLogger();

                services.AddLogging(b => b.AddSerilog(dispose: true));

                services.AddInfrastructureServices();
                services.AddApplicationServices();

                services.AddTransient<ProcessFileView>();
                services.AddTransient<ValidationResultsView>();
                services.AddTransient<HistoryView>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        var viewModel = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = viewModel;
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
