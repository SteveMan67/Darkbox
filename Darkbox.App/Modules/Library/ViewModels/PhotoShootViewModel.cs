using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Darkbox.ViewModels;

public partial class PhotoShootViewModel : ViewModelBase
{
    public List<PhotoPreviewViewModel> Photos { get; }

    [ObservableProperty] private string _name;

    [ObservableProperty] private string _dateRange;

    [ObservableProperty] private bool _isExpanded;
    
    
    
    public PhotoShootViewModel(string name, string dateRange, List<PhotoPreviewViewModel> photos)
    {
        _name = name;
        _dateRange = dateRange;
        Photos = photos;
    }
}

