using System;
using System.Collections.Generic;

namespace ProcessMonitor.Models;

public class TrackedProcess
{
    public int ProcessId { get; init; }
    public string ProcessName { get; set; } = string.Empty;
    public DateTime TrackingStartTime { get; init; }
    public DateTime? TrackingEndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public List<MemorySample> MemorySamples { get; set; } = [];

    public string Status => IsActive ? "Active" : "Terminated";
    public TimeSpan TrackingDuration => (TrackingEndTime ?? DateTime.Now) - TrackingStartTime;

    public long MinMemory => MemorySamples.Count > 0 ? MemorySamples.Min(s => s.WorkingSet) : 0;
    public long MaxMemory => MemorySamples.Count > 0 ? MemorySamples.Max(s => s.WorkingSet) : 0;
    public string MinMemoryMB => $"{MinMemory / (1024.0 * 1024.0):F2} MB";
    public string MaxMemoryMB => $"{MaxMemory / (1024.0 * 1024.0):F2} MB";
}

public class MemorySample
{
    public DateTime Timestamp { get; set; }
    public long WorkingSet { get; set; }
    public long PrivateMemory { get; set; }
}
