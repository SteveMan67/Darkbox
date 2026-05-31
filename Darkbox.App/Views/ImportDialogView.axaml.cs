using Avalonia;
using Avalonia.Controls;
using Darkbox.Core.Interfaces;
using Darkbox.Core.Services;
using Darkbox.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Darkbox.Views;

public partial class ImportDialogView : UserControl
{
    public ImportDialogView()
    {
        InitializeComponent();
    }

    private void ImageContainer_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control && control.DataContext is PhotoPreviewViewModel vm)
        {
            var importService = App.Services?.GetRequiredService<IImportService>();

            if (importService != null)
            {
                _ = vm.LoadPreviewAsync(importService);
            }
        }
    }
}