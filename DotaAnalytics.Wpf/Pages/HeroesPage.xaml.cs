using System.Windows;
using System.Windows.Controls;
using System.Linq;
using DotaAnalytics.Wpf.Services;
using DotaAnalytics.Shared.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace DotaAnalytics.Wpf.Pages
{
    public partial class HeroesPage : UserControl
    {
        private readonly IDotaAnalyticsService _apiService;

        public HeroesPage(IDotaAnalyticsService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            this.Loaded += HeroesPage_Loaded;
        }

        private async void HeroesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadHeroStatsAsync();
        }

        private async Task LoadHeroStatsAsync()
        {
            StatusText.Text = "Загрузка данных...";
            HeroesDataGrid.ItemsSource = null;

            var heroes = await _apiService.GetHeroStatsAsync();

            if (heroes != null && heroes.Count > 0)
            {
                var rankedHeroes = heroes.Select((hero, index) => new HeroViewModel
                {
                    Rank = index + 1,
                    HeroId = hero.HeroId,
                    GamesPlayed = hero.GamesPlayed,
                    Wins = hero.Wins,
                    Losses = hero.Losses,
                    WinRate = hero.WinRate,
                    AverageKills = hero.AverageKills,
                    AverageDeaths = hero.AverageDeaths,
                    AverageAssists = hero.AverageAssists
                }).ToList();

                HeroesDataGrid.ItemsSource = rankedHeroes;
                StatusText.Text = $"Показано топ-{rankedHeroes.Count} героев";
            }
            else
            {
                StatusText.Text = "Нет данных о героях";
            }
        }
    }

    public class HeroViewModel
    {
        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("heroId")]
        public int HeroId { get; set; }

        [JsonPropertyName("gamesPlayed")]
        public int GamesPlayed { get; set; }

        [JsonPropertyName("wins")]
        public int Wins { get; set; }

        [JsonPropertyName("losses")]
        public int Losses { get; set; }

        [JsonPropertyName("winRate")]
        public double WinRate { get; set; }

        [JsonPropertyName("averageKills")]
        public double AverageKills { get; set; }

        [JsonPropertyName("averageDeaths")]
        public double AverageDeaths { get; set; }

        [JsonPropertyName("averageAssists")]
        public double AverageAssists { get; set; }
    }
}