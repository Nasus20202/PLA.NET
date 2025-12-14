using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ProcessMonitor.Models;

namespace ProcessMonitor.Services;

public class ProcessService
{
    public async Task<List<ProcessInfo>> GetProcessesAsync()
    {
        return await Task.Run(() =>
        {
            var processes = new List<ProcessInfo>();

            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    DateTime startTime;
                    try
                    {
                        startTime = process.StartTime;
                    }
                    catch
                    {
                        startTime = DateTime.MinValue;
                    }

                    var processInfo = new ProcessInfo
                    {
                        Id = process.Id,
                        Name = process.ProcessName,
                        StartTime = startTime,
                        MainWindowTitle = process.MainWindowTitle,
                        WorkingSet = process.WorkingSet64,
                        PrivateMemory = process.PrivateMemorySize64,
                        Priority = process.PriorityClass,
                        ThreadCount = process.Threads.Count,
                        HandleCount = process.HandleCount,
                    };

                    processes.Add(processInfo);
                }
                catch
                {
                    // Skip processes we can't access
                }
            }

            return processes;
        });
    }

    public async Task<ProcessInfo?> GetProcessDetailsAsync(int processId)
    {
        return await Task.Run(() =>
        {
            try
            {
                var process = Process.GetProcessById(processId);

                DateTime startTime;
                try
                {
                    startTime = process.StartTime;
                }
                catch
                {
                    startTime = DateTime.MinValue;
                }

                List<ProcessThread> threads = new();
                try
                {
                    threads = process.Threads.Cast<ProcessThread>().ToList();
                }
                catch { }

                List<ProcessModule> modules = new();
                try
                {
                    modules = process.Modules.Cast<ProcessModule>().ToList();
                }
                catch { }

                var processInfo = new ProcessInfo
                {
                    Id = process.Id,
                    Name = process.ProcessName,
                    StartTime = startTime,
                    MainWindowTitle = process.MainWindowTitle,
                    Threads = threads,
                    Modules = modules,
                    WorkingSet = process.WorkingSet64,
                    PrivateMemory = process.PrivateMemorySize64,
                    Priority = process.PriorityClass,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                };

                return processInfo;
            }
            catch
            {
                return null;
            }
        });
    }

    public async Task<bool> KillProcessAsync(int processId)
    {
        return await Task.Run(() =>
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
        });
    }

    public async Task<bool> ChangePriorityAsync(int processId, ProcessPriorityClass priority)
    {
        return await Task.Run(() =>
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
        });
    }

    public async Task<bool> ProcessExistsAsync(int processId)
    {
        return await Task.Run(() =>
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
        });
    }

    public async Task<long> GetProcessMemoryAsync(int processId)
    {
        return await Task.Run(() =>
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
        });
    }
}
