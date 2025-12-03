using System;
using System.Collections.Generic;

namespace ProcessMonitor.Models;

public class TrackedProcess
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public DateTime TrackingStartTime { get; set; }
    public DateTime? TrackingEndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public List<MemorySample> MemorySamples { get; set; } = new();

    public string Status => IsActive ? "Active" : "Terminated";
    public TimeSpan TrackingDuration => (TrackingEndTime ?? DateTime.Now) - TrackingStartTime;
}

public class MemorySample
{
    public DateTime Timestamp { get; set; }
    public long WorkingSet { get; set; }
    public long PrivateMemory { get; set; }
}
