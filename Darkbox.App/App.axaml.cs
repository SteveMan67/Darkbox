using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Darkbox.Catalog.Database;
using Darkbox.Catalog.Repositories;
using Darkbox.Core.Interfaces;
using Darkbox.Core.Services;
using Darkbox.Modules.Library.ViewModels;
using Darkbox.ViewModels;
using Darkbox.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Darkbox;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var provider = services.BuildServiceProvider();

        Services = provider;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = provider.GetRequiredService<MainWindowViewModel>(),
            };
        }
    }
    
    
    private void ConfigureServices(IServiceCollection services)
    {
        var dbFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Darkbox"
        );
        var dbPath = Path.Combine(dbFolder, "catalog.db");
        var connectionString = $"Data Source={dbPath}";

        Directory.CreateDirectory(dbFolder);

        var db = new DarkboxDatabase(connectionString);
        db.InitializeAsync().GetAwaiter().GetResult();

        services.AddSingleton<ICatalogRepository, SqliteCatalogRepository>(
            _ => new SqliteCatalogRepository(connectionString)
        );
        services.AddTransient<LibraryViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<IImportService, ImportService>();
        
        services.AddTransient<IFileSystemService, FileSystemService>();
        
        services.AddTransient<ImportDialogViewModel>();
    }
}