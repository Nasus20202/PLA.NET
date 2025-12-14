using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ProcessMonitor.Models;

public class ProcessInfo
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public long WorkingSet { get; set; }
    public long PrivateMemory { get; set; }
    public ProcessPriorityClass Priority { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public DateTime StartTime { get; init; }
    public string? MainWindowTitle { get; init; }
    public bool IsTracked { get; set; }
    public DateTime? TrackedSince { get; set; }
    public List<ProcessThread> Threads { get; init; } = [];
    public List<ProcessModule> Modules { get; init; } = [];

    public string WorkingSetMB => $"{WorkingSet / (1024.0 * 1024.0):F2} MB";
    public string PrivateMemoryMB => $"{PrivateMemory / (1024.0 * 1024.0):F2} MB";
}
