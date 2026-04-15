namespace ForkFeedMobile.Controls;

public partial class AvatarView : ContentView
{
    // A palette of Material-Design-inspired colors for placeholders
    private static readonly Color[] AvatarColors =
    [
        Color.FromArgb("#E53935"), // Red
        Color.FromArgb("#8E24AA"), // Purple
        Color.FromArgb("#3949AB"), // Indigo
        Color.FromArgb("#039BE5"), // Light Blue
        Color.FromArgb("#00897B"), // Teal
        Color.FromArgb("#43A047"), // Green
        Color.FromArgb("#FF6B35"), // Orange (brand)
        Color.FromArgb("#6D4C41"), // Brown
        Color.FromArgb("#546E7A"), // Blue Grey
        Color.FromArgb("#D81B60"), // Pink
    ];

    public static readonly BindableProperty ImageUrlProperty =
        BindableProperty.Create(nameof(ImageUrl), typeof(string), typeof(AvatarView), string.Empty, propertyChanged: OnAppearanceChanged);

    public static readonly BindableProperty UserNameProperty =
        BindableProperty.Create(nameof(UserName), typeof(string), typeof(AvatarView), string.Empty, propertyChanged: OnAppearanceChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(AvatarView), 36.0, propertyChanged: OnAppearanceChanged);

    public string ImageUrl
    {
        get => (string)GetValue(ImageUrlProperty);
        set => SetValue(ImageUrlProperty, value);
    }

    public string UserName
    {
        get => (string)GetValue(UserNameProperty);
        set => SetValue(UserNameProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    // Computed properties for binding
    public double HalfSize => Size / 2;
    public double FontSize => Size * 0.42;

    public string Initial =>
        string.IsNullOrWhiteSpace(UserName) ? "?" : UserName.Trim()[..1].ToUpperInvariant();

    public bool ShowImage => !string.IsNullOrWhiteSpace(ImageUrl);
    public bool ShowPlaceholder => !ShowImage;

    public Color AvatarBackgroundColor
    {
        get
        {
            if (string.IsNullOrWhiteSpace(UserName))
                return AvatarColors[0];

            var hash = Math.Abs(UserName.GetHashCode());
            return AvatarColors[hash % AvatarColors.Length];
        }
    }

    public AvatarView()
    {
        InitializeComponent();
    }

    private static void OnAppearanceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AvatarView view)
        {
            view.OnPropertyChanged(nameof(ShowImage));
            view.OnPropertyChanged(nameof(ShowPlaceholder));
            view.OnPropertyChanged(nameof(Initial));
            view.OnPropertyChanged(nameof(AvatarBackgroundColor));
            view.OnPropertyChanged(nameof(HalfSize));
            view.OnPropertyChanged(nameof(FontSize));
        }
    }
}
