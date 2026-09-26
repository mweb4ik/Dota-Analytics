using DotaAnalytics.Shared.Models;
using DotaAnalytics.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DotaAnalytics.Wpf.Pages
{
    public partial class PlayersPage : UserControl
    {
        private readonly IDotaAnalyticsService _apiService;
        private bool _isInitialLoad = true;

        public PlayersPage(IDotaAnalyticsService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            this.Loaded += PlayersPage_Loaded;
        }

        private async void PlayersPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialLoad)
            {
                _isInitialLoad = false;
                await LoadMatchesForComboBoxAsync();
            }
        }

        private async Task LoadMatchesForComboBoxAsync()
        {
 
            var response = await _apiService.GetMatchesAsync(1, 20);

            if (response.Items != null && response.Items.Count > 0)
            {
                MatchComboBox.ItemsSource = response.Items;
                //First match by default
                MatchComboBox.SelectedIndex = 0;
            }
        }

        private async void MatchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MatchComboBox.SelectedItem is not OpenDotaMatchResponse selectedMatch) return;

            LoadingText.Visibility = Visibility.Visible;
            PlayersDataGrid.ItemsSource = null; 

            var players = await _apiService.GetPlayersByMatchIdAsync(selectedMatch.MatchId);

            PlayersDataGrid.ItemsSource = players;
            LoadingText.Visibility = Visibility.Collapsed;
        }
    }
}