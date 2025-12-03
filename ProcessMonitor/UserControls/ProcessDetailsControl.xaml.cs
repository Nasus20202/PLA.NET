using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ProcessMonitor.UserControls;

public partial class ProcessDetailsControl : UserControl
{
    public static readonly DependencyProperty ThreadsProperty = DependencyProperty.Register(
        nameof(Threads),
        typeof(ObservableCollection<ProcessThread>),
        typeof(ProcessDetailsControl),
        new PropertyMetadata(null)
    );

    public static readonly DependencyProperty ModulesProperty = DependencyProperty.Register(
        nameof(Modules),
        typeof(ObservableCollection<ProcessModule>),
        typeof(ProcessDetailsControl),
        new PropertyMetadata(null)
    );

    public ObservableCollection<ProcessThread> Threads
    {
        get => (ObservableCollection<ProcessThread>)GetValue(ThreadsProperty);
        set => SetValue(ThreadsProperty, value);
    }

    public ObservableCollection<ProcessModule> Modules
    {
        get => (ObservableCollection<ProcessModule>)GetValue(ModulesProperty);
        set => SetValue(ModulesProperty, value);
    }

    public ProcessDetailsControl()
    {
        InitializeComponent();
    }
}
