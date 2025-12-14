using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ProcessMonitor.Models;

namespace ProcessMonitor.Services;

public class ProcessTrackingService(ProcessService processService)
{
    private readonly ConcurrentDictionary<int, TrackedProcess> _trackedProcesses = new();
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _trackingTasks = new();
    private int _samplingInterval = 1000; // 1 second default

    public event EventHandler<TrackedProcess>? ProcessTracked;
    public event EventHandler<TrackedProcess>? ProcessUntracked;
    public event EventHandler<TrackedProcess>? ProcessTerminated;

    public int SamplingInterval
    {
        get => _samplingInterval;
        set => _samplingInterval = Math.Max(100, value);
    }

    public IEnumerable<TrackedProcess> GetTrackedProcesses()
    {
        return _trackedProcesses.Values;
    }

    public bool StartTracking(int processId, string processName)
    {
        if (_trackedProcesses.ContainsKey(processId))
            return false;

        var trackedProcess = new TrackedProcess
        {
            ProcessId = processId,
            ProcessName = processName,
            TrackingStartTime = DateTime.Now,
            IsActive = true,
        };

        if (!_trackedProcesses.TryAdd(processId, trackedProcess))
            return false;

        var cts = new CancellationTokenSource();
        _trackingTasks[processId] = cts;

        // Start async tracking task
        _ = TrackProcessAsync(processId, cts.Token);

        ProcessTracked?.Invoke(this, trackedProcess);
        return true;
    }

    public bool StopTracking(int processId)
    {
        if (!_trackingTasks.TryRemove(processId, out var cts))
            return false;

        cts.Cancel();
        cts.Dispose();

        if (!_trackedProcesses.TryGetValue(processId, out var tracked))
            return true;
        tracked.TrackingEndTime = DateTime.Now;
        tracked.IsActive = false;
        ProcessUntracked?.Invoke(this, tracked);

        return true;
    }

    public bool IsTracking(int processId)
    {
        return _trackingTasks.ContainsKey(processId);
    }

    public TrackedProcess? GetTrackedProcess(int processId)
    {
        return _trackedProcesses.GetValueOrDefault(processId);
    }

    private async Task TrackProcessAsync(int processId, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!await processService.ProcessExistsAsync(processId))
                {
                    // Process terminated
                    if (_trackedProcesses.TryGetValue(processId, out var tracked))
                    {
                        tracked.TrackingEndTime = DateTime.Now;
                        tracked.IsActive = false;
                        ProcessTerminated?.Invoke(this, tracked);
                    }

                    StopTracking(processId);
                    break;
                }

                // Sample memory
                var memory = await processService.GetProcessMemoryAsync(processId);
                if (_trackedProcesses.TryGetValue(processId, out var trackedProcess))
                {
                    var sample = new MemorySample
                    {
                        Timestamp = DateTime.Now,
                        WorkingSet = memory,
                        PrivateMemory = memory,
                    };

                    trackedProcess.MemorySamples.Add(sample);
                }

                await Task.Delay(_samplingInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping tracking
        }
        catch (Exception)
        {
            // Handle unexpected errors
            StopTracking(processId);
        }
    }

    public void Dispose()
    {
        foreach (var cts in _trackingTasks.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _trackingTasks.Clear();
    }
}
