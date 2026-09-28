using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DotaAnalytics.Wpf.Services;

namespace DotaAnalytics.Wpf.Pages
{
    public partial class SettingsPage : UserControl
    {
        private readonly IDotaAnalyticsService _apiService;
        private bool _isProcessing = false;

        public SettingsPage(IDotaAnalyticsService apiService)
        {
            InitializeComponent();
            _apiService = apiService;
        }

        private async void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isProcessing) return;

            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить все данные из базы?\n\nЭто действие необратимо. Все матчи и статистика игроков будут удалены.\n\nРекомендуется после очистки сразу загрузить свежие данные.",
                "Подтверждение очистки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await ExecuteOperationAsync(" Очистка базы данных...", () => _apiService.ClearDatabaseAsync());
            }
        }

        private async void FetchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isProcessing) return;

            var result = MessageBox.Show(
                "Загрузка свежих данных из OpenDota API займёт 30-60 секунд.\n\nВо время загрузки не закрывайте приложение. Вы хотите продолжить?",
                "Подтверждение загрузки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                await ExecuteOperationAsync(" Загрузка данных из OpenDota (30-60 сек)...", () => _apiService.FetchFreshDataAsync());
            }
        }

        private async Task ExecuteOperationAsync(string loadingText, Func<Task<string>> operation)
        {
            _isProcessing = true;
            ClearBtn.IsEnabled = false;
            FetchBtn.IsEnabled = false;
            StatusText.Text = loadingText;
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0)); 

            try
            {
                var result = await operation();
                StatusText.Text = result;
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(126, 217, 87)); 
            }
            catch (Exception ex)
            {
                StatusText.Text = $" Критическая ошибка: {ex.Message}";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)); 
            }
            finally
            {
                _isProcessing = false;
                ClearBtn.IsEnabled = true;
                FetchBtn.IsEnabled = true;
            }
        }
    }
}