using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ProcessMonitor.ViewModels;

namespace ProcessMonitor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Cleanup();
        }
        Application.Current.Shutdown();
    }

    private void ColumnHeader_Click(object sender, RoutedEventArgs e)
    {
        if (sender is DataGridColumnHeader header && header.Content is string columnName)
        {
            var viewModel = DataContext as MainViewModel;
            viewModel?.SortCommand.Execute(columnName);
        }
    }

    private void PriorityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (
            sender is ComboBox comboBox
            && comboBox.SelectedItem is ComboBoxItem item
            && item.Tag is string tag
        )
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null && Enum.TryParse<ProcessPriorityClass>(tag, out var priority))
            {
                viewModel.ChangePriorityCommand.Execute(priority);
            }
        }
    }
}
