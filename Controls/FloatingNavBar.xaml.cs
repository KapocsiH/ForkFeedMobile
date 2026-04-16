using Microsoft.Maui.Controls.Shapes;
using ShapePath = Microsoft.Maui.Controls.Shapes.Path;

namespace ForkFeedMobile.Controls;

public partial class FloatingNavBar : ContentView
{
    private static readonly string[] Routes = { "Home", "ShoppingList", "AddRecipe", "Favorites", "Profile" };

    private Ellipse[]? _glows;
    private ShapePath[]? _icons;
    private int _selectedIndex = -1;

    public FloatingNavBar()
    {
        InitializeComponent();
    }

    private void EnsureElements()
    {
        if (_glows != null) return;

        _glows = new[]
        {
            this.FindByName<Ellipse>("Glow0"),
            this.FindByName<Ellipse>("Glow1"),
            this.FindByName<Ellipse>("Glow2"),
            this.FindByName<Ellipse>("Glow3"),
            this.FindByName<Ellipse>("Glow4")
        };
        _icons = new[]
        {
            this.FindByName<ShapePath>("Icon0"),
            this.FindByName<ShapePath>("Icon1"),
            this.FindByName<ShapePath>("Icon2"),
            this.FindByName<ShapePath>("Icon3"),
            this.FindByName<ShapePath>("Icon4")
        };
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler != null && Shell.Current != null)
        {
            Shell.Current.Navigated -= OnShellNavigated;
            Shell.Current.Navigated += OnShellNavigated;

            if (Application.Current != null)
                Application.Current.RequestedThemeChanged -= OnThemeChanged;
            if (Application.Current != null)
                Application.Current.RequestedThemeChanged += OnThemeChanged;

            UpdateFromRoute();
        }
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        // Force re-apply icon colors for the new theme
        var prev = _selectedIndex;
        _selectedIndex = -1;
        SetSelected(prev);
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateFromRoute);
    }

    private void UpdateFromRoute()
    {
        var location = Shell.Current?.CurrentState?.Location?.ToString();
        if (string.IsNullOrEmpty(location)) return;

        for (int i = 0; i < Routes.Length; i++)
        {
            if (location.Contains(Routes[i], StringComparison.OrdinalIgnoreCase))
            {
                SetSelected(i);
                return;
            }
        }
    }

    private void SetSelected(int index)
    {
        if (_selectedIndex == index) return;
        _selectedIndex = index;

        EnsureElements();
        if (_glows == null || _icons == null) return;

        var appTheme = Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
        if (appTheme == AppTheme.Unspecified)
            appTheme = Application.Current?.RequestedTheme ?? AppTheme.Light;
        bool isDark = appTheme == AppTheme.Dark;

        var activeBrush = new SolidColorBrush(isDark ? Colors.White : Color.FromArgb("#1A1A2E"));
        var inactiveBrush = new SolidColorBrush(isDark ? Color.FromArgb("#55FFFFFF") : Color.FromArgb("#55000000"));

        for (int i = 0; i < 5; i++)
        {
            _glows[i].IsVisible = i == index;
            _icons[i].Fill = i == index ? activeBrush : inactiveBrush;
        }
    }

    private async void OnTabTapped(object? sender, TappedEventArgs e)
    {
        if (sender is View view && int.TryParse(view.ClassId, out var index))
        {
            if (index == _selectedIndex) return;
            SetSelected(index);

            try
            {
                await Shell.Current.GoToAsync($"//{Routes[index]}");
            }
            catch
            {
                // Navigation may fail if already on the route
            }
        }
    }
}
