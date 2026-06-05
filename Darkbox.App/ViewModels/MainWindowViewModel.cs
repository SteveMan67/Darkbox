using CommunityToolkit.Mvvm.ComponentModel;
using Darkbox.Modules.Library.ViewModels;

namespace Darkbox.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    public LibraryViewModel Library { get; }
    
    [ObservableProperty] private bool _isImportDialogOpen;
    
    public ImportDialogViewModel ImportDialog { get; }

    public MainWindowViewModel(LibraryViewModel library, ImportDialogViewModel importDialog)
    {
        Library = library;
        
        Library.ImportDialogRequested += () =>
        {
            IsImportDialogOpen = true;
        };
        
        ImportDialog = importDialog;

        ImportDialog.ImportDialogCloseRequested += () =>
        {
            IsImportDialogOpen = false;
        };
        
        _currentView = library;
        _ = library.LoadAsync();
    }
    
}