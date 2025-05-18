using System.Windows;
using Microsoft.Extensions.Configuration;

namespace construkto3._0;

public partial class App : Application
{
    public static IConfiguration Configuration { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        // Собираем конфиг из appsettings.json
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        base.OnStartup(e);
    }
}