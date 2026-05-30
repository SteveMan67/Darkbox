using Avalonia.Controls;
using Darkbox.Core.Services;
using Darkbox.ViewModels;

namespace Darkbox.Views;

public partial class ImportDialogView : UserControl
{
    public ImportDialogView()
    {
        InitializeComponent();

        var fileSystemService = new FileSystemService();
        DataContext = new ImportDialogViewModel(fileSystemService);
    }
}