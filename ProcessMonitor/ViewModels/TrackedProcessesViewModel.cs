using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ProcessMonitor.Commands;
using ProcessMonitor.Models;
using ProcessMonitor.Services;

namespace ProcessMonitor.ViewModels;

public class TrackedProcessesViewModel : ViewModelBase
{
    private readonly ProcessTrackingService _trackingService;
    private Timer? _refreshTimer;

    public ObservableCollection<TrackedProcess> TrackedProcesses { get; }
    public ICommand StopTrackingCommand { get; }
    public ICommand RefreshCommand { get; }

    public TrackedProcessesViewModel(ProcessTrackingService trackingService)
    {
        _trackingService = trackingService;
        TrackedProcesses = [];

        StopTrackingCommand = new RelayCommand<TrackedProcess>(StopTracking);
        RefreshCommand = new RelayCommand(_ => LoadTrackedProcesses());

        _trackingService.ProcessTracked += OnProcessTracked;
        _trackingService.ProcessUntracked += OnProcessUntracked;
        _trackingService.ProcessTerminated += OnProcessTerminated;

        LoadTrackedProcesses();
        StartAutoRefresh();
    }

    private void LoadTrackedProcesses()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            TrackedProcesses.Clear();
            foreach (
                var tracked in _trackingService
                    .GetTrackedProcesses()
                    .OrderByDescending(t => t.TrackingStartTime)
            )
            {
                TrackedProcesses.Add(tracked);
            }
        });
    }

    private void StopTracking(TrackedProcess? trackedProcess)
    {
        if (trackedProcess?.IsActive == true)
        {
            _trackingService.StopTracking(trackedProcess.ProcessId);
        }
    }

    private void OnProcessTracked(object? sender, TrackedProcess e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (TrackedProcesses.All(tp => tp.ProcessId != e.ProcessId))
            {
                TrackedProcesses.Insert(0, e);
            }
        });
    }

    private void OnProcessUntracked(object? sender, TrackedProcess e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = TrackedProcesses.FirstOrDefault(tp => tp.ProcessId == e.ProcessId);
            if (existing == null)
                return;
            var index = TrackedProcesses.IndexOf(existing);
            TrackedProcesses[index] = e;
        });
    }

    private void OnProcessTerminated(object? sender, TrackedProcess e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var existing = TrackedProcesses.FirstOrDefault(tp => tp.ProcessId == e.ProcessId);
            if (existing == null)
                return;
            var index = TrackedProcesses.IndexOf(existing);
            TrackedProcesses[index] = e;
        });
    }

    private void StartAutoRefresh()
    {
        _refreshTimer = new Timer(_ => LoadTrackedProcesses(), null, 1000, 1000);
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
        _trackingService.ProcessTracked -= OnProcessTracked;
        _trackingService.ProcessUntracked -= OnProcessUntracked;
        _trackingService.ProcessTerminated -= OnProcessTerminated;
    }
}
