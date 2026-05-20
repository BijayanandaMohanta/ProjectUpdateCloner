using System.Windows;
using System.Windows.Input;
using ProjectUpdateCloner.Services;
using ProjectUpdateCloner.ViewModels;

namespace ProjectUpdateCloner.Views;

public partial class MainWindow : Window
{
    private Rect restoreBoundsBeforeCustomMaximize;
    private bool isCustomMaximized;

    public MainWindow()
    {
        App.Log("MainWindow constructor started.");
        InitializeComponent();
        ApplyWorkAreaMaxSize();
        App.Log("MainWindow InitializeComponent completed.");
        DataContext = new MainViewModel(
            new FileScannerService(),
            new UpdatePackageService());
        StateChanged += (_, _) => ApplyWorkAreaMaxSize();
        Loaded += (_, _) => App.Log("MainWindow loaded.");
        ContentRendered += (_, _) => App.Log("MainWindow content rendered.");
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }

        if (e.ButtonState == MouseButtonState.Pressed)
        {
            if (isCustomMaximized)
            {
                RestoreFromCustomMaximize();
            }

            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        isCustomMaximized = false;
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ToggleWindowState()
    {
        if (isCustomMaximized)
        {
            RestoreFromCustomMaximize();
            return;
        }

        MaximizeToWorkArea();
    }

    private void ApplyWorkAreaMaxSize()
    {
        MaxWidth = SystemParameters.WorkArea.Width;
        MaxHeight = SystemParameters.WorkArea.Height;
    }

    private void MaximizeToWorkArea()
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
        }

        restoreBoundsBeforeCustomMaximize = new Rect(Left, Top, Width, Height);
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left;
        Top = workArea.Top;
        Width = workArea.Width;
        Height = workArea.Height;
        isCustomMaximized = true;
    }

    private void RestoreFromCustomMaximize()
    {
        Left = restoreBoundsBeforeCustomMaximize.Left;
        Top = restoreBoundsBeforeCustomMaximize.Top;
        Width = restoreBoundsBeforeCustomMaximize.Width;
        Height = restoreBoundsBeforeCustomMaximize.Height;
        isCustomMaximized = false;
    }
}
