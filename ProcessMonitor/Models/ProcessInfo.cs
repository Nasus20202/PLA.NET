﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ProcessMonitor.Models;

public class ProcessInfo : INotifyPropertyChanged
{
    private bool _isTracked;
    private DateTime? _trackedSince;

    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public long WorkingSet { get; set; }
    public long PrivateMemory { get; set; }
    public ProcessPriorityClass Priority { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public DateTime StartTime { get; init; }
    public string? MainWindowTitle { get; init; }

    public bool IsTracked
    {
        get => _isTracked;
        set
        {
            if (_isTracked != value)
            {
                _isTracked = value;
                OnPropertyChanged();
            }
        }
    }

    public DateTime? TrackedSince
    {
        get => _trackedSince;
        set
        {
            if (_trackedSince != value)
            {
                _trackedSince = value;
                OnPropertyChanged();
            }
        }
    }

    public List<ProcessThread> Threads { get; init; } = [];
    public List<ProcessModule> Modules { get; init; } = [];

    public string WorkingSetMB => $"{WorkingSet / (1024.0 * 1024.0):F2} MB";
    public string PrivateMemoryMB => $"{PrivateMemory / (1024.0 * 1024.0):F2} MB";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
