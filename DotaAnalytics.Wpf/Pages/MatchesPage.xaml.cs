using System.Windows;
using System.Windows.Controls;
using DotaAnalytics.Wpf.Services;

namespace DotaAnalytics.Wpf.Pages
{
    public partial class MatchesPage : UserControl
    {
        private readonly IDotaAnalyticsService _apiService;
        private int _currentPage = 1;
        private int _totalPages = 1;

        public MatchesPage(IDotaAnalyticsService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
            this.Loaded += MatchesPage_Loaded;
        }

        private async void MatchesPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMatchesAsync();
        }

        private async Task LoadMatchesAsync()
        {
            try
            {
                Console.WriteLine($"[DEBUG] Загрузка страницы {_currentPage}...");

                var response = await _apiService.GetMatchesAsync(_currentPage, 10);

                Console.WriteLine($"[DEBUG] Получено матчей: {response.Items?.Count ?? 0}");
                Console.WriteLine($"[DEBUG] TotalCount: {response.TotalCount}");
                Console.WriteLine($"[DEBUG] TotalPages: {response.TotalPages}");

                if (response.Items != null && response.Items.Count > 0)
                {
                    MatchesDataGrid.ItemsSource = response.Items;
                    Console.WriteLine("[DEBUG] Данные привязаны к DataGrid");
                }
                else
                {
                    Console.WriteLine("[WARNING] Items пуст или null!");
                }

                _totalPages = response.TotalPages > 0 ? response.TotalPages : 1;
                PageNumberText.Text = $"Страница {_currentPage} из {_totalPages}";
                BtnPrev.IsEnabled = _currentPage > 1;
                BtnNext.IsEnabled = _currentPage < _totalPages;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка загрузки: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
            }
        }

        private async void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadMatchesAsync();
            }
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                await LoadMatchesAsync();
            }
        }
    }
}