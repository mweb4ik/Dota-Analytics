using System.Windows;
using System.Windows.Controls;
using System.Linq;
using DotaAnalytics.Wpf.Services;
using DotaAnalytics.Shared.Models;
using System.Collections.Generic;

namespace DotaAnalytics.Wpf.Pages
{
    public partial class LeaderboardPage : UserControl
    {
        private readonly IDotaAnalyticsService _apiService;

        public LeaderboardPage(IDotaAnalyticsService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            this.Loaded += LeaderboardPage_Loaded;
        }

        private async void LeaderboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLeaderboardAsync();
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadLeaderboardAsync();
        }

        private async Task LoadLeaderboardAsync()
        {
            StatusText.Text = "Загрузка данных...";
            LeaderboardDataGrid.ItemsSource = null;

            var response = await _apiService.GetLeaderboardAsync();

            if (response?.TopPlayers != null && response.TopPlayers.Count > 0)
            {
                // Добавляем красивый порядковый номер (Ранг)
                var rankedPlayers = response.TopPlayers.Select((player, index) => new LeaderboardPlayerViewModel
                {
                    Rank = index + 1,
                    AccountId = player.AccountId,
                    TotalMatches = player.TotalMatches,
                    Wins = player.Wins,
                    Losses = player.Losses,
                    WinRate = player.WinRate
                }).ToList();

                LeaderboardDataGrid.ItemsSource = rankedPlayers;
                StatusText.Text = $"Показано топ-{rankedPlayers.Count} игроков (всего оценено: {response.TotalPlayersEvaluated})";
            }
            else
            {
                StatusText.Text = "Недостаточно данных для формирования лидерборда (нужно минимум 10 матчей на игрока)";
            }
        }
    }

    // Вспомогательный класс для отображения с рангом
    public class LeaderboardPlayerViewModel
    {
        public int Rank { get; set; }
        public long AccountId { get; set; }
        public int TotalMatches { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
    }
}