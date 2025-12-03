using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProcessMonitor.Models;

namespace ProcessMonitor.Services;

public class ProcessService
{
    public List<ProcessInfo> GetProcesses()
    {
        var processes = new List<ProcessInfo>();

        foreach (var process in Process.GetProcesses())
        {
            try
            {
                var processInfo = new ProcessInfo
                {
                    Id = process.Id,
                    Name = process.ProcessName,
                    WorkingSet = process.WorkingSet64,
                    PrivateMemory = process.PrivateMemorySize64,
                    Priority = process.PriorityClass,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    MainWindowTitle = process.MainWindowTitle,
                };

                try
                {
                    processInfo.StartTime = process.StartTime;
                }
                catch
                {
                    processInfo.StartTime = DateTime.MinValue;
                }

                processes.Add(processInfo);
            }
            catch
            {
                // Skip processes we can't access
            }
        }

        return processes;
    }

    public ProcessInfo? GetProcessDetails(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            var processInfo = new ProcessInfo
            {
                Id = process.Id,
                Name = process.ProcessName,
                WorkingSet = process.WorkingSet64,
                PrivateMemory = process.PrivateMemorySize64,
                Priority = process.PriorityClass,
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,
                MainWindowTitle = process.MainWindowTitle,
            };

            try
            {
                processInfo.StartTime = process.StartTime;
            }
            catch
            {
                processInfo.StartTime = DateTime.MinValue;
            }

            // Get threads
            try
            {
                processInfo.Threads = process.Threads.Cast<ProcessThread>().ToList();
            }
            catch { }

            // Get modules
            try
            {
                processInfo.Modules = process.Modules.Cast<ProcessModule>().ToList();
            }
            catch { }

            return processInfo;
        }
        catch
        {
            return null;
        }
    }

    public bool KillProcess(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ChangePriority(int processId, ProcessPriorityClass priority)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.PriorityClass = priority;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ProcessExists(int processId)
    {
        try
        {
            Process.GetProcessById(processId);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public long GetProcessMemory(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            return process.WorkingSet64;
        }
        catch
        {
            return 0;
        }
    }
}
