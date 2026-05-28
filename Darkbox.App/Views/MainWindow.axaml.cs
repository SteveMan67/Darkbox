using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Darkbox.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TopBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if     (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)  
        {
            this.BeginMoveDrag(e);
        } 
    }
    
    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}