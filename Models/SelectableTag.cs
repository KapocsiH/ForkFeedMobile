using CommunityToolkit.Mvvm.ComponentModel;

namespace ForkFeedMobile.Models;

public partial class SelectableTag : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
