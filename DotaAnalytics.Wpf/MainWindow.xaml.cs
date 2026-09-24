using System.Windows;
using System.Windows.Controls;

namespace DotaAnalytics.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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

        var title = new TextBlock
        {
            Text = page,
            Foreground = System.Windows.Media.Brushes.White,
            FontSize = 28,
            FontWeight = FontWeights.Bold
        };

        ContentArea.Children.Add(title);
    }
}