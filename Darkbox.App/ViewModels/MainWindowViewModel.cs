using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Modules.Library.ViewModels;

namespace Darkbox.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty] private bool _isImportDialogOpen = true;

    public LibraryViewModel Library { get; }

    public MainWindowViewModel(LibraryViewModel library)
    {
        Library = library;
        _currentView = library;
        _ = library.LoadAsync();
    }
    
}