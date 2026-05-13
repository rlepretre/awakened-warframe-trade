using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using GameOcrOverlay.Configuration;
using GameOcrOverlay.Services;

namespace GameOcrOverlay;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly AppSettings _settings;
    private readonly OverlayCoordinator _coordinator;
    private readonly ApiClient _apiClient;
    private readonly HotkeyService _hotkeyService;
    private string _statusText = "Ready. Move your cursor over game text and press the hotkey.";

    public MainWindow(AppSettings settings, OverlayCoordinator coordinator, ApiClient apiClient)
    {
        InitializeComponent();

        _settings = settings;
        _coordinator = coordinator;
        _apiClient = apiClient;
        HotkeyText = $"Hotkey: {settings.Hotkey.DisplayName}";
        Platform = settings.MarketAccount.Platform;
        Crossplay = settings.MarketAccount.Crossplay;
        LanguageCode = settings.MarketAccount.Language;
        DataContext = this;
        AccessTokenBox.Password = settings.MarketAccount.AccessToken;
        SelectComboItem(PlatformBox, Platform);
        CrossplayBox.IsChecked = Crossplay;
        SelectComboItem(LanguageBox, LanguageCode);

        _hotkeyService = new HotkeyService(this, settings.Hotkey);
        _hotkeyService.Pressed += async (_, _) => await RunCaptureAsync();

        Loaded += (_, _) => _hotkeyService.Register();
        Closed += (_, _) => _hotkeyService.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string HotkeyText { get; }

    public string Platform { get; set; } = "pc";

    public bool Crossplay { get; set; } = true;

    public string LanguageCode { get; set; } = "en";

    public string StatusText
    {
        get => _statusText;
        private set
        {
            _statusText = value;
            OnPropertyChanged();
        }
    }

    private void SaveAccount_Click(object sender, RoutedEventArgs e)
    {
        SaveAccountSettings();
        StatusText = "Account settings saved.";
    }

    private async void TestAccount_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveAccountSettings();
            StatusText = "Testing account...";
            var result = await _apiClient.GetCurrentAccountAsync();
            StatusText = result.Body;
        }
        catch (Exception ex)
        {
            StatusText = ExceptionFormatter.GetInnermostMessage(ex);
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private async Task RunCaptureAsync()
    {
        try
        {
            StatusText = "Capturing region and running OCR...";
            OverlayCaptureResult result = await _coordinator.CaptureAndLookupAsync();
            StatusText = result.StatusMessage;
        }
        catch (Exception ex)
        {
            StatusText = ExceptionFormatter.GetInnermostMessage(ex);
        }
    }

    private void SaveAccountSettings()
    {
        _settings.MarketAccount.AccessToken = AccessTokenBox.Password.Trim();
        _settings.MarketAccount.Platform = GetSelectedValue(PlatformBox, Platform);
        _settings.MarketAccount.Crossplay = CrossplayBox.IsChecked == true;
        _settings.MarketAccount.Language = GetSelectedValue(LanguageBox, LanguageCode);
        _settings.Save();

        Platform = _settings.MarketAccount.Platform;
        Crossplay = _settings.MarketAccount.Crossplay;
        LanguageCode = _settings.MarketAccount.Language;
    }

    private static string GetSelectedValue(System.Windows.Controls.ComboBox comboBox, string fallback)
    {
        return comboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item
            ? item.Content?.ToString() ?? fallback
            : fallback;
    }

    private static void SelectComboItem(System.Windows.Controls.ComboBox comboBox, string value)
    {
        foreach (object item in comboBox.Items)
        {
            if (item is System.Windows.Controls.ComboBoxItem comboBoxItem
                && string.Equals(comboBoxItem.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedItem = comboBoxItem;
                return;
            }
        }

        comboBox.SelectedIndex = 0;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
