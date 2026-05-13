using System.Windows;
using System.Windows.Interop;
using GameOcrOverlay.Configuration;
using GameOcrOverlay.Services;

namespace GameOcrOverlay.Windows;

public partial class OverlayWindow : Window
{
    private readonly OverlaySettings _settings;
    private string _currentItemId = "";
    private string _currentItemName = "";
    private string _currentItemSlug = "";
    private int? _currentMaxRank;
    private bool _isUpdatingRankFilter;

    public OverlayWindow(OverlaySettings settings)
    {
        InitializeComponent();
        _settings = settings;
        MinWidth = Math.Min(settings.Width, MaxWidth);

        SourceInitialized += (_, _) => MakeToolWindow();
        Deactivated += (_, _) =>
        {
            if (IsVisible && !IsKeyboardFocusWithin)
            {
                Hide();
            }
        };
        Hide();
    }

    public Func<ListingOrderRequest, Task<string>>? CreateOrderAsync { get; set; }

    public Func<string, int?, int?, Task<MarketTopOrdersResult>>? RefreshOrdersAsync { get; set; }

    public void ShowResult(Point cursorPosition, string title, string body)
    {
        ShowContent(cursorPosition, title, body, showListingForm: false);
    }

    public void ShowListingForm(
        Point cursorPosition,
        string title,
        string body,
        string itemId,
        string itemName,
        string itemSlug,
        int? maxRank,
        int? suggestedPrice,
        IReadOnlyList<MarketOrderRow> sellOrders)
    {
        _currentItemId = itemId;
        _currentItemName = itemName;
        _currentItemSlug = itemSlug;
        _currentMaxRank = maxRank;
        PriceBox.Text = suggestedPrice?.ToString() ?? "";
        QuantityBox.Text = "1";
        RankBox.Text = "0";
        VisibleBox.IsChecked = true;
        FormStatusText.Text = suggestedPrice is null
            ? "No sell price found. Enter a price before confirming."
            : $"Price prefilled from cheapest sell order: {suggestedPrice}p.";
        ConfirmButton.IsEnabled = true;
        PopulateRankFilter(maxRank);
        ApplyOrders(sellOrders, suggestedPrice);

        ShowContent(cursorPosition, title, body, showListingForm: true);
    }

    private void ShowContent(Point cursorPosition, string title, string body, bool showListingForm)
    {
        TitleText.Text = title;
        BodyText.Text = Limit(body, 700);
        ListingForm.Visibility = showListingForm ? Visibility.Visible : Visibility.Collapsed;
        FormActions.Visibility = showListingForm ? Visibility.Visible : Visibility.Collapsed;
        if (!showListingForm)
        {
            OrdersItems.ItemsSource = null;
            OrdersPanel.Visibility = Visibility.Collapsed;
            _currentItemSlug = "";
            _currentMaxRank = null;
        }
        BodyScrollViewer.VerticalScrollBarVisibility = BodyText.Text.Length > 500
            ? System.Windows.Controls.ScrollBarVisibility.Auto
            : System.Windows.Controls.ScrollBarVisibility.Disabled;
        Left = cursorPosition.X + _settings.OffsetX;
        Top = cursorPosition.Y + _settings.OffsetY;
        Show();
        Activate();
    }

    private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (CreateOrderAsync is null)
        {
            FormStatusText.Text = "Order creation is not configured.";
            return;
        }

        if (!TryReadOrder(out ListingOrderRequest? request, out string error))
        {
            FormStatusText.Text = error;
            return;
        }

        try
        {
            ConfirmButton.IsEnabled = false;
            FormStatusText.Text = "Creating order...";
            string message = await CreateOrderAsync(request!);
            FormStatusText.Text = message;
        }
        catch (Exception ex)
        {
            FormStatusText.Text = ExceptionFormatter.GetInnermostMessage(ex);
        }
        finally
        {
            ConfirmButton.IsEnabled = true;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private async void OrderRankFilterBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isUpdatingRankFilter || RefreshOrdersAsync is null || string.IsNullOrWhiteSpace(_currentItemSlug))
        {
            return;
        }

        try
        {
            FormStatusText.Text = "Refreshing orders...";
            int? rankFilter = OrderRankFilterBox.SelectedItem is RankFilterOption option ? option.Rank : null;
            MarketTopOrdersResult orders = await RefreshOrdersAsync(_currentItemSlug, _currentMaxRank, rankFilter);
            ApplyOrders(orders.SellOrders, orders.CheapestSellPrice);
            FormStatusText.Text = orders.CheapestSellPrice is null
                ? "No sell price found for this rank. Enter a price before confirming."
                : $"Price prefilled from cheapest sell order: {orders.CheapestSellPrice}p.";
        }
        catch (Exception ex)
        {
            FormStatusText.Text = ExceptionFormatter.GetInnermostMessage(ex);
        }
    }

    private bool TryReadOrder(out ListingOrderRequest? request, out string error)
    {
        request = null;
        error = "";

        if (!TryReadPositiveInt(PriceBox.Text, "Price", out int platinum, out error)
            || !TryReadPositiveInt(QuantityBox.Text, "Quantity", out int quantity, out error))
        {
            return false;
        }

        int? rank = null;
        if (!string.IsNullOrWhiteSpace(RankBox.Text))
        {
            if (!int.TryParse(RankBox.Text.Trim(), out int parsedRank) || parsedRank < 0)
            {
                error = "Rank must be zero or greater.";
                return false;
            }

            rank = parsedRank;
        }

        request = new ListingOrderRequest(
            _currentItemId,
            _currentItemName,
            platinum,
            quantity,
            rank,
            VisibleBox.IsChecked == true);
        return true;
    }

    private static bool TryReadPositiveInt(string value, string label, out int result, out string error)
    {
        error = "";
        if (int.TryParse(value.Trim(), out result) && result > 0)
        {
            return true;
        }

        error = $"{label} must be greater than zero.";
        return false;
    }

    private void ApplyOrders(IReadOnlyList<MarketOrderRow> sellOrders, int? suggestedPrice)
    {
        OrdersItems.ItemsSource = sellOrders;
        OrdersPanel.Visibility = sellOrders.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        if (suggestedPrice is not null)
        {
            PriceBox.Text = suggestedPrice.Value.ToString();
        }
    }

    private void PopulateRankFilter(int? maxRank)
    {
        _isUpdatingRankFilter = true;
        try
        {
            OrderRankFilterBox.Items.Clear();
            OrderRankFilterBox.Items.Add(new RankFilterOption("All", null));

            if (maxRank is not null)
            {
                for (int rank = 0; rank <= maxRank.Value; rank++)
                {
                    OrderRankFilterBox.Items.Add(new RankFilterOption(rank.ToString(), rank));
                }
            }

            RankFilterPanel.Visibility = maxRank is null ? Visibility.Collapsed : Visibility.Visible;
            OrderRankFilterBox.SelectedIndex = 0;
        }
        finally
        {
            _isUpdatingRankFilter = false;
        }
    }

    private void MakeToolWindow()
    {
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        int style = NativeMethods.GetWindowLong(hwnd, NativeMethods.GwlExStyle);
        NativeMethods.SetWindowLong(
            hwnd,
            NativeMethods.GwlExStyle,
            style | NativeMethods.WsExLayered | NativeMethods.WsExToolWindow);
    }

    private static string Limit(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "...";
    }

    private sealed record RankFilterOption(string Label, int? Rank)
    {
        public override string ToString()
        {
            return Label;
        }
    }
}
