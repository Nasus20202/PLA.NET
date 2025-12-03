using System.Windows;
using ProcessMonitor.ViewModels;

namespace ProcessMonitor.Views;

public partial class TrackedProcessesWindow : Window
{
    public TrackedProcessesWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(System.EventArgs e)
    {
        if (DataContext is TrackedProcessesViewModel viewModel)
        {
            viewModel.Dispose();
        }
        base.OnClosed(e);
    }
}
