using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Core.Domain;
using Darkbox.ViewModels;

namespace Darkbox.Modules.Library.ViewModels;

public partial class ShootViewModel : ViewModelBase
{
    public Shoot Shoot { get; }
    
    [ObservableProperty] private bool _isExpanded = true;
    [ObservableProperty] private bool _isSelected;

    public string Name => Shoot.Name;
    public int PhotoCount => Shoot.Photos.Count;

    public int Id => Shoot.Id;
    public string DateRange => $"{Shoot.DateStart:MMM d, yyyy h:mm tt} - {Shoot.DateEnd:MMM d, yyyy h:mm tt}";
    

    public ShootViewModel(Shoot shoot)
    {
        Shoot = shoot;
    }
}