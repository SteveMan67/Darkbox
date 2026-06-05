using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Core.Domain;
using Darkbox.ViewModels;

namespace Darkbox.Modules.Library.ViewModels;

public partial class PhotoShootViewModel : ViewModelBase
{
    public ObservableCollection<PhotoPreviewViewModel> Photos { get; }

    [ObservableProperty] private string _name;

    [ObservableProperty] private bool _isExpanded = true;

    [ObservableProperty] private bool _isAllSelected;
    
    public event Action<PhotoPreviewViewModel>? PhotoSelected;
    
    public PhotoShootViewModel(Shoot shoot)
    {
        var photoViewModels = shoot.Photos
            .Select(p => new PhotoPreviewViewModel(p))
            .ToList();
        _name = GetFormattedDateRange(shoot);
        Photos = new ObservableCollection<PhotoPreviewViewModel>(photoViewModels);

        foreach (var photo in Photos)
            photo.Selected += p => PhotoSelected?.Invoke(p);

    }

    private string GetFormattedDateRange(Shoot shoot)
    {
        var dateStart = shoot.DateStart;
        var dateEnd = shoot.DateEnd;
        
        if (dateStart.Date == dateEnd.Date)
        {
            return $"{dateStart.ToString("MMM dd, yyyy hh:mm tt")}-{dateEnd.ToString("hh:mm tt")}";
        }

        return $"{dateStart.ToString("MMM dd, yyyy hh:mm tt")} - {dateEnd.ToString("MMM dd, yyyy hh:mm tt")}";
    }
}

