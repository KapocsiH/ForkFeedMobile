namespace ForkFeedMobile.Controls;

public partial class AvatarView : ContentView
{
    private static readonly Color[] AvatarColors =
    [
        Color.FromArgb("#E53935"),
        Color.FromArgb("#8E24AA"),
        Color.FromArgb("#3949AB"),
        Color.FromArgb("#039BE5"),
        Color.FromArgb("#00897B"),
        Color.FromArgb("#43A047"),
        Color.FromArgb("#FF6B35"),
        Color.FromArgb("#6D4C41"),
        Color.FromArgb("#546E7A"),
        Color.FromArgb("#D81B60"),
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
