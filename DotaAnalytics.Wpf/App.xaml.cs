using DotaAnalytics.Wpf.Services;
using System.Net.Http;
using System.Windows;

namespace DotaAnalytics.Wpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://dota-analytics-8hnu.onrender.com")
        };

        var apiService = new DotaAnalyticsService(httpClient);
        var mainWindow = new MainWindow(apiService);
        mainWindow.Show();
    }
}