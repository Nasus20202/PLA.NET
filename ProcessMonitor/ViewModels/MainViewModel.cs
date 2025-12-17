using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ProcessMonitor.Commands;
using ProcessMonitor.Models;
using ProcessMonitor.Services;
using ProcessMonitor.Views;

namespace ProcessMonitor.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly ProcessService _processService;
    private readonly ProcessTrackingService _trackingService;
    private Timer? _autoRefreshTimer;
    private ProcessInfo? _selectedProcess;
    private string _filterText = string.Empty;
    private string _sortColumn = "Name";
    private ListSortDirection _sortDirection = ListSortDirection.Ascending;
    private bool _isAutoRefreshEnabled = false;
    private int _refreshInterval = 2000;
    private bool _isRefreshing = false;

    public ObservableCollection<ProcessInfo> Processes { get; }
    public ICollectionView ProcessesView { get; }
    public ObservableCollection<ProcessThread> Threads { get; }
    public ObservableCollection<ProcessModule> Modules { get; }

    public ICommand RefreshCommand { get; }
    public ICommand ToggleAutoRefreshCommand { get; }
    public ICommand KillProcessCommand { get; }
    public ICommand ChangePriorityCommand { get; }
    public ICommand ToggleTrackingCommand { get; }
    public ICommand ShowTrackedProcessesCommand { get; }
    public ICommand SortCommand { get; }

    public MainViewModel()
    {
        _processService = new ProcessService();
        _trackingService = new ProcessTrackingService(_processService);

        Processes = new ObservableCollection<ProcessInfo>();
        ProcessesView = CollectionViewSource.GetDefaultView(Processes);
        ProcessesView.Filter = FilterProcesses;
        ProcessesView.SortDescriptions.Add(
            new SortDescription("Name", ListSortDirection.Ascending)
        );

        Threads = new ObservableCollection<ProcessThread>();
        Modules = new ObservableCollection<ProcessModule>();

        RefreshCommand = new AsyncRelayCommand(
            async _ => await RefreshProcesses(),
            _ => !IsRefreshing
        );
        ToggleAutoRefreshCommand = new RelayCommand(_ => ToggleAutoRefresh());
        KillProcessCommand = new AsyncRelayCommand(
            async _ => await KillSelectedProcess(),
            _ => SelectedProcess != null
        );
        ChangePriorityCommand = new AsyncRelayCommand<ProcessPriorityClass?>(
            async p => await ChangePriority(p),
            _ => SelectedProcess != null
        );
        ToggleTrackingCommand = new RelayCommand(
            _ => ToggleTracking(),
            _ => SelectedProcess != null
        );
        ShowTrackedProcessesCommand = new RelayCommand(_ => ShowTrackedProcessesWindow());
        SortCommand = new RelayCommand<string>(ApplySort);

        _ = RefreshProcesses();
    }

    public ProcessInfo? SelectedProcess
    {
        get => _selectedProcess;
        set
        {
            if (SetProperty(ref _selectedProcess, value))
            {
                LoadProcessDetails();
            }
        }
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                ProcessesView.Refresh();
            }
        }
    }

    public bool IsAutoRefreshEnabled
    {
        get => _isAutoRefreshEnabled;
        set => SetProperty(ref _isAutoRefreshEnabled, value);
    }

    public int RefreshInterval
    {
        get => _refreshInterval;
        set
        {
            if (!SetProperty(ref _refreshInterval, value))
                return;
            if (!IsAutoRefreshEnabled)
                return;
            StopAutoRefresh();
            StartAutoRefresh();
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public int SamplingInterval
    {
        get => _trackingService.SamplingInterval;
        set
        {
            _trackingService.SamplingInterval = value;
            OnPropertyChanged();
        }
    }

    private bool FilterProcesses(object obj)
    {
        if (obj is not ProcessInfo process)
            return false;

        if (string.IsNullOrWhiteSpace(FilterText))
            return true;

        return process.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
            || process.Id.ToString().Contains(FilterText);
    }

    private void ApplySort(string? columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            return;

        if (_sortColumn == columnName)
        {
            _sortDirection =
                _sortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
        }
        else
        {
            _sortColumn = columnName;
            _sortDirection = ListSortDirection.Ascending;
        }

        ProcessesView.SortDescriptions.Clear();
        ProcessesView.SortDescriptions.Add(new SortDescription(_sortColumn, _sortDirection));
    }

    private async Task RefreshProcesses()
    {
        IsRefreshing = true;

        try
        {
            var processes = await _processService.GetProcessesAsync();
            var trackedProcessIds = _trackingService
                .GetTrackedProcesses()
                .Where(tp => tp.IsActive)
                .Select(tp => tp.ProcessId)
                .ToHashSet();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var selectedId = SelectedProcess?.Id;

                Processes.Clear();
                foreach (var process in processes)
                {
                    process.IsTracked = trackedProcessIds.Contains(process.Id);
                    if (process.IsTracked)
                    {
                        var tracked = _trackingService.GetTrackedProcess(process.Id);
                        process.TrackedSince = tracked?.TrackingStartTime;
                    }
                    Processes.Add(process);
                }

                if (selectedId.HasValue)
                {
                    SelectedProcess = Processes.FirstOrDefault(p => p.Id == selectedId.Value);
                }

                IsRefreshing = false;
            });
        }
        catch (UnauthorizedAccessException)
        {
            IsRefreshing = false;
            MessageBox.Show(
                "Access denied while refreshing processes.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        catch (InvalidOperationException ex)
        {
            IsRefreshing = false;
            MessageBox.Show(
                $"Failed to refresh processes: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void LoadProcessDetails()
    {
        Threads.Clear();
        Modules.Clear();

        if (SelectedProcess == null)
            return;

        _ = LoadProcessDetailsAsync();
    }

    private async Task LoadProcessDetailsAsync()
    {
        try
        {
            var details = await _processService.GetProcessDetailsAsync(SelectedProcess?.Id ?? -1);
            if (details != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var thread in details.Threads)
                    {
                        Threads.Add(thread);
                    }

                    foreach (var module in details.Modules)
                    {
                        Modules.Add(module);
                    }
                });
            }
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show(
                "Access denied while loading process details.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
        catch (InvalidOperationException)
        {
            MessageBox.Show(
                "Failed to load process details. The process may have exited.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void ToggleAutoRefresh()
    {
        if (IsAutoRefreshEnabled)
        {
            StopAutoRefresh();
        }
        else
        {
            StartAutoRefresh();
        }
        IsAutoRefreshEnabled = !IsAutoRefreshEnabled;
    }

    private void StartAutoRefresh()
    {
        _autoRefreshTimer = new Timer(
            _ => RefreshProcesses(),
            null,
            RefreshInterval,
            RefreshInterval
        );
    }

    private void StopAutoRefresh()
    {
        _autoRefreshTimer?.Dispose();
        _autoRefreshTimer = null;
    }

    private async Task KillSelectedProcess()
    {
        if (SelectedProcess == null)
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to kill process '{SelectedProcess.Name}' (PID: {SelectedProcess.Id})?",
            "Confirm Kill Process",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (result == MessageBoxResult.Yes)
        {
            var success = await _processService.KillProcessAsync(SelectedProcess.Id);
            if (success)
            {
                MessageBox.Show(
                    "Process killed successfully.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                await RefreshProcesses();
            }
            else
            {
                MessageBox.Show(
                    "Failed to kill process. Access denied or process already terminated.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }

    private async Task ChangePriority(ProcessPriorityClass? priority)
    {
        if (SelectedProcess == null || !priority.HasValue)
            return;

        var success = await _processService.ChangePriorityAsync(SelectedProcess.Id, priority.Value);
        if (success)
        {
            MessageBox.Show(
                "Process priority changed successfully.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
            await RefreshProcesses();
        }
        else
        {
            MessageBox.Show(
                "Failed to change process priority. Access denied.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void ToggleTracking()
    {
        if (SelectedProcess == null)
            return;

        // Find the process in the collection
        var processInList = Processes.FirstOrDefault(p => p.Id == SelectedProcess.Id);

        if (SelectedProcess.IsTracked)
        {
            _trackingService.StopTracking(SelectedProcess.Id);
            SelectedProcess.IsTracked = false;
            SelectedProcess.TrackedSince = null;
            if (processInList != null)
            {
                processInList.IsTracked = false;
                processInList.TrackedSince = null;
            }
        }
        else
        {
            if (_trackingService.StartTracking(SelectedProcess.Id, SelectedProcess.Name))
            {
                var now = DateTime.Now;
                SelectedProcess.IsTracked = true;
                SelectedProcess.TrackedSince = now;
                if (processInList != null)
                {
                    processInList.IsTracked = true;
                    processInList.TrackedSince = now;
                }
            }
        }

        OnPropertyChanged(nameof(SelectedProcess));
    }

    private void ShowTrackedProcessesWindow()
    {
        var window = new TrackedProcessesWindow
        {
            DataContext = new TrackedProcessesViewModel(_trackingService),
        };
        window.Show();
    }

    public void Cleanup()
    {
        StopAutoRefresh();
        _trackingService.Dispose();
    }
}
