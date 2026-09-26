using DotaAnalytics.Wpf.Services; 
using System.Windows;
using System.Windows.Controls;

namespace DotaAnalytics.Wpf;

public partial class MainWindow : Window
{
    private readonly IDotaAnalyticsService _apiService;

    public MainWindow(IDotaAnalyticsService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Dashboard");
    }

    private void Matches_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Matches");
    }

    private void Players_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Players");
    }

    private void Heroes_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Heroes");
    }

    private void Leaderboard_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Leaderboard");
    }

    private void ShowPage(string page)
    {
        ContentArea.Children.Clear();

        UserControl pageControl = page switch
        {
            "Matches" => new Pages.MatchesPage(_apiService),
            "Players" => new Pages.PlayersPage(_apiService),
            "Heroes" => new Pages.HeroesPage(),
            "Leaderboard" => new Pages.LeaderboardPage(),
            _ => new Pages.MatchesPage(_apiService) 
        };

        ContentArea.Children.Add(pageControl);
    }
}